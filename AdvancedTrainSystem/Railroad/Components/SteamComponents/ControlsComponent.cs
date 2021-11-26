using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Info;
using FusionLibrary;
using GTA.UI;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    public class ControlsComponent : Component
    {
        /// <summary>
        /// Gets or sets a normalized value indicating how much throttle is opened.
        /// </summary>
        public float Throttle { get; set; }

        /// <summary>
        /// Gets or sets a normalized value indicating how much drain cocks are opened.
        /// </summary>
        public float DrainCocks { get; set; }

        /// <summary>
        /// Gets or sets a normalized value indicating gear lever position.
        /// </summary>
        public float Gear { get; set; }

        private readonly Dictionary<string, Action<ControlsComponent, float>> _behaviours = new Dictionary<string, Action<ControlsComponent, float>>()
        {
            ["Throttle"] = (context, value) =>
            {
                context.Throttle = value;
            },
            ["Cocks"] = (context, value) =>
            {
                context.DrainCocks = value;
            },
            ["Gear"] = (context, value) =>
            {
                context.Gear = value;
            }
        };
        private readonly InteractiveController _interactableProps = new InteractiveController();

        private readonly Train train;

        public ControlsComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();

            train.TrainInfo.ControlBehaviourInfos.ForEach(info =>
            {
                var model = new CustomModel(info.ModelName);
                model.Request();

                // Create interactive prop
                InteractiveProp interactiveProp =  _interactableProps.Add(
                    model: model,
                    entity: train,
                    boneName: info.BoneName,
                    movementType: info.MovementType,
                    coordinateInteraction: info.Coordinate,
                    control: info.Control,
                    invert: info.Invert,
                    min: info.MinAngle,
                    max: info.MaxAngle,
                    startValue: info.StartValue,
                    sensitivityMultiplier: info.Sensetivity);

                // Create handle, if theres config for that
                if(info.AttachmentInfo != null)
                {
                    var handleInfo = info.AttachmentInfo;

                    var handle = new CustomModel(handleInfo.ModelName);
                    handle.Request();

                    var handleProp = _interactableProps.Add(
                        model: handle,
                        entity: interactiveProp,
                        boneName: handleInfo.BoneName,
                        movementType: handleInfo.MovementType,
                        coordinateInteraction: handleInfo.Coordinate,
                        toggle: false,
                        min: handleInfo.MinAngle,
                        max: handleInfo.MaxAngle,
                        startValue: 0f,
                        step: (float) handleInfo.Step,
                        stepRatio: 1f,
                        isIncreasing: handleInfo.MaxAngle < handleInfo.MinAngle,
                        smoothEnd: true);
                    interactiveProp.OnInteractionStarted += (_, __) => handleProp.Play();
                    interactiveProp.OnInteractionEnded += (_, __) => handleProp.Stop();
                }

                // Setup alternative controls
                if (info.AltControl != null)
                {
                    interactiveProp.SetupAltControl(
                        control: (GTA.Control)info.AltControl, 
                        invert: (bool)info.InvertAlt);
                }

                // Show label on hover
                interactiveProp.OnHover += DisplayTextPreview;
                interactiveProp.OnInteraction += DisplayTextPreview;
                interactiveProp.OnInteraction += UpdateBehaviour;

                // Save info in tag in order to access it later
                interactiveProp.Tag = info;
            });

            _interactableProps.Play();
        }

        private void UpdateBehaviour(object sender, InteractiveProp e)
        {
            var info = (TrainControlBehaviourInfo)e.Tag;

            float value = info.InvertValue ? e.CurrentValue : 1 - e.CurrentValue;

            _behaviours[info.ActionName]?.Invoke(this, value);
        }

        private void DisplayTextPreview(object sender, InteractiveProp e)
        {
            var info = (TrainControlBehaviourInfo) e.Tag;

            float value = info.InvertValue ? e.CurrentValue : 1 - e.CurrentValue;

            // Draw interactable text
            var textPosition = Screen.WorldToScreen(e.AnimateProp.WorldPosition);
            var text = $"{info.ActionName}: {value * 100:0}%";

            var textElement = new TextElement(
                caption: text, 
                position: textPosition, 
                scale: 0.6f,
                color: System.Drawing.Color.White,
                font: Font.ChaletComprimeCologne,
                alignment: Alignment.Center,
                shadow: true,
                outline: true);
            textElement.Draw();
        }

        public override void Start()
        {
            
        }

        public override void Update()
        {

        }

        public override void Dispose()
        {
            _interactableProps.Dispose();
        }
    }
}

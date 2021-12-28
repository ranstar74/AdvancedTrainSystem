using AdvancedTrainSystem.Core;
using AdvancedTrainSystem.Core.Components;
using AdvancedTrainSystem.Core.Info;
using FusionLibrary;
using GTA;
using GTA.UI;
using RageComponent;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    /// <summary>
    /// Defines interactive controls inside train.
    /// </summary>
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
        private readonly List<AnimateProp> _animatedProps = new List<AnimateProp>();
        private readonly Train _train;
        private CameraComponent _camera;
        private DerailComponent _derail;
        private DrivingComponent _driving;

        public ControlsComponent(ComponentCollection components) : base(components)
        {
            _train = GetParent<Train>();

            _train.TrainInfo.ControlBehaviourInfos.ToList().ForEach(bh =>
            {
                InteractiveProp interactiveProp = null;

                int index = 0;
                bh.AnimationProps.ForEach(prop =>
                {
                    CustomModel propModel = new CustomModel(prop.ModelName);
                    propModel.Request();

                    AnimateProp currentProp;
                    // The first prop is interactive one
                    if(index == 0)
                    {
                        // As mentioned below, first animation is used
                        // for interaciton
                        AnimationInfo anim = prop.Animations[0];

                        if(bh.Toggle)
                        {
                            interactiveProp = _interactableProps.Add(
                                model: propModel,
                                entity: _train,
                                boneName: prop.BoneName,
                                movementType: anim.AnimationType,
                                coordinateInteraction: anim.Coordinate,
                                min: anim.Minimum,
                                max: anim.Maximum,
                                startValue: bh.StartValue,
                                step: anim.Step,
                                isIncreasing: anim.IsIncreasing,
                                stepRatio: 1f,
                                toggle: true,
                                smoothEnd: true);
                        }
                        else
                        {
                            interactiveProp = _interactableProps.Add(
                                model: propModel,
                                entity: _train,
                                boneName: prop.BoneName,
                                movementType: anim.AnimationType,
                                coordinateInteraction: anim.Coordinate,
                                control: bh.ControlPrimary.Control,
                                invert: bh.InvertValue,
                                min: anim.Minimum,
                                max: anim.Maximum,
                                startValue: bh.StartValue,
                                sensitivityMultiplier: bh.Sensitivity);
                        }
                        interactiveProp.AnimateProp.PlayReverse = prop.PlayReverse;
                        interactiveProp.AnimateProp.PlayNextSteps = true;

                        if (bh.ControlSecondary != null)
                        {
                            interactiveProp.SetupAltControl(
                                control: bh.ControlSecondary.Control,
                                invert: bh.ControlSecondary.Invert);
                        }

                        // Show label on hover
                        interactiveProp.OnHover += DisplayTextPreview;
                        interactiveProp.OnInteraction += DisplayTextPreview;
                        interactiveProp.OnInteraction += UpdateBehaviour;

                        // Save info in tag in order to access it later
                        interactiveProp.Tag = bh;

                        interactiveProp.OnHover += (_, __) => CameraComponent.BlinkCrosshairThisFrame = true;

                        currentProp = interactiveProp;
                    }
                    // Every other prop is only visual
                    else
                    {
                        currentProp = new AnimateProp(
                            model: propModel,
                            entity: interactiveProp,
                            boneName: prop.BoneName,
                            keepCollision: false)
                        {
                            PlayReverse = prop.PlayReverse,
                            PlayNextSteps = true
                        };
                        currentProp.SpawnProp();

                        interactiveProp.OnInteractionStarted += (_, __) => currentProp.Play();
                        interactiveProp.OnInteractionEnded += (_, __) => currentProp.Play();

                        _animatedProps.Add(currentProp);
                    }

                    // For the first prop (which is interactive one) we skip
                    // the first animation because it's already was used in interactive prop constructor
                    List<AnimationInfo> anims = index == 0 ? prop.Animations.Skip(1).ToList() : prop.Animations;

                    anims.ForEach(anim =>
                    {
                        CoordinateSetting animSet = currentProp[anim.AnimationType][anim.AnimationStep][anim.Coordinate];

                        animSet.Setup(
                            stop: !anim.Loop,
                            isIncreasing: anim.IsIncreasing,
                            minimum: anim.Minimum,
                            maximum: anim.Maximum,
                            step: anim.Step,
                            maxMinRatio: 1f,
                            stepRatio: 1f,
                            smoothEnd: true);
                    });
                    index++;
                });
            });
            _interactableProps.Play();
        }

        public override void Start()
        {
            _camera = Components.GetComponent<CameraComponent>();
            _derail = Components.GetComponent<DerailComponent>();
            _driving = Components.GetComponent<DrivingComponent>();
        }

        private void UpdateBehaviour(object sender, InteractiveProp e)
        {
            if (!_driving.IsInCab)
                return;

            var behaviour = (TrainControlBehaviourInfo) e.Tag;

            float value = behaviour.InvertValue ? e.CurrentValue : 1 - e.CurrentValue;

            // No action required
            if (string.IsNullOrEmpty(behaviour.ActionName))
                return;

            _behaviours[behaviour.ActionName]?.Invoke(this, value);
        }

        private void DisplayTextPreview(object sender, InteractiveProp e)
        {
            if (!_driving.IsInCab)
                return;

            var info = (TrainControlBehaviourInfo) e.Tag;

            // TODO: Add display name and translations
            if (string.IsNullOrEmpty(info.ActionName))
                return;

            float value = info.InvertValue ? e.CurrentValue : 1 - e.CurrentValue;

            // Draw interactable text under corsshair
            var text = $"{info.ActionName}: {value * 100:0}%";

            var textElement = new TextElement(
                caption: text, 
                position: CameraComponent.CenterScreen + new System.Drawing.Size(0, 45), 
                scale: 0.53f,
                color: System.Drawing.Color.White,
                font: Font.ChaletLondon,
                alignment: Alignment.Center,
                shadow: true,
                outline: true);
            textElement.Draw();
        }

        public override void Update()
        {
            // Apply movement noise on each lever in train
            foreach(InteractiveProp inProp in _interactableProps.InteractiveProps)
            {
                AnimateProp anProp = inProp.AnimateProp;
                var info = inProp.Tag as TrainControlBehaviourInfo;

                if (info.Toggle == true)
                    continue;

                anProp.SecondRotation += _derail.Noise * 1.5f;
            }
        }

        public override void Dispose()
        {
            _interactableProps.Dispose();
            _animatedProps.ForEach(x => x.Dispose());
        }
    }
}

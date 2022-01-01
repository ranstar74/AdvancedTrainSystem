using AdvancedTrainSystem.Core.Data;
using FusionLibrary;
using GTA.UI;
using RageComponent.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedTrainSystem.Core.Components
{
    /// <summary>Handles interactive controls inside train.</summary>
    public abstract class Controls : TrainComponent
    {
        private readonly InteractiveController _interactableProps = new InteractiveController();
        private readonly List<AnimateProp> _animatedProps = new List<AnimateProp>();

        public Controls(ComponentCollection components) : base(components)
        {
            foreach (var bh in Train.TrainData.ControlBehaviourData)
            {
                InteractiveProp interactiveProp = null;

                int index = 0;
                bh.AnimationProps.ForEach(prop =>
                {
                    CustomModel propModel = new CustomModel(prop.ModelName);
                    propModel.Request();

                    AnimateProp currentProp;
                    // The first prop is interactive one
                    if (index == 0)
                    {
                        // As mentioned below, first animation is used
                        // for interaciton
                        AnimationInfo anim = prop.Animations[0];

                        if (bh.Toggle)
                        {
                            interactiveProp = _interactableProps.Add(
                                model: propModel,
                                entity: Train,
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
                                entity: Train,
                                boneName: prop.BoneName,
                                movementType: anim.AnimationType,
                                coordinateInteraction: anim.Coordinate,
                                control: bh.ControlPrimary.Control,
                                invert: bh.ControlPrimary.Invert,
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

                        interactiveProp.OnHover += (_, __) => Core.Components.CinematicCamera.BlinkCrosshairThisFrame = true;

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
            }
            _interactableProps.Play();
        }

        public override void Start()
        {
            base.Start();

            Driving.OnEnter += () =>
            {
                _interactableProps.UseAltControl = true;
            };
            Driving.OnLeave += () =>
            {
                _interactableProps.UseAltControl = false;
            };

            // Restore after reload
            if (Driving.IsControlledByPlayer)
            {
                _interactableProps.UseAltControl = true;
            }
        }

        /// <summary>Calls when control behaviour needs to be resolved.</summary>
        /// <param name="behaviour">Name of the behaviour.</param>
        /// <param name="value">Behaviour value to resolve.</param>
        /// <param name="resolved">Whether behaviour was resolved or not.</param>
        protected virtual void ResolveBehaviour(string behaviour, float value, bool resolved)
        {
            if(!resolved)
            {
                throw new Exception($"Behaviour: {behaviour} was not resolved.");
            }
        }

        private void UpdateBehaviour(object sender, InteractiveProp e)
        {
            if (!CanInteract())
                return;

            var behaviour = (TrainControlBehaviourData)e.Tag;

            float value = behaviour.InvertValue ? e.CurrentValue : 1 - e.CurrentValue;

            // No action required
            if (string.IsNullOrEmpty(behaviour.ActionName))
                return;

            ResolveBehaviour(behaviour.ActionName, value, false);
        }

        private void DisplayTextPreview(object sender, InteractiveProp e)
        {
            if (!CanInteract())
                return;

            var info = (TrainControlBehaviourData)e.Tag;

            // TODO: Add display name and translations
            if (string.IsNullOrEmpty(info.ActionName))
                return;

            float value = info.InvertValue ? e.CurrentValue : 1 - e.CurrentValue;

            // Draw interactable text under corsshair
            var text = $"{info.ActionName}: {value * 100:0}%";

            var textElement = new TextElement(
                caption: text,
                position: Core.Components.CinematicCamera.CenterScreen + new System.Drawing.Size(0, 45),
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
            foreach (InteractiveProp inProp in _interactableProps.InteractiveProps)
            {
                AnimateProp anProp = inProp.AnimateProp;
                var info = inProp.Tag as TrainControlBehaviourData;

                if (info.Toggle == true)
                    continue;

                anProp.SecondRotation += Motion.Noise * 1.5f;
            }
        }

        private bool CanInteract()
        {
            if (!FusionUtils.IsCameraInFirstPerson())
                return false;

            if (!Driving.IsInCab)
                return false;

            return true;
        }

        public override void Dispose()
        {
            _interactableProps.Dispose();
            _animatedProps.ForEach(x => x.Dispose());
        }
    }
}

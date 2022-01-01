using GTA;
using GTA.Math;
using System.Collections.Generic;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Core.Data
{
    /// <summary>Defines animation for a prop.</summary>
    public class AnimationInfo
    {
        /// <summary>Movement type of the animation.</summary>
        public AnimationType AnimationType { get; set; }

        /// <summary>Coordinate of movement of the animation.</summary>
        public Coordinate Coordinate { get; set; }

        /// <summary>Defines an order of playing this animation comparing to other anims.</summary>
        public AnimationStep AnimationStep { get; set; }

        /// <summary>Defines whether animation should stop after reaching start/min values or not.</summary>
        public bool Loop { get; set; }

        /// <summary>Defines speed of the animation.<para>Positive value.</para></summary>
        public float Step { get; set; }

        /// <summary>Whether value should go from min to max or from max to min.</summary>
        public bool IsIncreasing { get; set; }

        /// <summary>Minimum value (angle / offset) of the animation.</summary>
        public float Minimum { get; set; }

        /// <summary>Maximum value (angle / offset) of the animation.</summary>
        public float Maximum { get; set; }
    }

    /// <summary>
    /// Defines a prop with animation.
    /// </summary>
    public class AnimatedPropInfo
    {
        /// <summary>Name of the animated model.</summary>
        public string ModelName { get; set; }

        /// <summary>Name of bone that prop will be attached to.</summary>
        public string BoneName { get; set; }

        /// <summary>If True, animation will be acted like a switch, 
        /// otherwise animation will be stopped with interaction.</summary>
        public bool PlayReverse { get; set; }

        /// <summary>Animation set of this prop.</summary>
        public List<AnimationInfo> Animations { get; set; }
    }

    /// <summary>
    /// Defines a train control hot-key binding.
    /// </summary>
    public class TrainControlBinding
    {
        /// <summary>Type of the game control.</summary>
        public Control Control { get; set; }

        /// <summary>Defines whether control needs to be inverted.</summary>
        public bool Invert { get; set; }
    }

    /// <summary>
    /// Defines a train control (such as lever, valve or button) behaviour.
    /// </summary>
    public class TrainControlBehaviourData
    {
        /// <summary>
        /// Action name that is supported by train type.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Whether control acts like a switch or not.
        /// </summary>
        public bool Toggle { get; set; }

        /// <summary>
        /// Control that is used when player is not in a train.
        /// </summary>
        public TrainControlBinding ControlPrimary { get; set; }

        /// <summary>Control that is used when player is in a train.</summary>
        /// <remarks>Set to null if not required.</remarks>
        public TrainControlBinding ControlSecondary { get; set; }

        /// <summary>Defines whether control output value will be inverted.</summary>
        public bool InvertValue { get; set; }

        /// <summary>Defines start value in range from
        /// <see cref="AnimationInfo.Minimum"/> to 
        /// <see cref="AnimationInfo.Maximum"/>.</summary>
        public float StartValue { get; set; }

        /// <summary>Defines how fast control moves.</summary>
        public float Sensitivity { get; set; }

        /// <summary>Offset of label with information about control.</summary>
        public Vector3 LabelOffset { get; set; }

        /// <summary>List of props that are used in animation.
        /// <para>At least one is required.</para>
        /// <para>The first one will be used as primary so will be interactable.</para>
        /// </summary>
        public List<AnimatedPropInfo> AnimationProps { get; set; }
    }
}

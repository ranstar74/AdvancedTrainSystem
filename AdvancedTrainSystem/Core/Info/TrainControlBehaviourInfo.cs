using GTA;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Core.Info
{
    /// <summary>
    /// Defines a basic model with animation.
    /// </summary>
    public class AnimationInfo
    {
        /// <summary>
        /// Name of the animated model.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Name of bone that prop will be attached to.
        /// </summary>
        public string BoneName { get; set; }

        /// <summary>
        /// Movement type of the animation.
        /// </summary>
        public AnimationType MovementType { get; set; }

        /// <summary>
        /// Coordinate of movement of the animation.
        /// </summary>
        public Coordinate Coordinate { get; set; }

        /// <summary>
        /// Minimum angle of the animation.
        /// </summary>
        public float MinAngle { get; set; }

        /// <summary>
        /// Maximum angle of the animation.
        /// </summary>
        public float MaxAngle { get; set; }
    }

    /// <summary>
    /// Defines an animation that is played when interaction starts / ends.
    /// </summary>
    public class TrainControlAttachmentInfo : AnimationInfo
    {
        /// <summary>
        /// Defines speed of the animation.
        /// <para>
        /// Positive value.
        /// </para>
        /// </summary>
        public float Step { get; set; }
    }

    /// <summary>
    /// Defines a train control (such as lever, valve or button) behaviour.
    /// </summary>
    public class TrainControlBehaviourInfo : AnimationInfo
    {
        /// <summary>
        /// Action name that is supported by train type.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Control that is used when player not in train.
        /// </summary>
        public Control Control { get; set; }

        /// <summary>
        /// Control that is used when player in train.
        /// </summary>
        /// <remarks>
        /// Set to null if not required.
        /// </remarks>
        public Control? AltControl { get; set; }

        /// <summary>
        /// Defines whether control value will be inverted.
        /// </summary>
        public bool InvertValue { get; set; }

        /// <summary>
        /// Defines whether <see cref="Control"/> will be inverted.
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// Defines whether the <see cref="AltControl"/> will be inverted.
        /// </summary>
        public bool? InvertAlt { get; set; }

        /// <summary>
        /// Defines start value in range of <see cref="AnimationInfo.MinAngle"/> to <see cref="AnimationInfo.MaxAngle"/>.
        /// </summary>
        public float StartValue { get; set; }

        /// <summary>
        /// Defines how fast control moves.
        /// </summary>
        public float Sensetivity { get; set; }

        /// <summary>
        /// Additional animation that will be played when interaction starts / ends.
        /// </summary>
        public TrainControlAttachmentInfo AttachmentInfo { get; set; }
    }
}

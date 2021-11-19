using AdvancedTrainSystem.Core.Components.Enums;
using FusionLibrary;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Decorator constants.
    /// </summary>
    internal class Constants
    {
        /// <summary>
        /// Number of train carriages.
        /// </summary>
        internal const string TrainCarriagesNumber = "TrainCarriagesNumber";

        /// <summary>
        /// Handle of visible carriage of visible/invisible pair.
        /// </summary>
        internal const string TrainVisibleCarriageHandle = "TrainCarriageEntityHandle";

        /// <summary>
        /// Handle of train head.
        /// </summary>
        internal const string TrainHeadHandle = "TrainHeadEntityHandle";

        /// <summary>
        /// <see cref="TrainType"/> type.
        /// </summary>
        internal const string TrainType = "TrainType";

        /// <summary>
        /// Train handle.
        /// </summary>
        internal const string TrainHandle = "TrainHandle";

        /// <summary>
        /// Direction of the train.
        /// </summary>
        internal const string TrainDirection = "TrainDirection";

        /// <summary>
        /// Current <see cref="LightState"/>.
        /// </summary>
        internal const string TrainLightState = "TrainLightState";

        /// <summary>
        /// Config train was created from.
        /// </summary>
        internal const string TrainMissionId = "TrainMissionId";

        /// <summary>
        /// Registers all decorators. Call it in first frame.
        /// </summary>
        internal static void RegisterDecorators()
        {
            Decorator.Register(TrainCarriagesNumber, FusionEnums.DecorType.Int);
            Decorator.Register(TrainVisibleCarriageHandle, FusionEnums.DecorType.Int);
            Decorator.Register(TrainHeadHandle, FusionEnums.DecorType.Int);
            Decorator.Register(TrainType, FusionEnums.DecorType.Int);
            Decorator.Register(TrainHandle, FusionEnums.DecorType.Int);
            Decorator.Register(TrainDirection, FusionEnums.DecorType.Bool);
            Decorator.Register(TrainLightState, FusionEnums.DecorType.Int);
            Decorator.Register(TrainMissionId, FusionEnums.DecorType.Int);

            Decorator.Lock();
        }
    }
}

using FusionLibrary;

namespace AdvancedTrainSystem.Data
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
        /// Is this train is CustomTrain or not.
        /// </summary>
        internal const string TrainIsCustom = "IsCustomTrain";

        /// <summary>
        /// Train unique number.
        /// </summary>
        internal const string TrainGuid = "TrainGuid";

        /// <summary>
        /// Registers all decorators. Call it in first frame.
        /// </summary>
        internal static void RegisterDecorators()
        {
            Decorator.Register(TrainCarriagesNumber, FusionEnums.DecorType.Int);
            Decorator.Register(TrainVisibleCarriageHandle, FusionEnums.DecorType.Int);
            Decorator.Register(TrainHeadHandle, FusionEnums.DecorType.Int);
            Decorator.Register(TrainIsCustom, FusionEnums.DecorType.Bool);
            Decorator.Register(TrainGuid, FusionEnums.DecorType.Int);

            Decorator.Lock();
        }
    }
}

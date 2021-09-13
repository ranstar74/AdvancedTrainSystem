using FusionLibrary;

namespace RogersSierra.Train
{
    /// <summary>
    /// Contains both invisible and visible models of the train.
    /// </summary>
    public class TrainModel
    {
        /// <summary>
        /// Invisible model of carriage. Used only for position and rotation.
        /// </summary>
        public CustomModel InvisibleModel { get; set; }

        /// <summary>
        /// Visible model of carriage.
        /// </summary>
        public CustomModel VisibleModel { get; set; }

        /// <summary>
        /// Constructs new instance of <see cref="TrainModel"/>.
        /// </summary>
        /// <param name="invisibleModel"><paramref name="invisibleModel"/></param>
        /// <param name="visibleModel"><paramref name="visibleModel"/></param>
        public TrainModel(CustomModel invisibleModel, CustomModel visibleModel)
        {
            InvisibleModel = invisibleModel;
            VisibleModel = visibleModel;
        }
    }
}

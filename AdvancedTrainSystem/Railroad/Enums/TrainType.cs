namespace AdvancedTrainSystem.Railroad.Enums
{
    /// <summary>
    /// An enumeration of supported train types.
    /// </summary>
    public enum TrainType
    {
        /// <summary>
        /// Train that runs on steam power
        /// </summary>
        Steam,
        /// <summary>
        /// Train that runs on diesel engine
        /// </summary>
        Diesel,
        /// <summary>
        /// Train that runs on electricity
        /// </summary>
        Electric,
        /// <summary>
        /// Car that is powered by its passengers
        /// </summary>
        Handcar,
        /// <summary>
        /// Cart that is used in mines.
        /// </summary>
        /// <remarks>
        /// The only way to move is push it somehow
        /// </remarks>
        Minecart
    }
}

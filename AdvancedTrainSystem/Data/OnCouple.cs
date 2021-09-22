using AdvancedTrainSystem.Train;

namespace AdvancedTrainSystem.Data
{
    /// <summary>
    /// Invokes on coupling with other train.
    /// </summary>
    /// <param name="train">Train we coupling with.</param>
    /// <param name="s1">Speed of first train.</param>
    /// <param name="s2">Speed of second train.</param>
    public delegate void OnCouple(CustomTrain train, float s1, float s2);
}

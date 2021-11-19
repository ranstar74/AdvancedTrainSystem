using AdvancedTrainSystem.Railroad.SharedComponents.Abstract;

namespace AdvancedTrainSystem.Railroad.SharedComponents.Interfaces
{
    /// <summary>
    /// Defines a train that have electricity generator with lights.
    /// </summary>
    public interface IHasElectricity
    {
        LightComponent LightComponent { get; }

        GeneratorComponent GeneratorComponent { get; }
    }
}

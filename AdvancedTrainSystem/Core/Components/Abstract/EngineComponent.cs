using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core.Components.Abstract
{
    public class EngineComponent : Component
    {
        private PhysxComponent physx;
        public EngineComponent(ComponentCollection components) : base(components)
        {
            
        }

        public override void Start()
        {
            physx = Components.GetComponent<PhysxComponent>();
        }
    }
}

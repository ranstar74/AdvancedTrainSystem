using RageComponent;
using RageComponent.Core;

namespace AdvancedTrainSystem.Core.Components.Abstract.AnimComponents
{
    /// <summary>
    /// Defines a basic animation component.
    /// <para>
    /// A (temporary?) workaround until we have proper working AnimKit...
    /// </para>
    /// </summary>
    public abstract class AnimComponent : Component
    {
        protected PhysxComponent physx;
        protected DerailComponent derail;

        protected Train train;

        public AnimComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<Train>();
        }

        public override void Start()
        {
            physx = Components.GetComponent<PhysxComponent>();
            derail = Components.GetComponent<DerailComponent>();
        }
    }
}

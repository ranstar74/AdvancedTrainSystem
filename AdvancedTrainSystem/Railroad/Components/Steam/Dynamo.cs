using AdvancedTrainSystem.Railroad.Components.Common;
using FusionLibrary.Extensions;
using RageComponent.Core;

namespace AdvancedTrainSystem.Railroad.Components.Steam
{
    /// <summary>
    /// Simulates electric components such as lights that powered by dynamo generator.
    /// </summary>
    public class Dynamo : Generator
    {
        /// <summary>Gets a normalized value indicating how much steam is going through dynamo generator.</summary>
        public bool Valve { get; private set; }

        /// <summary>
        /// Gets a normalized value indicating dynamo generator output.</summary>
        public override float Output => output;

        /// <summary>
        /// How much pressure is required for dynamo to start working and reach its maximum power.
        /// </summary>
        /// <remarks>
        /// This value is mapped to output.
        /// <para>
        /// Means that when pressure reaches threshold power will start raising until it gets to maimumPressure,
        /// which will equal Power = 1f
        /// </para>
        /// </remarks>
        private const float pressureThreshold = 0.3f;

        private Boiler boiler;
        private float output;

        public Dynamo(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();

            boiler = Components.GetComponent<Boiler>();
        }

        public override void Update()
        {
            // Cut any pressure below threshold
            float pressure = boiler.Pressure - pressureThreshold;

            output = MathExtensions.Clamp(pressure, 0f, 1f);
        }
    }
}

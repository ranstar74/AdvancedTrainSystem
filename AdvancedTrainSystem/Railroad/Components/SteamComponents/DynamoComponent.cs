using AdvancedTrainSystem.Railroad.SharedComponents.Abstract;
using FusionLibrary.Extensions;
using RageComponent.Core;
using System;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    /// <summary>
    /// Simulates electric components such as lights that powered by dynamo generator.
    /// </summary>
    public class DynamoComponent : GeneratorComponent
    {
        /// <summary>
        /// Gets a normalized value indicating how much steam is going through dynamo generator.
        /// </summary>
        public bool Valve { get; private set; }

        /// <summary>
        /// Gets a normalized value indicating 
        /// </summary>
        public override float Output => output;

        /// <summary>
        /// Defines a minimum boiler pressure on which dynamo will start working.
        /// </summary>
        private const int pressureThreshold = 90;

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
        private const int maximumPressure = 60;

        private readonly SteamTrain train;
        private BoilerComponent boiler;
        private float output;

        public DynamoComponent(ComponentCollection components) : base(components)
        {
            train = GetParent<SteamTrain>();
        }

        public override void Start()
        {
            boiler = Components.GetComponent<BoilerComponent>();

            // Cut any pressure below threshold
            float pressure = Math.Abs(boiler.PressurePSI - pressureThreshold);

            output = pressure.Remap(0f, maximumPressure, 0f, 1f);
        }
    }
}

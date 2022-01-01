using FusionLibrary.Extensions;
using GTA;
using System;

namespace AdvancedTrainSystem.Railroad.Components.Common
{
    /// <summary>Defines a train fuel.</summary>
    public abstract class TrainFuel
    {
        /// <summary>Gets a value indicating how much heat fuel produces while burning.</summary>
        public abstract float Power { get; }

        /// <summary>Gets a value indicating how much time fuel will burn.</summary>
        public abstract int BurnTime { get; }

        /// <summary>Gets a value indicating abstract size of fuel. 
        /// <para>Used to calculate how many of fuel can be in firebox.</para></summary>
        public abstract int Size { get; }

        /// <summary>Gets a value indicating if fuel is done.</summary>
        public bool Burned { get; private set; } = false;

        /// <summary>Gets a value indicating how much size left from original size after burning.</summary>
        public float CurrentSize => Size * Health;

        /// <summary>Gets a normalized value indicating health of fuel.
        /// More time fuel burns, less health fuel have.</summary>
        public float Health => _health;

        private float _health;

        private static readonly Random _rand = new Random();

        private int burnStartTime = -1;
        private int burnUntilTime = -1;

        /// <summary>Creates a new instance of <see cref="TrainFuel"/> with defined power.</summary>
        public TrainFuel()
        {
            Ignite();
        }

        /// <summary>Ignites fuel.</summary>
        private void Ignite()
        {
            if (burnStartTime != -1)
                return;

            burnStartTime = Game.GameTime;
            burnUntilTime = burnStartTime + BurnTime;
        }

        /// <summary>Gets heat from burning fuel.</summary>
        public float GetHeat()
        {
            if (Game.GameTime < burnUntilTime)
            {
                // When fuel burns it's most efficient on beginning
                // and least efficient when it's almost burned
                float burnTimeLeft = (float)burnUntilTime - Game.GameTime;

                _health = burnTimeLeft.Remap(0f, BurnTime, 0f, 1f);

                return (float)_rand.NextDouble(0.05f, 0.1f) * Power * _health * Game.LastFrameTime;
            }
            Burned = true;

            return 0f;
        }
    }

    /// <summary>Defines a burnable coal.</summary>
    public class Coal : TrainFuel
    {
        /// <summary>Gets a value indicating how much heat coal produces while burning.</summary>
        public override float Power => 1f;

        /// <summary>Gets a value indicating how much time coal will burn.</summary>
        public override int BurnTime => 10 * 60 * 1000;

        /// <summary>Gets a value indicating abstract size of coal.</summary>
        public override int Size => 1;

        /// <summary>Creates a new instance of <see cref="Coal"/> with size of 1.
        /// <para>Coal burn with power of 1 for 10 minutes.</para></summary>
        public Coal()
        {

        }
    }
}

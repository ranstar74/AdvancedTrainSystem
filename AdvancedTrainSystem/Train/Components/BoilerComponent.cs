using GTA;
using RageComponent;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Simple simulation of boiler pressure.
    /// </summary>
    public class BoilerComponent : Component<CustomTrain>
    {
        /// <summary>
        /// Pressure of the boiler in PSI.
        /// </summary>
        public float Pressure { get; private set; }

        /// <summary>
        /// Is there steam coming cylinder.
        /// </summary>
        public bool CylindersSteam => Pressure > 160;

        private float _releaseTime = 0;

        public BoilerComponent() : base()
        {

        }

        public override void Start()
        {
            Pressure = 260;
        }

        public override void OnTick()
        {
            Pressure += 3f * Game.LastFrameTime;

            // Safety valve
            if (Pressure > 260)
                _releaseTime = Game.GameTime + 1000;
            else
                _releaseTime = 0;

            if (_releaseTime > Game.GameTime)
            {
                Pressure -= 10f * Game.LastFrameTime;
            }

            var throttle = Base.SpeedComponent.Throttle;

            Pressure -= 3.1f * throttle * Game.LastFrameTime;

            // GTA.UI.Screen.ShowSubtitle($"Boiler Pressure: {Pressure.ToString("0.00")}");
        }
    }
}

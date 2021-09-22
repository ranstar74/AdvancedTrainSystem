using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent;
using System;
using System.Linq;

namespace AdvancedTrainSystem.Train.Components
{
    /// <summary>
    /// Calculates speed of the train.
    /// </summary>
    public class SpeedComponent : Component<CustomTrain>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Entity Entity { get; set; }

        /// <summary>
        /// Speed of the train.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Previous frame <see cref="Speed"/>.
        /// </summary>
        private float _prevSpeed;

        /// <summary>
        /// How much throttle is opened. 0 is closed, 1 is fully opened.
        /// </summary>
        public float Throttle { get; internal set; }

        /// <summary>
        /// Gear. Also known as Johnson bar. 1 forward, -1 backward
        /// </summary>
        public float Gear { get; internal set; }

        /// <summary>
        /// How fast train accelerates.
        /// </summary>
        public float AccelerationMultiplier = 0.2f;

        /// <summary>
        /// Returns True if drive wheel are sparking, otherwise False.
        /// </summary>
        public bool AreWheelSpark { get; private set; }

        private bool _onTrainStartInvoked = false;
        /// <summary>
        /// Invokes when train started moving.
        /// </summary>
        public Action OnTrainStart { get; set; }

        /// <summary>
        /// Whether train is accelerating or not.
        /// </summary>
        public bool IsTrainAccelerating { get; private set; }

        /// <summary>
        /// Drive wheels speed. Could be higher than train speed if train slips.
        /// </summary>
        public float DriveWheelSpeed { get; private set; }

        /// <summary>
        /// Last forces that were applied on train.
        /// </summary>
        public float LastForces { get; private set; }

        public int lastfDir = 1;

        public float LastExternalForces = 0;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Start()
        {
            ((Vehicle)Entity).SetTrainCruiseSpeed(0);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void OnTick()
        {
            // Since invisible vehicle aren't used after derail we just set speed on zero
            if (Base.DerailComponent.IsDerailed)
            {
                Speed = 0;
                return;
            }

            (float totalForces, float externalForces) = CalculateForces();
            Speed += totalForces + LastExternalForces;// + LastExternalForces;

            // Set speed
            _prevSpeed = Speed;
            Base.Speed = Speed;

            //if (Base.TrainHead != Game.Player.Character.CurrentVehicle)
            //    GTA.UI.Screen.ShowSubtitle($"S1: {Speed} SA: {Base.Speed} tf: {totalForces} f:{externalForces}");

        }

        /// <summary>
        /// Calculates all train forces.
        /// </summary>
        /// <returns>All train forces of this frame. External forces on this train (i.e. other train pushing).</returns>
        private (float totalForces, float pushForces) CalculateForces()
        {
            float otherForces = 0;

            // TODO: Take uphill/downhill into account

            // Acceleration = (v1 - v2) / t
            float acceleration = (Speed - _prevSpeed) * Game.LastFrameTime;

            float velocty = Entity.Velocity.Length();
            float airBrakeInput = Base.BrakeComponent.AirbrakeForce;
            float steamBrakeInput = 1 - Base.BrakeComponent.FullBrakeForce;
            float boilerPressure = Base.BoilerComponent.Pressure.Remap(0, 300, 0, 1);

            // Calculate forces

            // Wheel traction - too fast acceleration will cause bad traction
            float wheelTraction = Math.Abs(Speed).Remap(2, 0, 0, 40).Remap(0, 40, 0, 18);
            wheelTraction *= ((float)Math.Pow(Throttle, 10)).Remap(0, 1, 0, 2);
            if (Speed > 10 || wheelTraction < 1)
                wheelTraction = 1;

            // Surface resistance force - wet surface increases resistance
            float surfaceResistance = RainPuddleEditor.Level + 1;

            float wheelRatio = (Speed.Remap(0, 40, 40, 0) + 0.01f) / (DriveWheelSpeed + 0.01f);
            wheelRatio = wheelRatio / (150 * surfaceResistance.Remap(1, 2, 1, 1.3f)) + 1;

            // Friction force = 0.2 * speed * difference between wheel and train speed
            float frictionForce = 0.2f * Speed / 2 * wheelRatio;

            // Brake force
            float brakeForce = Speed * airBrakeInput * 2;

            // Air resistance force = 0.02 * velocty^2
            float dragForce = (float)(0.02f * Math.Pow(velocty, 2)) / 8;

            // Inercia force = acceleration * mass
            float inerciaForce = acceleration * 5;

            // How much steam going into cylinder
            float steamForce = Throttle.Remap(0, 1, 0, 4) * Gear * boilerPressure;

            // Direction of force
            float forceFactor = Throttle <= 0.1f || Math.Abs(Gear) <= 0.1f ? Speed : Gear;
            int forceDirection = forceFactor >= 0 ? 1 : -1;

            // Brake multiplier
            float brakeMultiplier = airBrakeInput.Remap(0, 1, 1, 0);

            // Combine all forces
            float totalForce = (steamForce * brakeMultiplier * steamBrakeInput) - dragForce + inerciaForce - frictionForce - brakeForce;
            totalForce *= AccelerationMultiplier * Game.LastFrameTime;

            lastfDir = forceDirection;
            // Combine soft coupled trains
            for (int i = 0; i < Base.CollisionComponent.SoftCoupledTrains.Count; i++)
            {
                var train = Base.CollisionComponent.SoftCoupledTrains[i];

                // Two trains force = T1.Force * T1.Dir * T2.Force * T2.Dir
                // So basically Ft = F1 + F2 (if forces include direction)

                // The easiest way to get train direction is just reuse one that was
                // used when creating mission train, its not really right way but 
                // since theres only two options how train could be aligned (-- > -- > and -- > < --)
                // theres no reason to calculate dot vector or something


                //  TOTAL FORMULA EQUALS: S1 + I(MAX(S2 - S1, 0))
                //  invert = train1.dir != train2.dir
                // Lets test it in all conditions:
                // 
                //  Condition 1: S1 must be > S2
                // --- > --- >
                // invert = false
                // S1 = 50
                // S2 = 48
                // E1 = 50 + (48 - 50) = 50 + 0 = 50 OK
                // E2 = 48 + (50 - 48) = 48 + 2 = 50 OK
                // - Success
                // S1 = 48
                // S2 = 50
                // E1 = 48 + (50 - 48) = 48 + 2 = 50 OK (IMPOSSIBLE CONDITION WITH PUSHING)
                // E2 = 50 + (48 - 50) = 50 + 0 = 50 OK
                // - Success, Here train slow downs so it automatically detaches, so E1 is impossible (if not coupled)
                //
                //  Condition 2: S1 or S2 must be > 0
                // --- > < ---
                // invert = true
                // S1 = 10
                // S2 = 8
                // E1 = 10 + -0 = 10 OK 
                // E2 = 8 + -2  = 6 FAIL ( MUST BE -16 )
                // S1 = 48
                // S2 = 50

                //var speedDifference = train.Speed - Speed;

                //var force = Math.Max(train.Speed - Speed, 0);

                //var S1 = Speed;
                //var S2 = train.Speed;
                //var speedDiff = Math.Max(S2 - S1, 0);

                //if(Game.Player.Character.CurrentVehicle != Base.TrainHead)
                //    GTA.UI.Screen.ShowSubtitle($"S1 {S1} S2 {S2} R {speedDiff} R2 {speedDiff + S1}");

                //if (train.Direction != Base.Direction)
                //    speedDiff *= -1;
                //speedDiff += S1;

               // speedDiff;

                //force += Speed;
                //force *= Game.LastFrameTime;
                //LastExternalForces = force;
                //othersTrainForce = Math.Max(othersTrainForce, 0);

                //Calculate offset we need to move train on to perfectly balance
                //distance between trains
                //float feedBackVelocity = 0;
                //if (Base.TrainHead.IsTouching(train.TrainHead))
                //{
                //    var speedDifference = Math.Abs(Base.Speed - train.Speed);

                //    feedBackVelocity = -speedDifference * Game.LastFrameTime;
                //}


                //float offset = Math.Abs(Base.Speed - train.Speed);

                //var dir = train.TrainHead.FrontPosition - Base.TrainHead.Position;

                //if (dir.X < 0)
                //    offset *= -1;

                //GTA.UI.Screen.ShowSubtitle($"S1: {Base.SpeedComponent.LastForces} S2: {train.SpeedComponent.LastForces}");

                LastExternalForces = train.SpeedComponent.LastForces;
                //train.SpeedComponent.LastExternalForces = othersTrainForce;
                //otherForces = othersTrainForce; //othersTrainForce;// + feedBackVelocity;// + feedBackVelocity;// + offset;
                //totalForce += othersTrainForce;
                //Speed += othersTrainForce;
            }

            // We making it non directional because wheel and speed direction doesn't always match
            float baseWheelSpeed = Math.Abs(Speed);

            DriveWheelSpeed = baseWheelSpeed * wheelTraction * steamBrakeInput * forceDirection;

            // Check if train is accelerating
            IsTrainAccelerating = Math.Abs(steamForce) > 0;

            // Check if wheel are sparking
            AreWheelSpark = wheelTraction > 5 || (steamBrakeInput == 0 && baseWheelSpeed > 1.5f);

            // Invoke OnTrainStart
            var absSpeed = Math.Abs(Speed);
            if (absSpeed > 0.3f && absSpeed < 4f && IsTrainAccelerating)
            {
                if (!_onTrainStartInvoked)
                {
                    OnTrainStart?.Invoke();
                    _onTrainStartInvoked = true;
                }
            }
            else
            {
                _onTrainStartInvoked = false;
            }

            //GTA.UI.Screen.ShowSubtitle(
            //    $"F: {frictionForce.ToString("0.00")} " +
            //    $"D:{dragForce.ToString("0.00")} " +
            //    $"I: {inerciaForce.ToString("0.00")} " +
            //    $"S: {steamForce.ToString("0.00")} " +
            //    $"T: {totalForce.ToString("0.00")} " +
            //    $"FD: {forceDirection}");

            LastForces = totalForce; 
            //LastExternalForces = otherForces;
            return (totalForce, otherForces);
        }
        
        /// <summary>
        /// Applies some external force on train.
        /// </summary>
        public void ApplyForce(float force)
        {
            Speed += force;
        }
    }
}

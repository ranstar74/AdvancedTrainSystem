using FusionLibrary;

namespace AdvancedTrainSystem
{
    /// <summary>
    /// This class was implemented as temporary solution until
    /// we have properly working animation kit.
    /// </summary>
    internal class ModelHandler : CustomModelHandler
    {
        // TODO: Implement some system that will load/unloading components
        // as they needed / no longer needed

        // ROGERS SIERRA

        // Drivetrain
        public static CustomModel SierraCouplingRod = new CustomModel("sierra_coupling_rod");
        public static CustomModel SierraConnectingRod = new CustomModel("sierra_connecting_rod");
        public static CustomModel SierraPiston = new CustomModel("sierra_piston");
        public static CustomModel SierraCombinationLever = new CustomModel("sierra_combination_lever");
        public static CustomModel SierraRadiusRod = new CustomModel("sierra_radius_rod");
        public static CustomModel SierraValveRod = new CustomModel("sierra_valve_rod");

        // Wheels
        public static CustomModel SierraFrontWheel = new CustomModel("sierra_fwheel");
        public static CustomModel SierraDrivingWheel = new CustomModel("sierra_dwheel");
        public static CustomModel SierraTenderWheel = new CustomModel("sierra_twheel");

        // Brakes
        public static CustomModel SierraAirbrakeMain = new CustomModel("sierra_airbrake_main");
        public static CustomModel SierraAirbrakeRod = new CustomModel("sierra_airbrake_rod");
        public static CustomModel SierraAirbrakeLever = new CustomModel("sierra_airbrake_lever");
        public static CustomModel SierraBrake1 = new CustomModel("sierra_brake_1");
        public static CustomModel SierraBrake2 = new CustomModel("sierra_brake_2");
        public static CustomModel SierraBrake3 = new CustomModel("sierra_brake_3");

        // Cab
        public static CustomModel CabThrottleLever = new CustomModel("sierra_throttle_lever");
        public static CustomModel CabThrottleHandle = new CustomModel("sierra_throttle_lever_handle");
        public static CustomModel CabGearLever = new CustomModel("sierra_gear_lever");
        public static CustomModel CabGearHandle = new CustomModel("sierra_gear_lever_handle");
        public static CustomModel CabAirBrakeLever = new CustomModel("sierra_cab_airbrake_lever");
        public static CustomModel CabSteamBrakeLever = new CustomModel("sierra_cab_brake_lever");
        public static CustomModel CabWindowRight = new CustomModel("sierra_cab_window_right");
        public static CustomModel CabWindowLeft = new CustomModel("sierra_cab_window_left");

        /// <summary>
        /// Requests all models.
        /// </summary>
        public static void RequestAll()
        {
            var allModels = GetAllModels(typeof(ModelHandler));

            foreach(var model in allModels)
            {
                model.Request();
            }
        }
    }
}

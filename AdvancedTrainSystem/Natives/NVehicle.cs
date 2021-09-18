using GTA;
using GTA.Math;
using GTA.Native;

namespace AdvancedTrainSystem.Natives
{
    /// <summary>
    /// Vehicle natives.
    /// </summary>
    internal class NVehicle
    {
        /// <summary>
        /// Creates train.
        /// </summary>
        /// <param name="variation">ID of the train_config, look in trains.xml file.</param>
        /// <param name="pos">Position where train will be spawned, 
        /// it will be automatically snapped to closest train track point.</param>
        /// <param name="dir">Direction of train, can't be changed later.</param>
        /// <returns><see cref="Vehicle"/> instance of spawned train.</returns>
        internal static Vehicle CreateTrain(int variation, Vector3 pos, bool dir)
        {
            return Function.Call<Vehicle>(
                Hash.CREATE_MISSION_TRAIN, variation, pos.X, pos.Y, pos.Z, dir);
        }
    }
}

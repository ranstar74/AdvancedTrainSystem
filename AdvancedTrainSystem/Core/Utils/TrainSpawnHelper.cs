using AdvancedTrainSystem.Core.Info;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AdvancedTrainSystem.Core.Utils
{
    /// <summary>
    /// Methods used in process of spawning train.
    /// </summary>
    internal static class TrainSpawnHelper
    {
        /// <summary>
        /// Spawns <see cref="TrainCarriage"/>'s from locomotive vehicle and <see cref="TrainModelInfo"/> list.
        /// </summary>
        /// <param name="locomotive"><see cref="Vehicle"/> that represents vanila train locomotive</param>
        /// <param name="carriageData">Data to spawn carriage from</param>
        /// <returns>List of spawned <see cref="TrainCarriage"/>'s</returns>
        internal static List<TrainCarriage> SpawnCarriages(Vehicle locomotive, List<TrainModelInfo> carriageData)
        {
            List<TrainCarriage> trainCarriages = new List<TrainCarriage>();

            // Spawn vehicles slightly below player to prevent automatic despawn
            Vector3 spawnPos = Game.Player.Character.Position - Vector3.WorldUp * 10;

            Vehicle previousVehicle = null;
            for (int i = 0; i < carriageData.Count; i++)
            {
                TrainModelInfo data = carriageData[i];

                // Create visible vehicle from model
                CustomModel model = new CustomModel(data.VehicleModel);
                Vehicle vehicle = World.CreateVehicle(model, spawnPos);

                // Spawn visible vehicle of the carriage,
                // in case if its locomotive, it already was spawned by CreateTrainMission function
                Vehicle hiddenVehicle;
                // If model is head of the train or carriage
                if (i == 0)
                {
                    hiddenVehicle = locomotive;
                }
                else
                {
                    // Get train carriage by index
                    hiddenVehicle = locomotive.GetTrainCarriage(i);
                }

                // Set handle of visible vehicle as decorator for hidden vehicle so we can recover it after reload
                hiddenVehicle.Decorator().SetInt(Constants.TrainVisibleCarriageHandle, vehicle.Handle);

                // Attach visible vehicle to invisible one
                hiddenVehicle.IsVisible = false;
                vehicle.AttachTo(hiddenVehicle);

                // TODO: Check if this works before detach at all...
                // Attach this carriage to previous one for better looking derail
                if (previousVehicle != null)
                {
                    Function.Call(Hash.ATTACH_VEHICLE_TO_TRAILER, vehicle, previousVehicle, 180);
                }

                // Create carriage from configured vehicles
                trainCarriages.Add(new TrainCarriage(hiddenVehicle, vehicle));

                previousVehicle = vehicle;
            }

            return trainCarriages;
        }

        /// <summary>
        /// Creates a new <see cref="Train"/> instance from carriages.
        /// </summary>
        /// <param name="carriages">Train carriages including locomotive.</param>
        /// <param name="direction">Direction of the train</param>
        /// <returns>A new <see cref="Train"/> instance</returns>
        internal static T CreateFromCarriages<T>(TrainInfo trainInfo, List<TrainCarriage> carriages, bool direction) where T : Train
        {
            // Separate locomotive from carriages
            TrainCarriage locomotiveCarriage = carriages[0];
            TrainLocomotive locomotive = new TrainLocomotive(locomotiveCarriage);
            carriages.Remove(locomotiveCarriage);

            // Create train from created locomotive and carriages
            var trainSpawnData = new TrainSpawnData(trainInfo, locomotive, carriages, direction);
            T train = (T) Activator.CreateInstance(typeof(T),
                BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { trainSpawnData }, null);

            return train;
        }
    }
}

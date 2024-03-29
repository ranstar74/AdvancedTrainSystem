﻿using AdvancedTrainSystem.Core.Data;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
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
        /// Spawns <see cref="TrainCarriage"/>'s from locomotive vehicle and <see cref="TrainModelData"/> list.
        /// </summary>
        /// <param name="locomotive"><see cref="Vehicle"/> that represents vanila train locomotive</param>
        /// <param name="carriageData">Data to spawn carriage from</param>
        /// <returns>List of spawned <see cref="TrainCarriage"/>'s</returns>
        internal static List<TrainCarriage> SpawnCarriages(Vehicle locomotive, List<TrainModelData> carriageData)
        {
            List<TrainCarriage> carriages = new List<TrainCarriage>();

            // Spawn vehicles slightly below player to prevent automatic despawn
            Vector3 spawnPos = Game.Player.Character.Position - Vector3.WorldUp * 10;

            for (int i = 0; i < carriageData.Count; i++)
            {
                TrainModelData data = carriageData[i];

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
                hiddenVehicle.Decorator().SetInt(TrainConstants.TrainVisibleCarriageHandle, vehicle.Handle);

                // Attach visible vehicle to invisible one
                hiddenVehicle.IsVisible = false;
                vehicle.AttachTo(hiddenVehicle);

                // Create carriage from configured vehicles
                carriages.Add(new TrainCarriage(hiddenVehicle, vehicle));
            }

            return carriages;
        }

        /// <summary>
        /// Creates a new <see cref="Train"/> instance from carriages.
        /// </summary>
        /// <param name="carriages">Train carriages including locomotive.</param>
        /// <param name="direction">Direction of the train</param>
        /// <returns>A new <see cref="Train"/> instance</returns>
        internal static T CreateFromCarriages<T>(TrainData trainInfo, List<TrainCarriage> carriages, bool direction) where T : Train
        {
            // Separate locomotive from carriages
            TrainCarriage locomotiveCarriage = carriages[0];
            TrainLocomotive locomotive = new TrainLocomotive(locomotiveCarriage);

            // Create train from created locomotive and carriages
            var trainSpawnData = new TrainSpawnData(trainInfo, locomotive, carriages, direction);
            T train = (T) Activator.CreateInstance(typeof(T),
                BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { trainSpawnData }, null);

            return train;
        }
    }
}

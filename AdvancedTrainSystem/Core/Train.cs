using AdvancedTrainSystem.Core.Data;
using AdvancedTrainSystem.Core.Utils;
using AdvancedTrainSystem.Extensions;
using AdvancedTrainSystem.Railroad;
using AdvancedTrainSystem.Railroad.Enums;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// This class defines a train.
    /// </summary>
    public abstract class Train : IComponentObject
    {
        /// <summary>
        /// Speed of this train in m/s.
        /// </summary>
        public float Speed => Components.Physx.Speed;

        /// <summary>
        /// Speed along train path. Same for all speeds moving in the same direction.
        /// </summary>
        public float TrackSpeed => Components.Physx.TrackSpeed;

        /// <summary>
        /// Velocity of this train.
        /// </summary>
        /// <remarks>
        /// Can be set only if train is derailed.
        /// </remarks>
        public Vector3 Velocity
        {
            get
            {
                if (Components.Derail.IsDerailed)
                    return _trainLocomotive.Vehicle.Velocity;
                else
                    return _trainLocomotive.HiddenVehicle.Velocity;
            }
            set
            {
                if (Components.Derail.IsDerailed)
                    _trainLocomotive.Vehicle.Velocity = value;
            }
        }

        /// <summary>
        /// Position of this train.
        /// </summary>
        public Vector3 Position => GetActiveLocomotiveVehicle().Position;

        /// <summary>
        /// Rotation of this train.
        /// </summary>
        public Vector3 Rotation => GetActiveLocomotiveVehicle().Rotation;

        /// <summary>
        /// Quaternion of this train.
        /// </summary>
        public Quaternion Quaternion => GetActiveLocomotiveVehicle().Quaternion;

        /// <summary>
        /// Gets the vector that points in front of this train.
        /// </summary>
        public Vector3 ForwardVector => GetActiveLocomotiveVehicle().ForwardVector;

        /// <summary>
        /// Gets the vector that points above the this train.
        /// </summary>
        public Vector3 UpVector => GetActiveLocomotiveVehicle().UpVector;

        /// <summary>
        /// Gets the vector that points to the right of this train.
        /// </summary>
        public Vector3 RightVector => GetActiveLocomotiveVehicle().RightVector;

        /// <summary>
        /// Gets a position directly in front of this train.
        /// </summary>
        public Vector3 FrontPosition => GetActiveLocomotiveVehicle().FrontPosition;

        /// <summary>
        /// Gets a collection of <see cref="EntityBone"/> of this train.
        /// </summary>
        public EntityBoneCollection Bones => TrainLocomotive.Vehicle.Bones;

        /// <summary>
        /// Driver of the <see cref="TrainLocomotive"/>.
        /// </summary>
        public Ped Driver => GetActiveLocomotiveVehicle().Driver;

        /// <summary>
        /// Gets a value indicating whether player drives this train.
        /// </summary>
        public bool IsPlayerDriving => 
            _trainLocomotive.HiddenVehicle.Driver?.Equals(Game.Player.Character) == true ||
            _trainLocomotive.Vehicle.Driver?.Equals(Game.Player.Character) == true;

        /// <summary>
        /// Direction of the <see cref="Train"/> on the rail tracks.
        /// </summary>
        public bool Direction => _trainDirection;

        /// <summary>
        /// Gets value indicating whether train is derailed or not.
        /// </summary>
        public bool IsDerailed => Components.Derail.IsDerailed;

        /// <summary>
        /// <see cref="Decorator"/> of the <see cref="Train"/>.
        /// </summary>
        public Decorator Decorator => _trainLocomotive.HiddenVehicle.Decorator();

        /// <summary>
        /// <see cref="TrainLocomotive"/> of the <see cref="Train"/>.
        /// </summary>
        public TrainLocomotive TrainLocomotive => _trainLocomotive;

        /// <summary>
        /// All carriages of the <see cref="Train"/>.
        /// </summary>
        public List<TrainCarriage> Carriages => _carriages;

        /// <summary>
        /// Gets components of the <see cref="Train"/>.
        /// </summary>
        public TrainComponentCollection Components => (TrainComponentCollection)GetComponents();

        /// <summary>
        /// <see cref="TrainData"/> this train was created from.
        /// </summary>
        public TrainData TrainData => _spawnData.TrainInfo;

        /// <summary>
        /// Map blip of this train.
        /// </summary>
        public Blip Blip => _blip;

        /// <summary>
        /// Handle of this train.
        /// </summary>
        public int Handle => _trainLocomotive.HiddenVehicle.Handle;

        private readonly TrainLocomotive _trainLocomotive;
        private readonly List<TrainCarriage> _carriages;
        private readonly TrainSpawnData _spawnData;
        private readonly bool _trainDirection;

        private readonly Blip _blip;

        internal Train(TrainSpawnData trainSpawnData)
        {
            _trainLocomotive = trainSpawnData.Locomotive;
            _carriages = trainSpawnData.Carriages;
            _trainDirection = trainSpawnData.Direction;
            _spawnData = trainSpawnData;

            SetDecorators();

            // Add blip
            _blip = TrainLocomotive.Vehicle.AddBlip();
            _blip.Sprite = BlipSprite.Train;
            _blip.Color = BlipColor.Yellow4;
            _blip.Name = _spawnData.TrainInfo.Name;
        }

        /// <summary>
        /// Sets various decorators on train vehicles.
        /// </summary>
        internal void SetDecorators()
        {
            ForEachCarriage(vehicle =>
            {
                var decorator = vehicle.Decorator();

                // Direction
                decorator.SetBool(TrainConstants.TrainDirection, Direction);

                // Type
                int trainType = (int)_spawnData.TrainInfo.TrainType;
                decorator.SetInt(TrainConstants.TrainType, trainType);

                // Set number of carriages as decorator so we can recover them after reload
                int carriageCount = _spawnData.TrainInfo.TrainMissionData.Models.Count;
                decorator.SetInt(TrainConstants.CarriagesNumber, carriageCount);

                // Mission id
                decorator.SetInt(TrainConstants.TrainMissionId, _spawnData.TrainInfo.TrainMissionData.Id);

                // Train head
                decorator.SetInt(TrainConstants.TrainHeadHandle, TrainLocomotive.HiddenVehicle.Handle);

                // Handle
                decorator.SetInt(TrainConstants.TrainHandle, Handle);
            });
        }

        /// <summary>
        /// Invokes action for both hidden / visible vehicles of train carriages.
        /// </summary>
        /// <param name="action">Action to invoke.</param>
        internal void ForEachCarriage(Action<Vehicle> action)
        {
            foreach(TrainCarriage carriage in Carriages)
            {
                action.Invoke(carriage.HiddenVehicle);
                action.Invoke(carriage.Vehicle);
            }
        }

        internal static T Create<T>(TrainData trainInfo, Vector3 position, bool direction) where T : Train
        {
            TrainMissionData trainMission = trainInfo.TrainMissionData;

            // Load all models before spawning train
            trainInfo.TrainMissionData.Models.ForEach(modelInfo =>
            {
                CustomModel vehModel = new CustomModel(modelInfo.VehicleModel);
                CustomModel hidModel = new CustomModel(modelInfo.HiddenVehicleModel);

                vehModel.Request();
                hidModel.Request();
            });

            // Spawn vanila train
            Vehicle hiddenLocomotive = FusionUtils.CreateMissionTrain(trainMission.Id, position, direction);

            // Spawn and configure all train carriages
            List<TrainCarriage> carriages = TrainSpawnHelper.SpawnCarriages(hiddenLocomotive, trainMission.Models);

            return TrainSpawnHelper.CreateFromCarriages<T>(trainInfo, carriages, direction);
        }

        /// <summary>
        /// Respawns <see cref="Train"/> from any train vehicle.
        /// <para>
        /// Used for restoring train after reloading script.
        /// </para>
        /// </summary>
        /// <returns>A new instance of <see cref="Train"/></returns>
        internal static Train Respawn(List<TrainCarriage> carriages, bool direction)
        {
            Vehicle locomotive = carriages[0].HiddenVehicle;
            TrainData trainInfo = TrainData.Load(locomotive.GetAdvancedTrainMissionId());

            // Copy-pasted from train factory. Needs to be updated when new train is implemented.
            Train train;
            switch (trainInfo.TrainType)
            {
                case TrainType.Steam:
                    train = TrainSpawnHelper.CreateFromCarriages<SteamTrain>(trainInfo, carriages, direction);
                    break;
                case TrainType.Diesel:
                    throw new NotImplementedException();
                case TrainType.Electric:
                    throw new NotImplementedException();
                case TrainType.Handcar:
                    throw new NotImplementedException();
                case TrainType.Minecart:
                    throw new NotImplementedException();
                default:
                    throw new NotSupportedException();
            }
            TrainPool.Trains.Add(train);
            return train;
        }

        /// <summary>
        /// Gets <see cref="TrainComponentCollection"/>.
        /// </summary>
        /// <returns>A <see cref="TrainComponentCollection"/> instance of the <see cref="Train"/></returns>
        public abstract ComponentCollection GetComponents();

        /// <summary>
        /// Gets train carriage.
        /// </summary>
        /// <param name="index">Index of the carriage.</param>
        /// <returns>Carriage of specified index.</returns>
        public TrainCarriage GetCarriageAt(int index)
        {
            return Carriages[index];
        }

        /// <summary>
        /// Gets train carriages of specified model.
        /// </summary>
        /// <param name="model">Model of hidden or visible vehicle of carriage.</param>
        /// <returns>Array of carriages of specified model.</returns>
        public TrainCarriage[] GetCarriages(CustomModel model)
        {
            return GetCarriages(model.Model);
        }

        /// <summary>
        /// Gets train carriages of specified model.
        /// </summary>
        /// <param name="model">Model of hidden or visible vehicle of carriage.</param>
        /// <returns>Array of carriages of specified model.</returns>
        public TrainCarriage[] GetCarriages(Model model)
        {
            var searchModel = model;

            List<TrainCarriage> carriages = new List<TrainCarriage>();
            for (int i = 0; i < Carriages.Count; i++)
            {
                var carriage = Carriages[i];

                var invisibleModel = carriage.HiddenVehicle.Model;
                var visibleModel = carriage.Vehicle.Model;

                if (searchModel == invisibleModel || searchModel == visibleModel)
                {
                    carriages.Add(carriage);
                }
            }
            return carriages.ToArray();
        }

        /// <summary>
        /// Some properties require getting locomotive vehicle,
        /// we can just use hidden vehicle but it works until derail,
        /// cuz after derail hidden vehicle stays on tracks but
        /// visible vehicle gets off track.
        /// </summary>
        /// <returns></returns>
        internal Vehicle GetActiveLocomotiveVehicle()
        {
            // In case if its called before components got initialize
            if (Components?.Derail == null)
                return TrainLocomotive.HiddenVehicle;

            return Components.Derail.IsDerailed ? 
                TrainLocomotive.Vehicle : TrainLocomotive.HiddenVehicle;
        }

        /// <summary>
        /// Marks train as disposed but doesn't remove vehicles.
        /// <para>
        /// Used for respawn.
        /// </para>
        /// </summary>
        public void MarkAsNonScripted()
        {
            Components?.OnDispose();
            _blip?.Delete();
        }

        /// <summary>
        /// Disposes the <see cref="Train"/>.
        /// </summary>
        public void Dispose()
        {
            MarkAsNonScripted();

            for(int i = 0; i < Carriages.Count; i++)
            {
                Carriages[i].Dispose();
            }
        }

        /// <summary>
        /// Implicitely casts the <see cref="Train"/> to a <see cref="Entity"/>.
        /// </summary>
        /// <param name="train">Source object.</param>
        public static implicit operator Entity(Train train) => train._trainLocomotive.Vehicle;

        /// <summary>
        /// Implicitely casts the <see cref="Train"/> to a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="train">Source object.</param>
        public static implicit operator Vehicle(Train train) => train._trainLocomotive.Vehicle;
    }
}

using AdvancedTrainSystem.Core.Info;
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
                    return trainLocomotive.Vehicle.Velocity;
                else
                    return trainLocomotive.HiddenVehicle.Velocity;
            }
            set
            {
                if (Components.Derail.IsDerailed)
                    trainLocomotive.Vehicle.Velocity = value;
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
        /// Direction of the <see cref="Train"/> on the rail tracks.
        /// </summary>
        public bool Direction => trainDirection;

        /// <summary>
        /// Gets value indicating whether train is derailed or not.
        /// </summary>
        public bool IsDerailed => Components.Derail.IsDerailed;

        /// <summary>
        /// <see cref="Decorator"/> of the <see cref="Train"/>.
        /// </summary>
        public Decorator Decorator => trainLocomotive.HiddenVehicle.Decorator();

        /// <summary>
        /// <see cref="TrainLocomotive"/> of the <see cref="Train"/>.
        /// </summary>
        public TrainLocomotive TrainLocomotive => trainLocomotive;

        /// <summary>
        /// All carriages of the <see cref="Train"/>.
        /// </summary>
        public List<TrainCarriage> Carriages => trainCarriages;

        /// <summary>
        /// Handle of the <see cref="Train"/>.
        /// </summary>
        public int ComponentHandle => componentHandle;

        /// <summary>
        /// Gets components of the <see cref="Train"/>.
        /// </summary>
        public TrainComponentCollection Components => (TrainComponentCollection)GetComponents();

        /// <summary>
        /// <see cref="TrainInfo"/> this train was created from.
        /// </summary>
        public TrainInfo TrainInfo => spawnData.TrainInfo;

        private readonly TrainLocomotive trainLocomotive;
        private readonly List<TrainCarriage> trainCarriages;
        private readonly bool trainDirection;
        private readonly TrainSpawnData spawnData;
        private int componentHandle;

        internal Train(TrainSpawnData trainSpawnData)
        {
            trainLocomotive = trainSpawnData.Locomotive;
            trainCarriages = trainSpawnData.Carriages;
            trainDirection = trainSpawnData.Direction;

            spawnData = trainSpawnData;

            foreach(TrainCarriage carriage in Carriages)
            {
                carriage.SetTrain(this);

                SetDecorators(carriage.HiddenVehicle);
                SetDecorators(carriage.Vehicle);
            }

            // Add blip
            Blip blip = TrainLocomotive.Vehicle.AddBlip();
            blip.Sprite = BlipSprite.Train;
            blip.Color = BlipColor.Yellow4;
            blip.Name = spawnData.TrainInfo.Name;
        }

        /// <summary>
        /// Sets various decorators on train vehicle.
        /// </summary>
        internal void SetDecorators(Vehicle vehicle)
        {
            var decorator = vehicle.Decorator();

            // Direction
            decorator.SetBool(Constants.TrainDirection, Direction);

            // Type
            int trainType = (int)spawnData.TrainInfo.TrainType;
            decorator.SetInt(Constants.TrainType, trainType);

            // Set number of carriages as decorator so we can recover them after reload
            int carriageCount = spawnData.TrainInfo.TrainMissionInfo.Models.Count;
            decorator.SetInt(Constants.TrainCarriagesNumber, carriageCount);

            // Mission id
            decorator.SetInt(Constants.TrainMissionId, spawnData.TrainInfo.TrainMissionInfo.Id);

            // Train head
            decorator.SetInt(Constants.TrainHeadHandle, TrainLocomotive.HiddenVehicle.Handle);
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

        /// <summary>
        /// Creates a new <see cref="Train"/> instance.
        /// </summary>
        /// <returns></returns>
        internal static T Create<T>(TrainInfo trainInfo, Vector3 position, bool direction) where T : Train
        {
            TrainMissionInfo trainMission = trainInfo.TrainMissionInfo;

            // Load all models before spawning train
            trainInfo.TrainMissionInfo.Models.ForEach(modelInfo =>
            {
                CustomModel vehModel = new CustomModel(modelInfo.VehicleModel);
                CustomModel hidModel = new CustomModel(modelInfo.HiddenVehicleModel);

                vehModel.Request();
                hidModel.Request();
            });

            // Spawn vanila train
            Vehicle hiddenLocomotive = FusionUtils.CreateMissionTrain(trainMission.Id, position, direction);

            // Spawn and configure all train carriages
            List<TrainCarriage> trainCarriages = TrainSpawnHelper.SpawnCarriages(hiddenLocomotive, trainMission.Models);

            return TrainSpawnHelper.CreateFromCarriages<T>(trainInfo, trainCarriages, direction);
        }

        /// <summary>
        /// Respawns <see cref="Train"/> from any train vehicle.
        /// <para>
        /// Used for restoring train after reloading script.
        /// </para>
        /// </summary>
        /// <param name="vehicle">Any vehicle of the <see cref="Train"/></param>
        /// <returns>A new instance of <see cref="Train"/></returns>
        internal static Train Respawn(Vehicle vehicle)
        {
            int carriagesCount = vehicle.GetAtsCarriagesCount();
            bool direction = vehicle.GetAtsDirection();

            List<TrainCarriage> carriages = new List<TrainCarriage>();
            for (int i = 0; i < carriagesCount; i++)
            {
                Vehicle invisibleVehicle;

                // If model is locomotive / carriage
                if (i == 0)
                    invisibleVehicle = vehicle;
                else
                    invisibleVehicle = vehicle.GetTrainCarriage(i);

                // Get visible vehicle from handle
                Vehicle visibleVehicle = invisibleVehicle.GetAtsCarriageVehicle();

                // Create carriage from recovered vehicles
                carriages.Add(new TrainCarriage(invisibleVehicle, visibleVehicle));
            }

            TrainInfo trainInfo = TrainInfo.Load(vehicle.GetAdvancedTrainMissionId());

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
            ATSPool.Trains.Add(train);

            return train;
        }

        /// <summary>
        /// Gets <see cref="TrainComponentCollection"/>.
        /// </summary>
        /// <returns>A <see cref="TrainComponentCollection"/> instance of the <see cref="Train"/></returns>
        public abstract ComponentCollection GetComponents();

        /// <summary>
        /// Assigns handle to the <see cref="Train"/>.
        /// </summary>
        /// <param name="componentHandle">Handle to assign.</param>
        public void SetComponentHandle(int componentHandle)
        {
            this.componentHandle = componentHandle;

            ForEachCarriage(x =>
            {
                x.Decorator().SetInt(Constants.TrainHandle, componentHandle);
            });
        }

        /// <summary>
        /// Invalidates component handle by setting it to -1.
        /// </summary>
        public void InvalidateHandle()
        {
            ForEachCarriage(x =>
            {
                x.Decorator().SetInt(Constants.TrainHandle, -1);
            });
        }

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
        /// Disposes the <see cref="Train"/>.
        /// </summary>
        public void Dispose()
        {
            // Could be null if disposed before InitializeComponent
            Components?.OnDispose();

            for(int i = 0; i < Carriages.Count; i++)
            {
                Carriages[i].Dispose();
            }
        }

        /// <summary>
        /// Implicitely casts the <see cref="Train"/> to a <see cref="Entity"/>.
        /// </summary>
        /// <param name="train">Source object.</param>
        public static implicit operator Entity(Train train) => train.trainLocomotive.Vehicle;

        /// <summary>
        /// Implicitely casts the <see cref="Train"/> to a <see cref="Vehicle"/>.
        /// </summary>
        /// <param name="train">Source object.</param>
        public static implicit operator Vehicle(Train train) => train.trainLocomotive.Vehicle;
    }
}

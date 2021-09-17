using AdvancedTrainSystem.Data;
using AdvancedTrainSystem.Natives;
using AdvancedTrainSystem.Train.Components;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using RageComponent;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Train
{
    /// <summary>
    /// Custom train in which every carriage consists of invisible and visible models.
    /// </summary>
    public class CustomTrain
    {
        /// <summary>
        /// All spawned <see cref="CustomTrain"/> instances.
        /// </summary>
        internal static readonly List<CustomTrain> AllTrains = new List<CustomTrain>();

        /// <summary>
        /// Coupled trains.
        /// </summary>
        private readonly List<CustomTrain> _coupledTrains = new List<CustomTrain>();

        /// <summary>
        /// Head (in most cases - locomotive) of the train.
        /// </summary>
        public readonly Vehicle TrainHead;

        /// <summary>
        /// Visible model of train head (in most cases - locomotive).
        /// </summary>
        public readonly Vehicle TrainHeadVisible;

        /// <summary>
        /// All carriages of this train.
        /// </summary>
        public readonly List<Carriage> Carriages;
        
        private float _speed;
        /// <summary>
        /// Speed of this train in m/s.
        /// </summary>
        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                TrainHead.SetTrainSpeed(_speed);
            }
        }

        /// <summary>
        /// Unique number of this train.
        /// </summary>
        public readonly int Guid;

        /// <summary>
        /// Blip of this train.
        /// </summary>
        public readonly Blip Blip;

        /// <summary>
        /// Counter of spawned trains as Guid.
        /// </summary>
        private static int GuidCounter = Game.GameTime;

        /// <summary>
        /// All train components.
        /// </summary>
        public ComponentsHandler<CustomTrain> TrainComponents;

        [Entity(EntityProperty = nameof(TrainHead))]
        public SpeedComponent SpeedComponent;

        [Entity(EntityProperty = nameof(TrainHead))]
        public CollisionComponent CollisionComponent;

        [Entity(EntityProperty = nameof(TrainHeadVisible))]
        public DynamoComponent DynamoComponent;

        public BrakeComponent BrakeComponent;
        public BoilerComponent BoilerComponent;
        public ControlComponent ControlComponent;

        /// <summary>
        /// Constructs new instance of <see cref="CustomTrain"/>.
        /// </summary>
        /// <param name="config">Config of the train needs to be spawned.</param>
        CustomTrain(TrainConfig config, List<Carriage> carriages, Vehicle head)
        {
            Carriages = carriages;
            TrainHead = head;

            TrainHeadVisible = GetCarriage(head.Model).VisibleVehicle;

            // Set "Guid"
            Guid = GuidCounter++;
            for (int i = 0; i < Carriages.Count; i++)
            {
                var carriage = carriages[i];
                carriage.InvisibleVehicle.Decorator().SetInt(Constants.TrainGuid, Guid);
                carriage.VisibleVehicle.Decorator().SetInt(Constants.TrainGuid, Guid);

                // Set handle of train head
                carriage.VisibleVehicle.Decorator().SetInt(Constants.TrainHeadHandle, TrainHead.Handle);
            }

            // For new trains
            if (config != null)
            {
                // Add blip to train
                if (config.BlipName != "")
                {
                    Blip = TrainHead.AddBlip();
                    Blip.Sprite = (BlipSprite)795;
                    Blip.Color = (BlipColor)70;
                    Blip.Name = config.BlipName;
                }
            }

            TrainComponents = ComponentsHandler<CustomTrain>.RegisterComponentHandler();
            TrainComponents.RegisterComponents(this);

            AllTrains.Add(this);
        }

        /// <summary>
        /// Finds train by head handle.
        /// </summary>
        /// <param name="handle">Handle of the train head.</param>
        /// <returns><see cref="CustomTrain"/> of given head handle. Null if not found.</returns>
        public static CustomTrain Find(int handle)
        {
            for(int i = 0; i < AllTrains.Count; i++)
            {
                var train = AllTrains[i];

                if (train.TrainHead.Handle == handle)
                    return train;
            }
            return null;
        }

        /// <summary>
        /// Creates new train instance.
        /// </summary>
        /// <returns>New instance of <see cref="CustomTrain"/></returns>
        public static CustomTrain Create(TrainConfig config, Vector3 position, bool direction)
        {
            // Spawn new train. It returns first carriage.
            var trainHead = NVehicle.CreateTrain(config.Id, position, direction);

            // Mark train as custom
            trainHead.Decorator().SetBool(Constants.TrainIsCustom, true);

            // Set number of carriages as decorator so we can recover them after reload
            trainHead.Decorator().SetInt(Constants.TrainCarriagesNumber, config.Models.Count);

            Carriage nextCarriage = null;
            var carriages = new List<Carriage>();
            // Spawn all carriages from config models
            for (int i = 0; i < config.Models.Count; i++)
            {
                TrainModel trainModel = config.Models[i];

                Vehicle invisibleVehicle;
                // If model is head of the train or carriage
                if(i == 0)
                {
                    invisibleVehicle = trainHead;
                }
                else
                {
                    // Get train carriage by index
                    invisibleVehicle = Function.Call<Vehicle>(Hash.GET_TRAIN_CARRIAGE, trainHead, i);
                }

                // Spawn visible model for invisible carriage
                var visibleVehicle = World.CreateVehicle(trainModel.VisibleModel, position);

                // Set handle of visible model as decorator for invisible model so we can recover it after reload
                invisibleVehicle.Decorator().SetInt(Constants.TrainVisibleCarriageHandle, visibleVehicle.Handle);

                // Attach visible model to invisible
                invisibleVehicle.IsVisible = false;
                visibleVehicle.AttachTo(invisibleVehicle);

                // Create carriage from spawned vehicles
                var newCarriage = new Carriage(invisibleVehicle, visibleVehicle);
                newCarriage.Next = nextCarriage;

                carriages.Add(newCarriage);

                // Fill linked list
                if (nextCarriage != null)
                {
                    nextCarriage.Previous = newCarriage;

                    Function.Call(Hash.ATTACH_VEHICLE_TO_TRAILER, newCarriage.VisibleVehicle, nextCarriage.VisibleVehicle, 180);
                }

                nextCarriage = newCarriage;
            }

            return new CustomTrain(config, carriages, trainHead);
        }

        /// <summary>
        /// Respawns train from head (locomotive) vehicle.
        /// </summary>
        /// <param name="trainHead"><paramref name="trainHead"/></param>
        /// <returns>New instance of <see cref="CustomTrain"/></returns>
        public static CustomTrain Respawn(Vehicle trainHead)
        {
            var carriagesNumber = trainHead.Decorator().GetInt(Constants.TrainCarriagesNumber);

            var carriages = new List<Carriage>();
            for (int i = 0; i < carriagesNumber; i++)
            {
                Vehicle invisibleVehicle;

                // If model is locomotive / carriage
                if (i == 0)
                    invisibleVehicle = trainHead;
                else
                    invisibleVehicle = Function.Call<Vehicle>(Hash.GET_TRAIN_CARRIAGE, trainHead, i);

                // Get visible vehicle from handle
                Vehicle visibleVehicle = (Vehicle)Entity.FromHandle(
                    invisibleVehicle.Decorator().GetInt(Constants.TrainVisibleCarriageHandle));

                // Create carriage from recovered vehicles
                carriages.Add(new Carriage(invisibleVehicle, visibleVehicle));
            }

            return new CustomTrain(null, carriages, trainHead);
        }

        /// <summary>
        /// Checks if vehicle is <see cref="CustomTrain"/>.
        /// </summary>
        /// <param name="vehicle">Vehicle to check.</param>
        /// <returns>True if train is <see cref="CustomTrain"/>, otherwise False.</returns>
        public static bool IsCustomTrain(Vehicle vehicle)
        {
            return vehicle.Decorator().GetBool(Constants.TrainIsCustom);
        }

        internal static void OnTick()
        {
            for (int i = 0; i < AllTrains.Count; i++)
                AllTrains[i].Tick();
        }

        private void Tick()
        {
            //var thisTrainSpeedNegative = Speed < 0;
            //for(int i = 0; i < _coupledTrains.Count; i++)
            //{
            //    var coupledTrain = _coupledTrains[i];
            //    var coupledTrainSpeedNegative = coupledTrain.Speed < 0;

            //    if(thisTrainSpeedNegative != coupledTrainSpeedNegative)
            //    {
            //        Decouple(coupledTrain);
            //        coupledTrain.Decouple(this);
            //    }
            //}
        }

        /// <summary>
        /// Gets train carriage.
        /// </summary>
        /// <param name="index">Index of the carriage. Starts from 0.</param>
        /// <returns>Carriage of specified index.</returns>
        public Carriage GetCarriage(int index)
        {
            return Carriages[index];
        }

        /// <summary>
        /// Gets train carriage.
        /// </summary>
        /// <param name="model">Model of invisible or visible vehicle of carriage.</param>
        /// <returns>Carriage of specified model.</returns>
        public Carriage GetCarriage(CustomModel model)
        {
            return GetCarriage(model.Model);
        }

        /// <summary>
        /// Gets train carriage.
        /// </summary>
        /// <param name="model">Model of invisible or visible vehicle of carriage.</param>
        /// <returns>Carriage of specified model.</returns>
        public Carriage GetCarriage(Model model)
        {
            var searchModel = model;

            for (int i = 0; i < Carriages.Count; i++)
            {
                var carriage = Carriages[i];

                var invisibleModel = carriage.InvisibleVehicle.Model;
                var visibleModel = carriage.VisibleVehicle.Model;

                if (searchModel == invisibleModel || searchModel == visibleModel)
                {
                    return carriage;
                }
            }
            throw new ArgumentException($"Requested carriage {model.Hash} is not found.");
        }

        /// <summary>
        /// Couples this <see cref="CustomTrain"/> with other <see cref="CustomTrain"/>.
        /// This isn't locked couple, it works like "pushing".
        /// So it wont work if train goes in opposite direction.
        /// You must call <see cref="LockCouple(CustomTrain)"/> in order to lock couple.
        /// </summary>
        /// <param name="train"></param>
        public void Couple(CustomTrain train)
        {
            if(!_coupledTrains.Contains(train))
                _coupledTrains.Add(train);
        }

        public void Decouple(CustomTrain train)
        {
            _coupledTrains.Remove(train);
        }

        public void LockCouple(CustomTrain train)
        {

        }

        /// <summary>
        /// Disposes train.
        /// </summary>
        public void Dispose()
        {
            for(int i = 0; i < Carriages.Count; i++)
            {
                var carriage = Carriages[i];

                carriage.VisibleVehicle.Delete();
                carriage.InvisibleVehicle.Delete();
            }
        }
    }
}

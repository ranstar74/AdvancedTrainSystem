using AdvancedTrainSystem.Core.Data;
using AdvancedTrainSystem.Railroad;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.UI;
using RageMission.Core;
using RageMission.Objectives;

namespace AdvancedTrainSystem.Missions
{
    internal class TutorialMission : Mission
    {
        private SteamTrain _targetTrain;
        private static Vector3 _trainStationPos = new Vector3(2611, 1679, 27);
        private static Vector3 _woodFactoryPos = new Vector3(-474, 5266, 89);

        // Mission Objectives
        private Objective _goToTrainStation;
        private Objective _getInTrain;
        private Objective _switchToFirstPerson;
        private Objective _drainCocks;
        private Objective _releaseBrake;
        private Objective _setGearAndThrottle;
        private Objective _reach10Mph;
        private Objective _addFuel;
        private Objective _stop;

        int _timer = -1;
        int _drainCockStage = 0;
        int _gearThrottleStage = 0;
        int _addFuelStage = 0;

        public TutorialMission()
        {
            OnObjectiveStarted += ObjectiveStarted;
        }

        public override void Start()
        {
            _targetTrain = (SteamTrain)TrainFactory.CreateTrain(TrainData.Load("RogersSierra3"), _trainStationPos, false);

            // Testing
            _targetTrain.Components.Driving.Enter();

            _goToTrainStation = new WaypointObjective(_trainStationPos, "ATS_STORY_BLIP_TRAIN_STATION");
            _getInTrain = new PredicateObjective<SteamTrain>(
                target: _targetTrain,
                success: t => t.Driver == Game.Player.Character);
            _switchToFirstPerson = new FuncObjective(FusionUtils.IsCameraInFirstPerson);
            _drainCocks = new PredicateObjective<SteamTrain>(
                target: _targetTrain,
                success: t => t.Components.Boiler.WaterInCylinders < 0.1f);
            _releaseBrake = new PredicateObjective<SteamTrain>(
                target: _targetTrain,
                success: t => t.Components.SteamControls.AirBrake < 0.1f);
            _setGearAndThrottle = new PredicateObjective<SteamTrain>(
                target: _targetTrain,
                success: t => t.Components.SteamControls.Gear > 0.5f && t.Components.SteamControls.Throttle > 0.5f);
            _reach10Mph = new PredicateObjective<SteamTrain>(
                target: _targetTrain,
                success: t => t.Components.Physx.Speed >= 4f);
            _addFuel = new PredicateObjective<SteamTrain>(
                target: _targetTrain,
                success: t => t.Components.Boiler.FuelCapacityLeft() < 15);
            _stop = new WaypointObjective(_woodFactoryPos, "ATS_STORY_BLIP_FACTORY", false)
            {
                SuccessCondition = () => _targetTrain.Components.Physx.AbsoluteSpeed < 0.15f
            };

            _goToTrainStation.StartMessageKey = "ATS_STORY_OBJECTIVE_GO_TO_TRAIN_STATION";
            _getInTrain.StartMessageKey = "ATS_STORY_OBJECTIVE_ENTER_TRAIN";
            _stop.StartMessageKey = "ATS_STORY_OBJECTIVE_STOP";

            AddObjective(_goToTrainStation);
            AddObjective(_getInTrain);
            AddObjective(_switchToFirstPerson);
            AddObjective(_drainCocks);
            AddObjective(_releaseBrake);
            AddObjective(_setGearAndThrottle);
            AddObjective(_reach10Mph);
            AddObjective(_addFuel);
            AddObjective(_stop);

            base.Start();
        }

        private void ObjectiveStarted(Objective obj)
        {
            if(obj == _switchToFirstPerson)
            {
                Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_SWITCH_CAM"), 5000);
                _timer = Game.GameTime + 5000;
            }

            if(obj == _releaseBrake)
            {
                Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_BRAKE"), 5000);
                _timer = Game.GameTime + 5000;
            }
        }

        public override void Update()
        {
            base.Update();

            if (Game.GameTime < _timer)
            {
                return;
            }

            if (CurrentObjective == _drainCocks)
            {
                switch (_drainCockStage)
                {
                    // Introduction to hydrolock
                    case 0:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_COCK_C0"), 15000);
                            _timer = Game.GameTime + 15000;
                            _drainCockStage++;
                            return;
                        }
                    case 1:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_COCK_C1"));
                            _timer = Game.GameTime + 7500;
                            _drainCockStage++;
                            return;
                        }
                    case 2:
                        {
                            return;
                        }
                }
            }

            if(CurrentObjective == _setGearAndThrottle)
            {
                switch(_gearThrottleStage)
                {
                    case 0:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_GT_C0"), 20000);
                            _timer = Game.GameTime + 20000;
                            _gearThrottleStage++;
                            return;
                        }
                    case 1:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_GT_C1"), 10000);
                            _timer = Game.GameTime + 10000;
                            _gearThrottleStage++;
                            return;
                        }
                    case 2:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_GT_C2"), 7000);
                            _timer = Game.GameTime + 7000;
                            _gearThrottleStage++;
                            return;
                        }
                    case 3:
                        {
                            return;
                        }
                }
            }

            if(CurrentObjective == _addFuel)
            {
                switch(_addFuelStage)
                {
                    case 0:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_FUEL_C0"), 10000);
                            _timer = Game.GameTime + 10000;
                            _addFuelStage++;
                            return;
                        }
                    case 1:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_FUEL_C1"), 10000);
                            _timer = Game.GameTime + 10000;
                            _addFuelStage++;
                            return;
                        }
                    case 2:
                        {
                            Screen.ShowHelpText(Game.GetLocalizedString("ATS_STORY_OBJECTIVE_FUEL_C2"), 10000);
                            _timer = Game.GameTime + 10000;
                            _addFuelStage++;
                            return;
                        }
                }
            }
        }

        public override void Abort()
        {
            base.Abort();

            TrainPool.Trains.Remove(_targetTrain.Handle);

            _targetTrain.Dispose();
        }

        protected override void OnFinish(bool success)
        {
            base.OnFinish(success);

            if (success)
            {
                AtsStoryMgr.Instance.SetFlagState(AtsStoryFlags.AtsTutorialFinished, true);
            }
        }
    }
}

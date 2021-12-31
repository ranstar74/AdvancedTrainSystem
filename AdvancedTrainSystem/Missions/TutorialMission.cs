using AdvancedTrainSystem.Core.Info;
using AdvancedTrainSystem.Railroad;
using GTA;
using GTA.Math;
using RageMission.Core;
using RageMission.Objectives;

namespace AdvancedTrainSystem.Missions
{
    internal class TutorialMission : Mission
    {
        private SteamTrain _targetTrain;
        private static Vector3 _trainStationPos = new Vector3(2611, 1679, 27);

        // Mission Objectives
        private Objective _goToTrainStation;
        private Objective _getInTrain;

        public override void Start()
        {
            _targetTrain = (SteamTrain) TrainFactory.CreateTrain(TrainInfo.Load("RogersSierra3"), _trainStationPos, false);

            _goToTrainStation = new WaypointObjective(_trainStationPos, "ATS_TRAIN_STATION");
            _getInTrain = new PredicateObjective<SteamTrain>(
                target: _targetTrain,
                success: t => t.Driver == Game.Player.Character);

            _goToTrainStation.StartMessageKey = "VSTORY_GO_TO_TRAIN_STATION";
            _getInTrain.StartMessageKey = "VSTORY_ENTER_TRAIN";

            AddObjective(_goToTrainStation);
            AddObjective(_getInTrain);

            base.Start();
        }

        public override void Abort()
        {
            base.Abort();

            ATSPool.Trains.Remove(_targetTrain.Handle);

            _targetTrain.Dispose();
        }
    }
}

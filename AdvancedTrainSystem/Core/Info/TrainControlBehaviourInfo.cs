using GTA;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Core.Info
{
    public struct TrainControlBehaviourInfo
    {
        public string ActionName;
        public string ModelName;
        public string BoneName;
        public AnimationType MovementType;
        public Coordinate Coordinate;
        public Control Control;
        public Control? AltControl;
        public bool InvertValue;
        public bool Invert;
        public bool? InvertAlt;
        public float MinAngle;
        public float MaxAngle;
        public float StartValue;
        public float Sensetivity;
    }
}

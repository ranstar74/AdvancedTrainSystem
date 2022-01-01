using GTA;
using GTA.Math;
using RageMission.Core;
using System;
using System.Collections.Generic;

namespace AdvancedTrainSystem.Missions
{
    internal class AtsStoryMgr : StorylineMgr
    {
        public static AtsStoryMgr Instance { get; } = new AtsStoryMgr();

        protected override string SaveFileName => "ats.sav";

        protected override List<StoryMission> Missions { get; } = new List<StoryMission>()
        {
            new StoryMission()
            {
                MissionName = "ATS_TUTORIAL",
                Position = new Vector3(2465f, 1579f, 31.7f),
                BlipSprite = BlipSprite.StrangersAndFreaks,
                RequiredFlag = string.Empty,
                FinishedFlag = AtsStoryFlags.AtsTutorialFinished
            }
        };

        protected override Dictionary<string, Type> MissionMap { get; } = new Dictionary<string, Type>()
        {
            ["ATS_TUTORIAL"] = typeof(TutorialMission)
        };

        protected override Dictionary<string, bool> MissionFlags { get; } = new Dictionary<string, bool>()
        {
            [AtsStoryFlags.AtsTutorialFinished] = false
        };
    }

    internal class AtsStoryFlags
    {
        public static readonly string AtsTutorialFinished = "ATS_TUTORIAL_FINISHED";
    }
}

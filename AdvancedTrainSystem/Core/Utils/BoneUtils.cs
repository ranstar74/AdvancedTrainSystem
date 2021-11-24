using System;

namespace AdvancedTrainSystem.Core.Utils
{
    internal static class BoneUtils
    {
        /// <summary>
        /// Useful for cases when you have few bones like bone_left1, bone_right1. 
        /// This functinon allows to process them by just base bone name and bone count.
        /// </summary>
        /// <param name="baseBone">Shared part between all bones, for i.e: "veh_bone_".</param>
        /// <param name="totalBoneNumber">Total bone number (left + right), should be symmetrical.</param>
        /// <param name="action">Action to process.</param>
        public static void ProcessSideBones(string baseBone, int totalBoneNumber, Action<string> action)
        {
            for (int l = 0, r = 0; l + r < totalBoneNumber;)
            {
                // Generate bone name
                var bone = baseBone;

                if (l < 3)
                    bone += $"left_{l++ + 1}";
                else
                    bone += $"right_{r++ + 1}";

                action(bone);
            }
        }

        /// <summary>
        /// Useful for cases when you have few bones like bone_1, bone_2. 
        /// This functinon allows to process them by just base bone name and bone count.
        /// </summary>
        /// <param name="baseBone">Shared part between all bones, for i.e: "veh_bone_".</param>
        /// <param name="totalBoneNumber">Total bone number.</param>
        /// <param name="action">Action to process.</param>
        public static void ProcessMultipleBones(string baseBone, int totalBoneNumber, Action<string> action)
        {
            for (int i = 0; i < totalBoneNumber; i++)
            {
                // Generate bone name
                var bone = baseBone + (i + 1);

                action(bone);
            }
        }
    }
}

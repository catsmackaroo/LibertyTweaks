using IVSDKDotNet;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class ImprovedAIAccuracyAndFirerates
    {
        private static bool enableAccuracyFirerate;
        private static readonly HashSet<uint> policeHashes = new HashSet<uint>
        {
            4111764146, // Police
            2776029317,
            4205665177
        };
        private static readonly List<uint> gangTypes = new List<uint>
        {
            3, 4, 11, 13, 9, 10, 12, 14, 6, 8, 7, 5 // Gangs
        };

        private static int defaultPedAccuracy;
        private static int defaultPedFirerate;
        private static int followerAccuracy;
        private static int followerFirerate;
        private static int gangAccuracy;
        private static int gangFirerate;
        private static int fibAccuracy;
        private static int fibFirerate;
        private static int nooseAccuracy;
        private static int nooseFirerate;
        private static int policeAccuracy;
        private static int policeFirerate;
        private static int policeSixStarAccuracy;
        private static int policeSixStarFirerate;

        public static string section { get; private set; }

        public static void Init(SettingsFile settings, string section)
        {
            ImprovedAIAccuracyAndFirerates.section = section;
            string section2 = "Extensive Settings";

            enableAccuracyFirerate = settings.GetBoolean(section, "Increased Accuracy and Firerates", false);

            defaultPedAccuracy = settings.GetInteger(section2, "Default Accuracy", 40);
            defaultPedFirerate = settings.GetInteger(section2, "Default Firerate", 25);
            followerAccuracy = settings.GetInteger(section2, "Follower Accuracy", 100);
            followerFirerate = settings.GetInteger(section2, "Follower Firerate", 100);
            gangAccuracy = settings.GetInteger(section2, "Gang Accuracy", 20);
            gangFirerate = settings.GetInteger(section2, "Gang Firerate", 40);
            fibAccuracy = settings.GetInteger(section2, "FIB Accuracy", 80);
            fibFirerate = settings.GetInteger(section2, "FIB Firerate", 50);
            nooseAccuracy = settings.GetInteger(section2, "NOoSE Accuracy", 60);
            nooseFirerate = settings.GetInteger(section2, "NOoSE Firerate", 40);
            policeAccuracy = settings.GetInteger(section2, "Police Accuracy", 50);
            policeFirerate = settings.GetInteger(section2, "Police Firerate", 40);
            policeSixStarAccuracy = settings.GetInteger(section2, "Police Six Star Accuracy", 55);
            policeSixStarFirerate = settings.GetInteger(section2, "Police Six Star Firerate", 45);

            if (enableAccuracyFirerate)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enableAccuracyFirerate)
                return;

            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;
                GET_CHAR_MODEL(pedHandle, out uint pedModel);
                GET_PED_TYPE(pedHandle, out uint pedType);

                int accuracy = defaultPedAccuracy;
                int firerate = defaultPedFirerate;

                if (pedModel == 3295460374) // FIB
                {
                    accuracy = fibAccuracy;
                    firerate = fibFirerate;
                }
                else if (pedModel == 3290204350) // NOoSE
                {
                    accuracy = nooseAccuracy;
                    firerate = nooseFirerate;
                }

                STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);

                if (policeHashes.Contains(pedModel))
                {
                    if (currentWantedLevel == 6)
                    {
                        accuracy = policeSixStarAccuracy;
                        firerate = policeSixStarFirerate;
                    }
                    else
                    {
                        accuracy = policeAccuracy;
                        firerate = policeFirerate;
                    }
                }
                else if (gangTypes.Contains(pedType))
                {
                    accuracy = gangAccuracy;
                    firerate = gangFirerate;
                }
                else if (IS_PED_IN_GROUP(pedHandle))
                {
                    accuracy = followerAccuracy;
                    firerate = followerFirerate;
                }

                SET_CHAR_ACCURACY(pedHandle, (uint)accuracy);
                SET_CHAR_SHOOT_RATE(pedHandle, firerate);
            }
        }
    }
}

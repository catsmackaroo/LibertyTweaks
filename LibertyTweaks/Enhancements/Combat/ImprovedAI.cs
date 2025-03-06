using IVSDKDotNet;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class ImprovedAI
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
        private static readonly Dictionary<uint, (int accuracy, int firerate)> pedTypeSettings = new Dictionary<uint, (int accuracy, int firerate)>
        {
            {3295460374, (95, 80)},  // FIB
            {3290204350, (80, 70)}   // NOoSE
        };

        public static void Init(SettingsFile settings)
        {
            enableAccuracyFirerate = settings.GetBoolean("Improved AI", "Increased Accuracy & Firerate", true);

            defaultPedAccuracy = settings.GetInteger("Extensive Settings", "Default Accuracy", 40);
            defaultPedFirerate = settings.GetInteger("Extensive Settings", "Default Firerate", 40);

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

                if (pedTypeSettings.ContainsKey(pedModel))
                {
                    accuracy = pedTypeSettings[pedModel].accuracy;
                    firerate = pedTypeSettings[pedModel].firerate;
                }


                STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);

                if (policeHashes.Contains(pedModel))
                {
                    if (currentWantedLevel == 6)
                    {
                        accuracy = 100;
                        firerate = 100;
                    }
                    else
                    {
                        accuracy = 80;
                        firerate = 70;
                    }
                }
                else if (gangTypes.Contains(pedType))
                {
                    accuracy = 55;
                    firerate = 85;
                }
                else if (IS_PED_IN_GROUP(pedHandle))
                {
                    accuracy = 100;
                    firerate = 100;
                }

                SET_CHAR_ACCURACY(pedHandle, (uint)accuracy);
                SET_CHAR_SHOOT_RATE(pedHandle, firerate);
            }
        }
    }
}

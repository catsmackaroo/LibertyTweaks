using System;
using System.Collections.Generic;
using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class ImprovedAI
    {
        private static bool enable;
        private static bool enableFireMoreInCar;
        private static HashSet<uint> policeHashes = new HashSet<uint>
        {
            4111764146,
            2776029317,
            4205665177,
        };
        private static List<uint> gangTypes = new List<uint>
        {
            3,
            4,
            11,
            13,
            9,
            10,
            12,
            14,
            6,
            8,
            7,
            5,
        };

        private static int defaultPedAccuracy;
        private static int defaultPedFirerate;
        private static int gangAccuracy;
        private static int gangFirerate;
        private static int policeRegularAccuracy;
        private static int policeRegularFirerate;
        private static int fibAccuracy;
        private static int fibFirerates;
        private static int nooseAccuracy;
        private static int nooseFirerate;
        private static int policeSixStarsAccuracy;
        private static int policeSixStarsFirerate;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved AI", "Enable", true);
            enableFireMoreInCar = settings.GetBoolean("Improved AI", "Aggressive Vehicle Response", true);

            defaultPedAccuracy = settings.GetInteger("Extensive Settings", "Default Accuracy", 40);
            defaultPedFirerate = settings.GetInteger("Extensive Settings", "Default Firerate", 40);
            gangAccuracy = settings.GetInteger("Extensive Settings", "Gang Accuracy", 55);
            gangFirerate = settings.GetInteger("Extensive Settings", "Gang Firerate", 85);
            fibAccuracy = settings.GetInteger("Extensive Settings", "FIB Accuracy", 95);
            fibFirerates = settings.GetInteger("Extensive Settings", "FIB Firerate", 80);
            nooseAccuracy = settings.GetInteger("Extensive Settings", "NOoSE Accuracy", 80);
            nooseFirerate = settings.GetInteger("Extensive Settings", "NOoSE Firerate", 70);
            policeRegularAccuracy = settings.GetInteger("Extensive Settings", "Police Accuracy", 80);
            policeRegularFirerate = settings.GetInteger("Extensive Settings", "Police Firerate", 70);
            policeSixStarsAccuracy = settings.GetInteger("Extensive Settings", "Police Six Star Accuracy", 100);
            policeSixStarsFirerate = settings.GetInteger("Extensive Settings", "Police Six Star Firerate", 100);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
                    uint playerId = GET_PLAYER_ID();
                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    GET_CHAR_MODEL(pedHandle, out uint pedModel);
                    STORE_WANTED_LEVEL((int)playerId, out uint currentWantedLevel);
                    GET_PED_TYPE(pedHandle, out uint pedType);

                    // Default
                    SET_CHAR_ACCURACY(pedHandle, (uint)defaultPedAccuracy);
                    SET_CHAR_SHOOT_RATE(pedHandle, defaultPedFirerate);

                    // Police
                    if (policeHashes.Contains(pedModel))
                    {
                        if (IS_CHAR_IN_ANY_CAR(playerPed.GetHandle()) && enableFireMoreInCar)
                        {
                            SET_CHAR_SHOOT_RATE(pedHandle, 100);
                        }
                        else if (currentWantedLevel == 6)
                        {
                            SET_CHAR_ACCURACY(pedHandle, (uint)policeSixStarsAccuracy);
                            SET_CHAR_SHOOT_RATE(pedHandle, policeSixStarsFirerate);
                        }
                        else
                        {
                            SET_CHAR_ACCURACY(pedHandle, (uint)policeRegularAccuracy);
                            SET_CHAR_SHOOT_RATE(pedHandle, policeRegularFirerate);
                        }
                    }

                    // FIB
                    if (pedModel == 3295460374)
                    {
                        SET_CHAR_ACCURACY(pedHandle, (uint)fibAccuracy);
                        SET_CHAR_SHOOT_RATE(pedHandle, fibFirerates);
                    }

                    // NOoSE
                    if (pedModel == 3290204350)
                    {
                        SET_CHAR_ACCURACY(pedHandle, (uint)nooseAccuracy);
                        SET_CHAR_SHOOT_RATE(pedHandle, nooseFirerate);
                    }

                    // Gang
                    if (gangTypes.Contains(pedType))
                    {
                        SET_CHAR_ACCURACY(pedHandle, (uint)gangAccuracy);
                        SET_CHAR_SHOOT_RATE(pedHandle, gangFirerate);
                    }
                }
            }
        }
    }
}

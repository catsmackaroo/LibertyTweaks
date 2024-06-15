using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using System.Numerics;
using CCL.GTAIV;
using IVSDKDotNet.Enums;

namespace LibertyTweaks
{
    internal class PedsLockDoors
    {
        private static bool enable;
        private static List<int> lockedVehicles = new List<int>();

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Peds Lock Car Doors", "Enable", true);

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

                    int pedHandle = (int)pedPool.GetIndex(ptr);
                    IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

                    if (IS_CHAR_IN_ANY_CAR(pedHandle))
                    {
                        GET_CAR_CHAR_IS_USING(pedHandle, out int pedVehicle);
                        GET_DRIVER_OF_CAR(pedVehicle, out int pedDriver);

                        if (pedDriver == 0)
                            continue;

                        if (IS_CHAR_PLAYING_ANIM(pedHandle, "veh@std", "shock_left") || IS_CHAR_PLAYING_ANIM(pedHandle, "veh@bus", "shock_right") || IS_CHAR_PLAYING_ANIM(pedHandle, "veh@truck", "shock_left") || IS_CHAR_PLAYING_ANIM(pedHandle, "veh@low", "shock_left"))
                        {
                            _TASK_STAND_STILL(pedHandle, 1000);
                        }

                        // Check if this vehicle has already been processed
                        if (lockedVehicles.Contains(pedVehicle))
                            continue;

                        int rnd = Main.GenerateRandomNumber(0, 5);

                        if (rnd != 3)
                        {
                            LOCK_CAR_DOORS(pedVehicle, 7);
                        }
                        else
                        {
                            LOCK_CAR_DOORS(pedVehicle, 0);
                        }

                        // Add the vehicle to the list to prevent repeated locking
                        lockedVehicles.Add(pedVehicle);
                    }
                }
            }
        }
    }
}

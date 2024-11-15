using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using System.Numerics;
using CCL.GTAIV;
using IVSDKDotNet.Enums;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class PedsLockDoors
    {
        private static bool enable;
        private static readonly List<int> lockedVehicles = new List<int>();
        private static readonly Dictionary<int, int> pedToVehicleMap = new Dictionary<int, int>();

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Peds Lock Car Doors", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            // TODO: figure out a fix for when peds drive off too soon

            if (!enable)
                return;

            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;

                if (IS_CHAR_DEAD(pedHandle))
                    continue;

                if (IS_CHAR_IN_ANY_CAR(pedHandle))
                {
                    GET_CAR_CHAR_IS_USING(pedHandle, out int pedVehicle);
                    GET_DRIVER_OF_CAR(pedVehicle, out int pedDriver);

                    if (pedDriver == 0 || pedDriver == Main.PlayerPed.GetHandle() || IS_PED_A_MISSION_PED(pedDriver))
                        continue;

                    if (IS_CHAR_PLAYING_ANIM(pedHandle, "veh@std", "shock_left") || IS_CHAR_PLAYING_ANIM(pedHandle, "veh@bus", "shock_right") || IS_CHAR_PLAYING_ANIM(pedHandle, "veh@truck", "shock_left") || IS_CHAR_PLAYING_ANIM(pedHandle, "veh@low", "shock_left"))
                        _TASK_STAND_STILL(pedHandle, 2000);

                    if (!lockedVehicles.Contains(pedVehicle))
                    {
                        int rnd = Main.GenerateRandomNumber(0, 5);

                        if (rnd != 3)
                            LOCK_CAR_DOORS(pedVehicle, 7);
                        else
                            LOCK_CAR_DOORS(pedVehicle, 0);

                        lockedVehicles.Add(pedVehicle);
                    }

                    if (!pedToVehicleMap.ContainsKey(pedHandle))
                        pedToVehicleMap[pedHandle] = pedVehicle;
                }
                else
                {
                    if (pedToVehicleMap.ContainsKey(pedHandle))
                    {
                        int exitedVehicle = pedToVehicleMap[pedHandle];
                        LOCK_CAR_DOORS(exitedVehicle, 0);
                        lockedVehicles.Remove(exitedVehicle);
                        pedToVehicleMap.Remove(pedHandle);
                    }
                }
            }
        }
    }
}
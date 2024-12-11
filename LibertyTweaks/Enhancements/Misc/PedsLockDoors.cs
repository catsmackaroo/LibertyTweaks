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
            if (!enable)
                return;

            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;

                // Skip dead or invalid pedestrians
                if (IS_CHAR_DEAD(pedHandle) || !DOES_CHAR_EXIST(pedHandle))
                    continue;

                // Check if the pedestrian is in a vehicle
                if (!IS_CHAR_IN_ANY_CAR(pedHandle))
                {
                    if (pedToVehicleMap.ContainsKey(pedHandle))
                    {
                        int exitedVehicle = pedToVehicleMap[pedHandle];
                        LOCK_CAR_DOORS(exitedVehicle, 0); // Unlock the vehicle
                        lockedVehicles.Remove(exitedVehicle);
                        pedToVehicleMap.Remove(pedHandle);
                    }
                    continue;
                }

                GET_CAR_CHAR_IS_USING(pedHandle, out int pedVehicle);

                if (pedVehicle == 0 || !DOES_VEHICLE_EXIST(pedVehicle))
                    continue;

                GET_DRIVER_OF_CAR(pedVehicle, out int pedDriver);

                if (pedDriver == 0 || pedDriver == Main.PlayerPed.GetHandle() || IS_PED_A_MISSION_PED(pedDriver))
                    continue;

                // Lock vehicle doors if not already locked
                if (!lockedVehicles.Contains(pedVehicle))
                {
                    int rnd = Main.GenerateRandomNumber(0, 5);
                    uint lockMode = (rnd != 3) ? 7u : 0u;

                    LOCK_CAR_DOORS(pedVehicle, lockMode);
                    lockedVehicles.Add(pedVehicle);
                }

                // Map pedestrian to vehicle
                pedToVehicleMap[pedHandle] = pedVehicle;
            }
        }

    }
}
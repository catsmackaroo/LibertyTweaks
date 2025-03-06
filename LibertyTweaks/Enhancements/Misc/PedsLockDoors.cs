using CCL.GTAIV;
using IVSDKDotNet;
using System.Collections.Generic;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class PedsLockDoors
    {
        private static bool enable;
        private static readonly List<int> lockedVehicles = new List<int>();
        private static readonly Dictionary<int, int> pedToVehicleMap = new Dictionary<int, int>();
        private static bool pEnteringLocked = false;

        // Optimization Stuff
        private static int tickCounter = 0;
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
            tickCounter++;

            if (tickCounter % 30 == 0)
            {
                if (pEnteringLocked == false)
                    HandleLockingCars();

                HandleCancelling();
            }
        }

        private static void HandleLockingCars()
        {
            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;

                if (IS_CHAR_DEAD(pedHandle) || !DOES_CHAR_EXIST(pedHandle))
                    continue;

                if (!IS_CHAR_IN_ANY_CAR(pedHandle))
                {
                    if (pedToVehicleMap.ContainsKey(pedHandle))
                    {
                        int exitedVehicle = pedToVehicleMap[pedHandle];
                        LOCK_CAR_DOORS(exitedVehicle, 0);
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
        private static void HandleCancelling()
        {
            if (IS_CHAR_TRYING_TO_ENTER_A_LOCKED_CAR(Main.PlayerPed.GetHandle()))
                pEnteringLocked = true;

            if (!pEnteringLocked || lockedVehicles.Count == 0)
                return;

            GET_CHAR_COORDINATES(Main.PlayerPed.GetHandle(), out float playerX, out float playerY, out float playerZ);
            Vector3 playerPos = new Vector3(playerX, playerY, playerZ);

            float minDistance = float.MaxValue;
            int closestVehicle = 0;

            foreach (int vehicleHandle in lockedVehicles)
            {
                if (!DOES_VEHICLE_EXIST(vehicleHandle))
                    continue;

                GET_CAR_COORDINATES(vehicleHandle, out float carX, out float carY, out float carZ);
                Vector3 carPos = new Vector3(carX, carY, carZ);

                float distance = Vector3.Distance(playerPos, carPos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestVehicle = vehicleHandle;
                }
            }

            // Threshold for extreme distances where animations break
            if (minDistance > 1.0f && minDistance < 4f && closestVehicle != 0)
            {
                GET_CAR_COORDINATES(closestVehicle, out float carX, out float carY, out float carZ);
                Vector3 carPos = new Vector3(carX, carY, carZ);
                SWITCH_PED_TO_ANIMATED(Main.PlayerPed.GetHandle(), true);
                SWITCH_PED_TO_RAGDOLL(Main.PlayerPed.GetHandle(), 14, 500, true, true, true, true);
                pEnteringLocked = false;
                return;
            }
        }
    }
}
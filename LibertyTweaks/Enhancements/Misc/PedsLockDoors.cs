using CCL.GTAIV;
using IVSDKDotNet;
using System.Collections.Generic;
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
        private static int tickCounter = 0;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            PedsLockDoors.section = section;
            enable = settings.GetBoolean(section, "Peds Lock Car Doors", false);

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

                if (pedDriver == 0 || pedDriver == Main.PlayerPed.GetHandle() || IS_PED_A_MISSION_PED(pedDriver) || IS_CHAR_IN_TAXI(pedDriver))
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
            foreach (int vehicleHandle in lockedVehicles)
            {
                if (!DOES_VEHICLE_EXIST(vehicleHandle))
                    continue;

                GET_CAR_SPEED(vehicleHandle, out float speed);
                if (speed > 2f)
                    LOCK_CAR_DOORS(vehicleHandle, 3);
                else
                    LOCK_CAR_DOORS(vehicleHandle, 7);
            }
        }
    }
}
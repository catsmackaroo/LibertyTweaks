using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using System;
using System.Numerics;
using IVSDKDotNet.Enums;
using LibertyTweaks.Enhancements.Driving;
using DocumentFormat.OpenXml.Drawing;

namespace LibertyTweaks
{
    internal class ImprovedCrashes
    {
        private static bool enable;
        private static bool enableDoors;
        private static bool enableDetachables;
        private static bool enableTireBursts;
        private static bool enableWheelDetach;
        private static bool enableEngineCutoff;

        private const int MinorCrashThreshold = 15;
        private const int TireBurstThreshold = 10;
        private const int HarshCrashThreshold = 20;
        private const int AllWheelsDetachThreshold = 1;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Car Crashes", "Enable", true);
            enableDoors = settings.GetBoolean("Improved Car Crashes", "Doors Can Open", true);
            enableDetachables = settings.GetBoolean("Improved Car Crashes", "General Detachables", true);
            enableTireBursts = settings.GetBoolean("Improved Car Crashes", "Tire Can Burst", true);
            enableWheelDetach = settings.GetBoolean("Improved Car Crashes", "Wheels Can Detach", true);
            enableEngineCutoff = settings.GetBoolean("Improved Car Crashes", "Engine Can Break", true);

            if (enable)
                Main.Log("Improved Crashes script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || IS_PAUSE_MENU_ACTIVE()
                || IS_CHAR_IN_ANY_BOAT(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                return;
            }

            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

            if (Main.CarCrashLevel > 0)
            {
                HandleCarCrashes(vehicleIV);
            }
        }
        private static void HandleMinorCrashes(int rnd, IVVehicle vehicleIV, uint randomPart)
        {
            if (rnd <= MinorCrashThreshold && vehicleIV.DoorCount != 0 && enableDoors)
            {
                OpenAndBreakCarDoor(vehicleIV);
            }

            if (rnd <= MinorCrashThreshold && randomPart != 0 && enableDetachables)
            {
                IVPhInstGta.FromUIntPtr(vehicleIV.InstGta).DetachFragmentGroup(randomPart);
            }

            if (rnd <= TireBurstThreshold && vehicleIV.WheelCount != 0 && enableTireBursts)
            {
                BurstRandomTire(vehicleIV, rnd);
            }
        }
        private static void HandleHarshCrashes(int rnd, IVVehicle vehicleIV)
        {
            if (enableEngineCutoff)
            {
                CutOffEngine(vehicleIV);
            }

            if (enableWheelDetach)
            {
                DetachWheels(rnd, vehicleIV);
            }
        }
        private static void HandleCarCrashes(IVVehicle vehicleIV)
        {
            bool only2wheels = vehicleIV.WheelCount <= 2;
            uint randomPart = GenerateUniqueRandomPart(vehicleIV, only2wheels);

            if (Main.CarCrashLevel >= 2)
            {
                int rndMax = 100 - (Main.CarCrashLevel * 10);
                int rnd = Main.GenerateRandomNumber(0, rndMax);
                HandleMinorCrashes(rnd, vehicleIV, randomPart);
            }

            if (Main.CarCrashLevel >= 3 && vehicleIV.WheelCount != 0)
            {
                int rndMax = 100 - Main.CarCrashLevel;
                int rnd = Main.GenerateRandomNumber(0, rndMax);
                HandleHarshCrashes(rnd, vehicleIV);
            }
        }
        private static uint GenerateUniqueRandomPart(IVVehicle vehicleIV, bool only2wheels)
        {
            uint randomPart = (uint)Main.GenerateRandomNumber(1, 30);
            IVVehicleWheel vehWheel1 = vehicleIV.Wheels[0];
            IVVehicleWheel vehWheel2 = vehicleIV.Wheels[1];
            ushort vehwheelgroup1 = vehWheel1.GroupID;
            ushort vehwheelgroup2 = vehWheel2.GroupID;

            if (randomPart == vehwheelgroup1 || randomPart == vehwheelgroup2)
            {
                randomPart = (uint)Main.GenerateRandomNumber(1, 30);
            }

            if (!only2wheels)
            {
                IVVehicleWheel vehWheel3 = vehicleIV.Wheels[2];
                IVVehicleWheel vehWheel4 = vehicleIV.Wheels[3];
                ushort vehwheelgroup3 = vehWheel3.GroupID;
                ushort vehwheelgroup4 = vehWheel4.GroupID;

                if (randomPart == vehwheelgroup3 || randomPart == vehwheelgroup4)
                {
                    randomPart = (uint)Main.GenerateRandomNumber(1, 30);
                }
            }

            return randomPart;
        }
        private static void OpenAndBreakCarDoor(IVVehicle vehicleIV)
        {
            uint door = (uint)Main.GenerateRandomNumber(0, (int)vehicleIV.DoorCount);
            OPEN_CAR_DOOR(vehicleIV.GetHandle(), door);
            Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(Main.GenerateRandomNumber(0, 3)), "Main", () =>
            {
                BREAK_CAR_DOOR(vehicleIV.GetHandle(), door, false);
            });
        }
        private static void BurstRandomTire(IVVehicle vehicleIV, int rnd)
        {
            if (rnd <= 5)
            {
                IVVehicleWheel vehWheel = vehicleIV.Wheels[Main.GenerateRandomNumber(0, 3)];
                if (vehWheel != null && vehWheel.TireHealth != 0)
                {
                    vehWheel.TireHealth = 0;
                }
            }
            else if (rnd >= 6)
            {
                BURST_CAR_TYRE(vehicleIV.GetHandle(), (uint)Main.GenerateRandomNumber(0, 3));
            }
        }
        private static void CutOffEngine(IVVehicle vehicleIV)
        {
            vehicleIV.EngineHealth = 0;
            vehicleIV.Health = 0;
            vehicleIV.PetrolTankHealth = 0;
            SET_CAR_ENGINE_ON(vehicleIV.GetHandle(), false, false);
        }
        private static void DetachWheels(int rnd, IVVehicle vehicleIV)
        {
            ushort vehwheelgroup1 = vehicleIV.Wheels[0].GroupID;
            ushort vehwheelgroup2 = vehicleIV.Wheels[1].GroupID;
            ushort vehwheelgroup4 = vehicleIV.WheelCount > 2 ? vehicleIV.Wheels[3].GroupID : (ushort)0;

            ushort minGroup = Math.Min(vehwheelgroup1, vehwheelgroup4 != 0 ? vehwheelgroup4 : vehwheelgroup2);
            ushort maxGroup = Math.Max(vehwheelgroup1, vehwheelgroup4 != 0 ? vehwheelgroup4 : vehwheelgroup2);

            DetachRandomWheel(vehicleIV, minGroup, maxGroup);

            if (rnd <= HarshCrashThreshold)
            {
                DetachRandomWheel(vehicleIV, minGroup, maxGroup);
            }

            if (rnd <= HarshCrashThreshold / 2)
            {
                DetachRandomWheel(vehicleIV, minGroup, maxGroup);
            }

            if (rnd <= AllWheelsDetachThreshold)
            {
                DetachRandomWheel(vehicleIV, minGroup, maxGroup);
            }
        }
        private static void DetachRandomWheel(IVVehicle vehicleIV, ushort minGroup, ushort maxGroup)
        {
            IVPhInstGta.FromUIntPtr(vehicleIV.InstGta).DetachFragmentGroup((uint)Main.GenerateRandomNumber(minGroup, maxGroup));
        }
    }
}

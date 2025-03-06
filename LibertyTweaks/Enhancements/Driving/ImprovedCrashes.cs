using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// credits: catsmackaroo
// wheel 0 = front left, 1 = rear left, 2 = front right, 3 = rear right

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
        private const int WheelDetachThreshold = 40;
        private const int EngineCutOffThreshold = 40;

        private static List<int> excludedVehicleIDs;
        private static List<string> excludedVehicleModelsList;
        private static bool firstFrame = true;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Car Crashes", "Enable", true);
            enableDoors = settings.GetBoolean("Improved Car Crashes", "Doors Can Open", true);
            enableDetachables = settings.GetBoolean("Improved Car Crashes", "General Detachables", true);
            enableTireBursts = settings.GetBoolean("Improved Car Crashes", "Tires Can Burst", true);
            enableWheelDetach = settings.GetBoolean("Improved Car Crashes", "Wheels Can Detach", true);
            enableEngineCutoff = settings.GetBoolean("Improved Car Crashes", "Engine Can Break", true);

            excludedVehicleModelsList = new List<string>();
            string vehicleModels = settings.GetValue("Improved Car Crashes", "Excluded Vehicles", "");
            if (!string.IsNullOrWhiteSpace(vehicleModels))
                if (!string.IsNullOrWhiteSpace(vehicleModels))
                    excludedVehicleModelsList.AddRange(vehicleModels.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            if (enable)
                Main.Log("script initialized...");
        }
        private static int ConvertStringToID(string modelName)
        {
            var num = RAGE.AtStringHash(modelName);
            IVModelInfo.GetModelInfo(num, out var index);
            return index;
        }
        public static void IngameStartup()
        {
            if (!enable)
                return;

            firstFrame = true;
        }

        public static void Tick()
        {
            if (!enable) return;

            if (firstFrame)
            {
                excludedVehicleIDs = new List<int>();
                foreach (var modelName in excludedVehicleModelsList)
                    excludedVehicleIDs.Add(ConvertStringToID(modelName.Trim()));
                Main.Log("Excluded vehicles: " + string.Join(", ", excludedVehicleModelsList) + " with IDs: " + string.Join(", ", excludedVehicleIDs));

                if (excludedVehicleIDs.Contains(0))
                    Main.Log("An ID of 0 was found. Either the model can't be loaded, or an error occurred. Ensure no typos are in the .ini. The model may not have loaded if it's an episode specific one.");

                firstFrame = false;
            }

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || IS_PAUSE_MENU_ACTIVE()
                || IS_CHAR_IN_ANY_BOAT(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                return;
            }

            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

            if (excludedVehicleIDs.Contains(vehicleIV.ModelIndex))
                return;

            if (WeaponHelpers.HasCarBeenDamagedByAnyWeapon(vehicleIV))
            {
                CLEAR_CAR_LAST_WEAPON_DAMAGE(vehicleIV.GetHandle());
                return;
            }

            if (Main.CarCrashLevel > 0)
                HandleCarCrashes(vehicleIV);

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
        private static void HandleMinorCrashes(int rnd, IVVehicle vehicleIV, uint randomPart)
        {
            if (rnd <= MinorCrashThreshold && vehicleIV.DoorCount != 0 && enableDoors)
                OpenAndBreakCarDoor(vehicleIV);

            if (rnd <= MinorCrashThreshold && randomPart != 0 && enableDetachables)
                IVPhInstGta.FromUIntPtr(vehicleIV.InstGta).DetachFragmentGroup(randomPart);

            if (rnd <= TireBurstThreshold && vehicleIV.WheelCount != 0 && enableTireBursts)
                BurstRandomTire(vehicleIV, rnd);
        }
        private static void HandleHarshCrashes(int rnd, IVVehicle vehicleIV)
        {
            if (enableEngineCutoff)
                CutOffEngine(rnd, vehicleIV);

            if (enableWheelDetach)
                DetachWheels(rnd, vehicleIV);
        }

        private static uint GenerateUniqueRandomPart(IVVehicle vehicleIV, bool only2wheels)
        {
            uint randomPart = (uint)Main.GenerateRandomNumber(1, 30);
            IVVehicleWheel vehWheel1 = vehicleIV.Wheels[0];
            IVVehicleWheel vehWheel2 = vehicleIV.Wheels[1];
            ushort vehwheelgroup1 = vehWheel1.GroupID;
            ushort vehwheelgroup2 = vehWheel2.GroupID;

            if (randomPart == vehwheelgroup1 || randomPart == vehwheelgroup2)
                randomPart = (uint)Main.GenerateRandomNumber(1, 30);

            if (!only2wheels)
            {
                IVVehicleWheel vehWheel3 = vehicleIV.Wheels[2];
                IVVehicleWheel vehWheel4 = vehicleIV.Wheels[3];
                ushort vehwheelgroup3 = vehWheel3.GroupID;
                ushort vehwheelgroup4 = vehWheel4.GroupID;

                if (randomPart == vehwheelgroup3 || randomPart == vehwheelgroup4)
                    randomPart = (uint)Main.GenerateRandomNumber(1, 30);
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
                    vehWheel.TireHealth = 0;
            }
            else if (rnd >= 6)
                BURST_CAR_TYRE(vehicleIV.GetHandle(), (uint)Main.GenerateRandomNumber(0, 3));
        }
        private static void CutOffEngine(int rnd, IVVehicle vehicleIV)
        {
            if (rnd <= EngineCutOffThreshold)
            {
                vehicleIV.EngineHealth = 0;
                vehicleIV.Health = 0;
                vehicleIV.PetrolTankHealth = 0;
                SET_CAR_ENGINE_ON(vehicleIV.GetHandle(), false, false);
            }
        }
        private static void DetachWheels(int rnd, IVVehicle vehicleIV)
        {
            ushort vehwheelgroup0 = vehicleIV.Wheels[0].GroupID;
            ushort vehwheelgroup1 = vehicleIV.Wheels[1].GroupID;
            ushort vehwheelgroup2 = vehicleIV.WheelCount > 2 ? vehicleIV.Wheels[2].GroupID : (ushort)0;
            ushort vehwheelgroup3 = vehicleIV.WheelCount > 2 ? vehicleIV.Wheels[3].GroupID : (ushort)0;

            var (backDeformation, frontDeformation) = GetDeformation(vehicleIV);

            ushort minGroup, maxGroup;

            if (frontDeformation)
            {
                minGroup = vehwheelgroup0;
                maxGroup = vehwheelgroup2;
            }
            else if (backDeformation)
            {
                minGroup = vehwheelgroup1;
                maxGroup = vehwheelgroup3;
            }
            else
                return;

            if (rnd <= WheelDetachThreshold)
                DetachWheel(vehicleIV, minGroup, maxGroup);

            if (rnd <= WheelDetachThreshold / 2)
                DetachWheel(vehicleIV, minGroup, maxGroup);
        }

        private static void DetachWheel(IVVehicle vehicleIV, ushort minGroup, ushort maxGroup)
        {
            IVPhInstGta.FromUIntPtr(vehicleIV.InstGta).DetachFragmentGroup((uint)Main.GenerateRandomNumber(minGroup, maxGroup));
            SET_CAR_ENGINE_ON(vehicleIV.GetHandle(), false, false);
        }


        private static (bool backDeformation, bool frontDeformation) GetDeformation(IVVehicle vehicleIV)
        {
            if (vehicleIV != null)
            {
                GET_CAR_MODEL(vehicleIV.GetHandle(), out uint model);
                GET_MODEL_DIMENSIONS(model, out Vector3 min, out Vector3 max);
                GET_CAR_DEFORMATION_AT_POS(vehicleIV.GetHandle(), min, out Vector3 backdeformation);
                GET_CAR_DEFORMATION_AT_POS(vehicleIV.GetHandle(), max, out Vector3 frontdeformation);

                bool isBackDeformed = backdeformation != Vector3.Zero;
                bool isFrontDeformed = frontdeformation != Vector3.Zero;

                return (isBackDeformed, isFrontDeformed);
            }
            return (false, false);
        }

    }
}

using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LibertyTweaks
{
    internal class TrackedVehicle
    {
        private static NativeBlip vehBlip;
        private static readonly object vehicleLock = new object();

        /// <summary>
        /// Sets the current vehicle as a tracked personal vehicle by marking it as a mission vehicle, thus never deleting.
        /// </summary>
        public static void SetCurrentVehicle()
        {
            lock (vehicleLock)
            {
                Reset();
                PersonalVehicleHandler.trackerVehicle = Main.PlayerVehicle;
                BasicVehicle.Reset();
                Main.PlayerVehicle.SetAsMissionVehicle();
                TrackerServices.PlaySFX();
            }
        }

        /// <summary>
        /// Handles setting tracked vehicle as a mission vehicle if the player is far away; preventing deletion.
        /// </summary>
        public static void HandleMissionVehicleLogic()
        {
            lock (vehicleLock)
            {
                if (PersonalVehicleHandler.trackerVehicle != null)
                {
                    var distance = Vector3.Distance(PersonalVehicleHandler.trackerVehicle.Matrix.Pos, Main.PlayerPed.Matrix.Pos);

                    if (distance > 75f)
                        PersonalVehicleHandler.trackerVehicle.SetAsMissionVehicle();
                    else
                        PersonalVehicleHandler.trackerVehicle.MarkAsNoLongerNeeded();
                }
            }
        }

        /// <summary>
        /// Resets the tracked vehicle. Sets to null & marks as no longer needed.
        /// </summary>
        public static void Reset()
        {
            lock (vehicleLock)
            {
                if (PersonalVehicleHandler.trackerVehicle != null)
                {
                    PersonalVehicleHandler.trackerVehicle.MarkAsNoLongerNeeded();
                    PersonalVehicleHandler.trackerVehicle = null;
                }
            }
        }

        /// <summary>
        /// Checks if the tracked vehicle is dead. If so, it resets the vehicle.
        /// </summary>
        public static void HandleChecks()
        {
            lock (vehicleLock)
            {
                if (IS_CAR_DEAD(PersonalVehicleHandler.trackerVehicle.GetHandle()))
                    Reset();
            }
        }

        /// <summary>
        /// Handles blip behavior for tracked personal vehicles.
        /// </summary>
        public static void HandleBlip()
        {
            lock (vehicleLock)
            {
                if (IsVehicleInvalid(PersonalVehicleHandler.trackerVehicle))
                {
                    Reset();
                    return;
                }

                try
                {
                    bool isPlayerInCar = IS_CHAR_IN_CAR(Main.PlayerPed.GetHandle(), PersonalVehicleHandler.trackerVehicle.GetHandle());
                    bool isCarDead = IS_CAR_DEAD(PersonalVehicleHandler.trackerVehicle.GetHandle());

                    if (isPlayerInCar || isCarDead)
                    {
                        DeleteBlip();
                    }
                    else if (vehBlip == null || !vehBlip.IsValid)
                    {
                        CreateBlip(PersonalVehicleHandler.trackerVehicle, 1f);
                    }
                }
                catch
                {
                    Main.LogError("Error: Unable to handle blip.");
                }
            }
        }

        /// <summary>
        /// Handles saving of the tracked vehicle. Saves to LibertyTweaks.save file.
        /// </summary>
        public static void Save()
        {
            lock (vehicleLock)
            {
                if (Main.GameSaved)
                {
                    SaveVehicle(PersonalVehicleHandler.trackerVehicle, "TrackedVehicle");
                }
            }
        }

        /// <summary>
        /// Loads the tracked vehicle from the LibertyTweaks.save file, upon game load.
        /// </summary>
        public static void Load()
        {
            lock (vehicleLock)
            {
                LoadVehicle("TrackedVehicle", out PersonalVehicleHandler.trackerVehicle);
            }
        }

        // Private helper methods
        private static bool IsVehicleInvalid(IVVehicle vehicle)
        {
            return vehicle == null || vehicle.GetHandle() == 0;
        }

        private static void DeleteBlip()
        {
            vehBlip?.Delete();
            vehBlip = null;
        }

        private static void CreateBlip(IVVehicle vehicle, float scale)
        {
            vehBlip = vehicle.AttachBlip();
            vehBlip.Icon = BlipIcon.Building_Garage;
            vehBlip.Scale = scale;
        }

        private static void SaveVehicle(IVVehicle vehicle, string prefix)
        {
            var name = vehicle.Handling.Name;
            GET_CAR_MODEL(vehicle.GetHandle(), out uint modelID);
            var color1 = vehicle.PrimaryColor;
            var color2 = vehicle.SecondaryColor;
            var color3 = vehicle.TertiaryColor;
            var color4 = vehicle.QuaternaryColor;
            var engineHP = vehicle.EngineHealth;
            var petrolHP = vehicle.PetrolTankHealth;
            var heading = vehicle.GetHeading();
            var pos = vehicle.Matrix.Pos;
            var dirt = vehicle.DirtLevel;

            bool[] extras = new bool[10];
            for (int i = 1; i < extras.Length; i++)
            {
                extras[i] = IS_VEHICLE_EXTRA_TURNED_ON(vehicle.GetHandle(), (uint)i);
            }

            Main.GetTheSaveGame().SetValue($"{prefix}Name", name);
            Main.GetTheSaveGame().SetInteger($"{prefix}Model", (int)modelID);
            Main.GetTheSaveGame().SetInteger($"{prefix}Color1", color1);
            Main.GetTheSaveGame().SetInteger($"{prefix}Color2", color2);
            Main.GetTheSaveGame().SetInteger($"{prefix}Color3", color4);
            Main.GetTheSaveGame().SetInteger($"{prefix}Color4", color3);
            Main.GetTheSaveGame().SetFloat($"{prefix}EngineHealth", engineHP);
            Main.GetTheSaveGame().SetFloat($"{prefix}PetrolTankHealth", petrolHP);
            Main.GetTheSaveGame().SetFloat($"{prefix}Heading", heading);
            Main.GetTheSaveGame().SetVector3($"{prefix}Position", pos);
            Main.GetTheSaveGame().SetFloat($"{prefix}Dirt", dirt);

            for (int i = 1; i < extras.Length; i++)
            {
                Main.GetTheSaveGame().SetBoolean($"{prefix}Extra{i}", extras[i]);
            }

            Main.GetTheSaveGame().Save();
        }

        private static void LoadVehicle(string prefix, out IVVehicle vehicle)
        {
            string name = Main.GetTheSaveGame().GetValue($"{prefix}Name");
            uint modelID = (uint)Main.GetTheSaveGame().GetInteger($"{prefix}Model");
            Vector3 pos = Main.GetTheSaveGame().GetVector3($"{prefix}Position");
            byte color1 = (byte)Main.GetTheSaveGame().GetInteger($"{prefix}Color1");
            byte color2 = (byte)Main.GetTheSaveGame().GetInteger($"{prefix}Color2");
            byte color3 = (byte)Main.GetTheSaveGame().GetInteger($"{prefix}Color3");
            byte color4 = (byte)Main.GetTheSaveGame().GetInteger($"{prefix}Color4");
            float engineHP = Main.GetTheSaveGame().GetFloat($"{prefix}EngineHealth");
            float petrolHP = Main.GetTheSaveGame().GetFloat($"{prefix}PetrolTankHealth");
            float heading = Main.GetTheSaveGame().GetFloat($"{prefix}Heading");
            float dirt = Main.GetTheSaveGame().GetFloat($"{prefix}Dirt");
            bool[] extras = new bool[10];
            for (int i = 0; i < extras.Length; i++)
            {
                extras[i] = Main.GetTheSaveGame().GetBoolean($"{prefix}Extra{i}");
            }

            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    int closestCar = GET_CLOSEST_CAR(pos, 10f, 0, 70);
                    if (closestCar != 0)
                    {
                        MARK_CAR_AS_NO_LONGER_NEEDED(closestCar);
                        DELETE_CAR(ref closestCar);
                    }

                    vehicle = NativeWorld.SpawnVehicle(modelID, pos, out int savedVehicleHandle, true);
                    CHANGE_CAR_COLOUR(savedVehicleHandle, color1, color2);
                    SET_EXTRA_CAR_COLOURS(savedVehicleHandle, color3, color4);
                    SET_CAR_ON_GROUND_PROPERLY(savedVehicleHandle);
                    SET_CAR_HEADING(savedVehicleHandle, heading);
                    SET_ENGINE_HEALTH(savedVehicleHandle, (uint)engineHP);
                    SET_PETROL_TANK_HEALTH(savedVehicleHandle, (uint)petrolHP);
                    SET_VEHICLE_DIRT_LEVEL(savedVehicleHandle, dirt);
                    SET_HAS_BEEN_OWNED_BY_PLAYER(savedVehicleHandle, true);
                    vehicle.VehicleFlags.NeedsToBeHotWired = false;

                    for (int i = 0; i < extras.Length; i++)
                    {
                        if (extras[i])
                            TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, false);
                        else
                            TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, true);
                    }
                }
                catch (Exception ex)
                {
                    Main.LogError("Error spawning vehicle: " + ex.Message);
                    vehicle = null;
                }
            }
            else
            {
                Main.Log("No tracked vehicle found.");
                vehicle = null;
            }
        }

    }
}

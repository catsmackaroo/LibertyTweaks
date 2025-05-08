using CCL.GTAIV;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Numerics;
using IVSDKDotNet;

namespace LibertyTweaks
{
    internal class BasicVehicle
    {
        private static NativeBlip vehBlip;
        private static readonly object vehicleLock = new object(); // Lock object

        /// <summary>
        /// Sets the current vehicle as a basic personal vehicle by marking it as a mission vehicle, thus never deleting.
        /// </summary>
        public static void SetCurrentVehicle()
        {
            lock (vehicleLock)
            {
                if (Main.PlayerVehicle == null || Main.PlayerVehicle == PersonalVehicleHandler.trackerVehicle)
                    return;

                Reset();
                PersonalVehicleHandler.basicVehicle = Main.PlayerVehicle;
                Main.PlayerVehicle.SetAsMissionVehicle();
            }
        }

        /// <summary>
        /// Resets the basic vehicle. Sets to null & marks as no longer needed.
        /// </summary>
        public static void Reset()
        {
            lock (vehicleLock)
            {
                IVGame.ShowSubtitleMessage($"{PersonalVehicleHandler.basicVehicle.GetHandle().ToString()} and {PersonalVehicleHandler.trackerVehicle.GetHandle().ToString()}");
                if (PersonalVehicleHandler.basicVehicle != null)
                {
                    if (PersonalVehicleHandler.basicVehicle.GetHandle() != PersonalVehicleHandler.trackerVehicle.GetHandle())
                    {
                        PersonalVehicleHandler.basicVehicle.MarkAsNoLongerNeeded();
                    }
                }
                PersonalVehicleHandler.basicVehicle = null;
            }
        }

        /// <summary>
        /// Checks if the basic vehicle is dead. If so, it resets the vehicle.
        /// </summary>
        public static void HandleChecks()
        {
            lock (vehicleLock)
            {
                if (IsVehicleInvalid())
                {
                    Reset();
                    return;
                }

                try
                {
                    if (IS_CAR_DEAD(PersonalVehicleHandler.basicVehicle.GetHandle()))
                        Reset();
                }
                catch (Exception ex)
                {
                    Main.LogError("Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Handles blip behavior for basic personal vehicles.
        /// </summary>
        public static void HandleBlip()
        {
            lock (vehicleLock)
            {
                if (IsVehicleInvalid())
                {
                    Reset();
                    return;
                }

                try
                {
                    bool isPlayerInCar = IS_CHAR_IN_CAR(Main.PlayerPed.GetHandle(), PersonalVehicleHandler.basicVehicle.GetHandle());
                    bool isCarDead = IS_CAR_DEAD(PersonalVehicleHandler.basicVehicle.GetHandle());

                    if (isPlayerInCar || isCarDead)
                    {
                        DeleteBlip();
                    }
                    else if (vehBlip == null || !vehBlip.IsValid)
                    {
                        CreateBlip();
                    }
                }
                catch
                {
                    Main.LogError("Error: Unable to handle blip.");
                }
            }
        }

        /// <summary>
        /// Handles saving of the basic vehicle. Saves to LibertyTweaks.save file.
        /// </summary>
        public static void Save()
        {
            if (Main.GameSaved)
            {
                var name = PersonalVehicleHandler.basicVehicle.Handling.Name;
                GET_CAR_MODEL(PersonalVehicleHandler.basicVehicle.GetHandle(), out uint modelID);
                var color1 = PersonalVehicleHandler.basicVehicle.PrimaryColor;
                var color2 = PersonalVehicleHandler.basicVehicle.SecondaryColor;
                var color3 = PersonalVehicleHandler.basicVehicle.TertiaryColor;
                var color4 = PersonalVehicleHandler.basicVehicle.QuaternaryColor;
                var engineHP = PersonalVehicleHandler.basicVehicle.EngineHealth;
                var petrolHP = PersonalVehicleHandler.basicVehicle.PetrolTankHealth;
                var heading = PersonalVehicleHandler.basicVehicle.GetHeading();
                var pos = PersonalVehicleHandler.basicVehicle.Matrix.Pos;
                var dirt = PersonalVehicleHandler.basicVehicle.DirtLevel;
                var savedInCar = IS_CHAR_IN_CAR(Main.PlayerPed.GetHandle(), PersonalVehicleHandler.basicVehicle.GetHandle());

                bool[] extras = new bool[10];
                for (int i = 1; i < extras.Length; i++)
                {
                    extras[i] = IS_VEHICLE_EXTRA_TURNED_ON(PersonalVehicleHandler.basicVehicle.GetHandle(), (uint)i);
                }

                Main.GetTheSaveGame().SetValue("BasicVehicleName", name);
                Main.GetTheSaveGame().SetInteger("BasicVehicleModel", (int)modelID);
                Main.GetTheSaveGame().SetInteger("BasicVehicleColor1", color1);
                Main.GetTheSaveGame().SetInteger("BasicVehicleColor2", color2);
                Main.GetTheSaveGame().SetInteger("BasicVehicleColor3", color4);
                Main.GetTheSaveGame().SetInteger("BasicVehicleColor4", color3);
                Main.GetTheSaveGame().SetFloat("BasicVehicleEngineHealth", engineHP);
                Main.GetTheSaveGame().SetFloat("BasicVehiclePetrolTankHealth", petrolHP);
                Main.GetTheSaveGame().SetFloat("BasicVehicleHeading", heading);
                Main.GetTheSaveGame().SetVector3("BasicVehiclePosition", pos);
                Main.GetTheSaveGame().SetFloat("BasicVehicleDirt", dirt);
                Main.GetTheSaveGame().SetBoolean("BasicVehicleSavedInCar", savedInCar);

                for (int i = 1; i < extras.Length; i++)
                {
                    Main.GetTheSaveGame().SetBoolean($"BasicVehicleExtra{i}", extras[i]);
                }

                Main.GetTheSaveGame().Save();
                Main.Log("Basic vehicle saved.");
            }
        }

        /// <summary>
        /// Loads the basic vehicle from the LibertyTweaks.save file, upon game load.
        /// </summary>
        public static void Load()
        {
            lock (vehicleLock)
            {
                string name = Main.GetTheSaveGame().GetValue("BasicVehicleName");
                uint modelID = (uint)Main.GetTheSaveGame().GetInteger("BasicVehicleModel");
                Vector3 pos = Main.GetTheSaveGame().GetVector3("BasicVehiclePosition");
                byte color1 = (byte)Main.GetTheSaveGame().GetInteger("BasicVehicleColor1");
                byte color2 = (byte)Main.GetTheSaveGame().GetInteger("BasicVehicleColor2");
                byte color3 = (byte)Main.GetTheSaveGame().GetInteger("BasicVehicleColor3");
                byte color4 = (byte)Main.GetTheSaveGame().GetInteger("BasicVehicleColor4");
                float engineHP = Main.GetTheSaveGame().GetFloat("BasicVehicleEngineHealth");
                float petrolHP = Main.GetTheSaveGame().GetFloat("BasicVehiclePetrolTankHealth");
                float heading = Main.GetTheSaveGame().GetFloat("BasicVehicleHeading");
                float dirt = Main.GetTheSaveGame().GetFloat("BasicVehicleDirt");
                bool savedInCar = Main.GetTheSaveGame().GetBoolean("BasicVehicleSavedInCar");
                
                bool[] extras = new bool[10];
                for (int i = 0; i < extras.Length; i++)
                {
                    extras[i] = Main.GetTheSaveGame().GetBoolean($"BasicVehicleExtra{i}");
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

                        PersonalVehicleHandler.basicVehicle = NativeWorld.SpawnVehicle(modelID, pos, out int savedVehicleHandle, true);
                        CHANGE_CAR_COLOUR(savedVehicleHandle, color1, color2);
                        SET_EXTRA_CAR_COLOURS(savedVehicleHandle, color3, color4);
                        SET_CAR_ON_GROUND_PROPERLY(savedVehicleHandle);
                        SET_CAR_HEADING(savedVehicleHandle, heading);
                        SET_ENGINE_HEALTH(savedVehicleHandle, (uint)engineHP);
                        SET_PETROL_TANK_HEALTH(savedVehicleHandle, (uint)petrolHP);
                        SET_VEHICLE_DIRT_LEVEL(savedVehicleHandle, dirt);
                        SET_HAS_BEEN_OWNED_BY_PLAYER(savedVehicleHandle, true);
                        PersonalVehicleHandler.basicVehicle.VehicleFlags.NeedsToBeHotWired = false;

                        for (int i = 0; i < extras.Length; i++)
                        {
                            if (extras[i])
                                TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, false);
                            else
                                TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, true);
                        }

                        if (savedInCar)
                        {
                            _TASK_ENTER_CAR_AS_DRIVER(Main.PlayerPed.GetHandle(), savedVehicleHandle, 1);
                            SET_CAR_ENGINE_ON(savedVehicleHandle, true, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Main.LogError("Error spawning vehicle: " + ex.Message);
                    }
                }
                else
                {
                    Main.Log("No basic vehicle found.");
                }
            }
        }

        // Herlper methods for code readability.
        private static bool IsVehicleInvalid()
        {
            return PersonalVehicleHandler.basicVehicle == null || PersonalVehicleHandler.basicVehicle.GetHandle() == 0;
        }

        private static void DeleteBlip()
        {
            vehBlip?.Delete();
            vehBlip = null;
        }

        private static void CreateBlip()
        {
            vehBlip = PersonalVehicleHandler.basicVehicle.AttachBlip();
            vehBlip.Icon = BlipIcon.Building_Garage;
            vehBlip.Scale = 0f;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Input;
using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System.Diagnostics;
using IVSDKDotNet.Enums;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class PersonalVehicle
    {
        public static bool enable;
        private static NativeBlip vehBlip;
        private static IVVehicle savedVehicle;
        private static string savedVehicleName;
        private static uint savedVehicleModelId;
        private static byte savedVehiclePrimaryColor;
        private static byte savedVehicleSecondaryColor;
        private static byte savedVehicleQuaternaryColor;
        private static byte savedVehicleTertiaryColor;
        private static float savedVehicleEngineHealth;
        private static float savedVehiclePetrolTankHealth;
        private static float savedVehicleHeading;
        private static float savedVehicleDirt;
        private static bool[] savedVehicleExtras = new bool[11];
        private static Vector3 savedVehiclePosition;
        private static bool firstFrame = true;
        private static bool isSavingMessagePrinted;
        private static bool isBlipAttached;

        private static bool blipsSpawned = false;
        private static NativeBlip trackerBlip;
        private static List<Vector3> serviceLocations = new List<Vector3>();
        private static Vector3 northAlgonquinPNS = new Vector3(-335, 1531, 19);
        private static Vector3 southAlgonquinPNS = new Vector3(-481, 350, 6);
        private static Vector3 dukesPNS = new Vector3(1065, -286, 20);
        private static Vector3 northAlderneyPNS = new Vector3(-1125, 1185, 16);
        private static Vector3 southAlderneyPNS = new Vector3(-1308, 272, 10);
        private static Vector3 stevieLocation = new Vector3(722, 1392, 14);
        private static bool canBeTracked = false;
        private static bool canShowGuide = false;
        private static bool canFade = false;
        public static uint priceForTracking = 10000;
        private static uint priceForInsurance = 25000;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Personal Vehicle", "Enable", true);
        }
        //public static void LoadFiles()
        //{
        //    IVCDStream.AddImage("IVSDKDotNet/scripts/LibertyTweaks/PersonalVehicleBlips/personalvehicles", 1, -1);

        //}
        public static void IngameStartup()
        {
            if (!enable)
                return;

            firstFrame = true;
            isSavingMessagePrinted = false;
        }
        public static void Process()
        {
            if (!enable)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            uint playerMoney = IVPlayerInfoExtensions.GetMoney(playerPed.PlayerInfo);

            if (!IS_CHAR_IN_ANY_CAR(playerPed.GetHandle()))
                return;

            if (canBeTracked == false)
                return;

            if (playerMoney < priceForTracking)
                return;

            if (IS_CHAR_IN_CAR(playerPed.GetHandle(), savedVehicle.GetHandle()))
                return;

            Cleanup();
            savedVehicle = IVVehicle.FromUIntPtr(playerPed.GetVehicle());
            IVPlayerInfoExtensions.RemoveMoney(playerPed.PlayerInfo, (int)priceForTracking);
            canShowGuide = true;
            canFade = true;
            Main.Log("Set Unsaved Vehicle: " + savedVehicle.Handling.Name);
        }
        public static void Tick()
        {
            if (!enable)
                return;

            bool isSaving = GET_IS_DISPLAYINGSAVEMESSAGE();
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            uint playerMoney = IVPlayerInfoExtensions.GetMoney(playerPed.PlayerInfo);

            if (firstFrame)
            {
                SpawnSavedVehicle();
                firstFrame = false;
            }

            if (savedVehicle != null)
            {
                UpdateVehicleData();
                DynamicVehicleBlip(playerPed);
            }
            else
            {
                Cleanup();
            }

            if (isSaving)
            {
                SaveVehicleData();
            }
            else
            {
                isSavingMessagePrinted = false;
            }

            if (!blipsSpawned)
            {
                AddServiceLocations();
                AddBlipForTrackerService();
            }

            TrackerServiceFunctionality(playerPed, serviceLocations, playerMoney);

            if (canFade)
            {
                DO_SCREEN_FADE_OUT(2000);
                if (IS_SCREEN_FADING())
                    WAIT(1000);
                DO_SCREEN_FADE_IN(2000);
                canFade = false;
            }
        }
        private static void AddServiceLocations()
        {
            if (serviceLocations.Count == 0)
            {
                serviceLocations.Add(northAlgonquinPNS);
                serviceLocations.Add(southAlgonquinPNS);
                serviceLocations.Add(dukesPNS);
                serviceLocations.Add(northAlderneyPNS);
                serviceLocations.Add(southAlderneyPNS);
                serviceLocations.Add(stevieLocation);
            }
        }
        private static void AddBlipForTrackerService()
        {
            foreach (Vector3 location in serviceLocations)
            {
                NativeBlip trackerBlip = NativeBlip.AddBlip(location);
                trackerBlip.ShowOnlyWhenNear = true;
                trackerBlip.Icon = BlipIcon.Misc_Base;
                trackerBlip.Name = "Tracker Service";
                trackerBlip.Scale = 0.8f;
            }


            //int blipTexture = GET_TEXTURE_FROM_STREAMED_TXD("personalvehicleblips", "PERSONALVEHICLEBLIP");
            //IVGame.Console.Print(blipTexture.ToString());
            blipsSpawned = true;
        }
        private static void TrackerServiceDisplayTutorial()
        {
            if (canShowGuide)
            {
                IVGame.ShowSubtitleMessage("");
                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "You have tracked a vehicle. When tracking your vehicle, you can find it on the map. It will be saved, similar to vehicles parked in-front of safehouses. You may only have one tracked vehicle at a time.");
                PRINT_HELP("PLACEHOLDER_1");
                canShowGuide = false;
            }
        }
        private static void TrackerServiceFunctionality(IVPed playerPed, List<Vector3> locations, uint playerMoney)
        {
            if (!IS_CHAR_IN_ANY_CAR(playerPed.GetHandle()))
                return;

            foreach (Vector3 location in locations)
            {
                if (Vector3.Distance(playerPed.Matrix.Pos, location) < 5f)
                {
                    if (!IS_CHAR_IN_CAR(playerPed.GetHandle(), savedVehicle.GetHandle()))
                    {
                        canBeTracked = true;
                        IVGame.ShowSubtitleMessage("Press " + Main.personalVehicleKeyString + " to add a tracker to this vehicle.");
                    }

                    if (playerMoney < priceForTracking)
                    {
                        IVGame.ShowSubtitleMessage("You have insufficient funds, a tracker costs $" + priceForTracking);
                    }

                    TrackerServiceDisplayTutorial();
                    return; 
                }
            }

            canBeTracked = false;
        }
        private static void DynamicVehicleBlip(IVPed playerPed)
        {

            if (IS_CHAR_IN_CAR(playerPed.GetHandle(), savedVehicle.GetHandle()))
            {
                if (vehBlip != null)
                {
                    MARK_CAR_AS_NO_LONGER_NEEDED(savedVehicle.GetHandle());
                    vehBlip.Dispose();
                    vehBlip = null;
                    isBlipAttached = false;
                    Main.Log("Blip detached; player in saved vehicle.");
                }
            }
            else if (!isBlipAttached && savedVehicle != null)
            {
                SET_CAR_AS_MISSION_CAR(savedVehicle.GetHandle());
                vehBlip = savedVehicle.AttachBlip();
                vehBlip.Icon = BlipIcon.Building_Garage;
                vehBlip.Name = "Personal Vehicle";
                isBlipAttached = true;
                Main.Log("Blip attached; player not in saved vehicle.");
            }
        }
        private static void UpdateVehicleData()
        {
            try
            {
                if (savedVehicle  != null)
                {
                    GET_CAR_MODEL(savedVehicle.GetHandle(), out uint savedVehicleModel);
                    savedVehicleModelId = savedVehicleModel;
                    savedVehicleName = savedVehicle.Handling.Name;
                    savedVehiclePrimaryColor = savedVehicle.PrimaryColor;
                    savedVehicleSecondaryColor = savedVehicle.SecondaryColor;
                    savedVehicleQuaternaryColor = savedVehicle.QuaternaryColor;
                    savedVehicleTertiaryColor = savedVehicle.TertiaryColor;
                    savedVehicleEngineHealth = savedVehicle.EngineHealth;
                    savedVehiclePetrolTankHealth = savedVehicle.PetrolTankHealth;
                    savedVehicleHeading = savedVehicle.GetHeading();
                    savedVehiclePosition = savedVehicle.Matrix.Pos;
                    savedVehicleDirt = savedVehicle.DirtLevel;

                    for (int i = 1; i < savedVehicleExtras.Length; i++)
                    {
                        savedVehicleExtras[i] = IS_VEHICLE_EXTRA_TURNED_ON(savedVehicle.GetHandle(), (uint)i); // Assuming extra IDs start from 0
                    }

                    if (savedVehicle.Health < 1 || IS_CAR_DEAD(savedVehicle.GetHandle()))
                        Cleanup();
                }
            }
            catch (Exception ex)
            {
                Cleanup();
                Main.LogError("Error updating vehicle data: " + ex.Message);
            }
        }
        private static void SaveVehicleData()
        {
            Main.GetTheSaveGame().SetValue("VehicleName", savedVehicleName);
            Main.GetTheSaveGame().SetInteger("VehicleModel", (int)savedVehicleModelId);
            Main.GetTheSaveGame().SetInteger("VehicleColor1", savedVehiclePrimaryColor);
            Main.GetTheSaveGame().SetInteger("VehicleColor2", savedVehicleSecondaryColor);
            Main.GetTheSaveGame().SetInteger("VehicleColor3", savedVehicleQuaternaryColor);
            Main.GetTheSaveGame().SetInteger("VehicleColor4", savedVehicleTertiaryColor);
            Main.GetTheSaveGame().SetFloat("VehicleEngineHealth", savedVehicleEngineHealth);
            Main.GetTheSaveGame().SetFloat("VehiclePetrolTankHealth", savedVehiclePetrolTankHealth);
            Main.GetTheSaveGame().SetFloat("VehicleHeading", savedVehicleHeading);
            Main.GetTheSaveGame().SetVector3("VehiclePosition", savedVehiclePosition);
            Main.GetTheSaveGame().SetFloat("VehicleDirt", savedVehicleDirt);

            for (int i = 1; i < savedVehicleExtras.Length; i++)
            {
                Main.GetTheSaveGame().SetBoolean($"VehicleExtra{i}", savedVehicleExtras[i]);
            }

            Main.GetTheSaveGame().Save();

            if (!isSavingMessagePrinted)
            {
                if (savedVehicleName != "")
                {
                    Main.Log("Saved Vehicle: " + savedVehicleName + ".");
                }
                else
                    Main.Log("No Saved Vehicle.");

                isSavingMessagePrinted = true;
            }
        }
        private static void SpawnSavedVehicle()
        {
            string lastSavedVehicleName = Main.GetTheSaveGame().GetValue("VehicleName");
            uint lastSavedVehicleModel = (uint)Main.GetTheSaveGame().GetInteger("VehicleModel");
            Vector3 lastSavedVehiclePosition = Main.GetTheSaveGame().GetVector3("VehiclePosition");
            byte lastSavedVehiclePrimaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor1");
            byte lastSavedVehicleSecondaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor2");
            byte lastSavedVehicleQuaternaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor3");
            byte lastSavedVehicleTertiaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor4");
            float lastSavedVehicleEngineHealth = Main.GetTheSaveGame().GetFloat("VehicleEngineHealth");
            float lastSavedVehiclePetrolTankHealth = Main.GetTheSaveGame().GetFloat("VehiclePetrolTankHealth");
            float lastSavedVehicleHeading = Main.GetTheSaveGame().GetFloat("VehicleHeading");
            float lastSavedVehicleDirt = Main.GetTheSaveGame().GetFloat("VehicleDirt");
            bool[] lastSavedVehicleExtras = new bool[savedVehicleExtras.Length];
            for (int i = 0; i < lastSavedVehicleExtras.Length; i++)
            {
                lastSavedVehicleExtras[i] = Main.GetTheSaveGame().GetBoolean($"VehicleExtra{i}");
            }

            if (lastSavedVehicleName != "")
            {
                Cleanup();
                savedVehicle = NativeWorld.SpawnVehicle(lastSavedVehicleModel, lastSavedVehiclePosition, out int savedVehicleHandle, true, true);
                CHANGE_CAR_COLOUR(savedVehicleHandle, lastSavedVehiclePrimaryColor, lastSavedVehicleSecondaryColor);
                SET_EXTRA_CAR_COLOURS(savedVehicleHandle, lastSavedVehicleQuaternaryColor, lastSavedVehicleTertiaryColor);
                SET_CAR_ON_GROUND_PROPERLY(savedVehicleHandle);
                SET_CAR_HEADING(savedVehicleHandle, lastSavedVehicleHeading);
                SET_ENGINE_HEALTH(savedVehicleHandle, (uint)lastSavedVehicleEngineHealth);
                SET_PETROL_TANK_HEALTH(savedVehicleHandle, (uint)lastSavedVehiclePetrolTankHealth);
                SET_VEHICLE_DIRT_LEVEL(savedVehicleHandle, lastSavedVehicleDirt);
                SET_HAS_BEEN_OWNED_BY_PLAYER(savedVehicleHandle, true);

                for (int i = 0; i < lastSavedVehicleExtras.Length; i++)
                {
                    if (lastSavedVehicleExtras[i])
                    {
                        TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, false); 
                    }
                    else
                    {
                        TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, true);
                    }
                }
            }
        }
        private static void Cleanup()
        {
            MARK_CAR_AS_NO_LONGER_NEEDED(savedVehicle.GetHandle());

            if (vehBlip != null)
            {
                vehBlip.Dispose();
                vehBlip = null;
                isBlipAttached = false;
            }

            savedVehicle = null;
            ResetSavedVehicleState();
            CleanupServices();
        }
        private static void CleanupServices()
        {
            if (trackerBlip != null)
            {
                trackerBlip.Dispose();
                trackerBlip = null;
            }
        }
        private static void ResetSavedVehicleState()
        {
            savedVehicleName = "";
            savedVehiclePrimaryColor = 0;
            savedVehicleSecondaryColor = 0;
            savedVehicleQuaternaryColor = 0;
            savedVehicleTertiaryColor = 0;
            savedVehicleEngineHealth = 0;
            savedVehiclePetrolTankHealth = 0;
            savedVehicleHeading = 0;
            savedVehiclePosition = Vector3.Zero;
            savedVehicleDirt = 0;
            Array.Clear(savedVehicleExtras, 0, savedVehicleExtras.Length);
        }
    }
}

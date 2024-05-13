using System;
using System.IO;
using System.Numerics;
using CCL.GTAIV;
using DocumentFormat.OpenXml.Bibliography;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class PersonalVehicle
    {
        private static bool enable;
        private static NativeBlip vehBlip;
        private static IVVehicle savedVehicle;
        private static string savedVehicleName;
        private static byte savedVehiclePrimaryColor;
        private static byte savedVehiclePrimaryColor2;
        private static float savedVehicleEngineHealth;
        private static float savedVehiclePetrolTankHealth;
        private static float savedVehicleHeading;
        private static float savedVehicleDirt;
        //private static int[] savedVehicleExtras = new int[10];
        private static Vector3 savedVehiclePosition;
        private static bool firstFrame = true;
        private static bool isSavingMessagePrinted;

        // IVSDKDotNet.Manager.ManagerScript.RegisterPhoneNumber(System.Guid, string, System.Action)
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Personal Vehicle", "Enable", true);
        }
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

            if (IS_CHAR_IN_ANY_CAR(playerPed.GetHandle()))
            {
                Cleanup();

                savedVehicle = IVVehicle.FromUIntPtr(playerPed.GetVehicle());
                SET_CAR_AS_MISSION_CAR(savedVehicle.GetHandle());

                vehBlip = savedVehicle.AttachBlip();
                vehBlip.Icon = BlipIcon.Building_Garage;
            }
        }
        public static void Tick()
        {

            if (!enable)
                return;

            if (firstFrame)
            {
                SpawnSavedVehicle();
                firstFrame = false;
            }

            if (savedVehicle != null)
                UpdateVehicleData();
            else
                Cleanup();

            bool isSaving = GET_IS_DISPLAYINGSAVEMESSAGE();

            if (isSaving)
                SaveVehicleData();   
            else
                isSavingMessagePrinted = false;
        }
        public static void UpdateVehicleData()
        {
            savedVehicleName = savedVehicle.Handling.Name;
            savedVehiclePrimaryColor = savedVehicle.PrimaryColor;
            savedVehiclePrimaryColor2 = savedVehicle.PrimaryColor2;
            savedVehicleEngineHealth = savedVehicle.EngineHealth;
            savedVehiclePetrolTankHealth = savedVehicle.PetrolTankHealth;
            savedVehicleHeading = savedVehicle.GetHeading();
            savedVehiclePosition = savedVehicle.Matrix.Pos;
            savedVehicleDirt = savedVehicle.DirtLevel;

            if (savedVehicle.Health < 1 || IS_CAR_DEAD(savedVehicle.GetHandle()))
                Cleanup();
        }
        public static void SaveVehicleData()
        {
            Main.GetTheSaveGame().SetValue("VehicleName", savedVehicleName);
            Main.GetTheSaveGame().SetInteger("VehicleColor1", savedVehiclePrimaryColor);
            Main.GetTheSaveGame().SetInteger("VehicleColor2", savedVehiclePrimaryColor2);
            Main.GetTheSaveGame().SetFloat("VehicleEngineHealth", savedVehicleEngineHealth);
            Main.GetTheSaveGame().SetFloat("VehiclePetrolTankHealth", savedVehiclePetrolTankHealth);
            Main.GetTheSaveGame().SetFloat("VehicleHeading", savedVehicleHeading);
            Main.GetTheSaveGame().SetVector3("VehiclePosition", savedVehiclePosition);
            Main.GetTheSaveGame().SetFloat("VehicleDirt", savedVehicleDirt);

            Main.GetTheSaveGame().Save();

            bool isSaving = GET_IS_DISPLAYINGSAVEMESSAGE();

            if (!isSavingMessagePrinted)
            {
                if (savedVehicleName != "")
                    IVGame.Console.Print("LibertyTweaks - Saved Vehicle: " + savedVehicleName + ".");
                else
                    IVGame.Console.Print("Liberty Tweaks - No Saved Vehicle.");

                isSavingMessagePrinted = true;
            }
        }
        public static void SpawnSavedVehicle()
        {
            try
            {
                Cleanup();
                string lastSavedVehicleName = Main.GetTheSaveGame().GetValue("VehicleName");
                Vector3 lastSavedVehiclePosition = Main.GetTheSaveGame().GetVector3("VehiclePosition");
                int lastSavedVehiclePrimaryColor = Main.GetTheSaveGame().GetInteger("VehicleColor1");
                int lastSavedVehicleSecondaryColor = Main.GetTheSaveGame().GetInteger("VehicleColor2");
                float lastSavedVehicleEngineHealth = Main.GetTheSaveGame().GetFloat("VehicleEngineHealth");
                float lastSavedVehiclePetrolTankHealth = Main.GetTheSaveGame().GetFloat("VehiclePetrolTankHealth");
                float lastSavedVehicleHeading = Main.GetTheSaveGame().GetFloat("VehicleHeading");
                float lastSavedVehicleDirt = Main.GetTheSaveGame().GetFloat("VehicleDirt");

                if (lastSavedVehicleName != "")
                {
                    savedVehicle = CCL.GTAIV.NativeWorld.SpawnVehicle(lastSavedVehicleName.ToString(), (Vector3)lastSavedVehiclePosition, out int testVehHandle, true, true);

                    SET_CAR_AS_MISSION_CAR(testVehHandle);
                    SET_CAR_ON_GROUND_PROPERLY(testVehHandle);
                    CHANGE_CAR_COLOUR(testVehHandle, lastSavedVehiclePrimaryColor, lastSavedVehicleSecondaryColor);
                    SET_CAR_HEADING(testVehHandle, lastSavedVehicleHeading);
                    SET_ENGINE_HEALTH(testVehHandle, (uint)lastSavedVehicleEngineHealth);
                    SET_PETROL_TANK_HEALTH(testVehHandle, (uint)lastSavedVehiclePetrolTankHealth);
                    SET_VEHICLE_DIRT_LEVEL(testVehHandle, lastSavedVehicleDirt);
                    SET_HAS_BEEN_OWNED_BY_PLAYER(testVehHandle, true);

                    vehBlip = savedVehicle.AttachBlip();
                    vehBlip.Icon = BlipIcon.Building_Garage;
                }
            }
            catch (System.Exception)
            {
            }
        }
        public static void Cleanup()
        {
            if (vehBlip != null)
            {
                vehBlip.Dispose();
                vehBlip = null;
            }

            if (savedVehicle != null)
            {
                MARK_CAR_AS_NO_LONGER_NEEDED(savedVehicle.GetHandle());
                savedVehicle = null;
            }

            savedVehicleName = "";
            savedVehiclePrimaryColor = 0;
            savedVehiclePrimaryColor2 = 0;
            savedVehicleEngineHealth = 0;
            savedVehiclePetrolTankHealth = 0;
            savedVehicleHeading = 0;
            savedVehiclePosition = Vector3.Zero;
            savedVehicleDirt = 0;
        }
    }
}

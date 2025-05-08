using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class PersonalVehicleHandler
    {

        public static bool enable;
        public static bool enableBasicSystem;
        public static bool enableTrackerSystem;
        public static bool firstFrame = true;

        public static IVVehicle basicVehicle;
        public static IVVehicle trackerVehicle;

        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            PersonalVehicleHandler.section = section;
            enable = settings.GetBoolean(section, "Personal Vehicles", true);
            enableBasicSystem = settings.GetBoolean(section, "Personal Vehicles - Basic", true);
            enableTrackerSystem = settings.GetBoolean(section, "Personal Vehicles - Tracker Service", true);

            if (enableTrackerSystem)
                TrackerServices.Init(settings, section);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (firstFrame)
                InitializeFirstFrame();

            if (enableTrackerSystem)
            {
                HandleTrackerService();
                HandleTrackedVehicles();
            }

            if (enableBasicSystem)
                HandleBasicVehicle();

        }
        private static void InitializeFirstFrame()
        {
            if (enableBasicSystem)
                BasicVehicle.Load();

            if (enableTrackerSystem)
            {
                TrackerServices.Load();
                TrackedVehicle.Load();
            }

            firstFrame = false;
        }
        private static void HandleTrackerService()
        {
            TrackerServices.HasFoundTrackerLocation();
            TrackerServices.ManageBlips();
            TrackerServices.IsPlayerAtTrackerService();
            TrackerServices.Save();
        }
        private static void HandleTrackedVehicles()
        {
            if (trackerVehicle == null || trackerVehicle.GetHandle() == 0)
            {
                TrackedVehicle.Reset();
                return;
            }
            else
            {
                TrackedVehicle.HandleChecks();
                TrackedVehicle.HandleBlip();
                TrackedVehicle.Save();
            }
        }
        private static void HandleBasicVehicle()
        {
            if (trackerVehicle != null && Main.PlayerVehicle == trackerVehicle)
                return;

            if (CanVehicleBePersonal(false))
                BasicVehicle.SetCurrentVehicle();

            if (basicVehicle != null && basicVehicle.GetHandle() != 0)
            {
                BasicVehicle.HandleChecks();
                BasicVehicle.HandleBlip();
                BasicVehicle.Save();
            }
        }


        public static void IngameStartup()
        {
            firstFrame = true;
        }

        public static bool CanVehicleBePersonal(bool ignoreBasic)
        {
            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return false;

            GET_DRIVER_OF_CAR(Main.PlayerVehicle.GetHandle(), out int driver);
            if (driver != Main.PlayerPed.GetHandle())
                return false;

            if (Main.PlayerVehicle == null)
                return false;

            if (IS_CAR_DEAD(Main.PlayerVehicle.GetHandle()))
                return false;

            if (Main.PlayerVehicle.GetHandle() == basicVehicle.GetHandle() && !ignoreBasic)
                return false;

            if (Main.PlayerVehicle.GetHandle() == trackerVehicle.GetHandle())
                return false;

            return true;
        }
    }
}

using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using LibertyTweaks.Enhancements.Combat;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    [ShowStaticFieldsInInspector]
    internal class DynamicFOV
    {
        private static bool enable;
        private static float targetFOV = 1.1f;
        private static float currentFOV = 1.0f;
        private static float lerpSpeed = 0.05f;
        private static float defaultLerpSpeed = 0.05f;

        public static float MaxFOV { get; private set; }
        public static float MinFOV { get; private set; }
        public static float FOVInCombatOnFoot { get; private set; }
        public static float FOVInCombatInCar { get; private set; }
        public static float FOVInCombatInCarMax { get; private set; }
        public static float FOVInCombatInterior { get; private set; }
        public static float FOVOnFoot { get; private set; }
        public static float FOVOnFootGunEquipped { get; private set; }
        public static float FOVInCar { get; private set; }
        public static float FOVInCarInAir { get; private set; }
        public static float FOVInCarInAirMax { get; private set; }
        public static float FOVInCarRev { get; private set; }
        public static float FOVSkidding { get; private set; }
        public static float FOVInBoat { get; private set; }
        public static float FOVInHeli { get; private set; }
        public static float FOVInterior { get; private set; }
        public static float FOVKillcam { get; private set; }
        public static float FOVBlindfire { get; private set; }
        public static float FOVCinematic { get; private set; }
        public static bool FOVDriveBy { get; private set; }
        public static float FOVDriveByMultiplier { get; private set; }
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DynamicFOV.section = section;
            string section2 = "Extensive Settings";

            enable = settings.GetBoolean(section, "Dynamic Field of View", false);

            MaxFOV = settings.GetFloat(section2, "Max General FOV", 1.35f);
            MinFOV = settings.GetFloat(section2, "Min General FOV", 0.95f);
            FOVInCombatOnFoot = settings.GetFloat(section2, "In Combat - On Foot", 1.035f);
            FOVInCombatInCar = settings.GetFloat(section2, "In Combat - In Car", 1.045f);
            FOVInCombatInCarMax = settings.GetFloat(section2, "In Combat - In Car Max", 1.40f);
            FOVInCombatInterior = settings.GetFloat(section2, "In Combat - In Interior", 1.01f);
            FOVInCar = settings.GetFloat(section2, "In Car", 1.15f);
            FOVInCarInAir = settings.GetFloat(section2, "In Car - In Air", 1.20f);
            FOVInCarInAirMax = settings.GetFloat(section2, "In Car - Air Max", 1.50f);
            FOVInCarRev = settings.GetFloat(section2, "In Car - Rev Multiplier", 0.35f);
            FOVSkidding = settings.GetFloat(section2, "In Car - Skidding", 0.95f);
            FOVInBoat = settings.GetFloat(section2, "In Boat", 1.11f);
            FOVInHeli = settings.GetFloat(section2, "In Heli", 1.0f);
            FOVInterior = settings.GetFloat(section2, "In Interior", 1.0f);
            FOVKillcam = settings.GetFloat(section2, "In Killcam", 0.9f);
            FOVBlindfire = settings.GetFloat(section2, "In Blindfire", 1.3f);
            FOVCinematic = settings.GetFloat(section2, "Cinematic Cam", 0.6f);
            FOVOnFoot = settings.GetFloat(section2, "On Foot", 1.0f);
            FOVOnFootGunEquipped = settings.GetFloat(section2, "On Foot - With Gun", 1.025f);
            FOVDriveByMultiplier = settings.GetFloat(section2, "Driveby Multiplier", 0.1f);
            lerpSpeed = settings.GetFloat(section2, "Speed", 0.05f);
            defaultLerpSpeed = lerpSpeed;


            if (enable)
                Main.Log($"script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            var cam = IVCamera.TheFinalCam;
            if (Main.PlayerPed == null || cam == null) return;

            int playerPedHandle = Main.PlayerPed.GetHandle();
            if (playerPedHandle == 0) return;

            GET_CURRENT_CHAR_WEAPON(playerPedHandle, out int currentWeapon);
            uint currentWeapSlot = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot;

            ApplyDynamicFOV(playerPedHandle, cam, currentWeapon, currentWeapSlot);

        }

        private static void ApplyDynamicFOV(int playerPedHandle, IVCam cam, int currentWeapon, uint currentWeapSlot)
        {
            if (cam == null) return;

            bool isInVehicle = IS_CHAR_IN_ANY_CAR(playerPedHandle);
            GET_CINEMATIC_CAM(out int cinematicCam);
            bool cutsc = HAS_CUTSCENE_FINISHED();

            if (IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson == true
                && WeaponHelpers.IsPlayerAiming()
                || cutsc == false)
                HandleDefaultFOV();
            else if (cinematicCam != 0)
                HandleCinematicFOV();
            else if (Killcam.enable == true && Killcam.cam.IsActive)
                HandleKillcamFOV();
            else if (isInVehicle)
                HandleVehicleFOV(playerPedHandle, PlayerHelper.IsPlayerInOrNearCombat());
            else if (IS_INTERIOR_SCENE())
                HandleInteriorFOV(PlayerHelper.IsPlayerInOrNearCombat());
            else
                HandleOnFootFOV(PlayerHelper.IsPlayerInOrNearCombat(), currentWeapSlot);

            currentFOV = CommonHelpers.SmoothStep(currentFOV, targetFOV, lerpSpeed);
            cam.FOV *= currentFOV;
        }
        private static void HandleVehicleFOV(int playerPedHandle, bool isInOrNearCombat)
        {
            if (IS_CHAR_IN_ANY_HELI(playerPedHandle))
            {
                targetFOV = FOVInHeli;
            }
            else
            {
                GET_CAR_CHAR_IS_USING(playerPedHandle, out int vehicleHandle);
                if (vehicleHandle == 0) return;
                var vehicle = NativeWorld.GetVehicleInstanceFromHandle(vehicleHandle);
                GET_CHAR_HEIGHT_ABOVE_GROUND(playerPedHandle, out float height);
                var totalSpeed = PlayerHelper.GetTotalSpeedVehicle(vehicle);
                var rev = vehicle.EngineRevs;
                var isCarInAir = IS_CAR_IN_AIR_PROPER(vehicleHandle);

                if (isCarInAir && height > 0.6)
                {
                    float heightFactor = Math.Min(height * 0.75f, 10);

                    targetFOV = FOVInCarInAir + (totalSpeed / 175.0f) * heightFactor;
                    targetFOV = CommonHelpers.Clamp(targetFOV, MinFOV, FOVInCarInAirMax);
                }
                else if (PlayerHelper.IsPlayerSkidding())
                {
                    targetFOV = FOVSkidding + totalSpeed / 250.0f;
                    targetFOV = CommonHelpers.Clamp(targetFOV, MinFOV, MaxFOV);
                }
                else if (isInOrNearCombat)
                {
                    targetFOV = FOVInCombatInCar + totalSpeed / 175.0f + rev * FOVInCarRev;
                    targetFOV = CommonHelpers.Clamp(targetFOV, MinFOV, FOVInCombatInCarMax);
                }
                else
                {
                    targetFOV = FOVInCar + totalSpeed / 225.0f + rev * FOVInCarRev;
                    targetFOV = CommonHelpers.Clamp(targetFOV, MinFOV, MaxFOV);
                }

                // Halve FOV if hood camera (or if they've positioned it super close for some reason)
                NativeCamera cam = NativeCamera.GetGameCam();
                float distance = Vector3.Distance(cam.Position, Main.PlayerPos);

                // Drive-by tweaks
                if (WeaponHelpers.IsTryingToDriveBy() && distance > 2.5f)
                {
                    targetFOV *= FOVDriveByMultiplier;
                    lerpSpeed = 0.05f;
                    targetFOV = CommonHelpers.Clamp(targetFOV, MinFOV, MaxFOV);
                }
                else lerpSpeed = defaultLerpSpeed;

                // Rest of hood cam tweaks
                if (distance < 2.5f)
                {
                    targetFOV *= 0.9F;
                    lerpSpeed *= 10f;
                }
                else lerpSpeed = defaultLerpSpeed;

                // Reduce FOV when car & cam aren't aligned enough
                float headingDifference = VehicleHelpers.GetCameraCarMisalignment(vehicleHandle, cam);

                if (headingDifference > 120.0f)
                    targetFOV *= 0.9f;
            }
        }
        private static void HandleInteriorFOV(bool isInOrNearCombat)
        {
            targetFOV = isInOrNearCombat ? FOVInCombatInterior : FOVInterior;
        }
        private static void HandleCinematicFOV()
        {
            targetFOV = FOVCinematic;
        }
        private static void HandleKillcamFOV()
        {
            targetFOV = FOVKillcam;
        }
        private static void HandleDefaultFOV()
        {
            targetFOV = 1.0f;
        }
        private static void HandleOnFootFOV(bool isInOrNearCombat, uint currentWeapSlot)
        {
            if (isInOrNearCombat)
                targetFOV = FOVInCombatOnFoot;
            else if (currentWeapSlot == 0 || currentWeapSlot == 1)
                targetFOV = FOVOnFoot;
            else
                targetFOV = FOVOnFootGunEquipped;
        }
    }
}

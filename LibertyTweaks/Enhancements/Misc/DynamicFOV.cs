using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using System;
using IVSDKDotNet.Attributes;
using LibertyTweaks.Enhancements.Combat;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    [ShowStaticFieldsInInspector]

    internal class DynamicFOV
    {
        private static bool enable;
        private static float targetFOV = 1.1f;
        private static float currentFOV = 1.0f;
        private static readonly float lerpSpeed = 0.01f;

        public static float maxFOV;
        public static float FOVInCombatOnFoot;
        public static float FOVInCombatInCar;
        public static float FOVInCombatInterior;
        public static float FOVOnFoot;
        public static float FOVOnFootGunEquipped;
        public static float FOVInCar;
        public static float FOVInBoat;
        public static float FOVInHeli;
        public static float FOVInterior; 
        public static float FOVKillcam; 
        public static float FOVCinematic; 

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Dynamic Field of View", "Enable", true);

            maxFOV = settings.GetFloat("Extensive Settings", "Maximum FOV Increase", 1.35f);

            FOVInCombatOnFoot = settings.GetFloat("Extensive Settings", "In Combat - On Foot", 1.035f);
            FOVInCombatInCar = settings.GetFloat("Extensive Settings", "In Combat - In Car", 1.045f);
            FOVInCombatInterior = settings.GetFloat("Extensive Settings", "In Combat - In Interior", 1.01f);
            FOVInCar = settings.GetFloat("Extensive Settings", "In Car", 1.03f);
            FOVInBoat = settings.GetFloat("Extensive Settings", "In Boat", 1.11f);
            FOVInHeli = settings.GetFloat("Extensive Settings", "In Heli", 1.0f);
            FOVInterior = settings.GetFloat("Extensive Settings", "In Interior", 1.0f);
            FOVKillcam = settings.GetFloat("Extensive Settings", "In Killcam", 0.9f);
            FOVCinematic = settings.GetFloat("Extensive Settings", "Cinematic Cam", 0.6f);
            FOVOnFoot = settings.GetFloat("Extensive Settings", "On Foot", 1.0f);
            FOVOnFootGunEquipped = settings.GetFloat("Extensive Settings", "On Foot - With Gun", 1.025f);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            IVCam cam = IVCamera.TheFinalCam;
            if (Main.PlayerPed == null || cam == null) return;
            int playerPedHandle = Main.PlayerPed.GetHandle();
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
                && NativeControls.IsGameKeyPressed(0, GameKey.Aim)
                || cutsc == false)
                HandleDefaultFOV();
            else if (cinematicCam != 0)
                HandleCinematicFOV();
            else if (Killcam.enable == true && Killcam.cam.IsActive)
                HandleKillcamFOV();
            else if (isInVehicle)
                HandleVehicleFOV(playerPedHandle, PlayerChecks.IsPlayerInOrNearCombat());
            else if (IS_INTERIOR_SCENE())
                HandleInteriorFOV(PlayerChecks.IsPlayerInOrNearCombat());
            else
                HandleOnFootFOV(PlayerChecks.IsPlayerInOrNearCombat(), currentWeapSlot);

            currentFOV = Lerp(currentFOV, targetFOV, lerpSpeed);
            cam.FOV *= currentFOV;
        }
        
        private static void HandleVehicleFOV(int playerPedHandle, bool isInOrNearCombat)
        {
            if (IS_CHAR_IN_ANY_HELI(playerPedHandle))
                targetFOV = FOVInHeli;
            else
            {
                GET_CAR_CHAR_IS_USING(playerPedHandle, out int vehicleHandle);
                if (vehicleHandle == 0) return;

                GET_CAR_SPEED(vehicleHandle, out float vehicleSpeed);

                maxFOV = IS_CHAR_IN_ANY_BOAT(playerPedHandle) ? FOVInBoat : 1.35f;

                if (vehicleSpeed < 2)
                    targetFOV = FOVInCar;

                if (isInOrNearCombat)
                    targetFOV = FOVInCombatInCar + vehicleSpeed / 100.0f;
                else
                    targetFOV = FOVInCar + vehicleSpeed / 200.0f;

                targetFOV = Math.Min(targetFOV, maxFOV);
            }
        }
        private static void HandleInteriorFOV(bool isInOrNearCombat)
        {
            if (isInOrNearCombat)
                targetFOV = FOVInCombatInterior;
            else
                targetFOV = FOVInterior;
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
        private static float Lerp(float start, float end, float amount)
        {
            return start + (end - start) * amount;
        }
    }
}

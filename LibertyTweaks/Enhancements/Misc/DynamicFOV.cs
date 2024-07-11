using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL;
using CCL.GTAIV;
using System;

namespace LibertyTweaks
{
    internal class DynamicFOV
    {
        private static bool enableDynamicFov;
        private static float targetFOV = 1.1f;
        private static float currentFOV = 1.0f;
        private static readonly float lerpSpeed = 0.01f;
        private static float maxFOV;

        private static float FOVInCombatOnFoot;
        private static float FOVInCombatInCar;
        private static float FOVInCar;
        private static float FOVInBoat;
        private static float FOVInHeli;
        private static float FOVOnFoot;

        public static void Init(SettingsFile settings)
        {
            enableDynamicFov = settings.GetBoolean("Dynamic Field of View", "Enable", true);

            maxFOV = settings.GetFloat("Extensive Settings", "Maximum FOV Increase", 1.35f);

            FOVInCombatOnFoot = settings.GetFloat("Extensive Settings", "In Combat - On Foot", 1.025f);
            FOVInCombatInCar = settings.GetFloat("Extensive Settings", "In Combat - In Car", 1.04f);
            FOVInCar = settings.GetFloat("Extensive Settings", "In Car", 1.03f);
            FOVInBoat = settings.GetFloat("Extensive Settings", "In Boat", 1.11f);
            FOVInHeli = settings.GetFloat("Extensive Settings", "In Heli", 1.0f);
            FOVOnFoot = settings.GetFloat("Extensive Settings", "On Foot", 1.0f);

            if (enableDynamicFov)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            IVCam cam = IVCamera.TheFinalCam;
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            if (playerPed == null || cam == null) return;

            int playerPedHandle = playerPed.GetHandle();
            GET_CURRENT_CHAR_WEAPON(playerPedHandle, out int currentWeapon);

            if (enableDynamicFov)
                ApplyDynamicFOV(playerPedHandle, cam, currentWeapon);
        }
        private static void ApplyDynamicFOV(int playerPedHandle, IVCam cam, int currentWeapon)
        {
            if (cam == null) return;

            PlayerChecks combatChecker = new PlayerChecks();
            bool isInOrNearCombat = combatChecker.IsPlayerInOrNearCombat();
            bool isInVehicle = IS_CHAR_IN_ANY_CAR(playerPedHandle);

            if (isInVehicle)
            {
                HandleVehicleFOV(playerPedHandle, isInOrNearCombat);
            }
            else
            {
                HandleOnFootFOV(isInOrNearCombat, currentWeapon);
            }

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
                if (vehicleHandle == 0) return; // Handle case where vehicleHandle is not valid

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

        private static void HandleOnFootFOV(bool isInOrNearCombat, int currentWeapon)
        {
            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {
                if (NativeControls.IsGameKeyPressed(0, GameKey.Aim))
                {
                    targetFOV = 1.0f;
                }
            }
            else if (isInOrNearCombat)
                targetFOV = FOVInCombatOnFoot;
            else
                targetFOV = FOVOnFoot;
        }

        private static float Lerp(float start, float end, float amount)
        {
            return start + (end - start) * amount;
        }
    }
}

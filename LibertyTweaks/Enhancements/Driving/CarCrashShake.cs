using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class CarCrashShake
    {
        private static bool enable;
        private static float cameraShakeAmount;
        private static float crashShakeIntensity = 0f;
        private static float crashShakeTimer = 0f;
        private const float CrashShakeDecayRate = 5f;

        private const float LowIntensityThreshold = 0.2f;
        private const float MediumIntensityThreshold = 0.5f;
        private const float HighIntensityThreshold = 0.8f;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicle Camera Adjustments", "Car Crash Shake", true);
            cameraShakeAmount = settings.GetFloat("Vehicle Camera Adjustments", "Car Crash Shake Multiplier", 0.25f);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (IS_PAUSE_MENU_ACTIVE())
                return;

            NativeCamera cam = NativeCamera.GetGameCam();
            if (Main.PlayerPed == null || cam == null) return;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            if (WeaponHelpers.HasCarBeenDamagedByAnyWeapon(IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle())))
            {
                CLEAR_CAR_LAST_WEAPON_DAMAGE(IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle()).GetHandle());
                return;
            }

            HandleCrashShake(cam, Main.CarCrashDamageAmountNormalized);
        }

        public static void HandleCrashShake(NativeCamera cam, float amount)
        {
            if (amount > 0)
            {
                crashShakeIntensity = Math.Min(amount * cameraShakeAmount, 1f);
                crashShakeTimer = 0.5f;
            }

            if (crashShakeTimer > 0f)
            {
                float shakeAmount = 0.1f;

                if (crashShakeIntensity >= HighIntensityThreshold)
                    shakeAmount = 0.3f;
                else if (crashShakeIntensity >= MediumIntensityThreshold)
                    shakeAmount = 0.2f;
                else if (crashShakeIntensity >= LowIntensityThreshold)
                    shakeAmount = 0.1f;

                CameraShake.ApplyCameraShake(crashShakeIntensity, cam, shakeAmount);
                crashShakeIntensity = CommonHelpers.Lerp(crashShakeIntensity, 0f, CrashShakeDecayRate * NativeGame.FrameTime);
                crashShakeTimer -= NativeGame.FrameTime;
            }
        }
    }
}

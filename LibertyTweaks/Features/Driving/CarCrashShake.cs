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
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            CarCrashShake.section = section;
            enable = settings.GetBoolean(section, "Camera - Car Crash Shake", false);
            cameraShakeAmount = settings.GetFloat(section, "Camera - Car Crash Shake Multiplier", 0.25f);

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
            {
                Reset();
                return;
            }

            if (WeaponHelpers.HasCarBeenDamagedByAnyWeapon(IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle())))
            {
                Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(0.1), "Main", () =>
                {
                    CLEAR_CAR_LAST_WEAPON_DAMAGE(IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle()).GetHandle());
                });
                return;
            }

            HandleCrashShake(cam, Main.CarCrashDamageNormalized);
        }

        private static void Reset()
        {
            crashShakeIntensity = 0f;
            crashShakeTimer = 0f;
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
                crashShakeIntensity = CommonHelpers.SmoothStep(crashShakeIntensity, 0f, CrashShakeDecayRate * NativeGame.FrameTime);
                crashShakeTimer -= NativeGame.FrameTime;
            }
        }
    }
}

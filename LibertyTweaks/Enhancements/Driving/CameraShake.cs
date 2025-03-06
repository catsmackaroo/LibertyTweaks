using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class CameraShake
    {
        private static bool enable;
        private static float shakeMultiplier;
        private static readonly Random random = new Random();
        private static Vector3 lastShakeOffset = Vector3.Zero;

        private const float MaxCarSpeed = 40f;
        private const float BaseShakeFactor = 0.05f;
        private const float MinEngineRev = 0.7f;
        private const float RpmPowerExponent = 4f;
        private const float MaxShakeIntensity = 0.05f;
        private const float DistanceFactorSpeed = 7f;
        private const float DistanceFactorRpm = 4f;
        private const float LerpFactor = 0.1f;
        private const float HelicopterLerpFactor = 0.005f;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicle Camera Adjustments", "Speed Shake", true);
            shakeMultiplier = settings.GetFloat("Vehicle Camera Adjustments", "Speed Shake Multiplier", 1.0f);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE()) return;

            NativeCamera cam = NativeCamera.GetGameCam();
            if (Main.PlayerPed == null || cam == null) return;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())) return;

            GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int vehicle);
            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

            HandleCarCamShake(vehicle, vehicleIV, cam);
        }

        private static void HandleCarCamShake(int vehicle, IVVehicle vehicleIV, NativeCamera cam)
        {
            GET_CAR_SPEED(vehicle, out float speed);
            float rev = vehicleIV.EngineRevs;
            float driveForce = vehicleIV.Handling.DriveForce;

            GET_CAR_COORDINATES(vehicle, out Vector3 carPos);
            float distance = Vector3.Distance(carPos, cam.Position);

            if (IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                distance *= 0.2f;
            }

            float speedIntensity = CalculateSpeedIntensity(speed, distance);
            float rpmIntensity = CalculateRpmIntensity(rev, distance);

            float combinedIntensity = (speedIntensity + rpmIntensity) / 2 * driveForce * 4f;
            combinedIntensity = Math.Min(combinedIntensity, MaxShakeIntensity);

            combinedIntensity *= shakeMultiplier;

            float lerpFactor = LerpFactor;
            if (IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                combinedIntensity *= 10;
                lerpFactor = HelicopterLerpFactor;
            }
            else
                lerpFactor = LerpFactor;

            ApplyCameraShake(combinedIntensity, cam, lerpFactor);
        }

        private static float CalculateSpeedIntensity(float speed, float distance)
        {
            float speedIntensity = BaseShakeFactor * (speed / MaxCarSpeed);
            speedIntensity = Math.Min(speedIntensity, 1f);
            return speedIntensity / distance * DistanceFactorSpeed;
        }

        private static float CalculateRpmIntensity(float rev, float distance)
        {
            float normalizedRpm = Math.Min(Math.Max(rev - MinEngineRev, 0f), 1f);
            float rpmIntensity = (float)Math.Pow(normalizedRpm, RpmPowerExponent);
            rpmIntensity = Math.Min(rpmIntensity, 1f);
            return rpmIntensity / distance * DistanceFactorRpm;
        }

        public static void ApplyCameraShake(float intensity, NativeCamera cam, float lerpFactor)
        {
            Vector3 targetShakeOffset = new Vector3(
                (float)(random.NextDouble() * 2 - 1) * intensity,  // left-right
                (float)(random.NextDouble() * 2 - 1) * intensity,  // up-down
                (float)(random.NextDouble() * 2 - 1) * intensity
            );

            lastShakeOffset = Vector3.Lerp(lastShakeOffset, targetShakeOffset, lerpFactor);
            cam.Position += lastShakeOffset;
        }
    }
}

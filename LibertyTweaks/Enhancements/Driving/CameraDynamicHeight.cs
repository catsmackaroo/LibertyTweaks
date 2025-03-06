using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class CameraDynamicHeight
    {
        private static bool enable;
        private static float multiplier;
        private static float heightMultiplier = 4.5f;
        private static Vector3 lastHeightOffset = Vector3.Zero;
        private const float maxCarSpeed = 40f;
        private const float baseTiltFactor = 0.05f;
        private const float lerpFactor = 0.1f;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicle Camera Adjustments", "Dynamic Camera Height", true);
            multiplier = settings.GetFloat("Vehicle Camera Adjustments", "Dynamic Camera Height Multiplier", 1.0f);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE()) return;

            NativeCamera cam = NativeCamera.GetGameCam();
            if (Main.PlayerPed == null || cam == null || !IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())) return;

            GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int vehicle);
            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            HandleCarCamHeight(vehicle, vehicleIV, cam);
        }

        private static void HandleCarCamHeight(int vehicle, IVVehicle vehicleIV, NativeCamera cam)
        {
            GET_CAR_SPEED(vehicle, out float speed);
            float speedIntensity = CalculateSpeedIntensity(speed);
            float combinedIntensity = speedIntensity * heightMultiplier;
            ApplyCameraHeight(combinedIntensity, cam, lerpFactor);
        }

        private static float CalculateSpeedIntensity(float speed)
        {
            float speedIntensity = baseTiltFactor * (speed / maxCarSpeed);
            speedIntensity = Math.Min(speedIntensity, 1f);
            return speedIntensity;
        }

        public static void ApplyCameraHeight(float intensity, NativeCamera cam, float lerpFactor)
        {
            Vector3 targetHeight = new Vector3(
                0,
                0,
                intensity
            );

            targetHeight *= multiplier;
            lastHeightOffset = Vector3.Lerp(lastHeightOffset, targetHeight, lerpFactor);
            cam.Position += lastHeightOffset;
        }
    }
}

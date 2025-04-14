using CCL.GTAIV;
using DocumentFormat.OpenXml.EMMA;
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
        private static float heightMultiplier = 3.5f;
        private static Vector3 lastHeightOffset = Vector3.Zero;
        private const float maxCarSpeed = 40f;
        private const float baseTiltFactor = 0.05f;
        private const float lerpFactor = 0.1f;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            CameraDynamicHeight.section = section;
            enable = settings.GetBoolean(section, "Camera - Dynamic Camera Height", false);
            multiplier = settings.GetFloat(section, "Camera - Dynamic Camera Height Multiplier", 1.0f);

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

            float distance = Vector3.Distance(cam.Position, Main.PlayerPos);
            if (distance < 2.5f)
                combinedIntensity = 0;

            // Vehicle cam changes (when player presses V)
            if (distance >= 8f)
                combinedIntensity *= 1.8f;
            else if (distance >= 5)
                combinedIntensity *= 1.4f;

            // Drive-by
            if (WeaponHelpers.IsTryingToDriveBy()
                && distance > 2.5f)
            {
                combinedIntensity = 0;
            }

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

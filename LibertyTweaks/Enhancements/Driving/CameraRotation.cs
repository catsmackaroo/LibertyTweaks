using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class CameraRotation
    {
        private static bool enable;
        private const float maxCarSpeed = 60f;
        private const float tiltIntensityFactor = 0.3f;
        private static float tiltMultiplier; 
        private const float maxRollClamp = 5f;
        private static float lastTiltAmount = 0f;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicle Camera Adjustments", "Tilt", true);

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

            GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int vehicle);
            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            GET_CAR_SPEED(vehicle, out float speed);
            if (IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                tiltMultiplier = 1.0f;
            }
            else
            {
                tiltMultiplier = 1.25f;
            }

            GET_CAR_ROLL(vehicle, out float roll);

            ApplyCameraRollTilt(roll, speed, cam);
        }

        private static void ApplyCameraRollTilt(float roll, float speed, NativeCamera cam)
        {
            float clampedRoll = CommonHelpers.Clamp(roll, -maxRollClamp, maxRollClamp);

            clampedRoll = -clampedRoll;

            float speedFactor = Math.Min(speed / maxCarSpeed, 1f); 
            float tiltAmount = clampedRoll * speedFactor * tiltIntensityFactor;

            // lerp just to make it a bit smoother
            tiltAmount = MathHelper.Lerp(lastTiltAmount, tiltAmount, 0.01f); 

            if (Math.Abs(tiltAmount) > 0.05f)
            {
                cam.Rotation = new Vector3(
                    cam.Rotation.X,
                    cam.Rotation.Y + tiltAmount * tiltMultiplier,
                    cam.Rotation.Z
                );
            }

            lastTiltAmount = tiltAmount;
        }
    }

    public static class MathHelper
    {
        public static float Lerp(float start, float end, float amount)
        {
            return start + (end - start) * amount;
        }
    }
}

using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class CameraTiltAndRotation
    {
        private static bool enable;

        private const float maxCarSpeed = 2f;

        private const float tiltIntensityFactor = 0.3f;
        private static float tiltMultiplier = 0f;
        private static float tiltCustomMultiplier;
        private static float tiltAmount;
        private const float maxTiltClamp = 4f;
        private static float lastTiltAmount = 0f;

        private const float rotationIntensityFactor = 0.3f;
        private static float rotationCustomMultiplier;
        private static float rotationAmount;
        private const float maxRotationClamp = 3f;
        private static float lastRotationAmount = 0f;

        private static float pitchAmount;
        private static float maxPitchClamp = 10f;
        private static float maxPitchHardCap = 15f;
        private static float minPitchClamp = -10f;
        private static float minPitchHardCap = -12f;

        private static float upDownRotationCustomMultiplier;

        private static NativeCamera cam;
        private static Vector3 speedVector;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            CameraTiltAndRotation.section = section;
            enable = settings.GetBoolean(section, "Camera - Tilt and Rotation", false);
            tiltCustomMultiplier = settings.GetFloat(section, "Camera - Tilt Multiplier", 1.0f);
            rotationCustomMultiplier = settings.GetFloat(section, "Camera - Left/Right Rotation Multiplier", 1.0f);
            upDownRotationCustomMultiplier = settings.GetFloat(section, "Camera - Up/Down Rotation Multiplier", 1.0f);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;
            if (!InitialChecks()) return;

            ApplyCameraRollTilt(cam);
            ApplyCameraRotation(cam);
            ApplyCameraPitch(cam);
        }
        private static bool InitialChecks()
        {
            if (IS_SCREEN_FADED_OUT()) return false;
            if (IS_PAUSE_MENU_ACTIVE()) return false;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())) return false;
            cam = NativeCamera.GetGameCam();
            if (Main.PlayerPed == null || cam == null 
                || Main.PlayerVehicle == null || Main.PlayerVehicle.GetHandle() == 0) return false;

            speedVector = Main.PlayerVehicle.GetSpeedVector(true);
            return true;
        }
        private static void DetermineTiltAmount()
        {
            if (IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
                tiltMultiplier = 1.2f;
            else if (IS_CHAR_ON_ANY_BIKE(Main.PlayerPed.GetHandle()))
                tiltMultiplier = 2f;
            else
                tiltMultiplier = 1.8f;

            GET_CAR_ROLL(Main.PlayerVehicle.GetHandle(), out float roll);

            float clampedRoll = CommonHelpers.Clamp(roll, -maxTiltClamp, maxTiltClamp);
            clampedRoll = -clampedRoll;

            float forwardSpeed = speedVector.Y;
            float speedFactor = Math.Min(forwardSpeed / maxCarSpeed, 1f);

            // Disable when using mouse
            if (NativeControls.MouseInput != Vector2.Zero || WeaponHelpers.IsTryingToDriveBy() && tiltAmount > 0)
                tiltAmount -= 0.1f;
            else if (WeaponHelpers.IsTryingToDriveBy() && tiltAmount < 0)
                tiltAmount += 0.1f;

            tiltAmount = clampedRoll * speedFactor * tiltIntensityFactor;
            tiltAmount *= tiltCustomMultiplier;
            tiltAmount = CommonHelpers.SmoothStep(lastTiltAmount, tiltAmount, 0.05f);
        }
        private static void DetermineRotateAmount()
        {
            GET_CAR_HEADING(Main.PlayerVehicle.GetHandle(), out float heading);

            float clampedRotation = CommonHelpers.Clamp(heading, -maxRotationClamp, maxRotationClamp);
            clampedRotation = -clampedRotation;

            float sideSpeed = speedVector.X;
            float forwardSpeed = speedVector.Y;

            rotationAmount = clampedRotation * (sideSpeed / maxCarSpeed) * rotationIntensityFactor;

            // This multiplication is to make the positive values more noticeable, since the IV camera isn't centered by default
            if (rotationAmount > 0)
                rotationAmount *= 1.1f;

            // Disable when using mouse
            if (NativeControls.MouseInput != Vector2.Zero || WeaponHelpers.IsTryingToDriveBy() && rotationAmount > 0)
                rotationAmount -= 0.1f;
            else if (WeaponHelpers.IsTryingToDriveBy() && rotationAmount < 0)
                rotationAmount += 0.1f;

            if (IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
                rotationAmount *= 0.5f;

            rotationAmount *= rotationCustomMultiplier;
            rotationAmount = CommonHelpers.SmoothStep(lastRotationAmount, rotationAmount, 0.05f);
        }

        private static void DeterminePitchAmount()
        {
            GET_CAR_PITCH(Main.PlayerVehicle.GetHandle(), out float pitch);
            pitch *= upDownRotationCustomMultiplier;
            if (WeaponHelpers.IsTryingToDriveBy() && pitch > 0)
                pitch -= 0.1f;

            if (WeaponHelpers.IsTryingToDriveBy() && pitch < 0)
                pitch += 0.1f;
            if (pitch >= maxPitchClamp)
                pitch -= 0.1f;

            if (pitch <= minPitchClamp)
                pitch += 0.1f;
            if (pitch >= maxPitchHardCap)
                pitch = maxPitchHardCap;

            if (pitch <= minPitchHardCap)
                pitch = minPitchHardCap;

            pitchAmount = CommonHelpers.SmoothStep(pitchAmount, pitch, 0.1f);
        }

        private static void ApplyCameraPitch(NativeCamera cam)
        {
            DeterminePitchAmount();

            cam.Rotation = new Vector3(
                    cam.Rotation.X + pitchAmount * 0.1f,
                    cam.Rotation.Y,
                    cam.Rotation.Z
                );
        }

        private static void ApplyCameraRollTilt(NativeCamera cam)
        {
            DetermineTiltAmount();

            cam.Rotation = new Vector3(
                    cam.Rotation.X,
                    cam.Rotation.Y + tiltAmount * tiltMultiplier,
                    cam.Rotation.Z
                );

            lastTiltAmount = tiltAmount;
        }

        private static void ApplyCameraRotation(NativeCamera cam)
        {
            DetermineRotateAmount();

            cam.Rotation = new Vector3(
                    cam.Rotation.X,
                    cam.Rotation.Y,
                    cam.Rotation.Z - rotationAmount
                );

            lastRotationAmount = rotationAmount;
        }
    }
}

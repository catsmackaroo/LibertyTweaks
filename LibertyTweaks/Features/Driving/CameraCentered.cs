using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class CameraCentering
    {
        private static bool enable;
        private static NativeCamera cam;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicle Camera Adjustments", "Center Camera", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;
            if (!InitialChecks()) return;

            AdjustCamera(cam);
        }

        private static bool InitialChecks()
        {
            if (IS_SCREEN_FADED_OUT()) return false;
            if (IS_PAUSE_MENU_ACTIVE()) return false;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())) return false;
            cam = NativeCamera.GetGameCam();
            if (Main.PlayerPed == null || cam == null
                || Main.PlayerVehicle == null || Main.PlayerVehicle.GetHandle() == 0) return false;
            return true;
        }

        private static void AdjustCamera(NativeCamera cam)
        {
            Vector3 vehiclePosition = Main.PlayerVehicle.Matrix.Pos;
            Vector3 vehicleRight = Main.PlayerVehicle.Matrix.Right;
            Vector3 cameraPosition = cam.Position;

            Vector3 desiredCameraPosition = vehiclePosition + vehicleRight * 3.0f;

            Vector3 difference = desiredCameraPosition - cameraPosition;

            if (difference.Length() > 0.01f)
            {
                cam.Position += difference * 0.1f;
            }
        }

    }
}

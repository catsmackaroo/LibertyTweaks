using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL;

// Credits: catsmackaroo, ClonkAndre

namespace LibertyTweaks
{
    internal class TweakableFOV
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Tweakable FOV", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick(float fovMulti)
        {
            if (!enable)
                return;

            IVCam cam = IVCamera.TheFinalCam;
            uint playerId = GET_PLAYER_ID();

            if (cam != null)
                cam.FOV = cam.FOV * fovMulti;
        }
    }
}

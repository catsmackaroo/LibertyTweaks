using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class DisableHUDOnCinematic
    {
        private static bool enable;
        private static bool hudWasOn = false;
        private static uint originalRadarMode = 0;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Cinematics Disable HUD", true);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
                return;

            bool HudIsOn = IVMenuManager.HudOn;

            GET_CINEMATIC_CAM(out int cinematicCam);

            if (cinematicCam != 0)
            {
                if (HudIsOn)
                {
                    originalRadarMode = IVMenuManager.RadarMode;
                    IVMenuManager.RadarMode = 0;
                    IVMenuManager.HudOn = false;
                    hudWasOn = true;
                }
            }
            else if (hudWasOn)
            {
                IVMenuManager.RadarMode = originalRadarMode;
                IVMenuManager.HudOn = true;
                hudWasOn = false;
            }
        }
    }
}

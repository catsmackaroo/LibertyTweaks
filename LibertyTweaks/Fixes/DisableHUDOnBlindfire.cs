using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;

// Credits: ServalEd

namespace LibertyTweaks
{
    internal class DisableHUDOnBlindfire
    {
        private static bool enable;
        
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "BlindFire Disable HUD", true);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            bool HudIsOn = IVMenuManager.HudOn;
            if (enable)
            {
                if (PlayerHelper.IsInBlindCover()&& HudIsOn)
                    DISPLAY_HUD(false);
                else if (!PlayerHelper.IsInBlindCover() && HudIsOn)
                    DISPLAY_HUD(true);
            }
        }
    }
}

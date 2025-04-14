using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: ServalEd

namespace LibertyTweaks
{
    internal class DisableHUDOnBlindfire
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DisableHUDOnBlindfire.section = section;
            enable = settings.GetBoolean(section, "BlindFire Disable HUD", false);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
                return;

            bool HudIsOn = IVMenuManager.HudOn;

            if (WeaponHelpers.IsPlayerBlindfiring()&& HudIsOn)
                DISPLAY_HUD(false);
            else if (!WeaponHelpers.IsPlayerBlindfiring() && HudIsOn)
                DISPLAY_HUD(true);
        }
    }
}

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class DisableCrosshairWithNoHUD
    {
        private static bool enable;
        private static bool hudState = true;
        private static bool hudWasDisabledBeforeAiming;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Disable Crosshair When No HUD", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            bool currentHudState = IVMenuManager.HudOn;
            if (currentHudState != hudState)
            {
                DISPLAY_HUD(currentHudState);
                hudState = currentHudState;
            }

            // Check if the player is aiming with a sniper since aiming with it doesn't show sniper scope if hud is off
            if (WeaponHelpers.GetWeaponInfo().WeaponFlags.FirstPerson && PlayerHelper.IsAiming())
            {
                if (!hudState)
                {
                    DISPLAY_HUD(true);
                    hudWasDisabledBeforeAiming = true;
                }
            }
            else if (hudWasDisabledBeforeAiming)
            {
                DISPLAY_HUD(false);
                hudWasDisabledBeforeAiming = false;
            }
        }
    }
}

using IVSDKDotNet;

namespace LibertyTweaks
{
    internal class DynamicCrosshair
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Dynamic Crosshair", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;
            var currentWeaponTargetSetting = IVMenuManager.GetSetting(IVSDKDotNet.Enums.eSettings.SETTING_WEAPON_TARGET);

            if (PlayerHelper.IsPlayerAimingAtAnyChar() && currentWeaponTargetSetting == 1)
            {
                IVMenuManager.SetSetting(IVSDKDotNet.Enums.eSettings.SETTING_WEAPON_TARGET, 0);
            }
            else if (!PlayerHelper.IsPlayerAimingAtAnyChar() && currentWeaponTargetSetting == 0)
            {
                IVMenuManager.SetSetting(IVSDKDotNet.Enums.eSettings.SETTING_WEAPON_TARGET, 1);
            }
        }
    }
}

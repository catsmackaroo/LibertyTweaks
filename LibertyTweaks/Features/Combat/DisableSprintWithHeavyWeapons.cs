using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class DisableSprintWithHeavyWeapons
    {
        private static bool enable;
        private static bool toggled;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DisableSprintWithHeavyWeapons.section = section;
            enable = settings.GetBoolean(section, "Disable Sprint With Heavy Weapons", false);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE() || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            IVWeaponInfo weapon = WeaponHelpers.GetCurrentWeaponInfo();

            if (weapon.WeaponFlags.Heavy == true)
            {
                DISABLE_PLAYER_SPRINT(Main.PlayerIndex, true);
                toggled = true;
            }
            else if (toggled == true)
            {
                DISABLE_PLAYER_SPRINT(Main.PlayerIndex, false);
                toggled = false;
            }
        }
    }
}

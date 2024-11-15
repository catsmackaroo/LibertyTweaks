using CCL.GTAIV;
using IVSDKDotNet;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class HolsterWeapons
    {
        private static int lastWeapon;
        private static bool enableFix;
        public static Keys holsterKey;

        public static void Init(SettingsFile Settings)
        {
            enableFix = Settings.GetBoolean("Weapon Holstering", "Enable", true);
            holsterKey = Settings.GetKey("Weapon Holstering", "Key", Keys.H);

            if (enableFix)
                Main.Log("script initialized...");
        }

        public static void Process() 
        {
            if (!enableFix)
                return;

            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);

            if (IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            if (IS_PLAYER_PLAYING(Main.PlayerIndex))
            {
                if (currentWeapon != 0)
                {
                    GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out lastWeapon);
                    GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), 0, 1, true);
                }
                else
                {
                    GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), lastWeapon, 1, true);
                }

            }
        }
    }
}

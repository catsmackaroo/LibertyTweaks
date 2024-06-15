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

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            uint playerId = GET_PLAYER_ID();
            GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out int currentWeapon);

            if (IS_CHAR_IN_ANY_CAR(playerPed.GetHandle()))
                return;

            if (IS_PLAYER_PLAYING((int)playerId))
            {
                if (currentWeapon != 0)
                {
                    GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out lastWeapon);
                    GIVE_DELAYED_WEAPON_TO_CHAR(playerPed.GetHandle(), 0, 1, true);
                }
                else
                {
                    GIVE_DELAYED_WEAPON_TO_CHAR(playerPed.GetHandle(), lastWeapon, 1, true);
                }

            }
        }
    }
}

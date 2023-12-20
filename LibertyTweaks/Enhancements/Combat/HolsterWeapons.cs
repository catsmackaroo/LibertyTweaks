using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class HolsterWeapons
    {
        private static int playerId;
        private static uint currentWeapon;
        private static uint lastWeapon;
        private static bool enableFix;

        public static void Init(SettingsFile Settings)
        {
            enableFix = Settings.GetBoolean("Weapon Holstering", "Enable", true);
        }

        public static void Process() 
        {
            if (!enableFix)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            playerId = IVPedExtensions.GetHandle(playerPed);
            GET_CURRENT_CHAR_WEAPON(playerId, out currentWeapon);

            if (!IS_CHAR_IN_ANY_CAR(playerId))
            {
                if (currentWeapon != 0)
                {
                    GET_CURRENT_CHAR_WEAPON(playerId, out lastWeapon);
                    GIVE_DELAYED_WEAPON_TO_CHAR(playerId, 0, 1, true);
                }
                else
                { 
                    GIVE_DELAYED_WEAPON_TO_CHAR(playerId, (int)lastWeapon, 1, true);
                }
            }
        }
    }
}

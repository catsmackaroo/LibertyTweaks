using System;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks.HolsterWeapons
{
    internal class HolsterWeapons
    {
        private static int playerId;
        private static uint currentWeapon;
        private static uint lastWeapon;
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "Weapon Holstering", true);
        }

        public static void Process() 
        {
            if (!enableFix)
                return;

            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed());
            playerId = CPedExtensions.GetHandle(playerPed);
            GET_CURRENT_CHAR_WEAPON(playerId, out currentWeapon);

            if (currentWeapon != 0)
            {
                GET_CURRENT_CHAR_WEAPON(playerId, out lastWeapon);
                GIVE_DELAYED_WEAPON_TO_CHAR(playerId, 0, 1, true);
            }
            else 
            {
                if (lastWeapon == 15) 
                {
                    GIVE_DELAYED_WEAPON_TO_CHAR(playerId, 6, 1, true);
                }
                //SET_CURRENT_CHAR_WEAPON(playerId, lastWeapon, true);
                GIVE_DELAYED_WEAPON_TO_CHAR(playerId, (int)lastWeapon, 1, true);
            }
        }
    }
}

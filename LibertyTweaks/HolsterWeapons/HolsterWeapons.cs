using System;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo
// todo: Disable when in car

namespace LibertyTweaks
{
    internal class HolsterWeapons
    {
        private static int playerId;
        private static uint currentWeapon;
        private static uint lastWeapon;
        private static bool enableFix;
        private static uint lastWeaponSlot;

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
            lastWeaponSlot = CWeaponInfo.GetWeaponInfo(lastWeapon).Slot;

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

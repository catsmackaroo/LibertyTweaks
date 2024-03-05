using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Numerics;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class RealisticReloading
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Realistic Reloading", "Enable", true);
        }

        public static void Tick()
        {
            if (!enable)
                return;

            int playerId;
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            playerId = IVPedExtensions.GetHandle(playerPed);

            if (NativeControls.IsGameKeyPressed(0, GameKey.Reload))
            {
                // Get current weapon
                GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out uint currentWeapon);

                // Get total ammo for weapon
                GET_AMMO_IN_CHAR_WEAPON(playerPed.GetHandle(), currentWeapon, out uint weaponAmmo);

                // Get ammo in current clip
                GET_AMMO_IN_CLIP(playerPed.GetHandle(), currentWeapon, out uint clipAmmo);

                // Get max ammo that can be in weapon clip
                GET_MAX_AMMO_IN_CLIP(playerPed.GetHandle(), currentWeapon, out uint clipAmmoMax);

                if (clipAmmo < clipAmmoMax && weaponAmmo - clipAmmo > 0)
                {
                    if (currentWeapon == (int)eWeaponType.WEAPON_SHOTGUN || currentWeapon == (int)eWeaponType.WEAPON_BARETTA
                        || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_11 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_10
                        || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_2 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_6)
                    {
                    }
                    else
                    {
                        SET_AMMO_IN_CLIP(playerPed.GetHandle(), (int)currentWeapon, 0);
                    }
                }
            }
        }
    }
}

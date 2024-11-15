using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class RealisticReloading
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Realistic Reloading", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.Reload))
            {
                GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);

                GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), currentWeapon, out int weaponAmmo);

                GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, out int clipAmmo);

                GET_MAX_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, out int clipAmmoMax);

                if (clipAmmo == 0)
                    return;

                if (clipAmmo < clipAmmoMax && weaponAmmo - clipAmmo > 0)
                {
                    if (currentWeapon == (int)eWeaponType.WEAPON_SHOTGUN || currentWeapon == (int)eWeaponType.WEAPON_BARETTA
                        || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_11 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_10
                        || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_2 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_6)
                    {
                        return;
                    }
                    else if (!IS_MOUSE_BUTTON_PRESSED(1))
                        SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), (int)currentWeapon, 0);
                }
            }
        }
    }
}

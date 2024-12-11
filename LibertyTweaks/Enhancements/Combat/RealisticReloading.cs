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
                if (WeaponHelpers.IsReloading())
                {
                    int currentWeapon = WeaponHelpers.GetWeaponType();
                    GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, out int clipAmmo);
                    if (clipAmmo == 0)
                        return;

                    if (currentWeapon == (int)eWeaponType.WEAPON_SHOTGUN || currentWeapon == (int)eWeaponType.WEAPON_BARETTA
                        || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_11 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_10
                        || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_2 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_6)
                        return;

                    if (!IS_MOUSE_BUTTON_PRESSED(1))
                        SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), (int)currentWeapon, 0);
                }
            }
        }
    }
}

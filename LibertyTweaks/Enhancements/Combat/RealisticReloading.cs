using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class RealisticReloading
    {
        public static bool enable;
        public static bool enableQuickReloadMechanic;
        public static bool enableWeaponMagazineInteraction;
        private static bool isReloadingStarted;
        private static readonly HashSet<int> ExcludedWeapons = new HashSet<int>
        {
            (int)eWeaponType.WEAPON_SHOTGUN,
            (int)eWeaponType.WEAPON_BARETTA,
            (int)eWeaponType.WEAPON_EPISODIC_11,
            (int)eWeaponType.WEAPON_EPISODIC_10,
            (int)eWeaponType.WEAPON_EPISODIC_2,
            (int)eWeaponType.WEAPON_EPISODIC_6
        };

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Realistic Reloading", "Enable", true);
            enableQuickReloadMechanic = settings.GetBoolean("Realistic Reloading", "Enable Quick Reload Mechanic", true);
            enableWeaponMagazineInteraction = settings.GetBoolean("Realistic Reloading", "Quick Reload - Dynamic Disposable Magazines", true);

            if (enable)
                Main.Log("script initialized...");

            if (!WeaponMagazines.enable)
                enableWeaponMagazineInteraction = false;
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (WeaponHelpers.IsReloading())
            {
                if (!isReloadingStarted)
                {
                    isReloadingStarted = true;

                    // Fast reload for empty weapons always
                    GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetWeaponType(), out int ammoInClip);
                    if (ammoInClip == 0)
                    {
                        var animGroup = WeaponHelpers.GetReloadingAnimGroup();
                        FastReloadAndDispose(animGroup);
                    }

                    // Fast reload when shift is held
                    if (WeaponHelpers.CanReload())
                    {
                        var animGroup = WeaponHelpers.GetReloadingAnimGroup();

                        if (NativeControls.IsGameKeyPressed(0, GameKey.Sprint))
                            FastReloadAndDispose(animGroup);
                        else
                            Reset(animGroup);
                    }
                }
            }
            else
            {
                isReloadingStarted = false;
            }
        }

        private static void FastReloadAndDispose(string animGroup)
        {
            if (enableWeaponMagazineInteraction)
                WeaponMagazines.canDispose = true;

            if (enableQuickReloadMechanic)
            {
                if (PlayerHelper.isPlayerDucking)
                    SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "reload_crouch", 1.5f);
                else
                {
                    SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "reload", 1.5f);
                    SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "p_load", 1.5f);
                }
            }

            var currentWeapon = WeaponHelpers.GetWeaponType();
            if (ExcludedWeapons.Contains(currentWeapon))
                return;

            SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, 0);
        }

        private static void Reset(string animGroup)
        {
            if (enableWeaponMagazineInteraction)
                WeaponMagazines.canDispose = false;

            if (PlayerHelper.isPlayerDucking)
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "reload_crouch", 1.0f);
            else
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "reload", 1.0f);
        }
    }
}

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
        public static bool canQuickReload;
        private static bool hasReleasedReload = false;

        private static bool shouldReload = false;

        private static readonly HashSet<int> ExcludedWeapons = new HashSet<int>
        {
            (int)eWeaponType.WEAPON_SHOTGUN,
            (int)eWeaponType.WEAPON_BARETTA,
            (int)eWeaponType.WEAPON_EPISODIC_11,
            (int)eWeaponType.WEAPON_EPISODIC_10,
            (int)eWeaponType.WEAPON_EPISODIC_2,
            (int)eWeaponType.WEAPON_EPISODIC_6
        };
        private static string animGroup;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            RealisticReloading.section = section;
            enable = settings.GetBoolean(section, "Reload Depletes Mag", false);
            enableQuickReloadMechanic = settings.GetBoolean(section, "Reload Depletes Mag - Quick Reload System", false);

            if (enable)
            {
                Main.Log("script initialized...");

                if (enableQuickReloadMechanic)
                    Main.Log("Quick Reload system enabled...");
            }

            if (!WeaponMagazines.enable)
                enableWeaponMagazineInteraction = false;
            else
                enableWeaponMagazineInteraction = true;
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (WeaponHelpers.IsReloading() && shouldReload == true)
            {
                animGroup = WeaponHelpers.GetReloadingAnimGroup();
                if (!NativeControls.IsGameKeyPressed(0, GameKey.Reload))
                    hasReleasedReload = true;

                if (enableQuickReloadMechanic 
                    && animGroup != null
                    && hasReleasedReload)
                    HandleFastReload(animGroup);
                else if (!enableQuickReloadMechanic)
                    PerformBasicRealReload();
            }

            if (!WeaponHelpers.IsReloading())
            {
                if (!canQuickReload && animGroup != null)
                    Reset(animGroup);

                shouldReload = true;
                canQuickReload = true;
                hasReleasedReload = false;
            }
        }
        private static void HandleFastReload(string animGroup)
        {
            if (!canQuickReload)
                return;

            GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out int ammoInClip);

            if (NativeControls.IsGameKeyPressed(0, GameKey.Reload) || ammoInClip == 0)
                PerformQuickRealReload(animGroup, ammoInClip);
        }

        private static void PerformQuickRealReload(string animGroup, int ammoInClip)
        {
            if (!canQuickReload)
                return;

            var currentWeapon = WeaponHelpers.GetCurrentWeaponType();
            if (ExcludedWeapons.Contains(currentWeapon))
                return;

            if (WeaponHelpers.CanReload())
            {
                if (ammoInClip > 0)
                {
                    SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, 0);
                    WeaponMagazines.canDispose = true;
                }
            }

            if (PlayerHelper.isPlayerDucking)
            {
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "reload_crouch", 2f);
            }
            else
            {
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "reload", 2f);
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), animGroup, "p_load", 2f);
            }

            canQuickReload = false;
            shouldReload = false;
        }

        private static void PerformBasicRealReload()
        {
            var currentWeapon = WeaponHelpers.GetCurrentWeaponType();
            if (ExcludedWeapons.Contains(currentWeapon))
                return;

            GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out int ammoInClip);

            if (WeaponHelpers.CanReload())
            {
                if (ammoInClip > 0)
                {
                    SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, 0);
                    WeaponMagazines.canDispose = true;
                    shouldReload = false;
                }
            }
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

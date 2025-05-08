using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCL.GTAIV;
using DocumentFormat.OpenXml.Presentation;

namespace LibertyTweaks
{
    internal class FixedShotgunReload
    {
        private static bool enable;

        private static float shellGrabStartTime = 0.2577f;
        private static float shellGrabEndTime = 0.3f;

        private static float shellResetTime = 0.1f;
        private static float shellResetTime2 = 0.25f;

        private static float secondShellGrabStartTime = 0.5055f;
        private static float secondShellGrabEndTime = 1f;

        private static bool ammoAdded;
        private static bool ammo2Added;

        private static bool quickEndedReload = false;
        private static bool isRestarting = false;
        private static bool was1ShellReload = false;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixed Shotgun Reload", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (!InitialChecks())
                return;

            GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out var ammoInClip);
            GET_MAX_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out var maxAmmoInClip);
            var isAnyShotgunReloading = WeaponHelpers.IsShotgunReloading()
                || WeaponHelpers.IsBarettaReloading()
                || WeaponHelpers.IsAA12Reloading()
                || WeaponHelpers.IsSawnReloading();

            if (isAnyShotgunReloading)
            {
                PrepResetAnimForRegularReload(ref ammoInClip, ref maxAmmoInClip);
                RegularReload();
                HandleShotgunAmmo(ref ammoInClip, ref maxAmmoInClip);
                QuickEndReload();
                OneShellReload(ref ammoInClip, ref maxAmmoInClip);
            }
            else if (ammoInClip == maxAmmoInClip - 1)
            {
                was1ShellReload = true;
            }
            else
            {
                ammoAdded = false;
                ammo2Added = false;
                quickEndedReload = false;
                was1ShellReload = false;
            }

            IVGame.ShowSubtitleMessage(isRestarting.ToString());
        }

        private static void HandleShotgunAmmo(ref int ammoInClip, ref int maxAmmoInClip)
        {
            if (ammoInClip < maxAmmoInClip)
            {
                var currentReloadAnimTime = WeaponHelpers.GetShotgunReloadAnimTime();
                if (currentReloadAnimTime.InRange(shellGrabStartTime, shellGrabEndTime))
                {
                    if (!ammoAdded)
                    {
                        GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out var ammoInWeapon);

                        if (!PlayerHasEnoughAmmo(ref ammoInClip, ref maxAmmoInClip, ref ammoInWeapon))
                            return;

                        SET_CHAR_AMMO(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), ammoInWeapon - 1);
                        SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), ammoInClip + 1);
                        ammoAdded = true;
                        ammo2Added = false;
                    }
                }
                else if (currentReloadAnimTime.InRange(secondShellGrabStartTime, secondShellGrabEndTime)
                    && !ammo2Added
                    && !quickEndedReload)
                {
                    if (ammoInClip >= maxAmmoInClip - 1)
                    {
                        GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out var ammoInWeapon);

                        if (!PlayerHasEnoughAmmo(ref ammoInClip, ref maxAmmoInClip, ref ammoInWeapon))
                            return;

                        SET_CHAR_AMMO(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), ammoInWeapon - 1);
                        SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), ammoInClip + 1);
                        IVGame.ShowSubtitleMessage("Added ammo2");
                        ammo2Added = true;
                    }
                }
            }
        }
        private static bool PlayerHasEnoughAmmo(ref int ammoInClip, ref int maxAmmoInClip, ref int ammoInWeapon)
        {
            if (ammoInWeapon - ammoInClip == 0)
                return false;

            return true;
        }
        private static void PrepResetAnimForRegularReload(ref int ammoInClip, ref int maxAmmoInClip)
        {
            var currentReloadAnimTime = WeaponHelpers.GetShotgunReloadAnimTime();
            if (currentReloadAnimTime.InRange(shellGrabEndTime, secondShellGrabStartTime))
            {
                if (ammoInClip <= maxAmmoInClip - 2)
                {
                    GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out var ammoInWeapon);
                    if (!PlayerHasEnoughAmmo(ref ammoInClip, ref maxAmmoInClip, ref ammoInWeapon))
                        return;

                    var animController = Main.PlayerPed.GetAnimationController();
                    var anim = WeaponHelpers.GetCurrentPlayerWeaponAnim();
                    SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), anim.animGroup, anim.animName, -1.0f);
                    isRestarting = true;
                }
            }
        }
        private static void RegularReload()
        {
            var currentReloadAnimTime = WeaponHelpers.GetShotgunReloadAnimTime();
            if (currentReloadAnimTime.InRange(shellResetTime, shellResetTime2)
                && isRestarting)
            {
                var animController = Main.PlayerPed.GetAnimationController();
                var anim = WeaponHelpers.GetCurrentPlayerWeaponAnim();
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), anim.animGroup, anim.animName, 1.0f);
                ammoAdded = false;
                isRestarting = false;
            }
        }
        private static void OneShellReload(ref int ammoInClip, ref int maxAmmoInClip)
        {
            var currentReloadAnimTime = WeaponHelpers.GetShotgunReloadAnimTime();

            if (!was1ShellReload)
                return;

            GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out var ammoInWeapon);
            if (!PlayerHasEnoughAmmo(ref ammoInClip, ref maxAmmoInClip, ref ammoInWeapon))
                return;

            if (currentReloadAnimTime.InRange(shellGrabStartTime, shellGrabEndTime))
            {
                var animController = Main.PlayerPed.GetAnimationController();
                var anim = WeaponHelpers.GetCurrentPlayerWeaponAnim();
                SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), anim.animGroup, anim.animName, secondShellGrabStartTime);
                was1ShellReload = false;
            }
            else
            {
                var animController = Main.PlayerPed.GetAnimationController();
                var anim = WeaponHelpers.GetCurrentPlayerWeaponAnim();
            }
        }


        private static void QuickEndReload()
        {
            var currentReloadAnimTime = WeaponHelpers.GetShotgunReloadAnimTime();
            if (NativeControls.IsGameKeyPressed(0, GameKey.Attack) && currentReloadAnimTime < secondShellGrabEndTime)
            {
                if (currentReloadAnimTime.InRange(shellGrabStartTime, shellGrabEndTime))
                {
                    var animController = Main.PlayerPed.GetAnimationController();
                    var anim = WeaponHelpers.GetCurrentPlayerWeaponAnim();
                    SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), anim.animGroup, anim.animName, secondShellGrabStartTime);
                    quickEndedReload = true;
                }
            }
        }
        private static bool InitialChecks()
        {
            // Add meaningful checks here if needed
            return true;
        }
    }
}

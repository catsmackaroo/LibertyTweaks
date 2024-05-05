using System;
using System.Collections.Generic;
using System.Linq;

using CCL.GTAIV;
using CCL.GTAIV.AnimationController;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre

namespace LibertyTweaks
{
    internal class WeaponMagazines
    {

        private static bool enableFix;
        private static List<string> disableForWeapons;

        private static float currentReloadAnimTime;
        private static bool isAnyGunReloadingAnimPlaying;
        private static int magObj1, magObj2;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Gun Magazines", "Enable", true);
            disableForWeapons = settings.GetValue("Gun Magazines", "DisabledWeaponTypes", "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static void CheckMagazineObjects()
        {
            if (!enableFix)
                return;

            if (magObj1 != 0)
            {
                if (DOES_OBJECT_EXIST(magObj1))
                {
                    if (IS_OBJECT_ATTACHED(magObj1))
                    {
                        DETACH_OBJECT(magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                    }
                }
                else
                {
                    magObj1 = 0;
                }
            }
            if (magObj2 != 0)
            {
                if (DOES_OBJECT_EXIST(magObj2))
                {
                    if (IS_OBJECT_ATTACHED(magObj2))
                    {
                        DETACH_OBJECT(magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                    }
                }
                else
                {
                    magObj2 = 0;
                }
            }
        }

        // Clonk: Quite alot of duplicated code in those methods, maybe make the animation time check code universal
        private static void ProcessHandgunReloading(IVPed playerPed, int playerPedHandle, PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enableFix)
                return;

            // Handgun / Deagle
            bool isHandgunReloading = animController.IsPlaying("gun@handgun", isPlayerDucking ? "reload_crouch" : "reload");
            bool isDeagleReloading = animController.IsPlaying("gun@deagle", isPlayerDucking ? "reload_crouch" : "reload");

            if (isHandgunReloading || isDeagleReloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isHandgunReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@handgun", isPlayerDucking ? "reload_crouch" : "reload");
                else
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@deagle", isPlayerDucking ? "reload_crouch" : "reload");

                if (currentReloadAnimTime.InRange(0.15f, 0.18f)) // Create old mag
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.22f, 0.28f)) // Throw away old mag
                {
                    if (magObj1 != 0)
                        DETACH_OBJECT(magObj1, true);
                }
                else if (currentReloadAnimTime.InRange(0.3422542f, 0.5062909f)) // Create new mag
                {
                    if (magObj2 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.5062909f, 1.0f)) // Delete new mag
                {
                    if (magObj2 != 0)
                    {
                        DELETE_OBJECT(ref magObj2);
                        magObj2 = 0;
                    }
                    magObj1 = 0;
                }
            }
            else
            {
                isAnyGunReloadingAnimPlaying = false;
            }
        }
        private static void ProcessShotgunReloading(IVPed playerPed, int playerPedHandle, PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enableFix)
                return;

            bool isBarettaReloading = animController.IsPlaying("gun@baretta", isPlayerDucking ? "reload_crouch" : "reload");
            bool isShotgunReloading = animController.IsPlaying("gun@shotgun", isPlayerDucking ? "reload_crouch" : "reload");

            if (isBarettaReloading || isShotgunReloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isBarettaReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@baretta", isPlayerDucking ? "reload_crouch" : "reload");
                else
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@shotgun", isPlayerDucking ? "reload_crouch" : "reload");

                if (currentReloadAnimTime.InRange(0.24f, 0.3f)) // Create Shell Obj 1
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_shshell"), playerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.09f, 0.01f, 0f, 0f, 0f, 0f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.35f, 0.4f)) // Delete Shell Obj 1
                {
                    if (magObj1 != 0)
                    {
                        DELETE_OBJECT(ref magObj1);
                        magObj1 = 0;
                    }
                }
                else if (currentReloadAnimTime.InRange(0.5f, 0.6f)) // Create Shell Obj 2
                {
                    if (magObj2 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_shshell"), playerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.09f, 0.01f, 0f, 0f, 0f, 0f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.65f, 1.0f)) // Delete Shell Obj 2
                {
                    if (magObj2 != 0)
                    {
                        DELETE_OBJECT(ref magObj2);
                        magObj2 = 0;
                    }
                }
            }
            else
            {
                isAnyGunReloadingAnimPlaying = false;
            }
        }
        private static void ProcessUziReloading(IVPed playerPed, int playerPedHandle, PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enableFix)
                return;

            bool isUziReloading = animController.IsPlaying("gun@uzi", isPlayerDucking ? "reload_crouch" : "reload");
            bool isMp5Reloading = animController.IsPlaying("gun@mp5k", isPlayerDucking ? "reload_crouch" : "p_load");

            if (isUziReloading || isMp5Reloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isUziReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@uzi", isPlayerDucking ? "reload_crouch" : "reload");
                else
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@mp5k", isPlayerDucking ? "reload_crouch" : "p_load");

                if (currentReloadAnimTime.InRange(0.13f, 0.15f)) // Create Old Mag Obj
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.16f, 0.27f)) // Detach Old Mag Obj
                {
                    if (magObj1 != 0)
                        DETACH_OBJECT(magObj1, true);
                }
                else if (currentReloadAnimTime.InRange(0.27f, 0.3f)) // Create New Mag Obj
                {
                    if (magObj2 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.4f, 1.0f)) // Delete New Mag Obj
                {
                    if (magObj2 != 0)
                    {
                        DELETE_OBJECT(ref magObj2);
                        magObj2 = 0;
                    }
                    magObj1 = 0;
                }
            }
            else
            {
                isAnyGunReloadingAnimPlaying = false;
            }
        }
        private static void ProcessAssaultRifleReloading(IVPed playerPed, int playerPedHandle, PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enableFix)
                return;

            // The AK47 and M4 share the same animation set
            bool ak47Reloading = animController.IsPlaying("gun@ak47", isPlayerDucking ? "reload_crouch" : "p_load");

            if (ak47Reloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@ak47", isPlayerDucking ? "reload_crouch" : "p_load");

                if (currentReloadAnimTime.InRange(0.16f, 0.2f)) // Create Old Mag Obj
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.21f, 0.3f)) // Detach Old Mag Obj
                {
                    if (magObj1 != 0)
                        DETACH_OBJECT(magObj1, true);
                }
                else if (currentReloadAnimTime.InRange(0.3f, 0.5f)) // Create New Mag Obj
                {
                    if (magObj2 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.5f, 1.0f)) // Destroy New Mag Obj
                {
                    if (magObj2 != 0)
                    {
                        DELETE_OBJECT(ref magObj2);
                        magObj2 = 0;
                    }
                    magObj1 = 0;
                }
            }
            else
            {
                isAnyGunReloadingAnimPlaying = false;
            }
        }
        private static void ProcessRifleReloading(IVPed playerPed, int playerPedHandle, PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enableFix)
                return;

            bool isRifleReloading = animController.IsPlaying("gun@rifle", isPlayerDucking ? "reload_crouch" : "p_load");
            bool isRifle2Reloading = animController.IsPlaying("gun@rifle", isPlayerDucking ? "reload_crouch" : "reload");

            if (isRifleReloading || isRifle2Reloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isRifleReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@rifle", isPlayerDucking ? "reload_crouch" : "p_load");
                else
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@rifle", isPlayerDucking ? "reload_crouch" : "reload");

                if (currentReloadAnimTime.InRange(0.16f, 0.2f)) // Create Old Mag Obj
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.2f, 0.3f)) // Detach Old Mag Obj
                {
                    if (magObj1 != 0)
                        DETACH_OBJECT(magObj1, true);
                }
                else if (currentReloadAnimTime.InRange(0.3f, 0.5f)) // Create New Mag Obj
                {
                    if (magObj2 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), playerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.5f, 1.0f)) // Destroy New Mag Obj
                {
                    if (magObj2 != 0)
                    {
                        DELETE_OBJECT(ref magObj2);
                        magObj2 = 0;
                    }
                    magObj1 = 0;
                }
            }
            else
            {
                isAnyGunReloadingAnimPlaying = false;
            }
        }
        private static void ProcessRPGReloading(IVPed playerPed, int playerPedHandle, PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enableFix)
                return;

            bool isRPGReloading = animController.IsPlaying("gun@rocket", isPlayerDucking ? "reload_crouch" : "reload");

            if (isRPGReloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@rocket", isPlayerDucking ? "reload_crouch" : "reload");

                if (currentReloadAnimTime.InRange(0.28f, 0.35f)) // Create New Rocket Obj
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("cj_rpg_rocket"), playerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, playerPedHandle, (uint)eBone.BONE_LEFT_HAND, 0.13f, 0.02f, 0.1f, 1.5f, 0f, 0f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.47f, 1.0f)) // Delete New Rocket Obj
                {
                    if (magObj1 != 0)
                    {
                        DELETE_OBJECT(ref magObj1);
                    }
                }
            }
            else
            {
                isAnyGunReloadingAnimPlaying = false;
            }
        }
        public static void LoadFiles()
        {
            IVCDStream.AddImage("IVSDKDotNet/scripts/LibertyTweaks/WeaponMagazineFiles/mags.img", 1, -1);
            IVFileLoader.LoadLevel("IVSDKDotNet/scripts/LibertyTweaks/WeaponMagazineFiles/mags.dat", 0);
        }

        public static void Tick()
        {
            if (!enableFix)
                return;

            // If amb_magazine is not in cdimage then return so game will not crash when we try to use this model
            if (!IS_MODEL_IN_CDIMAGE(GET_HASH_KEY("amb_magazine")))
                return;

            // Gets the player ped
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            // Gets the handle of the player ped
            int playerPedHandle = playerPed.GetHandle();

            // Gets the current weapon of the player
            GET_CURRENT_CHAR_WEAPON(playerPedHandle, out uint currentWeapon);

            // Check if the reloading can't be done
            if (currentWeapon == (uint)eWeaponType.WEAPON_UNARMED || playerPed.Dead)
            {
                // Check if magazine objects exists and are attached somewhere but shouldn't
                CheckMagazineObjects();
                return;
            }

            // Check if the magazine objects are still attached somewhere but shouldn't
            if (!isAnyGunReloadingAnimPlaying)
                CheckMagazineObjects();

            // Gets if the player ped is ducking
            bool isPlayerDucking = IS_CHAR_DUCKING(playerPedHandle);

            // Gets the PedAnimationController of the player ped used to play animations, get the time of the currently playing animation etc.
            PedAnimationController animController = playerPed.GetAnimationController();

            // Only do the animation checks and weapon reloading things for the currently equipped weapon
            switch ((eWeaponType)currentWeapon)
            {
                case eWeaponType.WEAPON_PISTOL:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_PISTOL).ToString()))
                        ProcessHandgunReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_DEAGLE:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_DEAGLE).ToString()))
                        ProcessHandgunReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_SHOTGUN:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_SHOTGUN).ToString()))
                        ProcessShotgunReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_BARETTA:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_BARETTA).ToString()))
                        ProcessShotgunReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_MICRO_UZI:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_MICRO_UZI).ToString()))
                        ProcessUziReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_MP5:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_MP5).ToString()))
                        ProcessUziReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_AK47:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_AK47).ToString()))
                        ProcessAssaultRifleReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_M4:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_M4).ToString()))
                        ProcessAssaultRifleReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_SNIPERRIFLE:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_SNIPERRIFLE).ToString()))
                        ProcessRifleReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_M40A1:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_M40A1).ToString()))
                        ProcessRifleReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_RLAUNCHER:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_RLAUNCHER).ToString()))
                        ProcessRPGReloading(playerPed, playerPedHandle, animController, isPlayerDucking);
                    break;

                default:
                    CheckMagazineObjects();
                    break;

                    // TODO(Clonk): Do episode weapons later...
                    //case eWeaponType.WEAPON_EPISODIC_1:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_2:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_3:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_4:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_5:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_6:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_7:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_8:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_9:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_10:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_11:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_12:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_13:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_14:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_15:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_16:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_17:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_18:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_19:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_20:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_21:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_22:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_23:
                    //    break;
                    //case eWeaponType.WEAPON_EPISODIC_24:
                    //    break;
            }
        }

    }
}
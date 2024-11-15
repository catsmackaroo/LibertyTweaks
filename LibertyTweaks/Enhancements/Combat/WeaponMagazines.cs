using System;
using System.Collections.Generic;
using System.Linq;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre

namespace LibertyTweaks
{
    internal class WeaponMagazines
    {

        public static bool enable;
        private static List<string> disableForWeapons;
        private static bool firstFrame = true;

        private static float currentReloadAnimTime;
        private static bool isAnyGunReloadingAnimPlaying;
        private static int magObj1, magObj2;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Gun Magazines", "Enable", true);
            disableForWeapons = settings.GetValue("Gun Magazines", "DisabledWeaponTypes", "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (enable)
                Main.Log("script initialized...");
        }
        public static void LoadFiles()
        {
            IVCDStream.AddImage("IVSDKDotNet/scripts/LibertyTweaks/WeaponMagazineFiles/mags.img", 1, -1);
            IVFileLoader.LoadLevel("IVSDKDotNet/scripts/LibertyTweaks/WeaponMagazineFiles/mags.dat", 0);
        }

        public static void IngameStartup()
        {
            if (!enable)
                return;

            firstFrame = true;
        }
        private static void CheckMagazineObjects()
        {
            if (!enable)
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

            if (magObj1 != 0)
            {
                DELETE_OBJECT(ref magObj1);
                MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                magObj1 = 0;
            }
            if (magObj2 != 0)
            {
                DELETE_OBJECT(ref magObj2);
                MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                magObj2 = 0;
            }
        }

        // Clonk: Quite alot of duplicated code in those methods, maybe make the animation time check code universal
        private static void ProcessHandgunReloading(PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enable)
                return;

            // Handgun / Deagle
            bool isHandgunReloading = animController.IsPlaying("gun@handgun", isPlayerDucking ? "reload_crouch" : "reload");
            bool isDeagleReloading = animController.IsPlaying("gun@deagle", isPlayerDucking ? "reload_crouch" : "reload");
            bool isCzReloading = animController.IsPlaying("gun@cz75", isPlayerDucking ? "reload_crouch" : "reload");
            bool is44Reloading = animController.IsPlaying("gun@44A", isPlayerDucking ? "reload_crouch" : "p_load");

            if (isHandgunReloading || isDeagleReloading || isCzReloading || is44Reloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isHandgunReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@handgun", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isDeagleReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@deagle", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isCzReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@cz75", isPlayerDucking ? "reload_crouch" : "reload");
                else
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@44A", isPlayerDucking ? "reload_crouch" : "p_load");


                if (currentReloadAnimTime.InRange(0.15f, 0.18f)) // Create old mag
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
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
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
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
        private static void ProcessShotgunReloading(PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enable)
                return;

            bool isBarettaReloading = animController.IsPlaying("gun@baretta", isPlayerDucking ? "reload_crouch" : "reload");
            bool isShotgunReloading = animController.IsPlaying("gun@shotgun", isPlayerDucking ? "reload_crouch" : "reload");
            bool isAA12Reloading = animController.IsPlaying("gun@aa12", isPlayerDucking ? "reload_crouch" : "reload");
            bool isStreetReloading = animController.IsPlaying("gun@test_gun", isPlayerDucking ? "reload_crouch" : "reload");
            bool isSawnReloading = animController.IsPlaying("gun@sawnoff", isPlayerDucking ? "reload_crouch" : "reload");

            if (isBarettaReloading || isShotgunReloading || isAA12Reloading || isSawnReloading || isStreetReloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isBarettaReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@baretta", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isShotgunReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@shotgun", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isAA12Reloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@aa12", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isStreetReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@test_gun", isPlayerDucking ? "reload_crouch" : "reload");
                else
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@sawnoff", isPlayerDucking ? "reload_crouch" : "reload");


                if (currentReloadAnimTime.InRange(0.24f, 0.3f)) // Create Shell Obj 1
                {
                    if (magObj1 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_shshell"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.09f, 0.01f, 0f, 0f, 0f, 0f, 0);
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
                        CREATE_OBJECT(GET_HASH_KEY("amb_shshell"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.09f, 0.01f, 0f, 0f, 0f, 0f, 0);
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
        private static void ProcessUziReloading(PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enable)
                return;

            bool isUziReloading = animController.IsPlaying("gun@uzi", isPlayerDucking ? "reload_crouch" : "reload");
            bool isMp5Reloading = animController.IsPlaying("gun@mp5k", isPlayerDucking ? "reload_crouch" : "p_load");
            bool isP90Reloading = animController.IsPlaying("gun@p90", isPlayerDucking ? "reload_crouch" : "p_load");
            bool isGoldUziReloading = animController.IsPlaying("gun@gold_uzi", isPlayerDucking ? "reload_crouch" : "p_load");

            if (isUziReloading || isMp5Reloading || isP90Reloading || isGoldUziReloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isUziReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@uzi", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isMp5Reloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@mp5k", isPlayerDucking ? "reload_crouch" : "p_load");
                else if (isP90Reloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@p90", isPlayerDucking ? "reload_crouch" : "p_load");
                else if (isGoldUziReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@gold_uzi", isPlayerDucking ? "reload_crouch" : "p_load");

                if (currentReloadAnimTime.InRange(0.13f, 0.15f)) // Create Old Mag Obj
                {
                    if (magObj1 == 0 && !isP90Reloading && !isGoldUziReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                    if (magObj1 == 0 && isP90Reloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_p90_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                    if (magObj1 == 0 && isGoldUziReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_uzi_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.16f, 0.27f)) // Detach Old Mag Obj
                {
                    if (magObj1 != 0)
                        DETACH_OBJECT(magObj1, true);
                }
                else if (currentReloadAnimTime.InRange(0.27f, 0.3f)) // Create New Mag Obj
                {
                    if (magObj2 == 0 && !isP90Reloading && !isGoldUziReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                    if (magObj2 == 0 && isP90Reloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_p90_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                    if (magObj2 == 0 && isGoldUziReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_uzi_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
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
        private static void ProcessAssaultRifleReloading(PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enable)
                return;

            // The AK47 and M4 share the same animation set
            bool ak47Reloading = animController.IsPlaying("gun@ak47", isPlayerDucking ? "reload_crouch" : "p_load");
            bool lmgReloading = animController.IsPlaying("gun@m249", isPlayerDucking ? "reload_crouch" : "p_load");

            if (ak47Reloading || lmgReloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (ak47Reloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@ak47", isPlayerDucking ? "reload_crouch" : "p_load");
                else if (lmgReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@m249", isPlayerDucking ? "reload_crouch" : "p_load");

                if (currentReloadAnimTime.InRange(0.16f, 0.2f)) // Create Old Mag Obj
                {
                    if (magObj1 == 0 && !lmgReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                    if (magObj1 == 0 && lmgReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_m249_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.21f, 0.3f)) // Detach Old Mag Obj
                {
                    if (magObj1 != 0)
                        DETACH_OBJECT(magObj1, true);
                }
                else if (currentReloadAnimTime.InRange(0.3f, 0.5f)) // Create New Mag Obj
                {
                    if (magObj2 == 0 && !lmgReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                    if (magObj2 == 0 && lmgReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_m249_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
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
        private static void ProcessRifleReloading(PedAnimationController animController, bool isPlayerDucking)
        {
            if (!enable)
                return;

            bool isRifleReloading = animController.IsPlaying("gun@rifle", isPlayerDucking ? "reload_crouch" : "p_load");
            bool isRifle2Reloading = animController.IsPlaying("gun@rifle", isPlayerDucking ? "reload_crouch" : "reload");
            bool isDsrReloading = animController.IsPlaying("gun@dsr1", isPlayerDucking ? "reload_crouch" : "p_load");

            if (isRifleReloading || isRifle2Reloading || isDsrReloading)
            {

                isAnyGunReloadingAnimPlaying = true;

                if (isRifleReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@rifle", isPlayerDucking ? "reload_crouch" : "p_load");
                else if (isRifle2Reloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@rifle", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isDsrReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@dsr1", isPlayerDucking ? "reload_crouch" : "p_load");

                if (currentReloadAnimTime.InRange(0.16f, 0.2f)) // Create Old Mag Obj
                {
                    if (magObj1 == 0 && !isDsrReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                    else if (magObj1 == 0 && isDsrReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_dsr1_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }
                }
                else if (currentReloadAnimTime.InRange(0.2f, 0.3f)) // Detach Old Mag Obj
                {
                    if (magObj1 != 0)
                        DETACH_OBJECT(magObj1, true);
                }
                else if (currentReloadAnimTime.InRange(0.3f, 0.5f)) // Create New Mag Obj
                {
                    if (magObj2 == 0 && !isDsrReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    }

                    if (magObj2 == 0 && isDsrReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("amb_dsr1_magazine"), Main.PlayerPed.Matrix.Pos, out magObj2, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj2);
                        ATTACH_OBJECT_TO_PED(magObj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
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
        private static void ProcessRPGReloading(PedAnimationController animController, bool isPlayerDucking, uint ep)
        {
            if (!enable)
                return;

            bool isRPGReloading = animController.IsPlaying("gun@rocket", isPlayerDucking ? "reload_crouch" : "reload");
            bool isGLauncherReloading = animController.IsPlaying("gun@grnde_launch", isPlayerDucking ? "reload_crouch" : "reload");

            if (isRPGReloading || isGLauncherReloading)
            {
                isAnyGunReloadingAnimPlaying = true;

                if (isRPGReloading) 
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@rocket", isPlayerDucking ? "reload_crouch" : "reload");
                else if (isGLauncherReloading)
                    currentReloadAnimTime = animController.GetCurrentAnimationTime("gun@grnde_launch", isPlayerDucking ? "reload_crouch" : "reload");


                if (currentReloadAnimTime.InRange(0.28f, 0.35f)) // Create New Rocket Obj
                {
                    if (magObj1 == 0 && isRPGReloading)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("cj_rpg_rocket"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.13f, 0.02f, 0.1f, 1.5f, 0f, 0f, 0);
                    }
                    else if (magObj1 == 0 && isGLauncherReloading && ep == 1)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("w_e1_grenade"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, 0.062f, -0.015f, 1.5f, 0f, 0f, 0);
                    }
                    else if (magObj1 == 0 && isGLauncherReloading && ep == 2)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("w_e2_grenade"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                        MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                        ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, 0.062f, -0.015f, 1.5f, 0f, 0f, 0);
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
        private static void HandleCollisionFix()
        {
            if (magObj1 == 0)
            {
                Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(1), "Main", () =>
                {
                    CREATE_OBJECT(GET_HASH_KEY("amb_magazine"), Main.PlayerPed.Matrix.Pos, out magObj1, true);
                    MARK_OBJECT_AS_NO_LONGER_NEEDED(magObj1);
                    //DELETE_OBJECT(ref magObj1);
                    ATTACH_OBJECT_TO_PED(magObj1, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_LEFT_HAND, 0.11f, -0.04f, 0.06f, -6.6f, 8.7f, 5.6f, 0);
                    DETACH_OBJECT(magObj1, true);
                });
            }
            magObj1 = 0;

        }
        public static void Tick()
        {
            if (!enable)
                return;

            // If amb_magazine is not in cdimage then return so game will not crash when we try to use this model
            if (!IS_MODEL_IN_CDIMAGE(GET_HASH_KEY("amb_magazine")))
                return;

            // Gets episode
            uint ep = GET_CURRENT_EPISODE();

            if (ep == (uint)Episode.TBoGT) 
                if (!IS_MODEL_IN_CDIMAGE(GET_HASH_KEY("amb_dsr1_magazine")) 
                    || !IS_MODEL_IN_CDIMAGE(GET_HASH_KEY("amb_uzi_magazine")) 
                    || !IS_MODEL_IN_CDIMAGE(GET_HASH_KEY("amb_p90_magazine")) 
                    || !IS_MODEL_IN_CDIMAGE(GET_HASH_KEY("amb_m249_magazine")))
                    return;

            // This firstframe check is simply to avoid a strange collision bug for the EFLC magazines
            // In-short, EFLC mags would not have collision until amb_magazine was loaded, and so I load it for the first frame
            if (firstFrame)
            {
                CheckMagazineObjects();
                HandleCollisionFix();
                firstFrame = false;
            }

            // Gets the current weapon of the player
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);

            // Check if the reloading can't be done
            if (currentWeapon == (int)eWeaponType.WEAPON_UNARMED || Main.PlayerPed.Dead)
            {
                // Check if magazine objects exists and are attached somewhere but shouldn't
                CheckMagazineObjects();
                return;
            }

            // Check if the magazine objects are still attached somewhere but shouldn't
            if (!isAnyGunReloadingAnimPlaying)
                CheckMagazineObjects();

            // Gets if the player ped is ducking
            bool isPlayerDucking = IS_CHAR_DUCKING(Main.PlayerPed.GetHandle());

            // Gets the PedAnimationController of the player ped used to play animations, get the time of the currently playing animation etc.
            PedAnimationController animController = Main.PlayerPed.GetAnimationController();

            

            // Only do the animation checks and weapon reloading things for the currently equipped weapon
            switch ((eWeaponType)currentWeapon)
            {
                case eWeaponType.WEAPON_PISTOL:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_PISTOL).ToString()))
                        ProcessHandgunReloading(animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_DEAGLE:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_DEAGLE).ToString()))
                        ProcessHandgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_SHOTGUN:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_SHOTGUN).ToString()))
                        ProcessShotgunReloading(animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_BARETTA:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_BARETTA).ToString()))
                        ProcessShotgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_MICRO_UZI:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_MICRO_UZI).ToString()))
                        ProcessUziReloading(animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_MP5:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_MP5).ToString()))
                        ProcessUziReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_AK47:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_AK47).ToString()))
                        ProcessAssaultRifleReloading(animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_M4:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_M4).ToString()))
                        ProcessAssaultRifleReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_SNIPERRIFLE:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_SNIPERRIFLE).ToString()))
                        ProcessRifleReloading(animController, isPlayerDucking);
                    break;
                case eWeaponType.WEAPON_M40A1:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_M40A1).ToString()))
                        ProcessRifleReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_RLAUNCHER:
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_RLAUNCHER).ToString()))
                        ProcessRPGReloading(animController, isPlayerDucking, ep);
                    break;

                case eWeaponType.WEAPON_EPISODIC_1: // GRENADE LAUNCHER
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_1).ToString()))
                        ProcessRPGReloading(animController, isPlayerDucking, ep);
                    break;

                case eWeaponType.WEAPON_EPISODIC_2: // TLAD: STREET SWEEPER 
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_2).ToString()))
                        ProcessShotgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_6: // TLAD: SAWN-OFF
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_6).ToString()))
                        ProcessShotgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_7: // TLAD: AUTO 9MM
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_7).ToString()))
                        ProcessHandgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_9: // TBoGT: 44. AUTOMAG
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_9).ToString()))
                        ProcessHandgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_10: // TBoGT: AA12 EXPLOSIVE
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_10).ToString()))
                        ProcessShotgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_11: // TBoGT: AA12 REGULAR
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_11).ToString()))
                        ProcessShotgunReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_12: // TBoGT: P90/ UNFINISHED
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_12).ToString()))
                        ProcessUziReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_13: // TBoGT: GOLDEN UZI/ UNFINISHED
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_13).ToString()))
                        ProcessUziReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_14: // TBoGT: LMG
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_14).ToString()))
                        ProcessAssaultRifleReloading(animController, isPlayerDucking);
                    break;

                case eWeaponType.WEAPON_EPISODIC_15: // TBoGT: ADVANCED SNIPER / UNFINISHED
                    if (!disableForWeapons.Contains(((int)eWeaponType.WEAPON_EPISODIC_15).ToString()))
                        ProcessRifleReloading(animController, isPlayerDucking);
                    break;

                default:
                    CheckMagazineObjects();
                    break;

            }
        }

    }
}
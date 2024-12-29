using IVSDKDotNet;
using System.Collections.Generic;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using System.IO;
using System.Diagnostics;
using System;
using System.Numerics;
using IVSDKDotNet.Native;

namespace LibertyTweaks
{
    internal class BlindFireDisableHUD
    {
        private static bool enable;
        private static bool isAiming(IVPed ped) => IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@handgun", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@handgun", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@deagle", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@deagle", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@uzi", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@uzi", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@mp5k", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@mp5k", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@sawnoff", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@sawnoff", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@shotgun", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@shotgun", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@baretta", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@baretta", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@cz75", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@cz75", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@grnde_launch", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@grnde_launch", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@p90", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@p90", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@gold_uzi", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@gold_uzi", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@aa12", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@aa12", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@44a", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@44a", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@ak47", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@ak47", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@ak47", "fire_up") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@ak47", "fire_down") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@test_gun", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@test_gun", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@test_gun", "fire_up") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@test_gun", "fire_down") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@m249", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@m249", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@m249", "fire_up") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@m249", "fire_down") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@rifle", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@rifle", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@rifle", "fire_alt") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@rifle", "fire_crouch_alt") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@dsr1", "fire") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@dsr1", "fire_crouch") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@dsr1", "fire_alt") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "gun@dsr1", "fire_crouch_alt");
        private static bool isInBlindCover(IVPed ped) => IS_PED_IN_COVER(Main.PlayerPed.GetHandle()) && !isAiming(ped) && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "pistol_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "rifle_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "pistol_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "rifle_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "pistol_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "rifle_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "pistol_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "rifle_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "pistol_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "rifle_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "pistol_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "rifle_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "pistol_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "rifle_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "pistol_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "rifle_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_centre", "pistol_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_centre", "rifle_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_centre", "pistol_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_centre", "rifle_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_centre", "pistol_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_centre", "rifle_normal_fire_intro") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_centre", "pistol_peek") && !IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_centre", "rifle_peek");

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("BlindFire Disable HUD", "Enable", true);
        }
        public static void Tick()
        {
            bool HudIsOn = IVMenuManager.HudOn;
            if (enable)
            {
                if (isInBlindCover(Main.PlayerPed) && HudIsOn)
                    DISPLAY_HUD(false);
                else if (!isInBlindCover(Main.PlayerPed) && HudIsOn)
                    DISPLAY_HUD(true);
            }
        }
    }
}

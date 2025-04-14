using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo 

namespace LibertyTweaks
{
    internal class RollInAirFix
    {
        private static bool enable;
        private static bool isRollingLeft;
        private static bool isRollingRight;
        private static bool isRolling;
        private static bool hasRagdolled = false;
        public static string section { get; private set; }

        public static void Init(SettingsFile settings, string section)
        {
            RollInAirFix.section = section;
            enable = settings.GetBoolean(section, "Roll In Air Fix", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            isRollingLeft = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_left") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_l");
            isRollingRight = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_right") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_r");
            isRolling = isRollingLeft || isRollingRight;
            GET_CHAR_HEIGHT_ABOVE_GROUND(Main.PlayerPed.GetHandle(), out float heightAboveGround);

            if (heightAboveGround > 5)
            {
                if (isRolling)
                {
                    var time = heightAboveGround += 2000;
                    Main.PlayerPed.ActivateDrunkRagdoll((int)heightAboveGround);
                    hasRagdolled = true;
                }

                if (hasRagdolled && !IS_PED_RAGDOLL(Main.PlayerPed.GetHandle()))
                {
                    var time = heightAboveGround += 2000;
                    Main.PlayerPed.ActivateDrunkRagdoll((int)heightAboveGround);
                }
            }
            else
            {
                if (!IS_PED_RAGDOLL(Main.PlayerPed.GetHandle()))
                {
                    hasRagdolled = false;
                }
            }
        }
    }
}

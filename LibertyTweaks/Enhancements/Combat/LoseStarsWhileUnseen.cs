using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo 

namespace LibertyTweaks
{
    internal class LoseStarsWhileUnseen
    {
        private static bool enable;
        private static DateTime timer = DateTime.MinValue;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Lose Stars While Unseen", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick(DateTime timer, int unseenSlipAwayMinTimer, int unseenSlipAwayMaxTimer)
        {
            if (!enable)
                return;

            // Grab player ID
            uint playerId = GET_PLAYER_ID();


            if (LoseStarsWhileUnseen.timer == DateTime.MinValue)
                LoseStarsWhileUnseen.timer = DateTime.UtcNow;

            if (IS_PAUSE_MENU_ACTIVE())
            {
                LoseStarsWhileUnseen.timer = DateTime.MinValue;
                return;
            }

            // Check when cops see player
            if (PLAYER_HAS_GREYED_OUT_STARS((int)playerId))
            {
                if (IVTheScripts.IsPlayerOnAMission()) 
                    return;

                if (LoseStarsWhileUnseen.timer != DateTime.MinValue)
                {
                    if (DateTime.UtcNow > LoseStarsWhileUnseen.timer.AddSeconds(Main.GenerateRandomNumber(unseenSlipAwayMinTimer, unseenSlipAwayMaxTimer)))
                    {
                        // Grab player wanted level
                        STORE_WANTED_LEVEL((int)playerId, out uint currentWantedLevel);

                        // Removes 1 star off current wanted level
                        uint alteredWantedLevel = currentWantedLevel - 1;
                        ALTER_WANTED_LEVEL((int)playerId, alteredWantedLevel);
                        APPLY_WANTED_LEVEL_CHANGE_NOW((int)playerId);
                    }
                }
            }

            if (!PLAYER_HAS_GREYED_OUT_STARS((int)playerId))
                // Reset so this can all happen again
                LoseStarsWhileUnseen.timer = DateTime.MinValue;
        }
    }
}

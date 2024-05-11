using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo 

namespace LibertyTweaks
{
    internal class UnseenSlipAway
    {
        private static bool enable;
        private static DateTime timer = DateTime.MinValue;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Lose Stars While Unseen", true);
        }

        public static void Tick(DateTime timer, int unseenSlipAwayMinTimer, int unseenSlipAwayMaxTimer)
        {
            if (!enable)
                return;

            // Grab player ID
            uint playerId = GET_PLAYER_ID();


            if (UnseenSlipAway.timer == DateTime.MinValue)
                UnseenSlipAway.timer = DateTime.UtcNow;

            // Check when cops see player
            if (PLAYER_HAS_GREYED_OUT_STARS((int)playerId))
            {
                if (IVTheScripts.IsPlayerOnAMission()) 
                    return;


                if (UnseenSlipAway.timer != DateTime.MinValue)
                {
                    if (DateTime.UtcNow > UnseenSlipAway.timer.AddSeconds(Main.GenerateRandomNumber(unseenSlipAwayMinTimer, unseenSlipAwayMaxTimer)))
                    {
                        // Grab player wanted level
                        STORE_WANTED_LEVEL((int)playerId, out uint currentWantedLevel);

                        // Removes 1 star off current wanted level
                        uint alteredWantedLevel = currentWantedLevel - 1;
                        ALTER_WANTED_LEVEL((int)playerId, alteredWantedLevel);
                    }
                }
            }

            if (!PLAYER_HAS_GREYED_OUT_STARS((int)playerId))
                // Reset so this can all happen again
                UnseenSlipAway.timer = DateTime.MinValue;

            if (IS_PAUSE_MENU_ACTIVE())
            {
                UnseenSlipAway.timer = DateTime.MinValue;
            }
        }
    }
}

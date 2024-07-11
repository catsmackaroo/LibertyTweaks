using DocumentFormat.OpenXml.Wordprocessing;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo 

namespace LibertyTweaks
{
    internal class LoseStarsWhileUnseen
    {
        private static bool enable;
        private static DateTime lastUnseenTime = DateTime.MinValue;
        private static readonly object lockObject = new object();
        private static int minUnseenTimeToLoseStars;
        private static int maxUnseenTimeToLoseStars;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Lose Stars While Unseen", true);

            minUnseenTimeToLoseStars = settings.GetInteger("Improved Police", "Lose Stars While Unseen Minimum Count", 60);
            maxUnseenTimeToLoseStars = settings.GetInteger("Improved Police", "Lose Stars While Unseen Maximum Count", 120);

            if (enable)
            {
                Main.Log("script initialized...");
            }
        }
        public static void Tick()
        {
            if (!enable)
                return;

            if (Main.gxtEntries == true)
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("WANTED3", "To gradually reduce your wanted level, stay out of sight. To completely lose it, escape the flashing zone.");
            }

            uint playerId = GET_PLAYER_ID();


            if (IS_PAUSE_MENU_ACTIVE())
            {
                lock (lockObject)
                {
                    lastUnseenTime = DateTime.MinValue;
                }
                return;
            }


            lock (lockObject)
            {

                if (lastUnseenTime == DateTime.MinValue)
                    lastUnseenTime = DateTime.UtcNow;

                if (PLAYER_HAS_GREYED_OUT_STARS((int)playerId))
                {
                    if (PLAYER_HAS_FLASHING_STARS_ABOUT_TO_DROP((int)playerId) || IS_INTERIOR_SCENE())
                    {
                        lastUnseenTime = DateTime.UtcNow;

                    }

                    if (DateTime.UtcNow > lastUnseenTime.AddSeconds(Main.GenerateRandomNumber(minUnseenTimeToLoseStars, maxUnseenTimeToLoseStars)))
                    {
                        STORE_WANTED_LEVEL((int)playerId, out uint currentWantedLevel);

                        if (currentWantedLevel > 0)
                        {
                            uint alteredWantedLevel = currentWantedLevel - 1;
                            ALTER_WANTED_LEVEL((int)playerId, alteredWantedLevel);
                            APPLY_WANTED_LEVEL_CHANGE_NOW((int)playerId);
                            Main.Log($"Player's wanted level decreased to {alteredWantedLevel}");
                        }

                        lastUnseenTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    lastUnseenTime = DateTime.MinValue;
                }
            }
        }
    }
}

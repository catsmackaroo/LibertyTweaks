﻿using CCL.GTAIV;
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
        public static string section { get; private set; }

        public static void Init(SettingsFile settings, string section)
        {
            LoseStarsWhileUnseen.section = section;
            enable = settings.GetBoolean(section, "Lose Stars While Unseen", false);

            minUnseenTimeToLoseStars = settings.GetInteger(section, "Lose Stars While Unseen - Min Time", 60);
            maxUnseenTimeToLoseStars = settings.GetInteger(section, "Lose Stars While Unseen - Max Time", 120);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
                return;

            if (Main.gxtEntries == true && NativeGame.IsScriptRunning("wantedhelp"))
                IVText.TheIVText.ReplaceTextOfTextLabel("WANTED3", "To gradually reduce your wanted level, stay out of sight. To completely lose it, escape the flashing zone.");


            if (IS_PAUSE_MENU_ACTIVE())
            {
                lock (lockObject)
                    lastUnseenTime = DateTime.MinValue;
                return;
            }


            lock (lockObject)
            {

                if (lastUnseenTime == DateTime.MinValue)
                    lastUnseenTime = DateTime.UtcNow;

                if (PLAYER_HAS_GREYED_OUT_STARS(Main.PlayerIndex))
                {
                    if (PLAYER_HAS_FLASHING_STARS_ABOUT_TO_DROP(Main.PlayerIndex) || IS_INTERIOR_SCENE())
                        lastUnseenTime = DateTime.UtcNow;

                    if (DateTime.UtcNow > lastUnseenTime.AddSeconds(Main.GenerateRandomNumber(minUnseenTimeToLoseStars, maxUnseenTimeToLoseStars)))
                    {
                        STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);

                        if (currentWantedLevel > 0)
                        {
                            uint alteredWantedLevel = currentWantedLevel - 1;
                            ALTER_WANTED_LEVEL(Main.PlayerIndex, alteredWantedLevel);
                            APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                        }

                        lastUnseenTime = DateTime.UtcNow;
                    }
                }
                else
                    lastUnseenTime = DateTime.MinValue;
            }
        }
    }
}

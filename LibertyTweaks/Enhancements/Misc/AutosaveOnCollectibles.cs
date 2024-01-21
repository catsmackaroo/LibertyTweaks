using CCL.GTAIV;
using CLR;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Numerics;
using System.Reflection;

// Credits: Gillian

namespace LibertyTweaks
{
    internal class AutosaveOnCollectibles
    {
        private static bool enableFix;
        private static uint lastEpisode = 3; // this is done so it'll init the stats
        private static uint pigeons;
        private static uint stuntJumps;
        private static uint seagullsTLAD;
        private static uint seagullsTBoGT;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Autosave on Collectibles", "Enable", true);
        }
        private static void InitStats()
        {
            pigeons = Natives.GET_INT_STAT(361);
            stuntJumps = Natives.GET_INT_STAT(270);
            seagullsTLAD = Natives.GET_INT_STAT(143);
            seagullsTBoGT = Natives.GET_INT_STAT(211);
        }

        public static void Tick()
        {
            if (!enableFix)
                return;
            bool autoSaveStatus = Natives.GET_IS_AUTOSAVE_OFF();
            if (autoSaveStatus)
                return;
            // get episode and declare variables
            uint episode = Natives.GET_CURRENT_EPISODE();
            uint tickPigeons = Natives.GET_INT_STAT(361);
            uint tickStuntJumps = Natives.GET_INT_STAT(270);
            uint tickSeagullsTLAD = Natives.GET_INT_STAT(143);
            uint tickSeagullsTBoGT = Natives.GET_INT_STAT(211);
            // incase the stats still didn't initialize, return; alternatively, if there's nothing to check
            if (tickPigeons == 0 && tickStuntJumps == 0 && tickSeagullsTLAD == 0 && tickSeagullsTBoGT == 0)
            {
                return;
            }

            // return if all the collectibles in the episode are already acquired
            if (tickPigeons == 200 && tickStuntJumps == 50 || tickSeagullsTLAD == 50 || tickSeagullsTBoGT == 50)
                return;

            // first initialize the stats, after that just reinitialize stats incase user changes the episode
            if (episode != lastEpisode)
            {
                InitStats();
                lastEpisode = episode;
            }
            // do the actual checks based on the episode that you're currently in and autosave
            switch (episode)
            {
                case 0:
                    AutosaveOnChange(tickPigeons, ref pigeons);
                    AutosaveOnChange(tickStuntJumps, ref stuntJumps);
                    break;
                case 1:
                    AutosaveOnChange(tickSeagullsTLAD, ref seagullsTLAD);
                    break;
                case 2:
                    AutosaveOnChange(tickSeagullsTBoGT, ref seagullsTBoGT);
                    break;
            }
        }

        private static void AutosaveOnChange(uint tickValue, ref uint value)
        {
            // compare the stats, and if the value got incremented, autosave
            if (tickValue > value)
            {
                value = tickValue;
                NativeGame.DoAutoSave();
            }
            else if (tickValue < value)
            {
                // user likely changed the savefile, reinitialize stats
                InitStats();
            }
        }
    }
}

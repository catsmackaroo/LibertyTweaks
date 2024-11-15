using CCL.GTAIV;
using IVSDKDotNet.Native;
using IVSDKDotNet;

// Credits: Gillian

namespace LibertyTweaks
{
    internal class AutosaveOnCollectibles
    {
        private static bool enable;
        private static uint lastEpisode = 3; // this is done so it'll init the stats
        private static int pigeons;
        private static int stuntJumps;
        private static int seagullsTLAD;
        private static int seagullsTBoGT;

        private static readonly int delayInMilliseconds = 5000;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Autosave on Collectibles", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
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
            if (!enable)
                return;

            if (!CommonHelpers.ShouldExecute(delayInMilliseconds))
                return;

            bool autoSaveStatus = Natives.GET_IS_AUTOSAVE_OFF();
            if (autoSaveStatus)
                return;

            // get episode and declare variables
            uint episode = Natives.GET_CURRENT_EPISODE();
            int tickPigeons = Natives.GET_INT_STAT(361);
            int tickStuntJumps = Natives.GET_INT_STAT(270);
            int tickSeagullsTLAD = Natives.GET_INT_STAT(143);
            int tickSeagullsTBoGT = Natives.GET_INT_STAT(211);

            // in case the stats still didn't initialize, return; alternatively, if there's nothing to check
            if (tickPigeons == 0 && tickStuntJumps == 0 && tickSeagullsTLAD == 0 && tickSeagullsTBoGT == 0)
            {
                return;
            }

            // return if all the collectibles in the episode are already acquired
            if (tickPigeons == 200 && tickStuntJumps == 50 || tickSeagullsTLAD == 50 || tickSeagullsTBoGT == 50)
                return;

            // first initialize the stats, after that just reinitialize stats in case user changes the episode
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

        private static void AutosaveOnChange(int tickValue, ref int value)
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

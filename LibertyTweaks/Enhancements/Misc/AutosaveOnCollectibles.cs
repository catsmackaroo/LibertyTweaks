using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;

// Credits: Gillian

namespace LibertyTweaks
{
    internal class AutosaveOnCollectibles
    {
        private static bool enableFix;
        private static bool firstCheck = true;
        private static int dlc;
        private static int pigeons;
        private static int stuntJumps;
        private static int seagullsTLAD;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Autosave on Collectibles", "Enable", true);
        }

        private static void AutosaveOnChange(int tickValue, int value)
        {
            // check the stats, and if the value got incremented, autosave
            if (tickValue > value)
            {
                value = tickValue;
                if (!firstCheck)
                {
                    NativeGame.DoAutoSave();
                }
                else
                {
                    firstCheck = false;
                }
            }
        }

        public static void Tick()
        {
            if (!enableFix)
                return;
            bool autoSaveStatus = Natives.GET_IS_AUTOSAVE_OFF();
            if (autoSaveStatus == true)
                return;

            // get the stats
            int tickPigeons = Natives.GET_INT_STAT(361);
            int tickStuntJumps = Natives.GET_INT_STAT(270);
            if (tickPigeons == 200 && tickStuntJumps == 50)
                return;
            int tickSeagullsTLAD = Natives.GET_INT_STAT(143);
            if (tickSeagullsTLAD == 50)
                return;

            // check the code for this above
            AutosaveOnChange(tickPigeons, pigeons);
            AutosaveOnChange(tickStuntJumps, stuntJumps);
            AutosaveOnChange(tickSeagullsTLAD, seagullsTLAD);
        }
    }
}
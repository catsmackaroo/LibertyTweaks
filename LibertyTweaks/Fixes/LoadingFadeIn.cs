using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Native;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class LoadingFadeIn
    {
        private static bool enable;
        private static bool firstFrame = true;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Loading Fade In", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void IngameStartup()
        {
            if (!enable)
                return;

            firstFrame = true;
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (firstFrame)
            {
                IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
                uint missionsComplete = GET_INT_STAT(253);
                
                // Check if player has beaten the first mission
                // Simply to avoid a bug
                if (missionsComplete > 0)
                {
                    DO_SCREEN_FADE_IN(10000);
                }
                firstFrame = false;
            }
        }
    }
}
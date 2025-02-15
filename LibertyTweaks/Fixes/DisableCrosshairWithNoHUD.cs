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
    internal class DisableCrosshairWithNoHUD
    {
        private static bool enable;
        private static bool hudState = true; 

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Disable Crosshair When No HUD", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            bool currentHudState = IVMenuManager.HudOn;
            if (currentHudState != hudState)
            {
                DISPLAY_HUD(currentHudState);
                hudState = currentHudState;
            }
        }

    }
}

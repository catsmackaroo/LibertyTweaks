using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Numerics;
using IVSDKDotNet.Enums;
using CCL.GTAIV;
using System.Windows.Forms;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SkipRadioTrack
    {
        private static bool enable;
        public static Keys key;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Skip Radio", "Enable", true);
            key = settings.GetKey("Skip Radio", "Key", Keys.B);

            if (enable)
                Main.Log("script initialized...");
        }   
        public static void Process()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE()) return;

            SKIP_RADIO_FORWARD();
        }
    }
}

using IVSDKDotNet;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class StunPunch
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Main", "Stun Punching", true);
        }

        public static void Tick()
        {
            if (!enable)
                return;

        }

        public static void Process()
        {
            if (!enable)
                return;
        }
    }
}

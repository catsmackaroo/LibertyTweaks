using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class DeathTimescaleFix
    {

        public static bool enable;
        public static bool shouldReset = false;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DeathTimescaleFix.section = section;
            enable = settings.GetBoolean(section, "Death Timescale Fix", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (!IS_CHAR_DEAD(Main.PlayerPed.GetHandle()))
            {
                if (shouldReset)
                {
                    shouldReset = false;
                    NativeGame.TimeScale = 1f;
                }
            }
            else
            {
                NativeGame.TimeScale = 0.2f;
                shouldReset = true;
            }
        }
    }
}

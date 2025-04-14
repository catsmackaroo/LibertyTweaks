using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: ItsClonkAndre

namespace LibertyTweaks
{
    internal class LoadingFadeIn
    {
        private static bool enable;
        private static bool firstFrame = true;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            LoadingFadeIn.section = section;
            enable = settings.GetBoolean(section, "Loading Fade In", false);

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
                int missionsComplete = GET_INT_STAT(253);

                // Check if player has beaten the first mission
                // Simply to avoid a bug
                if (missionsComplete > 0)
                {
                    DO_SCREEN_FADE_IN(12000);
                }
                firstFrame = false;
            }
        }
    }
}
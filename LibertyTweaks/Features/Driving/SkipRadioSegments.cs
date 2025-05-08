using IVSDKDotNet;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SkipRadioSegments
    {
        private static bool enable;
        public static Keys key;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            SkipRadioSegments.section = section;
            enable = settings.GetBoolean(section, "Skip Radio Segments", false);
            key = settings.GetKey(section, "Skip Radio Segements - Key", Keys.B);

            if (enable)
            {
                Main.Log("script initialized...");
                Main.Log($"Key: {key}");
            }
        }
        public static void Process()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE()) return;

            SKIP_RADIO_FORWARD();
        }
    }
}

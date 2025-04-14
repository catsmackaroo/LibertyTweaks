using IVSDKDotNet;
using System.Windows.Forms;

// Credits: ItsClonkAndre

namespace LibertyTweaks
{
    internal class NoCursorEscape
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            NoCursorEscape.section = section;
            enable = settings.GetBoolean(section, "No Cursor Escape", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Process()
        {
            if (!enable)
                return;
            if (!IVGame.IsInFocus())
                return;

            Cursor.Clip = IVGame.Bounds;
        }

    }
}

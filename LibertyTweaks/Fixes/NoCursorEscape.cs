using IVSDKDotNet;
using System.Windows.Forms;

// Credits: ItsClonkAndre

namespace LibertyTweaks
{
    internal class NoCursorEscape
    {
        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "No Cursor Escape", true);

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

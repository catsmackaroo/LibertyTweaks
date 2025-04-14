using CCL.GTAIV;
using IVSDKDotNet;
using System.Windows.Forms;

namespace LibertyTweaks
{
    public class CopyLocationToClipboard
    {
        private static bool enable;
        public static Keys key;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Debug", "Enable", true);
            key = settings.GetKey("Debug Key", "Key", Keys.I);


            if (enable)
                Main.Log("script initialized...");
        }
        public static void Process()
        {
            if (!enable)
                return;

            var playerPos = Main.PlayerPos;
            float heading = Main.PlayerPed.GetHeading();
            string formattedCoordinates = $"{(int)playerPos.X} {(int)playerPos.Y} {(int)playerPos.Z + 1} {(int)heading}";
            Clipboard.SetText(formattedCoordinates);
            IVGame.ShowSubtitleMessage($"Copied {formattedCoordinates}");
        }
    }
}

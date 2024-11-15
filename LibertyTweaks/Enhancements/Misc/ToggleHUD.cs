using IVSDKDotNet;
using System.Windows.Forms;

// Credits: ClonkAndre 

namespace LibertyTweaks
{
    internal class ToggleHUD
    {
        private static bool enable;
        public static Keys toggleHudKey;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Toggle HUD", "Enable", true);
            toggleHudKey = settings.GetKey("Toggle HUD", "Key", Keys.LMenu);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Process()
        {
            if (!enable)
                return;

            if (IVMenuManager.RadarMode == 0)
            {
                IVMenuManager.RadarMode = 1;
                IVMenuManager.HudOn = true;
                
            }
            else
            {
                IVMenuManager.RadarMode = 0;
                IVMenuManager.HudOn = false;
                IVGame.ShowSubtitleMessage("", 0); 
            }
        }
    }
}

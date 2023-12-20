using IVSDKDotNet;

// Credits: ClonkAndre 

namespace LibertyTweaks
{
    internal class ToggleHUD
    {
        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Toggle HUD", "Keys", true);
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

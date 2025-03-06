using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class ToggleHUD
    {
        private static bool enable;
        public static Keys key;

        // Controller Support
        private static readonly uint padIndex = 0;
        private static ControllerButton controllerKey1;
        private static ControllerButton controllerKey2;
        private static DateTime lastProcessTime = DateTime.MinValue;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(500);

        private const uint radarOn = 1;
        private const uint radarOff = 0;
        private const uint radarBlipsOnly = 2;
        private static uint originalRadarMode = radarOn;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Toggle HUD", "Enable", true);
            key = settings.GetKey("Toggle HUD", "Key", Keys.LMenu);

            controllerKey1 = (ControllerButton)settings.GetInteger("Toggle HUD", "Controller Key 1", (int)ControllerButton.BUTTON_DPAD_DOWN);
            controllerKey2 = (ControllerButton)settings.GetInteger("Toggle HUD", "Controller Key 2", (int)ControllerButton.BUTTON_B);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (!IS_PLAYER_PLAYING(Main.PlayerIndex)) return;

            if (IS_USING_CONTROLLER())
            {
                bool bothKeysPressed = NativeControls.IsControllerButtonPressed(padIndex, controllerKey1)
                                    && NativeControls.IsControllerButtonPressed(padIndex, controllerKey2);

                if (bothKeysPressed && DateTime.Now - lastProcessTime >= delay)
                {
                    Process();
                    lastProcessTime = DateTime.Now;
                }
            }
        }
        public static void Process()
        {
            if (!enable) return;

            if (!IS_PLAYER_PLAYING(Main.PlayerIndex)) return;

            // Ensure the original value is saved so the player's radar settings are respected
            uint currentRadarMode = IVMenuManager.RadarMode;
            if (currentRadarMode == radarOn || currentRadarMode == radarBlipsOnly)
                originalRadarMode = currentRadarMode;

            // Toggle radar stuff
            if (IVMenuManager.RadarMode == radarOn || IVMenuManager.RadarMode == radarBlipsOnly)
            {
                DisableHud();
            }
            else
            {
                EnableHud();
            }
        }

        private static void DisableHud()
        {
            IVMenuManager.RadarMode = radarOff;
            IVMenuManager.HudOn = false;
            IVGame.ShowSubtitleMessage("", 0);
        }

        private static void EnableHud()
        {
            IVMenuManager.RadarMode = originalRadarMode;
            IVMenuManager.HudOn = true;
        }
    }
}

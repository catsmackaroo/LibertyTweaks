using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    [ShowStaticFieldsInInspector]
    internal class StaminaProgression
    {
        private static bool enable;
        private static int savedStaminaLevel;
        private static int activeStaminaLevel;
        private static int notifiedStaminaLevel; // Tracks notifications for the current session
        private static double milesOnFoot;

        public static int staminaLevel4 = 20;
        public static int staminaLevel3 = 15;
        public static int staminaLevel2 = 8;
        public static int staminaLevel1 = 3;

        public static void Init(SettingsFile Settings)
        {
            enable = Settings.GetBoolean("Progression System", "Enable for Stamina", true);
            savedStaminaLevel = Main.GetTheSaveGame().GetInteger("PlayerStaminaLevel");
            notifiedStaminaLevel = savedStaminaLevel; // Initialize to avoid redundant notifications on load

            if (enable)
                Main.Log("Stamina progression script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            milesOnFoot = GET_INT_STAT(80) / 1610;

            StaminaLevelUp();
            AlterStaminaLevel(activeStaminaLevel);
        }

        private static void AlterStaminaLevel(int level)
        {
            switch (level)
            {
                case 1:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 300)
                        Main.PlayerPed.PlayerInfo.Stamina = 300;
                    break;

                case 2:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 500)
                        Main.PlayerPed.PlayerInfo.Stamina = 500;
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.05f);
                    break;

                case 3:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 600)
                        Main.PlayerPed.PlayerInfo.Stamina = 600;
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.075f);
                    break;

                case 4:
                    Main.PlayerPed.PlayerInfo.Stamina = 999;
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.1f);
                    break;

                default:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 200)
                        Main.PlayerPed.PlayerInfo.Stamina = 200;
                    break;
            }
        }

        private static void StaminaLevelUp()
        {
            int newActiveStaminaLevel =
                (milesOnFoot >= staminaLevel4) ? 4 :
                (milesOnFoot >= staminaLevel3) ? 3 :
                (milesOnFoot >= staminaLevel2) ? 2 :
                (milesOnFoot >= staminaLevel1) ? 1 : 0;

            if (newActiveStaminaLevel != activeStaminaLevel)
            {
                activeStaminaLevel = newActiveStaminaLevel;

                if (activeStaminaLevel > savedStaminaLevel)
                {
                    savedStaminaLevel = activeStaminaLevel;
                    Main.GetTheSaveGame().SetInteger("PlayerStaminaLevel", savedStaminaLevel);
                }

                // Check for notification display
                if (activeStaminaLevel > notifiedStaminaLevel)
                {
                    notifiedStaminaLevel = activeStaminaLevel;
                    Notifications(activeStaminaLevel);
                }

                Main.PlayerPed.PlayerInfo.Stamina = 600; // Reset stamina on level-up
            }
        }

        private static void Notifications(int level)
        {
            string message = "";

            switch (level)
            {
                case 1:
                    message = "Upgraded Stamina - You can now run for longer durations.";
                    break;
                case 2:
                    message = "Upgraded Stamina - You can now run for even longer & your running speed has increased.";
                    break;
                case 3:
                    message = "Upgraded Stamina - Your stamina capacity & running speed have increased further.";
                    break;
                case 4:
                    message = "Upgraded Stamina - You have reached maximum running speed and you now have unlimited stamina.";
                    break;
            }

            if (!string.IsNullOrEmpty(message))
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
                PRINT_HELP("PLACEHOLDER_1");
            }
        }
    }
}

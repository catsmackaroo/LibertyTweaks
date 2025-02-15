using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using IVSDKDotNet.Enums;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    [ShowStaticFieldsInInspector]
    internal class StaminaProgression
    {
        private static bool enable;
        private static bool enableTLADnerf;
        private static int savedStaminaLevel;
        private static int activeStaminaLevel;
        private static double milesOnFoot;
        private static double milesOnFootInitial;

        // Optimization Stuff
        private static int tickCounter = 0;

        private static bool firstFrame = true;
        private static bool hasSaved;

        public static int staminaLevel4;
        public static int staminaLevel3;
        public static int staminaLevel2;
        public static int staminaLevel1;

        public static void Init(SettingsFile Settings)
        {
            enable = Settings.GetBoolean("Stamina Progression", "Enable", true);
            enableTLADnerf = Settings.GetBoolean("Stamina Progression", "Limit on TLAD", true);
            staminaLevel1 = Settings.GetInteger("Stamina Progression", "Level 1 Threshold", 3);
            staminaLevel2 = Settings.GetInteger("Stamina Progression", "Level 2 Threshold", 8);
            staminaLevel3 = Settings.GetInteger("Stamina Progression", "Level 3 Threshold", 15);
            staminaLevel4 = Settings.GetInteger("Stamina Progression", "Level 4 Threshold", 20);

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
            tickCounter++;
            if (!enable)
                return;

            if (firstFrame)
                InitializeFirstFrame();

            milesOnFoot = GET_INT_STAT(80) / 1610;
            AlterStaminaLevel(activeStaminaLevel);
            if (HavePlayerStatsChanged())
                StaminaLevelUp();

            HandleSaving();
        }
        public static void Process()
        {
            // Simulate progression through each stamina level for testing
            if (milesOnFoot < staminaLevel1)
            {
                SET_INT_STAT(80, staminaLevel1 * 1610); // Set to level 1 threshold
                Main.Log("Debug: Testing Level 1 progression...");
            }
            else if (milesOnFoot < staminaLevel2)
            {
                SET_INT_STAT(80, staminaLevel2 * 1610); // Set to level 2 threshold
                Main.Log("Debug: Testing Level 2 progression...");
            }
            else if (milesOnFoot < staminaLevel3)
            {
                SET_INT_STAT(80, staminaLevel3 * 1610); // Set to level 3 threshold
                Main.Log("Debug: Testing Level 3 progression...");
            }
            else if (milesOnFoot < staminaLevel4)
            {
                SET_INT_STAT(80, staminaLevel4 * 1610); // Set to level 4 threshold
                Main.Log("Debug: Testing Level 4 progression...");
            }
            else
            {
                SET_INT_STAT(80, 0); // Reset to 0 to start over
                Main.Log("Debug: Reset to Level 0 for testing...");
            }
        }

        private static void InitializeFirstFrame()
        {
            uint currentEpisode = GET_CURRENT_EPISODE();

            // Nerf TLAD as Johnny has injuries
            if (currentEpisode == 1 && enableTLADnerf)
                staminaLevel4 = 9999;

            milesOnFootInitial = GET_INT_STAT(80) / 1610;
            StaminaLevelUp();
            savedStaminaLevel = Main.GetTheSaveGame().GetInteger("PlayerStaminaLevel");
            activeStaminaLevel = Main.GetTheSaveGame().GetInteger("PlayerStaminaLevel");
            firstFrame = false;
        }
        private static bool HavePlayerStatsChanged()
        {
            return milesOnFoot != milesOnFootInitial;
        }
        private static void AlterStaminaLevel(int level)
        {
            switch (level)
            {
                case 1:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 450 && Main.PlayerPed.GetSpeed() >= 1)
                    {
                        Main.PlayerPed.PlayerInfo.Stamina = 450;
                        SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.0f);
                    }
                    break;

                case 2:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 550 && Main.PlayerPed.GetSpeed() >= 1)
                    {
                        Main.PlayerPed.PlayerInfo.Stamina = 500;
                        SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.05f);
                    }
                    break;

                case 3:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 600 && Main.PlayerPed.GetSpeed() >= 1)
                    {
                        Main.PlayerPed.PlayerInfo.Stamina = 600;
                        SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.075f);
                    }
                    break;

                case 4:
                    Main.PlayerPed.PlayerInfo.Stamina = 999;
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.1f);
                    break;

                default:
                    if (Main.PlayerPed.PlayerInfo.Stamina > 350 && Main.PlayerPed.GetSpeed() >= 1)
                    {
                        SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerPed.GetHandle(), 1.0f);
                        Main.PlayerPed.PlayerInfo.Stamina = 350;
                    }
                    break;
            }
        }
        private static void StaminaLevelUp()
        {
            activeStaminaLevel =
                (milesOnFoot >= staminaLevel4) ? 4 :
                (milesOnFoot >= staminaLevel3) ? 3 :
                (milesOnFoot >= staminaLevel2) ? 2 :
                (milesOnFoot >= staminaLevel1) ? 1 : 0;


            if (activeStaminaLevel != savedStaminaLevel)
                Notifications(activeStaminaLevel);

            Main.PlayerPed.PlayerInfo.Stamina = 600; // Reset stamina on level-up
            milesOnFootInitial = milesOnFoot;

        }
        private static void HandleSaving()
        {
            if (GET_IS_DISPLAYINGSAVEMESSAGE() && !hasSaved)
            {
                hasSaved = true;
            }
            else if (!GET_IS_DISPLAYINGSAVEMESSAGE() && hasSaved)
            {
                savedStaminaLevel = activeStaminaLevel;
                Main.GetTheSaveGame().SetInteger("PlayerStaminaLevel", savedStaminaLevel);
                Main.GetTheSaveGame().Save();
                Main.Log("Saved PlayerStaminaLevel as activeStaminaLevel: " + activeStaminaLevel);
                hasSaved = false;
            }
        }   
        private static void Notifications(int level)
        {
            if (activeStaminaLevel == savedStaminaLevel)
                return;

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

                default:
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

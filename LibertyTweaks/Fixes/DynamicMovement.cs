using CCL.GTAIV;
using IVSDKDotNet;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class DynamicMovement
    {
        private const int SprintCooldownTime = 30;
        private static int sprintCooldownTimer;
        public static bool enableSprintFix;
        public static bool enableLowHealthExhaustion;
        private static bool disableInCombat;
        private static bool isPlayerHealthLow;

        public static bool IsSprintEnabled { get; private set; } = true;

        public static bool IsCapsLockActive() => Control.IsKeyLocked(Keys.Capital);
        public static string section { get; private set; }

        public static void Init(SettingsFile settings, string section)
        {
            DynamicMovement.section = section;
            enableLowHealthExhaustion = settings.GetBoolean(section, "Disable Sprint With Low Health", false);
            enableSprintFix = settings.GetBoolean(section, "Dynamic Sprint With Keyboard", false);
            disableInCombat = settings.GetBoolean(section, "Dynamic Sprint Disabled in Combat", false);

            if (enableLowHealthExhaustion)
                Main.Log("Disable Sprint With Low Health script initialized...");

            if (enableSprintFix && disableInCombat)
                Main.Log("Dynamic Sprint Enabled. Disabled in combat.");
            else if (enableSprintFix && !disableInCombat)
                Main.Log("Dynamic Sprint Enabled. Enabled in combat.");
        }

        public static void Tick()
        {
            if (enableSprintFix)
                SprintFix();

            if (enableLowHealthExhaustion)
                LowHealthExhaustionTick();
        }

        private static void LowHealthExhaustionTick()
        {
            GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out uint playerHealth);

            if (enableSprintFix)
            {
                isPlayerHealthLow = playerHealth < 126;
            }

            if (!enableSprintFix)
            {
                IsSprintEnabled = playerHealth >= 126;
                DISABLE_PLAYER_SPRINT(0, !IsSprintEnabled);
            }
        }

        private static void SprintFix()
        {
            uint alwaysSprintSetting = IVMenuManager.GetSetting(IVSDKDotNet.Enums.eSettings.SETTING_ALWAYS_SPRINT);
            bool isUsingController = IS_USING_CONTROLLER();

            if (isPlayerHealthLow && enableLowHealthExhaustion)
            {
                IsSprintEnabled = false;
                DISABLE_PLAYER_SPRINT(0, true);
                return;
            }
            if (isUsingController || alwaysSprintSetting == 0 || (PlayerHelper.IsPlayerInOrNearCombat() && disableInCombat))
            {
                IsSprintEnabled = true;
                DISABLE_PLAYER_SPRINT(0, false);
                return;
            }

            if (!IsCapsLockActive())
            {
                if (sprintCooldownTimer == 0)
                {
                    IsSprintEnabled = false;
                    DISABLE_PLAYER_SPRINT(0, true);
                }
                else
                {
                    sprintCooldownTimer--;
                    IsSprintEnabled = true;
                    DISABLE_PLAYER_SPRINT(0, false);
                }
            }
            else
            {
                IsSprintEnabled = true;
                DISABLE_PLAYER_SPRINT(0, false);
            }
        }

        public static void Process()
        {
            if (IS_USING_CONTROLLER())
                return;

            if (enableLowHealthExhaustion)
            {
                sprintCooldownTimer = isPlayerHealthLow ? 0 : SprintCooldownTime;
            }

            if (!enableLowHealthExhaustion && enableSprintFix)
            {
                sprintCooldownTimer = SprintCooldownTime;
            }
        }
    }
}

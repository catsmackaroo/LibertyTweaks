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

        public static void Init(SettingsFile settings)
        {
            enableLowHealthExhaustion = settings.GetBoolean("Low Health Exhaustion", "Enable", true);
            enableSprintFix = settings.GetBoolean("Fixes", "Sprint Fix", true);
            disableInCombat = settings.GetBoolean("Fixes", "Sprint Fix Disabled In Combat", true);

            if (enableLowHealthExhaustion)
                Main.Log("Low Health Exhaustion script initialized...");

            if (enableSprintFix && disableInCombat)
                Main.Log("Sprint Fix script initialized, disabled in combat...");
            else if (enableSprintFix && !disableInCombat)
                Main.Log("Sprint Fix script initialized, enabled in combat...");
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

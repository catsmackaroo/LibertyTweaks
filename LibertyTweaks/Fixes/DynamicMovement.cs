using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Windows.Forms;
using CCL.GTAIV;

namespace LibertyTweaks
{
    internal class DynamicMovement
    {
        private const int SprintCooldownTime = 30;
        private static int sprintCooldownTimer;
        private static bool enableSprintFix;
        private static bool enableLowHealthExhaustion;
        private static bool disableInCombat;
        private static bool isPlayerHealthLow;
        private static bool playerIsLowest;

        private static bool IsCapsLockActive() => Control.IsKeyLocked(Keys.Capital);

        public static void Init(SettingsFile settings)
        {
            enableLowHealthExhaustion = settings.GetBoolean("Low Health Exhaustion", "Enable", true);
            enableSprintFix = settings.GetBoolean("Fixes", "Sprint Fix", true);
            disableInCombat = settings.GetBoolean("Fixes", "Sprint Fix Disabled In Combat", true);

            if (enableLowHealthExhaustion)
                Main.Log("Low Health Exhaustion script initialized...");

            if (enableSprintFix)
                Main.Log("Sprint Fix script initialized...");
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
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            GET_CHAR_HEALTH(playerPed.GetHandle(), out uint playerHealth);

            if (enableSprintFix)
            {
                if (playerHealth < 126)
                {
                    playerIsLowest = true;
                }
                else
                {
                    playerIsLowest = false;
                }
            }

            if (!enableSprintFix)
            {
                if (playerHealth < 126)
                    DISABLE_PLAYER_SPRINT(0, true);
                else
                    DISABLE_PLAYER_SPRINT(0, false);
            }
        }

        private static void SprintFix()
        {
            uint alwaysSprintSetting = IVMenuManager.GetSetting(IVSDKDotNet.Enums.eSettings.SETTING_ALWAYS_SPRINT);
            bool isUsingController = IS_USING_CONTROLLER();
            PlayerChecks combatChecker = new PlayerChecks();
            bool isInOrNearCombat = combatChecker.IsPlayerInOrNearCombat();

            if (isUsingController || alwaysSprintSetting == 0 || isInOrNearCombat && disableInCombat == true)
            {
                DISABLE_PLAYER_SPRINT(0, false);
                return;
            }

            if (isPlayerHealthLow)
                return;

            if (!IsCapsLockActive())
            {
                if (sprintCooldownTimer == 0)
                {
                    DISABLE_PLAYER_SPRINT(0, true);
                }
                else
                {
                    sprintCooldownTimer--;
                    DISABLE_PLAYER_SPRINT(0, false);
                }
            }
            else
            {
                DISABLE_PLAYER_SPRINT(0, false);
            }
        }

        public static void Process()
        {
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

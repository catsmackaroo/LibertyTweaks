using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Windows.Forms;
using CCL.GTAIV;
using LibertyTweaks;

namespace LibertyTweaks
{
    internal class DynamicMovement
    {
        private const int KeyTime = 18;
        private static int sprintTimer;
        private static bool enableSprintFix;
        private static bool enableLowHealthExhaustion;
        private static bool playerIsLowest = false;

        private static bool CapsLockActive() => Control.IsKeyLocked(Keys.Capital);

        public static void Init(SettingsFile settings)
        {
            enableLowHealthExhaustion = settings.GetBoolean("Low Health Exhaustion", "Enable", true);
            enableSprintFix = settings.GetBoolean("Fixes", "Sprint Fix", true);

            if (enableLowHealthExhaustion)
                Main.Log("LowHealthExhaustion script initialized...");

            if (enableSprintFix)
                Main.Log("SprintFix script initialized...");
        }

        public static void Tick()
        {

            if (enableSprintFix)
                SprintFixTick();

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
        private static void SprintFixTick()
        {
            if (IS_USING_CONTROLLER())
            {
                DISABLE_PLAYER_SPRINT(0, false);
                return;
            }

            if (!CapsLockActive() || playerIsLowest)
            {

                if (sprintTimer == 0)
                {
                    DISABLE_PLAYER_SPRINT(0, true);
                }
                else
                {
                    sprintTimer--;
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

            if (playerIsLowest && enableLowHealthExhaustion)
                sprintTimer = 0;

            if (!playerIsLowest && enableLowHealthExhaustion)
                sprintTimer = KeyTime;

            if (!enableLowHealthExhaustion && enableSprintFix)
                sprintTimer = KeyTime;
        }
    }
}

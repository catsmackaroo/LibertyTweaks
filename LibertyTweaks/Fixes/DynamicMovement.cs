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
        private const int KeyTime = 30;
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
            if (!enableLowHealthExhaustion)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            GET_CHAR_HEALTH(playerPed.GetHandle(), out uint playerHealth);

            if (playerHealth < 126)
                playerPed.PlayerInfo.Stamina = -140;
        }
        private static void SprintFixTick()
        {
            if (IS_USING_CONTROLLER())
            {
                DISABLE_PLAYER_SPRINT(0, false);
                return;
            }

            uint alwaysSprint = IVMenuManager.GetSetting(IVSDKDotNet.Enums.eSettings.SETTING_ALWAYS_SPRINT);

            if (alwaysSprint == 0)
            {
                DISABLE_PLAYER_SPRINT(0, false);
                return;
            }

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    if (IS_PED_IN_COMBAT(pedHandle))
                    {
                        if (playerIsLowest)
                            return;

                        DISABLE_PLAYER_SPRINT(0, false);
                        return;
                    }
                }
            }

            if (!CapsLockActive())
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

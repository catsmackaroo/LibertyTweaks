using IVSDKDotNet;
using System.Numerics;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using CCL.GTAIV;

// Credits: catsmackaroo 
namespace LibertyTweaks
{
    internal class RegenerateHP
    {
        private static bool enable;
        private static DateTime timer = DateTime.MinValue;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Main", "Health Regeneration", true);
        }

        public static void Tick(DateTime timer, int regenHealthMinTimer, int regenHealthMaxTimer, int regenHealthMinHeal, int regenHealthMaxHeal)
        {
            if (!enable)
                return;

            // Grab player ID
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            if (RegenerateHP.timer == DateTime.MinValue)
                RegenerateHP.timer = DateTime.UtcNow;

            // Grab  player health
            GET_CHAR_HEALTH(playerPed.GetHandle(), out uint playerHealth);

            if (playerHealth < 130)
            {
                if (IS_CHAR_DEAD(playerPed.GetHandle()))
                {
                    RegenerateHP.timer = DateTime.MinValue;
                    return;
                }    

                if (RegenerateHP.timer != DateTime.MinValue)
                {
                    if (DateTime.UtcNow > RegenerateHP.timer.AddSeconds(Main.GenerateRandomNumber(regenHealthMinTimer, regenHealthMaxTimer)))
                    {

                        SET_CHAR_HEALTH(playerPed.GetHandle(), (uint)(playerHealth+Main.GenerateRandomNumber(regenHealthMinHeal, regenHealthMaxHeal)));
                        RegenerateHP.timer = DateTime.MinValue;
                    }
                }
            }
        }
    }
}

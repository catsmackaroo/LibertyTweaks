using System;
using System.Collections.Generic;
using System.Numerics;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credit: catsmackaroo

namespace LibertyTweaks
{
    internal class UnholsteredGunFix
    {
        private static bool enable;
        private static readonly List<int> excludedWeapons = new List<int>
        {
            0,
            1,
            2, 
            46, 
            41,
            24
        };


        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Unholstered Wanted Fix", true);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
            {
                return;
            }

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out int currentWeap);

            if (!excludedWeapons.Contains(currentWeap))
            {
                if (!IS_CHAR_IN_ANY_CAR(playerPed.GetHandle()) && PlayerChecks.IsPlayerSeenByPolice())
                {
                    uint playerId = GET_PLAYER_ID();
                    STORE_WANTED_LEVEL((int)playerId, out uint currentWantedLevel);

                    if (currentWantedLevel == 0)
                    {
                        Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(2), "Main", () =>
                        {
                            ALTER_WANTED_LEVEL((int)playerId, 1);
                            APPLY_WANTED_LEVEL_CHANGE_NOW((int)playerId);
                        });
                    }      
                }
            }
        }
    }
}

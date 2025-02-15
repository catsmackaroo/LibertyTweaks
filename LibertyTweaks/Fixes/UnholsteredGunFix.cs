using System;
using System.Collections.Generic;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
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
            24,
            (int)eWeaponType.WEAPON_KNIFE
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
                return;

            STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);
            uint pEpisode = GET_CURRENT_EPISODE();

            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeap);

            if (!excludedWeapons.Contains(currentWeap))
            {
                if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()) && PlayerHelper.IsPlayerSeenByPolice())
                {
                    if (currentWantedLevel == 0 && !IS_INTERIOR_SCENE())
                        Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(2), "Main", () =>
                        {
                            ALTER_WANTED_LEVEL(Main.PlayerIndex, 1);
                            APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                        });

                    if (currentWantedLevel < 4 && !IS_INTERIOR_SCENE() && currentWeap == (int)eWeaponType.WEAPON_RLAUNCHER)
                        Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(2), "Main", () =>
                        {
                            ALTER_WANTED_LEVEL(Main.PlayerIndex, 4);
                            APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                        });
                }
            }

            if (IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()) && pEpisode == 2)
            {
                GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int pVehicle);
                IVVehicle pVeh = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

                if (pVehicle == 2563)
                        if (currentWantedLevel < 6)
                            Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(2), "Main", () =>
                            {
                                ALTER_WANTED_LEVEL(Main.PlayerIndex, 6);
                                APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                            });
            }
        }
    }
}

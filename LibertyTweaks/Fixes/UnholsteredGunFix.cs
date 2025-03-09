using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credit: catsmackaroo

namespace LibertyTweaks
{
    internal class UnholsteredGunFix
    {
        private static bool enable;
        private static DateTime? policeSeePlayerStartTime;

        private static readonly double oneHandedTime = 6;
        private static readonly double twoHandedTime = 4;
        private static readonly double rpgTime = 2;

        private static readonly uint oneHandedStars = 1;
        private static readonly uint twoHandedStars = 2;
        private static readonly uint rpgStars = 4;

        private static readonly float nightMultiplier = 1.5f;
        private static readonly Dictionary<eWeather, float> weatherMultipliers = new Dictionary<eWeather, float>
        {
            { eWeather.WEATHER_EXTRA_SUNNY, 0.75f },
            { eWeather.WEATHER_SUNNY, 0.9f },
            { eWeather.WEATHER_DRIZZLE, 1.05f },
            { eWeather.WEATHER_FOGGY, 1.15f },
            { eWeather.WEATHER_RAINING, 1.2f },
            { eWeather.WEATHER_LIGHTNING, 1.35f }
        };

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

            if (!InitialChecks())
                return;

            HandleUnholsteredWantedFix();
        }
        private static bool InitialChecks()
        {
            if (IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return false;

            if (excludedWeapons.Contains(WeaponHelpers.GetCurrentWeaponType()))
                return false;

            return true;
        }
        private static void HandleUnholsteredWantedFix()
        {
            if (PlayerHelper.IsPlayerSeenByPolice())
            {
                if (policeSeePlayerStartTime == null)
                    policeSeePlayerStartTime = DateTime.Now;

                TimeSpan seenDuration = DateTime.Now - policeSeePlayerStartTime.Value;

                // Logic to determine both wanted level & how long cops need to see you based on weapon type
                double seenThreshold = 0;
                uint wantedLevel = 0;
                var weapon = WeaponHelpers.GetCurrentWeaponType();
                DetermineThresholds(weapon, ref wantedLevel, ref seenThreshold);

                if (seenDuration.TotalSeconds >= seenThreshold)
                {
                    if (Main.PlayerWantedLevel < wantedLevel)
                        ApplyWantedLevelChange(wantedLevel);
                }
            }
            else
            {
                policeSeePlayerStartTime = null;
            }
        }
        private static void ApplyWantedLevelChange(uint wantedLevel)
        {
            ALTER_WANTED_LEVEL(Main.PlayerIndex, wantedLevel);
            APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
            policeSeePlayerStartTime = null;
        }
        private static void DetermineThresholds(int weapon, ref uint wantedLevel, ref double seenThreshold)
        {
            // Weapon based adjustments
            switch (weapon)
            {
                case int w when WeaponHelpers.IsHeavyGun(w):
                    seenThreshold = rpgTime;
                    wantedLevel = rpgStars;
                    break;

                case int w when WeaponHelpers.IsTwoHandedGun(w):
                    seenThreshold = twoHandedTime;
                    wantedLevel = twoHandedStars;
                    break;

                default:
                    seenThreshold = oneHandedTime;
                    wantedLevel = oneHandedStars;
                    break;
            }

            // Weather based adjustments
            GET_CURRENT_WEATHER(out int weather);
            if (weatherMultipliers.TryGetValue((eWeather)weather, out float multiplier))
                seenThreshold *= multiplier;

            // Time based adjustments
            GET_TIME_OF_DAY(out int hour, out _);
            if (hour >= 20 || hour <= 7)
                seenThreshold *= nightMultiplier;

            seenThreshold = Math.Ceiling(seenThreshold);
        }
        private static void APCLogic()
        {
            // Supposed to give player wanted level with APC.
            // Unfinished, don't even think it works yet, need to make a method to see if police see player's car
            if (IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()) && Main.Episode == 2)
            {
                GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int pVehicle);
                IVVehicle pVeh = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

                int apc = 2563;
                if (pVehicle == apc)
                    if (Main.PlayerWantedLevel < 6)
                        Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(2), "Main", () =>
                        {
                            ALTER_WANTED_LEVEL(Main.PlayerIndex, 6);
                            APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                        });
            }
        }
    }
}

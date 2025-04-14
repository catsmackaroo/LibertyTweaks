using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: ServalEd & catsmackaroo

namespace LibertyTweaks
{
    internal class CarRollover
    {
        private static bool enable;
        private static float BaseRollForce;
        private static float amount;
        private static float speedThreshold;
        private static float RollForce;
        private static bool CheckDateTime;
        private static DateTime currentDateTime;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            CarRollover.section = section;
            enable = settings.GetBoolean(section, "Hollywood Rollover", false);
            amount = settings.GetFloat(section, "Hollywood Rollover - Force", 0.2f);
            speedThreshold = settings.GetFloat(section, "Hollywood Rollover - Speed Threshold", 30f);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE()) return;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || IS_PAUSE_MENU_ACTIVE()
                || IS_CHAR_IN_ANY_BOAT(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                return;
            }

            if (CheckDateTime == false)
            {
                currentDateTime = DateTime.Now;
                CheckDateTime = true;
            }

            GET_CAR_SPEED(Main.PlayerVehicle.GetHandle(), out float speed);

            if (speed <= speedThreshold)
                return;

            if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > 75)
            {
                CheckDateTime = false;

                GET_CAR_MODEL(Main.PlayerVehicle.GetHandle(), out var pValue);
                GET_MODEL_DIMENSIONS(pValue, out var pMinVector, out var pMaxVector);

                BaseRollForce = Main.PlayerVehicle.GetSpeedVector(true).X * (amount / Main.PlayerVehicle.Handling.TractionCurveMin);

                GET_CAR_MODEL(Main.PlayerVehicle.GetHandle(), out uint vehModel);
                eWeather Wthr = NativeWorld.CurrentWeather;
                if (Wthr == eWeather.WEATHER_RAINING || Wthr == eWeather.WEATHER_LIGHTNING)
                    RollForce = BaseRollForce * 1.35f;
                else if (Wthr == eWeather.WEATHER_DRIZZLE)
                    RollForce = BaseRollForce * 1.3f;
                else
                    RollForce = BaseRollForce;

                float sideSpeed = Main.PlayerVehicle.GetSpeedVector(true).X;
                float frontSpeed = Main.PlayerVehicle.GetSpeedVector(true).Y;

                if (!IS_CAR_IN_AIR_PROPER(Main.PlayerVehicle.GetHandle())
                    && PlayerHelper.IsPlayerSkidding()
                    && Math.Abs(frontSpeed) <= speedThreshold + 10
                    && Math.Abs(sideSpeed) >= speedThreshold)
                {
                    Main.PlayerVehicle.ApplyForceRelative(new Vector3(RollForce, 0, 0), new Vector3(0, 0, pMaxVector.Z * 0.5f));
                    Main.PlayerVehicle.ApplyForceRelative(new Vector3(-RollForce, 0, 0), new Vector3(0, 0, pMaxVector.Z * -0.5f));
                }
            }
        }
    }
}

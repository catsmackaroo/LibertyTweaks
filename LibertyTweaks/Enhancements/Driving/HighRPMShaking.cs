using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class HighRPMShaking
    {
        private static bool enable;
        private const float rpmThreshold = 0.7f;
        private const float shakeIntensity = 0.025f;
        private const float forceThreshold = 0.19f;

        private static Random random = new Random();
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            HighRPMShaking.section = section;
            enable = settings.GetBoolean(section, "RPM Shake", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (Main.PlayerPed == null
                || !IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || IS_PAUSE_MENU_ACTIVE()
                || IS_CHAR_IN_ANY_BOAT(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
                return;

            GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int vehicle);
            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            if (vehicleIV == null) return;

            float rpm = vehicleIV.EngineRPM;
            if (rpm < rpmThreshold) return;
            float force = vehicleIV.Handling.DriveForce;

            if (force < forceThreshold) return;

            float shakeForce = shakeIntensity * (rpm / rpmThreshold);
            bool isbike = IS_CHAR_ON_ANY_BIKE(Main.PlayerPed.GetHandle());
            if (isbike)
                shakeForce /= 2;

            Vector3 randomShake = new Vector3(
                (float)(random.NextDouble() - 0.5) * shakeForce,
                (float)(random.NextDouble() - 0.5) * shakeForce,
                0
            );
            vehicleIV.ApplyForce(randomShake, Vector3.Zero);
        }
    }
}

using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class DrivebyInPolice
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DrivebyInPolice.section = section;
            enable = settings.GetBoolean(section, "Driveby in Stationary Police Car", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (Main.PlayerPed == null || !IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int vehicle);
            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            GET_CAR_SPEED(vehicle, out float speed);
            if (vehicleIV == null || speed <= 0f) return;

            if (vehicleIV.VehicleFlags3.PoliceVehicle == true)
                Main.PlayerPed.PlayerInfo.CanDoDriveby = 1;
        }
    }
}

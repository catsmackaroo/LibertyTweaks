using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class CarExplosionsRandomized
    {
        private static bool enable;
        private static readonly HashSet<int> attachedVehicles = new HashSet<int>();
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            CarExplosionsRandomized.section = section;
            enable = settings.GetBoolean(section, "Randomized Explosions", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            foreach (var veh in PedHelper.VehHandles)
            {
                int vehicleHandle = veh.Value;  

                if (attachedVehicles.Contains(vehicleHandle))
                    continue;

                if (vehicleHandle == Main.PlayerVehicle.GetHandle())
                {
                    attachedVehicles.Add(vehicleHandle);
                    continue;
                }

                if (IS_CAR_ON_FIRE(vehicleHandle))
                {
                    int rndTimer = Main.GenerateRandomNumber(-999, 0);
                    SET_PETROL_TANK_HEALTH(vehicleHandle, rndTimer);
                    attachedVehicles.Add(vehicleHandle);
                }
            }
        }
    }
}

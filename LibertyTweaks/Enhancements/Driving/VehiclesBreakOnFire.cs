using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credit: catsmackaroo

namespace LibertyTweaks
{
    internal class VehiclesBreakOnFire
    {
        private static bool enable;
        private static readonly List<int> attachedVehicles = new List<int>();
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            VehiclesBreakOnFire.section = section;
            enable = settings.GetBoolean(section, "Break on Fire", false);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
                return;

            IVPool vehPool = IVPools.GetVehiclePool();
            for (int i = 0; i < vehPool.Count; i++)
            {
                UIntPtr ptr = vehPool.Get(i);

                if (ptr != UIntPtr.Zero)
                {
                    IVVehicle v = IVVehicle.FromUIntPtr(ptr);

                    if (!attachedVehicles.Contains(v.GetHandle()) && IS_CAR_ON_FIRE(v.GetHandle()))
                    {

                        SET_CAR_ENGINE_ON(v.GetHandle(), false, false);
                        attachedVehicles.Add(v.GetHandle());
                    }
                }
            }
        }
    }
}


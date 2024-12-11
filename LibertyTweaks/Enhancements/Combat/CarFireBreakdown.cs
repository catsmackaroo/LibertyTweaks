using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credit: catsmackaroo

namespace LibertyTweaks
{
    internal class CarFireBreakdown
    {
        private static bool enable;
        private static readonly List<int> attachedVehicles = new List<int>();
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicles Break on Fire", "Enable", true);

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
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

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


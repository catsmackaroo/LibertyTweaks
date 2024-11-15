using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using CCL.GTAIV;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class CarsMayExplode
    {
        private static bool enable;
        private static readonly List<int> attachedVehicles = new List<int>();

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicles May Explode on Fire", "Enable", true);

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
                        int rnd = Main.GenerateRandomNumber(0, 3);

                        if (rnd == 1)
                            EXPLODE_CAR(v.GetHandle(), true, false);

                        attachedVehicles.Add(v.GetHandle());
                    }
                }
            }
        }
    }
}

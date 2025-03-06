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
        private static readonly List<int> attachedVehicles = new List<int>();

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Randomized Car Explosions", "Enable", true);

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

                    UIntPtr pVuIntPtr = Main.PlayerPed.GetVehicle();
                    IVVehicle pV = IVVehicle.FromUIntPtr(pVuIntPtr);
                    if (v == pV)
                        attachedVehicles.Add(v.GetHandle());

                    if (!attachedVehicles.Contains(v.GetHandle()) && IS_CAR_ON_FIRE(v.GetHandle()))
                    {
                        // An immediate car explosion system
                        int rndImmediate = Main.GenerateRandomNumber(0, 3);
                        if (rndImmediate == 3 && v != pV)
                        {
                            EXPLODE_CAR(v.GetHandle(), true, false);
                            attachedVehicles.Add(v.GetHandle());
                        }

                        // A more randomized car explosion system
                        int rndTimer = Main.GenerateRandomNumber(-999, 0);

                        SET_PETROL_TANK_HEALTH(v.GetHandle(), rndTimer);
                        attachedVehicles.Add(v.GetHandle());
                    }
                }
            }
        }
    }
}

using System;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class HigherPedAccuracy
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved AI", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick(int pedAccuracy, int pedFirerate)
        {
            if (!enable)
                return;


            // Grab all peds
            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {

                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    SET_CHAR_ACCURACY(pedHandle, (uint)pedAccuracy);
                    SET_CHAR_SHOOT_RATE(pedHandle, pedFirerate);
                }
            }
        }
    }
}

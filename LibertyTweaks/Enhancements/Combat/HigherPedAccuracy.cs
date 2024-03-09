using System;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class HigherPedAccuracy
    {
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Improved AI", "Enable", true);
        }

        public static void Tick(int pedAccuracy, int pedFirerate)
        {
            if (!enableFix)
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

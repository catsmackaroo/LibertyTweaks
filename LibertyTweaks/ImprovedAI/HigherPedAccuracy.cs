using System;
using System.Numerics;

using CCL.GTAIV;

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
            enableFix = settings.GetBoolean("Main", "Improved AI", true);
        }

        public static void Tick(int pedAccuracy, int pedFirerate)
        {
            if (!enableFix)
                return;

            //GET_CURRENT_BASIC_COP_MODEL(out uint copModel);

            // Grab all peds
            CPool pedPool = CPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    //GET_CHAR_MODEL(pedHandle, out uint pedModel);

                    SET_CHAR_ACCURACY(pedHandle, (uint)pedAccuracy);
                    SET_CHAR_SHOOT_RATE(pedHandle, pedFirerate);

                    //if (pedModel == copModel)
                    //{
                    //    SET_CHAR_ACCURACY(pedHandle, (uint)Main.GenerateRandomNumber(80, 100));
                    //    SET_CHAR_SHOOT_RATE(pedHandle, Main.GenerateRandomNumber(95, 100));
                    //}
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: cock :shyneed:

namespace LibertyTweaks
{
    internal class DeathBlips
    {

        private static bool enable;
        private static List<int> corpseBlipsCount = new List<int>();

        public static void Init(SettingsFile settings)
        {

            enable = settings.GetBoolean("Main", "Death Blips", true);
        }

        public static void Tick()
        {

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    GET_CHAR_COORDINATES(pedHandle, out Vector3 pedCoords);

                    if (corpseBlipsCount.Contains(pedHandle))
                        continue;

                    if (!IS_CHAR_DEAD(pedHandle))
                        continue;

                    if (Vector3.Distance(playerPed.Matrix.Pos, pedCoords) > 10f)
                        continue;

                    NativeBlip.AddBlip(pedCoords);
                    corpseBlipsCount.Add(pedHandle);
                }
            }

            for (int i = 0; i < corpseBlipsCount.Count; i++)
            {
                int pedHandle = corpseBlipsCount[i];

                // Check if ped still exists
                if (!DOES_CHAR_EXIST(pedHandle))
                    corpseBlipsCount.RemoveAt(i); // Remove ped from list because they dont exists anymore
            }
        }
    }
}

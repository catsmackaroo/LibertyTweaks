using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace LibertyTweaks
{
    internal class PedHelper
    {
        public static Dictionary<UIntPtr, int> PedHandles { get; private set; } = new Dictionary<UIntPtr, int>();

        public static void GrabAllPeds()
        {
            PedHandles.Clear();

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);

                if (ptr != UIntPtr.Zero && ptr != Main.PlayerPed.GetUIntPtr())
                {
                    int pedHandle = (int)pedPool.GetIndex(ptr);
                    PedHandles[ptr] = pedHandle;
                }
            }
        }
    }
}


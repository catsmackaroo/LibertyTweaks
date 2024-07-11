using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using CCL.GTAIV;


namespace LibertyTweaks
{
    public class PlayerChecks
    {
        public bool IsPlayerInOrNearCombat()
        {
            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    if (IS_PED_IN_COMBAT(pedHandle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsPlayerSeenByPolice()
        {
            IVPool pedPool = IVPools.GetPedPool();
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);

                if (ptr == UIntPtr.Zero || ptr == IVPlayerInfo.FindThePlayerPed())
                    continue;

                int pedHandle = (int)pedPool.GetIndex(ptr);
                IVPed pedPed = NativeWorld.GetPedInstaceFromHandle(pedHandle);

                if (pedPed == null || IS_CHAR_DEAD(pedHandle))
                    continue;

                GET_CHAR_MODEL(pedHandle, out uint pedModel);
                GET_CURRENT_BASIC_COP_MODEL(out uint copModel);

                if (pedModel != copModel)
                    continue;

                if (pedPed.CanCharSeeChar(playerPed, 50, 130))
                    return true;
            }

            return false;
        }
    }
}

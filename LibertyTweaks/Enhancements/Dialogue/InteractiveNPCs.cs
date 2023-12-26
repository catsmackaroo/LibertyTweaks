using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Numerics;

namespace LibertyTweaks
{
    internal class InteractiveNPCs
    {
        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("More Dialogue", "Interactive NPCs", true);
        }
        public static void Tick()
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
                    // Ignore player ped
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    // Grab player ID & ped
                    uint playerId = GET_PLAYER_ID();
                    IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

                    // Get ped handles
                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    // Get the model of the ped
                    GET_CHAR_MODEL(pedHandle, out uint pedModel);

                    GET_PED_TYPE(pedHandle, out uint pedType);
                }
            }

        }
    }
}

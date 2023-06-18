using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: cock

namespace LibertyTweaks.DeathBlips
{
    internal class DeathBlips
    {
        public static void Init(SettingsFile settings)
        {

        }

        public static void Tick()
        {
            int playerHandle;
            uint playerId;

            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed());
            playerHandle = CPedExtensions.GetHandle(playerPed);
            playerId = GET_PLAYER_ID();

            CPool pedPool = CPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    int pedHandle = (int)pedPool.GetIndex(ptr);
                    CPed pedsCPeds = CPed.FromPointer(ptr);

                    GET_CHAR_COORDINATES(pedHandle, out Vector3 pedCoords);

                    if (IS_CHAR_DEAD(pedHandle)) 
                    {
                        NativeBlip.AddBlip(pedCoords);
                        continue;
                    }
                }
            }
        }
    }
}

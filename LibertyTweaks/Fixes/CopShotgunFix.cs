using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using System.Numerics;
using CCL.GTAIV;
using IVSDKDotNet.Native;

namespace LibertyTweaks
{
    internal class CopShotgunFix
    {
        public static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Cop Shotgun Fix", true);
        }

        public static void Tick()
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
                    GET_CHAR_MODEL(pedHandle, out uint pedModel);
                    Natives.GET_CURRENT_CHAR_WEAPON(pedHandle, out uint currentPedWeapon);

                    if (pedModel == 4111764146 || pedModel == 2776029317)
                    {
                        if (currentPedWeapon == 10)
                        {
                            GIVE_WEAPON_TO_CHAR(pedHandle, 11, 30, false);
                        }
                    }
                    
                }
            }
        }
    }
}
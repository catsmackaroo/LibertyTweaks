using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using IVSDKDotNet.Native;

namespace LibertyTweaks
{
    internal class CopShotgunFix
    {
        public static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Cop Shotgun Fix", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

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
                    Natives.GET_CURRENT_CHAR_WEAPON(pedHandle, out int currentPedWeapon);

                    if (pedModel == 4111764146 || pedModel == 2776029317 || pedModel == 4205665177 || pedModel == 3295460374)
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
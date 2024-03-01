using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;

namespace LibertyTweaks
{
    internal class RandomNoEuphoria
    {
        private static bool enable;


        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Less Euphoria", "Enable", true);
        }

        public static void Tick()
        {
            if (!enable)
                return;

            bool pedCombat;
            bool pedRagdoll;

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);

                if (ptr != UIntPtr.Zero)
                {
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    // Get ped handles & info
                    int pedHandle = (int)pedPool.GetIndex(ptr);
                    IVPed thePed = NativeWorld.GetPedInstaceFromHandle(pedHandle);
                    pedCombat = IS_PED_IN_COMBAT(pedHandle);
                    pedRagdoll = IS_PED_RAGDOLL(pedHandle);
                    GET_CHAR_HEALTH(pedHandle, out uint pedhealth);
                    GET_CHAR_MODEL(pedHandle, out uint pedModel);

                    //Ignore noose
                    if (pedModel == 3290204350)
                        continue;

                    //Prevent visual bugs
                    if (IS_CHAR_DEAD(pedHandle))
                    {
                        thePed.PreventRagdoll(false);
                        continue;
                    }

                    if (IS_CHAR_ON_FIRE(pedHandle))
                        continue;

                    if (!IS_PED_IN_COMBAT(pedHandle))
                        continue;

                    switch (Main.GenerateRandomNumber(0, 3))
                    {
                        case 0:
                            if (pedRagdoll == false)
                            {
                                if (pedhealth > 60)
                                {
                                    thePed.PreventRagdoll(true);
                                }
                                else
                                {
                                    thePed.PreventRagdoll(false);
                                }
                            }
                            break;

                        default:
                            thePed.PreventRagdoll(false);
                            break;
                    }
                }
            }
        }
    }
}

using IVSDKDotNet;
using System;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;
using System.Numerics;
using CCL.GTAIV;
using System.Windows.Forms;
using IVSDKDotNet.Enums;
using CCL.GTAIV.AnimationController;

namespace LibertyTweaks
{
    internal class StunPunch
    {
        private static bool enable;
        private static DateTime timer = DateTime.MinValue;
        private static bool bVar3;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Stun Punch", "Enable", true);
        }
        public static void Tick(DateTime timer)
        {
            if (!enable)
                return;

            bVar3 = false;

            if (StunPunch.timer == DateTime.MinValue)
                StunPunch.timer = DateTime.UtcNow;

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);

                if (ptr != UIntPtr.Zero)
                {
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    uint playerId = GET_PLAYER_ID();
                    IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
                    int timerStart;
                    timerStart = 0; 
                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    if (IS_PED_IN_COMBAT(playerPed.GetHandle()))
                        continue;

                    if (IS_CHAR_IN_ANY_CAR(pedHandle))
                        continue;

                    if (IS_CHAR_DEAD(pedHandle))
                        continue;

                    if (!IS_PLAYER_TARGETTING_CHAR((int)playerId, pedHandle))
                        continue;

                    SET_CHAR_READY_TO_BE_STUNNED(pedHandle, true);

                    if (!IS_CHAR_INJURED(pedHandle))
                    {
                        if (GET_CHAR_READY_TO_BE_STUNNED(pedHandle)) 
                        {
                            if (HAVE_ANIMS_LOADED("melee_unarmed_base"))
                            {
                                if (IS_PED_IN_COMBAT(pedHandle) && !IS_CHAR_PLAYING_ANIM(playerPed.GetHandle(), "melee_unarmed_base", "stun_punch"))
                                {
                                    bVar3 = true;
                                }
                                if (IS_CHAR_IN_MELEE_COMBAT(pedHandle) &&  !IS_CHAR_PLAYING_ANIM(playerPed.GetHandle(), "melee_unarmed_base", "stun_punch"))
                                {
                                    bVar3 = true;
                                }
                            }
                            if (IS_PLAYER_TARGETTING_CHAR((int)playerId, pedHandle) && IS_CHAR_ARMED(pedHandle, 4)) 
                            {
                                bVar3 = true;
                            }

                            if ((IS_PLAYER_FREE_AIMING_AT_CHAR((int)playerId, pedHandle) && IS_CHAR_ARMED(pedHandle, 4)))
                            {
                                bVar3 = true;
                            }

                            if (HAS_CHAR_BEEN_DAMAGED_BY_CHAR(pedHandle, playerPed.GetHandle(), false))
                            {
                                bVar3 = true;
                            }
                        }
                    }

                    if (bVar3)
                    {
                        if (!IS_CHAR_INJURED(pedHandle))
                        {
                            SET_CHAR_READY_TO_BE_STUNNED(pedHandle, false);
                        }
                    }

                    if (IS_CHAR_PLAYING_ANIM(playerPed.GetHandle(), "melee_unarmed_base", "stun_punch"))
                    {
                        TRIGGER_PTFX_ON_PED_BONE("blood_stun_punch", pedHandle, 0.0f, 0.12f, 0.0f, 0.0f, 0.0f, 0.0f, 43254, 0);
                        _TASK_PLAY_ANIM_NON_INTERRUPTABLE(pedHandle, "Hit_Jab", "melee_unarmed_base", 8.00000000f, 0, 0, 0, 0, -1);

                        if (HAS_CHAR_ANIM_FINISHED(playerPed.GetHandle(), "melee_unarmed_base", "stun_punch"))
                        {
                            SET_CHAR_HEALTH(pedHandle, 99);
                        }

                        //IVPed thePed = NativeWorld.GetPedInstaceFromHandle(pedHandle);
                        //thePed.GetAnimationController();
                        //PedAnimationController.Play("melee_unarmed_base", "Hit_Jab", 1f, 0);
                        //_TASK_PLAY_ANIM_NON_INTERRUPTABLE(pedHandle, "MELEE_UNARMED_BASE", "Hit_Jab", 1.00000000f, 0, 0, 0, 0, -1);
                        //START_PTFX_ON_PED_BONE("blood_stun_punch", pedHandle, 0.00000000f, 0.13000000f, 0.00000000f, 0.00000000f, 0.00000000f, 0.00000000f, (int)eBone.BONE_HD_FACE_FOREHEAD, 1);
                        //SET_CHAR_HEALTH(pedHandle, 99);
                    }

                }
            }
        }
    }
}

using IVSDKDotNet;
using System;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;
using System.Numerics;
using CCL.GTAIV;
using System.Windows.Forms;

namespace LibertyTweaks
{
    internal class InteractiveNPCs
    {
        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("More Dialogue", "Interactive NPCs", true);
        }
        public static void ProcessPositive()
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

                    // Get the model & type of the ped
                    //GET_CHAR_MODEL(pedHandle, out uint pedModel);
                    //GET_PED_TYPE(pedHandle, out uint pedType);

                    if (IS_PED_IN_COMBAT(pedHandle))
                        continue;

                    if (IS_CHAR_IN_ANY_CAR(pedHandle))
                        continue;

                    if (IS_CHAR_DEAD(pedHandle))
                        continue;

                    if (!IS_PLAYER_TARGETTING_CHAR((int)playerId, pedHandle))
                        continue;

                    switch (Main.GenerateRandomNumber(0, 6))
                    {
                        case 0:
                            playerPed.SayAmbientSpeech("DARTS_HAPPY");
                            IVGame.ShowSubtitleMessage("DARTS_HAPPY");
                            break;

                        case 1:
                            playerPed.SayAmbientSpeech("THANKS");

                            IVGame.ShowSubtitleMessage("THX");
                            break;

                        case 2:
                            playerPed.SayAmbientSpeech("GCK2_DRUNK3");
                            IVGame.ShowSubtitleMessage("GCK");
                            break;

                    }

                }
            }

        }

        public static void ProcessNegative()
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

                    // Get the model & type of the ped
                    //GET_CHAR_MODEL(pedHandle, out uint pedModel);
                    //GET_PED_TYPE(pedHandle, out uint pedType);

                    if (IS_PED_IN_COMBAT(pedHandle))
                        continue;

                    if (IS_CHAR_IN_ANY_CAR(pedHandle))
                        continue;

                    if (IS_CHAR_DEAD(pedHandle))
                        continue;

                    if (!IS_PLAYER_TARGETTING_CHAR((int)playerId, pedHandle))
                        continue;

                    IVGame.ShowSubtitleMessage("Negative");

                }
            }

        }
    }
}

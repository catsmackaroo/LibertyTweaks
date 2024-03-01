using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using System.Numerics;
using CCL.GTAIV;

// Credits: catsmackaroo & ItsClonkAndre

// Add armor model

namespace LibertyTweaks
{
    internal class ArmoredCops
    {
        private static bool enable;
        private static bool enableBuffSWAT;
        private static List<int> copsHadArmor = new List<int>();


        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Armored Cops", true);
            enableBuffSWAT = settings.GetBoolean("Improved Police", "Buff SWAT", true);
        }

        public static void Tick(int armoredCopsStars)
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

                    // Get the models of basic policia
                    GET_CURRENT_BASIC_COP_MODEL(out uint copModel);

                    // Check player's wanted level
                    STORE_WANTED_LEVEL((int)playerId, out uint currentWantedLevel);

                    // Get ped coords
                    GET_CHAR_COORDINATES(pedHandle, out Vector3 pedCoords);

                    // Check if ped is dead
                    if (IS_CHAR_DEAD(pedHandle))
                        continue;

                    // Check if NOoSE
                    if (pedModel == 3290204350)
                    {
                        if (!enableBuffSWAT)
                            return;

                        GET_CHAR_ARMOUR(pedHandle, out uint nooseArmor);
                        IVPed noosePed = NativeWorld.GetPedInstaceFromHandle(pedHandle);

                        if (IS_CHAR_DEAD(pedHandle))
                        {
                            noosePed.PreventRagdoll(false);
                        }

                        if (nooseArmor == 0)
                        {
                            noosePed.PedFlags.NoHeadshots = false;
                            noosePed.PreventRagdoll(false);
                        }
                        else
                        {
                            GET_CHAR_PROP_INDEX(pedHandle, 0, out int pedPropIndex);

                            noosePed.PedFlags.NoHeadshots = true;

                            if (pedPropIndex == -1)
                            {
                                noosePed.PedFlags.NoHeadshots = false;
                            }

                            if (!IS_CHAR_ON_FIRE(pedHandle))
                            {
                                noosePed.PreventRagdoll(true);
                            }
                        }
                    }

                    // If player has more than 4 or more stars
                    if (currentWantedLevel < armoredCopsStars)
                        continue;

                    // Check if it's a cop
                    if (pedModel != copModel)
                        continue;

                    // Check if ped is not for a mission
                    if (IS_PED_A_MISSION_PED(pedHandle))
                        continue;

                    // Check for FatCop
                    if (pedModel == 3924571768)
                        continue;

                    // Check if the grabbed ped has already been given armor
                    if (copsHadArmor.Contains(pedHandle))
                        continue;

                    // Check distance between police & player
                    if (Vector3.Distance(playerPed.Matrix.Pos, pedCoords) < 75f)
                        continue;

                    // Finally adds armor to the policia
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsHadArmor.Add(pedHandle);
                }
            }

            // Loop through the list of peds that already got armor and check if we can delete them from the list
            for (int i = 0; i < copsHadArmor.Count; i++)
            {
                int pedHandle = copsHadArmor[i];

                // Check if ped still exists
                if (!DOES_CHAR_EXIST(pedHandle))
                    copsHadArmor.RemoveAt(i); // Remove ped from list because they dont exists anymore
            }
        }
    }
}

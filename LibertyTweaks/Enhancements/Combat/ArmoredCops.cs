using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using System.Numerics;

// Credits: catsmackaroo & ItsClonkAndre

// Create a distance check for all copmodels (if higher than 75f or so.) This'll stop nearby cops from getting armor, and thus kinda messing up immersion.
// Disable for fatmobs, FIB, & NOOSE.
// Add armor model

namespace LibertyTweaks
{
    internal class ArmoredCops
    {
        private static bool enable;
        private static List<int> copsHadArmor = new List<int>();
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Enabled", true);
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

                    // Check distance between police & player
                    if (Vector3.Distance(playerPed.Matrix.Pos, pedCoords) < 75f)
                        continue;

                    // Check if the grabbed ped has already been given armor
                    if (copsHadArmor.Contains(pedHandle))
                        continue;

                    // Check if it's a cop
                    if (pedModel != copModel)
                        continue;

                    // Check if ped is dead
                    if (IS_CHAR_DEAD(pedHandle))
                        continue;

                    // Check if ped is not for a mission
                    if (IS_PED_A_MISSION_PED(pedHandle))
                        continue;

                    // If player has more than 4 or more stars
                    if (currentWantedLevel < armoredCopsStars)
                        continue;

                    // Check for FatCop, FIB, & SWAT
                    if (pedModel == 3924571768 || pedModel == 3295460374 || pedModel == 3290204350)
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

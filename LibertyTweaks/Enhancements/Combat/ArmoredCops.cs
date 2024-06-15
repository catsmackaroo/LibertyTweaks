using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using System.Numerics;
using CCL.GTAIV;
using IVSDKDotNet.Enums;

// Credits: catsmackaroo, ItsClonkAndre, GQComms (for model)
namespace LibertyTweaks
{
    internal class ArmoredCops
    {
        private static bool enable;
        private static bool enableBuffSWAT;
        public static bool enableVests;
        private static List<int> copsHadArmor = new List<int>();
        private static int armoredCopsStars;


        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Armored Cops", true);
            enableVests = settings.GetBoolean("Improved Police", "Armored Cops Have Vests", true);
            enableBuffSWAT = settings.GetBoolean("Improved Police", "Buff SWAT", true);
            armoredCopsStars = settings.GetInteger("Improved Police", "Armored Cops Start At", 4);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void LoadFiles()
        {
            IVCDStream.AddImage("IVSDKDotNet/scripts/LibertyTweaks/ArmoredCopFiles/armoredCops.img", 1, -1);
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
                            GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out int currentWeapon);

                            if (currentWeapon == (int)eWeaponType.WEAPON_SNIPERRIFLE || currentWeapon == (int)eWeaponType.WEAPON_M40A1 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_15)
                            {
                                // Chat GPT test
                                noosePed.PedFlags.NoHeadshots = false;
                                noosePed.PreventRagdoll(false);
                            }
                            else
                            {
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
                    }

                    // With or without stars, if a cop spawns with the armor vests they will be given armor (consistency!)
                    if (enableVests == true)
                    {
                        if (pedModel == 4111764146)
                        {
                            if (GET_CHAR_DRAWABLE_VARIATION(pedHandle, 1) == 4)
                            {
                                SET_CHAR_COMPONENT_VARIATION(pedHandle, 2, 0, 0);
                                if (currentWantedLevel > armoredCopsStars)
                                    continue;

                                if (copsHadArmor.Contains(pedHandle))
                                    continue;

                                ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                                copsHadArmor.Add(pedHandle);
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

                    // Check for Fat cops
                    if (pedModel == 3924571768)
                        continue;

                    // Check for Alderney cops
                    if (pedModel == 4205665177)
                        continue;

                    // Check if the grabbed ped has already been given armor
                    if (copsHadArmor.Contains(pedHandle))
                        continue;

                    // Check distance between police & player
                    if (Vector3.Distance(playerPed.Matrix.Pos, pedCoords) < 125f)
                        continue;

                    // Finally adds armor to the policia
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);

                    // Gives cops visible armor vests if the feature is enabled
                    if (enableVests == true)
                        SET_CHAR_COMPONENT_VARIATION(pedHandle, 1, 4, 0);

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

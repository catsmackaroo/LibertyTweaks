using IVSDKDotNet;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using IVSDKDotNet.Enums;
using System.Diagnostics;
using System;

// Credits: catsmackaroo, ItsClonkAndre, GQComms

namespace LibertyTweaks
{
    internal class ArmoredCops
    {
        private static bool CheckDateTime;
        private static DateTime currentDateTime;
        private static bool enable;
        private static bool enableNoHeadshotNOoSE;
        private static bool enableNoRagdollNOoSE;
        public static bool enableVests;
        public static int ragdollTime;
        public static int ragdollTimeShotgun;

        private static readonly List<int> copsWithArmor = new List<int>();

        private static int armoredCopsStars;

        // Models
        private static uint nooseModel = 3290204350;
        private static uint policeModel = 4111764146;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Armored Cops", true);
            enableVests = settings.GetBoolean("Improved Police", "Armored Cops Have Vests", true);
            enableNoHeadshotNOoSE = settings.GetBoolean("Improved Police", "No Headshot NOoSE", true);
            enableNoRagdollNOoSE = settings.GetBoolean("Improved Police", "No Ragdoll NOoSE", true);
            ragdollTime = settings.GetInteger("Improved Police", "NOoSE Ragdoll Time", 100);
            ragdollTimeShotgun = settings.GetInteger("Improved Police", "NOoSE Shotgun Ragdoll Time", 250);
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

            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;

                if (IS_CHAR_DEAD(pedHandle))
                    continue;

                GET_CHAR_MODEL(pedHandle, out uint pedModel);

                if (pedModel == nooseModel)
                    HandleNOoSEBehavior(pedHandle);
                else if (pedModel == policeModel && enable)
                    HandleGeneralPoliceBehavior(pedHandle);
            }

            RemoveInvalidCops();
        }

        private static void HandleNOoSEBehavior(int pedHandle)
        {
            // Return early if both features are disabled
            if (!enableNoHeadshotNOoSE && !enableNoRagdollNOoSE)
                return;

            // Grab IVPed version of noose ped
            IVPed noosePed = NativeWorld.GetPedInstanceFromHandle(pedHandle);

            if (noosePed == null)
                return;

            // Disable ragdoll on death
            if (IS_CHAR_DEAD(pedHandle))
            {
                noosePed.PreventRagdoll(false);
                return;
            }

            // Determine if player is using sniper, disabling if true
            bool isSniperWeapon = IsSniperWeapon();
            if (isSniperWeapon)
            {
                noosePed.PedFlags.NoHeadshots = false;
                noosePed.PreventRagdoll(false);
                return;
            }

            // If armor is low, disable both features
            GET_CHAR_ARMOUR(pedHandle, out uint armor);
            if (armor <= 50)
            {
                noosePed.PedFlags.NoHeadshots = false;
                noosePed.PreventRagdoll(false);
                return;
            }

            // Check for headgear
            GET_CHAR_PROP_INDEX(pedHandle, 0, out int headgearIndex);

            // Handle individual features
            if (enableNoHeadshotNOoSE)
                noosePed.PedFlags.NoHeadshots = headgearIndex != -1; // Enable if ped has headgear

            if (enableNoRagdollNOoSE && !IS_CHAR_ON_FIRE(pedHandle) && noosePed.GetHeightAboveGround() < 3 && IS_PED_RAGDOLL(pedHandle) && HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 57))
            {
                if (CheckDateTime == false)
                {
                    currentDateTime = DateTime.Now;
                    CheckDateTime = true;
                }

                if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > ragdollTime && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 10) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 11) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 22) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 26) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 30) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 31))
                {
                    CheckDateTime = false;
                    if (!HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 49) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 50) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 51) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 54) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 55))
                    {
                        SWITCH_PED_TO_ANIMATED(pedHandle, false);
                    }
                    CLEAR_CHAR_LAST_WEAPON_DAMAGE(pedHandle);
                }

                else if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > ragdollTimeShotgun)
                {
                    CheckDateTime = false;
                    if (!HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 49) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 50) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 51) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 54) && !HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 55))
                    {
                        SWITCH_PED_TO_ANIMATED(pedHandle, false);
                    }
                    CLEAR_CHAR_LAST_WEAPON_DAMAGE(pedHandle);
                }
            }
        }

        // Determine if player is using sniper
        private static bool IsSniperWeapon()
        {
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);
            return currentWeapon == (int)eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_15;
        }

        private static void HandleGeneralPoliceBehavior(int pedHandle)
        {
            // If cop already had armor, don't run further code
            if (copsWithArmor.Contains(pedHandle))
                return;

            // While wanted
            STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);
            if (currentWantedLevel >= armoredCopsStars)
            {
                // Suppress FatCop so it shouldn't spawn as often if the player gets stars too quickly.
                // Seems to help but doesn't remove them completely?
                SUPPRESS_PED_MODEL(3924571768);

                // If the added vests aren't enabled, it'll give them armor regardless
                if (enableVests)
                    SET_CHAR_COMPONENT_VARIATION(pedHandle, 1, 4, 0);
                else
                {
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsWithArmor.Add(pedHandle);
                }
                    
                // Only add armor if they have vest to prevent them getting armor mid-fight
                if (GET_CHAR_DRAWABLE_VARIATION(pedHandle, 1) == 4 && enableVests)
                {
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsWithArmor.Add(pedHandle);
                }
                else
                    copsWithArmor.Add(pedHandle);
            }
            // Not wanted, if cop spawns with vest. Game gives them vest automatically somehow
            else
            {
                if (GET_CHAR_DRAWABLE_VARIATION(pedHandle, 1) == 4 && enableVests)
                {
                    SET_CHAR_COMPONENT_VARIATION(pedHandle, 2, 0, 0);
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsWithArmor.Add(pedHandle);
                }
                else
                    // Add all cops to list regardless to prevent them getting armor mid-fight
                    copsWithArmor.Add(pedHandle);
            }
        }

        private static void RemoveInvalidCops()
        {
            for (int i = copsWithArmor.Count - 1; i >= 0; i--)
            {
                int pedHandle = copsWithArmor[i];
                if (!DOES_CHAR_EXIST(pedHandle))
                    copsWithArmor.RemoveAt(i);
            }
        }
    }
}

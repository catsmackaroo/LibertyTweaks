using IVSDKDotNet;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using IVSDKDotNet.Enums;
using System.Diagnostics;

// Credits: catsmackaroo, ItsClonkAndre, GQComms

namespace LibertyTweaks
{
    internal class ArmoredCops
    {
        private static bool enable;
        private static bool enableNoHeadshotNOoSE;
        private static bool enableNoRagdollNOoSE;
        public static bool enableVests;

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

            if (enableNoRagdollNOoSE && !IS_CHAR_ON_FIRE(pedHandle))
                noosePed.PreventRagdoll(true);
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

                if (enableVests)
                    SET_CHAR_COMPONENT_VARIATION(pedHandle, 1, 4, 0);

                ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                copsWithArmor.Add(pedHandle);
            }
            // Not wanted if cop spawns with vest randomly
            else
            {
                if (GET_CHAR_DRAWABLE_VARIATION(pedHandle, 1) == 4 && enableVests)
                {
                    SET_CHAR_COMPONENT_VARIATION(pedHandle, 2, 0, 0);
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsWithArmor.Add(pedHandle);
                }
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

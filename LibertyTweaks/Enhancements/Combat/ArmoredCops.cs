using IVSDKDotNet;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using IVSDKDotNet.Enums;

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

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Improved Police", "Armored Cops", true);
            enableVests = settings.GetBoolean("Improved Police", "Armored Cops Have Vests", true);
            enableNoHeadshotNOoSE = settings.GetBoolean("Improved Police", "No Headshot NOoSE", true);
            enableNoRagdollNOoSE = settings.GetBoolean("Improved Police", "No Ragdoll NOoSE", true);
            armoredCopsStars = settings.GetInteger("Improved Police", "Armored Cops Start At", 4);

            if (enable)
                Main.Log("ArmoredCops script initialized...");
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

                if (pedModel == 3290204350) // NOoSE
                    HandleNOoSEBehavior(pedHandle);
                else if (pedModel == 4111764146) // General Police
                    HandleGeneralPoliceBehavior(pedHandle);
            }

            RemoveInvalidCops();
        }

        private static void HandleNOoSEBehavior(int pedHandle)
        {
            if (!enableNoHeadshotNOoSE && !enableNoRagdollNOoSE)
                return;

            IVPed noosePed = NativeWorld.GetPedInstanceFromHandle(pedHandle);
            GET_CHAR_ARMOUR(pedHandle, out uint nooseArmor);

            if (IS_CHAR_DEAD(pedHandle))
                noosePed.PreventRagdoll(false);
            else if (nooseArmor <= 50)
            {
                noosePed.PedFlags.NoHeadshots = false;
                noosePed.PreventRagdoll(false);
            }
            else
            {
                GET_CHAR_PROP_INDEX(pedHandle, 0, out int pedPropIndex);
                GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);

                bool isSniperWeapon = currentWeapon == (int)eWeaponType.WEAPON_SNIPERRIFLE
                                      || currentWeapon == (int)eWeaponType.WEAPON_M40A1
                                      || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_15;

                if (isSniperWeapon)
                {
                    noosePed.PedFlags.NoHeadshots = false;
                    noosePed.PreventRagdoll(false);
                }
                else
                {
                    if (enableNoHeadshotNOoSE)
                        noosePed.PedFlags.NoHeadshots = pedPropIndex != -1;

                    if (enableNoRagdollNOoSE && !IS_CHAR_ON_FIRE(pedHandle))
                        noosePed.PreventRagdoll(true);
                }
            }
        }

        private static void HandleGeneralPoliceBehavior(int pedHandle)
        {
            if (!enableVests || GET_CHAR_DRAWABLE_VARIATION(pedHandle, 1) != 4)
                return;

            SET_CHAR_COMPONENT_VARIATION(pedHandle, 2, 0, 0);
            STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);

            if (currentWantedLevel > armoredCopsStars || copsWithArmor.Contains(pedHandle))
                return;

            ADD_ARMOUR_TO_CHAR(pedHandle, 100);
            copsWithArmor.Add(pedHandle);
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

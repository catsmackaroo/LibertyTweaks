using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo, ItsClonkAndre, GQComms, ServalEd

namespace LibertyTweaks
{
    internal class ArmoredCops
    {
        private static bool enableArmored;
        private static bool enableNoHeadshotNOoSE;
        private static bool enableLessRagdollNOoSE;
        public static bool loadVests;
        public static int ragdollTime;
        public static int ragdollTimeShotgun;

        private static readonly HashSet<int> copsWithArmor = new HashSet<int>();

        private static int armoredCopsStars;

        // Models
        private const uint nooseModel = 3290204350;
        private const uint policeModel = 4111764146;

        // Weapon Types
        private static readonly int[] nonRagdollWeapons = { 10, 11, 22, 26, 30, 31 };
        private static readonly int[] nonRagdollShotgunWeapons = { 49, 50, 51, 54, 55 };

        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            ArmoredCops.section = section;
            enableArmored = settings.GetBoolean(section, "Armored Cops", false);
            loadVests = settings.GetBoolean(section, "Armored Cops - Load Vests", false);
            armoredCopsStars = settings.GetInteger(section, "Armored Cops - Start At Star", 4);
            enableNoHeadshotNOoSE = settings.GetBoolean(section, "No Headshot Armored NOoSE", false);
            enableLessRagdollNOoSE = settings.GetBoolean(section, "Less NOoSE Ragdoll", false);
            ragdollTime = settings.GetInteger(section, "Less NOoSE Ragdoll - Default Time MS", 100);
            ragdollTimeShotgun = settings.GetInteger(section, "Less NOoSE Ragdoll - Shotgun Time MS", 250);

            if (enableArmored)
                Main.Log("Armored Cops enabled...");

            if (enableNoHeadshotNOoSE)
                Main.Log("No Headshot Armored NOoSE enabled...");

            if (enableLessRagdollNOoSE)
                Main.Log("Less NOoSE Ragdoll enabled...");

        }

        public static void LoadFiles()
        {
            if (!loadVests)
                return;

            IVCDStream.AddImage("IVSDKDotNet/scripts/LibertyTweaks/ArmoredCopFiles/armoredCops.img", 1, -1);
            Main.Log("loaded armoredCops.img...");
        }

        public static void Tick()
        {
            if (!enableArmored && !enableLessRagdollNOoSE && !enableNoHeadshotNOoSE)
                return;

            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;

                if (IS_CHAR_DEAD(pedHandle))
                    continue;

                GET_CHAR_MODEL(pedHandle, out uint pedModel);

                if (pedModel == nooseModel)
                    HandleNOoSEBehavior(pedHandle);
                else if (pedModel == policeModel)
                    HandleArmoredCops(pedHandle);
            }

            RemoveInvalidCops();
        }

        private static void HandleNOoSEBehavior(int pedHandle)
        {
            if (!enableNoHeadshotNOoSE && !enableLessRagdollNOoSE)
                return;

            IVPed noosePed = NativeWorld.GetPedInstanceFromHandle(pedHandle);

            if (noosePed == null)
                return;

            if (IS_CHAR_DEAD(pedHandle))
            {
                noosePed.PreventRagdoll(false);
                return;
            }

            if (IsSniperWeapon())
            {
                noosePed.PedFlags.NoHeadshots = false;
                noosePed.PreventRagdoll(false);
                return;
            }

            GET_CHAR_ARMOUR(pedHandle, out uint armor);
            if (armor <= 50)
            {
                noosePed.PedFlags.NoHeadshots = false;
                noosePed.PreventRagdoll(false);
                return;
            }

            GET_CHAR_PROP_INDEX(pedHandle, 0, out int headgearIndex);

            if (enableNoHeadshotNOoSE)
                noosePed.PedFlags.NoHeadshots = headgearIndex != -1;

            if (enableLessRagdollNOoSE && !IS_CHAR_ON_FIRE(pedHandle) && noosePed.GetHeightAboveGround() < 3 && IS_PED_RAGDOLL(pedHandle) && HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 57))
            {
                HandleRagdollBehavior(pedHandle);
            }
        }

        private static void HandleRagdollBehavior(int pedHandle)
        {
            int delay = HasBeenDamagedByWeapons(pedHandle, nonRagdollWeapons) ? ragdollTimeShotgun : ragdollTime;

            Main.TheDelayedCaller.Add(TimeSpan.FromMilliseconds(delay), "Main", () =>
            {
                if (!HasBeenDamagedByWeapons(pedHandle, nonRagdollShotgunWeapons))
                    SWITCH_PED_TO_ANIMATED(pedHandle, false);
                CLEAR_CHAR_LAST_WEAPON_DAMAGE(pedHandle);
            });
        }

        private static bool HasBeenDamagedByWeapons(int pedHandle, int[] weapons)
        {
            foreach (var weapon in weapons)
            {
                if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, weapon))
                    return true;
            }
            return false;
        }

        private static bool IsSniperWeapon()
        {
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);
            return currentWeapon == (int)eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_15;
        }

        private static void HandleArmoredCops(int pedHandle)
        {
            if (copsWithArmor.Contains(pedHandle) || !enableArmored)
                return;

            STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);
            if (currentWantedLevel >= armoredCopsStars)
            {
                SUPPRESS_PED_MODEL(3924571768);

                if (loadVests)
                    SET_CHAR_COMPONENT_VARIATION(pedHandle, 1, 4, 0);
                else
                {
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsWithArmor.Add(pedHandle);
                }

                if (GET_CHAR_DRAWABLE_VARIATION(pedHandle, 1) == 4 && loadVests)
                {
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsWithArmor.Add(pedHandle);
                }
                else
                    copsWithArmor.Add(pedHandle);
            }
            else
            {
                if (GET_CHAR_DRAWABLE_VARIATION(pedHandle, 1) == 4 && loadVests)
                {
                    SET_CHAR_COMPONENT_VARIATION(pedHandle, 2, 0, 0);
                    ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                    copsWithArmor.Add(pedHandle);
                }
                else
                    copsWithArmor.Add(pedHandle);
            }
        }

        private static void RemoveInvalidCops()
        {
            copsWithArmor.RemoveWhere(pedHandle => !DOES_CHAR_EXIST(pedHandle));
        }
    }
}

using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class RandomizedPedEuphoria
    {
        private static bool enable;
        public static int ragdollTimeMin;
        public static int ragdollTimeMax;
        public static int ragdollTimeShotgun;
        public static int healthThreshold;

        // Weapon Types
        private static readonly int[] nonRagdollWeapons = { 10, 11, 22, 26, 30, 31 };
        private static readonly int[] nonRagdollShotgunWeapons = { 49, 50, 51, 54, 55 };

        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            RandomizedPedEuphoria.section = section;
            enable = settings.GetBoolean(section, "Randomized Ped Euphoria", false);
            ragdollTimeMin = settings.GetInteger(section, "Randomized Ped Euphoria - Ragdoll Time Min", 300);
            ragdollTimeMax = settings.GetInteger(section, "Randomized Ped Euphoria - Ragdoll Time Max", 900);
            ragdollTimeShotgun = settings.GetInteger(section, "Randomized Ped Euphoria - Shotgun Time", 1000);
            healthThreshold = settings.GetInteger(section, "Randomized Ped Euphoria - Health Threshold", 130);

            if (enable)
                Main.Log("script initialized...");
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

                GET_PED_TYPE(pedHandle, out uint type);
                if (type == (uint)ePedType.PED_TYPE_CIV_MALE || type == (uint)ePedType.PED_TYPE_CIV_FEMALE || type == (uint)ePedType.PED_TYPE_BUM)
                    continue;

                GET_CHAR_HEIGHT_ABOVE_GROUND(pedHandle, out float height);
                if (!IS_CHAR_ON_FIRE(pedHandle) && IS_PED_RAGDOLL(pedHandle) && HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 57) && height < 2)
                {
                    HandleRagdollBehavior(pedHandle);
                }
            }
        }
        private static void HandleRagdollBehavior(int pedHandle)
        {
            int delay = HasBeenDamagedByWeapons(pedHandle, nonRagdollWeapons) ? ragdollTimeShotgun : Main.GenerateRandomNumber(ragdollTimeMin, ragdollTimeMax);
            IVPed ivPedHandle = NativeWorld.GetPedInstanceFromHandle(pedHandle);

            GET_CHAR_ARMOUR(pedHandle, out uint armor);
            if (armor > 0)
                delay /= 2;

            GET_CHAR_LAST_DAMAGE_BONE(pedHandle, out int lastDamageBone);

            if (lastDamageBone == (int)eBone.BONE_LEFT_CALF
                || lastDamageBone == (int)eBone.BONE_RIGHT_CALF
                || lastDamageBone == (int)eBone.BONE_LEFT_FOOT
                || lastDamageBone == (int)eBone.BONE_RIGHT_FOOT
                || lastDamageBone == (int)eBone.BONE_LEFT_THIGH
                || lastDamageBone == (int)eBone.BONE_RIGHT_THIGH
                || lastDamageBone == (int)eBone.BONE_LEFT_CALF_ROLL
                || lastDamageBone == (int)eBone.BONE_RIGHT_CALF_ROLL)
            {
                delay *= 2;
            }

            Main.TheDelayedCaller.Add(TimeSpan.FromMilliseconds(delay), "Main", () =>
            {
                if (!HasBeenDamagedByWeapons(pedHandle, nonRagdollShotgunWeapons)
                    && ivPedHandle.Health > healthThreshold
                    && ivPedHandle.GetHeightAboveGround() < 3f)
                {
                    SWITCH_PED_TO_ANIMATED(pedHandle, false);
                }
                CLEAR_CHAR_LAST_WEAPON_DAMAGE(pedHandle);
                CLEAR_CHAR_LAST_DAMAGE_BONE(pedHandle);

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
    }
}

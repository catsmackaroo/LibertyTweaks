using IVSDKDotNet;
using System;
using System.Collections.Generic;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class PedHelper
    {
        public static Dictionary<UIntPtr, int> PedHandles { get; private set; } = new Dictionary<UIntPtr, int>();
        public static Dictionary<UIntPtr, int> VehHandles { get; private set; } = new Dictionary<UIntPtr, int>();
        
        public static void GrabAllPeds()
        {
            PedHandles.Clear();

            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);

                if (ptr != UIntPtr.Zero && ptr != Main.PlayerPed.GetUIntPtr())
                {
                    int pedHandle = (int)pedPool.GetIndex(ptr);
                    PedHandles[ptr] = pedHandle;
                }
            }
        }
        public static void GrabAllVehicles()
        {
            VehHandles.Clear();

            IVPool vehPool = IVPools.GetVehiclePool();
            for (int i = 0; i < vehPool.Count; i++)
            {
                UIntPtr ptr = vehPool.Get(i);

                if (ptr != UIntPtr.Zero)
                {
                    int vehHandle = (int)vehPool.GetIndex(ptr);
                    VehHandles[ptr] = vehHandle;
                }
            }
        }

        public static int GetActivePedsCount(Vector3 playerPos, float radius, bool combatPedsOnly, bool includeAllies)
        {
            int count = 0;

            foreach (var kvp in PedHelper.PedHandles)
            {
                int handle = kvp.Value;

                // Only count alive and existent peds
                if (!DOES_CHAR_EXIST(handle) || IS_CHAR_DEAD(handle))
                    continue;

                // Skip non-combat peds if combatOnly is enabled
                if (combatPedsOnly && !IS_PED_IN_COMBAT(handle))
                    continue;

                if (!includeAllies && IS_PED_IN_GROUP(handle))
                    continue;

                // Get the ped's position and check distance
                GET_CHAR_COORDINATES(handle, out Vector3 pedPos);
                float distance = Vector3.Distance(playerPos, pedPos);

                if (distance <= radius)
                {
                    count++;
                }
            }

            return count;
        }

        //private static Dictionary<int, uint> previousPedVehicleHealth = new Dictionary<int, uint>();

        //public static (int pedDamageAmount, int pedDamageLevel, float pedNormalizedDamage) GetPedVehicleDamage()
        //{
        //    int totalDamageAmount = 0;
        //    int maxDamageLevel = 0;
        //    float normalizedDamage = 0f;

        //    foreach (var kvp in PedHelper.VehHandles)
        //    {
        //        int vehHandle = kvp.Value;

        //        GET_CAR_HEALTH(vehHandle, out uint currentHealth);

        //        if (!previousPedVehicleHealth.TryGetValue(vehHandle, out uint previousHealth))
        //        {
        //            previousPedVehicleHealth[vehHandle] = currentHealth;
        //            continue;
        //        }

        //        if (currentHealth < previousHealth)
        //        {
        //            int damageAmount = (int)(previousHealth - currentHealth);
        //            float vehicleNormalizedDamage = CommonHelpers.Clamp((float)damageAmount / 500f, 0f, 1f);

        //            int damageLevel = 0;
        //            if (damageAmount > 450) damageLevel = 4;
        //            else if (damageAmount > 250) damageLevel = 3;
        //            else if (damageAmount > 100) damageLevel = 2;
        //            else if (damageAmount > 0) damageLevel = 1;

        //            totalDamageAmount += damageAmount;
        //            normalizedDamage = Math.Max(normalizedDamage, vehicleNormalizedDamage);
        //            maxDamageLevel = Math.Max(maxDamageLevel, damageLevel);
        //        }

        //        previousPedVehicleHealth[vehHandle] = currentHealth;
        //    }

        //    return (totalDamageAmount, maxDamageLevel, normalizedDamage);
        //}

        //public static bool ActivePedsMeetsCombatThreshold(Vector3 playerPos, float radius, int threshold)
        //{
        //    int count = 0;

        //    foreach (var kvp in PedHelper.PedHandles)
        //    {
        //        int handle = kvp.Value;

        //        // Skip non-existent, dead, or non-combat peds early
        //        if (!DOES_CHAR_EXIST(handle) || IS_CHAR_DEAD(handle) || !IS_PED_IN_COMBAT(handle))
        //            continue;

        //        // Check if ped is within the radius
        //        GET_CHAR_COORDINATES(handle, out Vector3 pedPos);
        //        if (Vector3.Distance(playerPos, pedPos) <= radius)
        //        {
        //            count++;
        //            // If the count meets the threshold, return true immediately
        //            if (count >= threshold)
        //                return true;
        //        }

        //        IVGame.ShowSubtitleMessage(count.ToString() + " " + Killcam.thresholdMet.ToString());
        //    }

        //    return false; // Return false if the threshold is not met
        //}

    }
}

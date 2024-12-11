using CCL.GTAIV;
using IVSDKDotNet;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    // TODO: As a start point for next time, figure out a way for the weapons to adjust their max ammo without them being selected.
    // Use GET_CHAR_WEAPON_IN_SLOT
    
    internal class WeaponProgression
    {
        private static bool enable;
        private static bool firstFrame = true;

        // Constants to represent weapon slots
        private const int SLOT_PISTOL = 0; // 2
        private const int SLOT_SHOTGUN = 1; // 3
        private const int SLOT_SMG = 2; // 4
        private const int SLOT_RIFLE = 3; // 5
        private const int SLOT_SNIPER = 4; // 6
        private const int SLOT_SPECIAL = 5; // 7
        private const int SLOT_GRENADE = 6; // 8
        private const int SLOT_COUNT = 7;

        // Matching the above slots to their game IDs 
        private static readonly Dictionary<int, uint> SlotToGameID = new Dictionary<int, uint>
        {
            { SLOT_PISTOL, 2 },
            { SLOT_SHOTGUN, 3 },
            { SLOT_SMG, 4 },
            { SLOT_RIFLE, 5 },
            { SLOT_SNIPER, 6 },
            { SLOT_SPECIAL, 7 },
            { SLOT_GRENADE, 8 }
        };

        // Arrays to store levels for each slot
        private static int[] savedWeaponLevels = new int[SLOT_COUNT];
        private static int[] activeWeaponLevels = new int[SLOT_COUNT];
        private static int[] notificationWeaponLevels = new int[SLOT_COUNT];

        // Define thresholds for level progression
        private static readonly int[] levelThresholds = { 1, 5, 10, 15 };

        // Weapon stat mappings for each slot
        private static readonly Dictionary<int, int[]> slotStats = new Dictionary<int, int[]>
        {
            { SLOT_PISTOL, new[] { 382, 383, 401, 403 } }, // Pistol, Deagle, Auto9mm, Automag44
            { SLOT_SHOTGUN, new[] { 384, 385, 396, 400, 404, 405 } }, // Shotguns
            { SLOT_SMG, new[] { 386, 387, 406, 407 } }, // SMGs
            { SLOT_RIFLE, new[] { 388, 389, 408 } }, // Rifles
            { SLOT_SNIPER, new[] { 390, 391, 409 } }, // Snipers
            { SLOT_SPECIAL, new[] { 392, 395, 399 } }, // RPGs & Grenade Launchers
            { SLOT_GRENADE, new[] { 379, 380, 402, 410 } } // Grenades
        };

        private static float level0MultiplierMaxAmmo = 0.2f;
        private static float level1MultiplierMaxAmmo = 0.4f;
        private static float level2MultiplierMaxAmmo = 0.6f;
        private static float level3MultiplierMaxAmmo = 0.8f;
        private static float level4MultiplierMaxAmmo = 1.0f;

        public static void Init(SettingsFile Settings)
        {
            enable = Settings.GetBoolean("Weapon Progression", "Enable", true);

            if (enable)
                Main.Log("Weapon Progression initialized...");
        }

        public static void IngameStartup()
        {
            if (!enable) return;

            firstFrame = true;
        }

        public static void Tick()
        {
            if (!enable) return;

            // First frame initialization
            if (firstFrame)
            {
                FirstFrameSaveChecks();
                firstFrame = false;
            }

            // Check for level up based on kills
            CheckForSlotLevelUp();

            // Update weapon stats if the level has changed
            UpdateWeaponStats();
        }

        #region Helper Methods
        private static double GetTotalKillsForSlot(int slot)
        {
            return slotStats[slot].Sum(stat => GET_INT_STAT(stat)); // Fetch and sum kills dynamically
        }

        private static uint MapSlotToGameID(int slot)
        {
            return SlotToGameID.TryGetValue(slot, out uint gameID) ? gameID : 0; // Return 0 for invalid slot
        }

        private static void CheckForSlotLevelUp()
        {
            // Check each slot to see if the player has met the level-up threshold
            for (int slot = 0; slot < SLOT_COUNT; slot++)
            {
                double totalKills = GetTotalKillsForSlot(slot);

                // Check thresholds for level progression
                for (int level = 0; level < levelThresholds.Length; level++)
                {
                    if (totalKills >= levelThresholds[level] && activeWeaponLevels[slot] <= level)
                    {
                        // Level up the weapon and notify
                        activeWeaponLevels[slot] = level + 1;
                        notificationWeaponLevels[slot] = level + 1;

                        Main.Log($"Unlocked Weapon Level {level + 2} for Slot {slot} with kills: {totalKills}");
                        IVGame.ShowSubtitleMessage($"Slot {slot} Weapon Level {level + 2} Unlocked!");
                    }
                }
            }
        }

        private static void UpdateWeaponStats()
        {
            // Apply stats only if the weapon level has changed
            for (int slot = 0; slot < SLOT_COUNT; slot++)
            {
                if (activeWeaponLevels[slot] != savedWeaponLevels[slot])
                {
                    savedWeaponLevels[slot] = activeWeaponLevels[slot]; // Update saved level
                    ApplyWeaponStatsForLevel(slot, activeWeaponLevels[slot]); // Apply stats for the new level
                }
            }
        }

        private static void ApplyWeaponStatsForLevel(int slot, int level)
        {
            Main.Log($"Applying stats for Slot {slot} at Level {level}...");

            // Only proceed if the slot has valid weapons
            if (!slotStats.ContainsKey(slot)) return;

            // Get the current weapon in use by the player
            // Use GET_CHAR_WEAPON_IN_SLOT instead
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);

            // Ensure the weapon belongs to the correct slot
            uint currentSlot = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot;
            if (currentSlot != MapSlotToGameID(slot)) return;

            // Apply max ammo multipliers based on weapon level
            if (slot == SLOT_PISTOL)
            {
                IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo = 1000;
                float maxAmmo = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo;

                switch (level)
                {
                    case 0:
                        IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo = (uint)(maxAmmo * level0MultiplierMaxAmmo);
                        break;
                    case 1:
                        IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo = (uint)(maxAmmo * level1MultiplierMaxAmmo);
                        break;
                    case 2:
                        IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo = (uint)(maxAmmo * level2MultiplierMaxAmmo);
                        break;
                    case 3:
                        IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo = (uint)(maxAmmo * level3MultiplierMaxAmmo);
                        break;
                    case 4:
                        IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo = (uint)(maxAmmo * level4MultiplierMaxAmmo);
                        break;
                }

                Main.Log($"MaxAmmo for Pistol at Level {level}: {maxAmmo} -> {IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).MaxAmmo}");
            }

            // Add more adjustments for other weapons or stats here as needed
        }

        private static void FirstFrameSaveChecks()
        {
            // Initialize saved weapon levels from saved game
            for (int slot = 0; slot < SLOT_COUNT; slot++)
            {
                savedWeaponLevels[slot] = Main.GetTheSaveGame().GetInteger($"Slot{slot}WeaponLevel");
                activeWeaponLevels[slot] = savedWeaponLevels[slot];
            }
        }
        #endregion
    }
}

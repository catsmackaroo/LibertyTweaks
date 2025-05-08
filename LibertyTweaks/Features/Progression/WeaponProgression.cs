using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System.Collections.Generic;
using System.Linq;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{

    internal class WeaponProgression
    {
        private static bool enable;
        private static bool enableLogging;
        private static bool firstFrame = true;
        private static bool hasSaved = true;

        // Optimization stuff
        private static int frameCounter = 0;
        private static readonly int checkInterval = 30;

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

        // Matching the above slots to legible names for notifications
        private static readonly Dictionary<int, string> SlotNames = new Dictionary<int, string>
        {
            { SLOT_PISTOL, "Pistols" },
            { SLOT_SHOTGUN, "Shotguns" },
            { SLOT_SMG, "SMGs" },
            { SLOT_RIFLE, "Rifles" },
            { SLOT_SNIPER, "Snipers" },
            { SLOT_SPECIAL, "Special Weapons" },
            { SLOT_GRENADE, "Throwables" }
        };

        // Arrays to store levels for each slot
        private static readonly int[] savedWeaponLevels = new int[SLOT_COUNT];
        private static readonly int[] activeWeaponLevels = new int[SLOT_COUNT];

        // Defines thresholds for level progression
        // Default for other possible slots (whatever they may be)
        private static readonly int[] levelThresholds = { 25, 50, 100, 200 };

        // Specifics
        private static readonly Dictionary<int, int[]> slotLevelThresholds = new Dictionary<int, int[]>
        {
            { SLOT_PISTOL, new[] { 20, 40, 60, 80 } },       // Pistols mid
            { SLOT_SHOTGUN, new[] { 15, 30, 50, 120 } },     // Shotguns are risky, especially in early levels
            { SLOT_SMG, new[] { 25, 50, 100, 200 } },        // SMGs are meta as hell, should be higher
            { SLOT_RIFLE, new[] { 30, 60, 120, 240 } },      // ARs same as above
            { SLOT_SNIPER, new[] { 15, 35, 55, 85 } },      // Snipers
            { SLOT_SPECIAL, new[] { 5, 15, 30, 60 } },       // RPG, grenade launchers are rarer
            { SLOT_GRENADE, new[] { 10, 20, 30, 40 } }       // Grenades mid
        };

        // Grab base stats read in WeaponInfo.xml to avoid feedback loop
        private static readonly Dictionary<int, float> baseMaxAmmoCache = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> baseAccuracyCache = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> baseDamageCache = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> baseRangeCache = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> baseFirerateCache = new Dictionary<int, float>();

        // Track inventory to ensure levels change properly
        private static List<eWeaponType> lastInventory = new List<eWeaponType>();
        private static int lastInventoryCount = 0;

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

        private static float[] levelMultipliersMaxAmmo = new float[]
        {
            0.6f, // 0 // added
            0.7f, // 1
            0.8f, // 2 
            0.9f, // 3
            1.0f // 4
        };

        private static float[] levelMultipliersRange = new float[]
        {
            1.0f, // 0 
            1.1f, // 1 // added
            1.2f, // 2 
            1.3f, // 3
            1.4f // 4
        };

        private static float[] levelMultipliersFirerate = new float[]
        {
            0.95f, // 0 
            0.95f, // 1
            1.0f, // 2 // added
            1.02f, // 3
            1.04f // 4

        };

        private static float[] levelMultipliersAccuracy = new float[]
        {
            1.0f, // 0 
            1.0f, // 1
            1.0f, // 2 
            0.9f, // 3 // added
            0.8f // 4
        };

        private static float[] levelMultipliersDamage = new float[]
        {
            1.0f, // 0 
            1.0f, // 1
            1.0f, // 2 
            1.0f, // 3
            1.15f // 4 // added
        };

        public static string section { get; private set; }
        public static void Init(SettingsFile Settings, string section)
        {
            WeaponProgression.section = section;
            enable = Settings.GetBoolean(section, "Weapon Progression", false);
            enableLogging = Settings.GetBoolean(section, "Weapon Progression - Verbose Logging", false);
            levelMultipliersMaxAmmo = CommonHelpers.ParseFloatArray(Settings.GetValue(section, "Weapon Progression - Max Ammo Multipliers", "0.6,0.7,0.8,0.9,1.0"));
            levelMultipliersRange = CommonHelpers.ParseFloatArray(Settings.GetValue(section, "Weapon Progression - Range Multipliers", "1.0,1.1,1.2,1.3,1.4"));
            levelMultipliersFirerate = CommonHelpers.ParseFloatArray(Settings.GetValue(section, "Weapon Progression - Firerate Multipliers", "0.95,0.95,1.0,1.02,1.04"));
            levelMultipliersAccuracy = CommonHelpers.ParseFloatArray(Settings.GetValue(section, "Weapon Progression - Accuracy Multipliers", "1.0,1.0,1.0,0.9,0.8"));
            levelMultipliersDamage = CommonHelpers.ParseFloatArray(Settings.GetValue(section, "Weapon Progression - Damage Multipliers", "1.0,1.0,1.0,1.0,1.15"));

            if (enable)
            {
                Main.Log("script initialized...");
                Main.Log($"Max Ammo Multipliers: {string.Join(", ", levelMultipliersMaxAmmo)}");
                Main.Log($"Range Multipliers: {string.Join(", ", levelMultipliersRange)}");
                Main.Log($"Firerate Multipliers: {string.Join(", ", levelMultipliersFirerate)}");
                Main.Log($"Accuracy Multipliers: {string.Join(", ", levelMultipliersAccuracy)}");
            }
        }

        public static void IngameStartup()
        {
            if (!enable) return;

            firstFrame = true;
        }

        public static void Tick()
        {
            if (!enable) return;

            if (firstFrame)
            {
                InitializeFirstFrame();
                UpdateWeaponStats();
                return;
            }

            // Throttle
            frameCounter++;
            if (frameCounter >= checkInterval)
            {
                frameCounter = 0;

                if (HasPlayerInventoryChanged())
                {
                    UpdateWeaponStats();
                }

                if (HasPlayerLevelChanged())
                {
                    UpdateWeaponStats();
                }

                HandleSaving();
            }
        }

        private static void InitializeFirstFrame()
        {
            // Initialize saved weapon levels from saved game
            for (int slot = 0; slot < SLOT_COUNT; slot++)
            {
                int savedLevel = Main.GetTheSaveGame().GetInteger($"Slot{slot}WeaponLevel");
                if (savedLevel == null)
                {
                    savedLevel = 0;
                }
                savedWeaponLevels[slot] = savedLevel;
                activeWeaponLevels[slot] = savedLevel;
            }

            firstFrame = false;
        }


        private static bool HasPlayerLevelChanged()
        {
            bool hasChanged = false;

            // Iterate through each weapon slot
            for (int slot = 0; slot < SLOT_COUNT; slot++)
            {
                double totalKills = GetTotalKillsForSlot(slot);

                // Find the appropriate level based on kill thresholds
                int[] thresholds = slotLevelThresholds.ContainsKey(slot) ? slotLevelThresholds[slot] : levelThresholds;
                int newLevel = 0; // default 

                for (int level = 0; level < thresholds.Length; level++)
                {
                    if (totalKills >= thresholds[level])
                    {
                        newLevel = level + 1;
                    }
                }

                // Update the level only if it has changed
                if (activeWeaponLevels[slot] != newLevel)
                {
                    activeWeaponLevels[slot] = newLevel;
                    hasChanged = true;

                    if (newLevel != savedWeaponLevels[slot])
                    {
                        Notifications(newLevel, slot);

                        if (enableLogging)
                        {
                            Main.Log($"Unlocked Weapon Level {newLevel} for Slot {slot} with kills: {totalKills}");
                        }
                    }
                    else
                    {
                        if (enableLogging)
                        {
                            Main.Log($"Downgraded Weapon Level to {newLevel} for Slot {slot} due to reduced kills: {totalKills}");
                        }
                    }
                }
            }

            return hasChanged;
        }

        public static bool HasPlayerInventoryChanged()
        {
            // Get the current inventory
            var currentInventory = WeaponHelpers.GetWeaponInventory(false);

            // Compare count and contents
            bool inventoryCountChanged = currentInventory.Count != lastInventoryCount;
            bool inventoryContentChanged = !currentInventory.SequenceEqual(lastInventory);

            // If either the count or the content has changed
            if (inventoryCountChanged || inventoryContentChanged)
            {
                if (enableLogging)
                {
                    Main.Log($"Player inventory has changed. Previous count: {lastInventoryCount}, Current count: {currentInventory.Count}");
                    Main.Log($"Previous inventory: {string.Join(", ", lastInventory)}");
                    Main.Log($"Current inventory: {string.Join(", ", currentInventory)}");
                }

                // Update the record
                lastInventoryCount = currentInventory.Count;
                lastInventory = new List<eWeaponType>(currentInventory); // Deep copy
            }

            return inventoryCountChanged || inventoryContentChanged;
        }

        private static double GetTotalKillsForSlot(int slot)
        {
            return slotStats[slot].Sum(stat => GET_INT_STAT(stat)); // Fetch and sum kills dynamically
        }

        private static void UpdateWeaponStats()
        {
            for (int slot = 0; slot < SLOT_COUNT; slot++)
            {
                ApplyWeaponStatsForLevel(slot, activeWeaponLevels[slot]);
            }
        }

        private static void ApplyWeaponStatsForLevel(int slot, int level)
        {
            if (enableLogging)
            {
                Main.Log($"Applying stats for Slot {slot} at Level {level}...");
            }

            if (!slotStats.ContainsKey(slot)) return;

            var inventory = WeaponHelpers.GetWeaponInventory(false);

            foreach (var weapon in inventory)
            {
                var weaponInfo = IVWeaponInfo.GetWeaponInfo((uint)weapon);

                if (weaponInfo.WeaponSlot != MapSlotToGameID(slot)) continue;

                float ammoMultiplier = levelMultipliersMaxAmmo[level];
                float accuracyMultiplier = levelMultipliersAccuracy[level];
                float damageMultiplier = levelMultipliersDamage[level];
                float rangeMultiplier = levelMultipliersRange[level];
                float firerateMultiplier = levelMultipliersFirerate[level];

                float maxAmmo = GetCachedMaxAmmoForWeapon((int)weapon);
                float accuracy = GetCachedAccuracyForWeapon((int)weapon);
                float damage = GetCachedDamageForWeapon((int)weapon);
                float range = GetCachedRangeForWeapon((int)weapon);
                float fireRate = GetCachedFirerateForWeapon((int)weapon);

                weaponInfo.MaxAmmo = (uint)(maxAmmo * ammoMultiplier);
                weaponInfo.Accuracy = accuracy * accuracyMultiplier;
                weaponInfo.Damage = (ushort)(damage * damageMultiplier);
                weaponInfo.WeaponRange = (range * rangeMultiplier);
                weaponInfo.FireRate = (fireRate * firerateMultiplier);

                // Balance Tweaks
                // RPG Ammo so it's not only 8 at max level
                if (weaponInfo.MaxAmmo < 15 && level == 4)
                    weaponInfo.MaxAmmo = (uint)(maxAmmo * ammoMultiplier * 2);

                // MP5/Deagle/etc so it's not too much too early
                if (weaponInfo.MaxAmmo > 300 && level <= 1)
                    weaponInfo.MaxAmmo = (uint)(maxAmmo * ammoMultiplier * 0.5);

                // Shotgun
                if (weaponInfo.WeaponSlot == SLOT_SHOTGUN)
                    weaponInfo.MaxAmmo=(uint)(maxAmmo * ammoMultiplier * 3);

                // Sniper early game
                if (weaponInfo.WeaponSlot == SLOT_SNIPER && weaponInfo.MaxAmmo < 15 && level < 2)
                    weaponInfo.MaxAmmo = (uint)(maxAmmo * ammoMultiplier * 2);

                if (enableLogging)
                {
                    Main.Log($"Updated MaxAmmo for {weapon} (Slot {slot}, Level {level}): {maxAmmo} -> {weaponInfo.MaxAmmo}");
                    Main.Log($"Updated Accuracy for {weapon} (Slot {slot}, Level {level}): {accuracy} -> {weaponInfo.Accuracy}");
                    Main.Log($"Updated Damage for {weapon} (Slot {slot}, Level {level}): {damage} -> {weaponInfo.Damage}");
                    Main.Log($"Updated Range for {weapon} (Slot {slot}, Level {level}): {range} -> {weaponInfo.WeaponRange}");
                }
            }
        }

        private static uint MapSlotToGameID(int slot)
        {
            if (SlotToGameID.TryGetValue(slot, out uint gameID))
            {
                return gameID;
            }

            if (enableLogging)
            {
                Main.Log($"Invalid Slot {slot}");
            }
            return 0;
        }

        private static float GetCachedMaxAmmoForWeapon(int weaponID)
        {
            if (baseMaxAmmoCache.ContainsKey(weaponID))
                return baseMaxAmmoCache[weaponID];

            float baseMaxAmmo = IVWeaponInfo.GetWeaponInfo((uint)weaponID).MaxAmmo;
            baseMaxAmmoCache[weaponID] = baseMaxAmmo;

            return baseMaxAmmo;
        }

        private static float GetCachedAccuracyForWeapon(int weaponID)
        {
            if (baseAccuracyCache.ContainsKey(weaponID))
                return baseAccuracyCache[weaponID];

            float baseAccuracy = IVWeaponInfo.GetWeaponInfo((uint)weaponID).Accuracy;
            baseAccuracyCache[weaponID] = baseAccuracy;

            return baseAccuracy;
        }

        private static float GetCachedDamageForWeapon(int weaponID)
        {
            if (baseDamageCache.ContainsKey(weaponID))
                return baseDamageCache[weaponID];

            float baseDamage = IVWeaponInfo.GetWeaponInfo((uint)weaponID).Damage;
            baseDamageCache[weaponID] = baseDamage;

            return baseDamage;
        }
        private static float GetCachedRangeForWeapon(int weaponID)
        {
            if (baseRangeCache.ContainsKey(weaponID))
                return baseRangeCache[weaponID];

            float baseRange = IVWeaponInfo.GetWeaponInfo((uint)weaponID).WeaponRange;
            baseRangeCache[weaponID] = baseRange;

            return baseRange;
        }

        private static float GetCachedFirerateForWeapon(int weaponID)
        {
            if (baseFirerateCache.ContainsKey(weaponID))
                return baseFirerateCache[weaponID];

            float baseFireRate = IVWeaponInfo.GetWeaponInfo((uint)weaponID).FireRate;
            baseFirerateCache[weaponID] = baseFireRate;

            return baseFireRate;
        }
        private static void Notifications(int level, int slot)
        {
            string message = "";
            string slotName = SlotNames.ContainsKey(slot) ? SlotNames[slot] : "Unknown Slot";

            switch (slot)
            {
                case SLOT_SPECIAL:
                case SLOT_GRENADE:
                    switch (level)
                    {
                        case 1:
                            message = $"Upgraded {slotName} Level - Increased ammo capacity.";
                            break;
                        case 2:
                            message = $"Upgraded {slotName} Level - Further increased ammo capacity.";
                            break;
                        case 3:
                            message = $"Upgraded {slotName} Level - Greatly increased ammo capacity.";
                            break;
                        case 4:
                            message = $"Upgraded {slotName} Level - Maximum ammo capacity unlocked!";
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    switch (level)
                    {
                        case 1:
                            message = $"Upgraded {slotName} Level - Increased ammo & range.";
                            break;
                        case 2:
                            message = $"Upgraded {slotName} Level - Increased ammo, range, & firerate.";
                            break;
                        case 3:
                            message = $"Upgraded {slotName} Level - Increased ammo, range, firerate, & accuracy!";
                            break;
                        case 4:
                            message = $"Upgraded {slotName} Level - Increased ammo, range, firerate, accuracy, & damage!";
                            break;
                        default:
                            break;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(message))
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
                PRINT_HELP("PLACEHOLDER_1");
            }
        }

        private static void HandleSaving()
        {
            if (GET_IS_DISPLAYINGSAVEMESSAGE() && !hasSaved)
            {
                hasSaved = true;
            }
            else if (!GET_IS_DISPLAYINGSAVEMESSAGE() && hasSaved)
            {
                for (int slot = 0; slot < SLOT_COUNT; slot++)
                {
                    Main.GetTheSaveGame().SetInteger($"Slot{slot}WeaponLevel", activeWeaponLevels[slot]);
                    savedWeaponLevels[slot] = activeWeaponLevels[slot];
                }
                Main.GetTheSaveGame().Save();
                Main.Log($"Saved WeaponLevels as {string.Join(", ", activeWeaponLevels)}");

                hasSaved = false;
            }
        }
    }
}

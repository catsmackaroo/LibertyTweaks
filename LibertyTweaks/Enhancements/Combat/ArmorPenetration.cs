﻿using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using System;
using System.Collections.Generic;
using IVSDKDotNet.Enums;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class ArmorPenetration
    {
        private static bool enable;
        public static int DamageMinimumPercent;
        public static int DamageMaximumPercent;
        public static float damageFraction = 10;
        public static int ArmourThreshold1;
        public static int ArmourThreshold2;
        private static readonly List<eWeaponType> StrongWeapons = new List<eWeaponType>();

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Armor Penetration", "Enable", true);
            DamageMinimumPercent = settings.GetInteger("Armor Penetration", "Damage Minimum Percent", 0);
            DamageMaximumPercent = settings.GetInteger("Armor Penetration", "Damage Maximum Percent", 5);
            ArmourThreshold1 = settings.GetInteger("Armor Penetration", "Armour Threshold 1", 33);
            ArmourThreshold2 = settings.GetInteger("Armor Penetration", "Armour Threshold 2", 66);

            string weaponsString = settings.GetValue("Extensive Settings", "Included Weapons", "");
            StrongWeapons.Clear();
            foreach (var weaponName in weaponsString.Split(','))
            {
                try
                {
                    eWeaponType weaponType = (eWeaponType)Enum.Parse(typeof(eWeaponType), weaponName.Trim(), true);
                    StrongWeapons.Add(weaponType);
                }
                catch (Exception ex)
                {
                    Main.Log($"Invalid weapon type: {weaponName.Trim()}. Error: {ex.Message}");
                }
            }

            Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (PlayerHelper.HasPlayerBeenDamagedArmor())
            {
                GET_CHAR_ARMOUR(Main.PlayerPed.GetHandle(), out uint pArmour);

                if (pArmour > ArmourThreshold2 || pArmour == 0)
                {
                    SET_CHAR_BULLETPROOF_VEST(Main.PlayerPed.GetHandle(), false);
                    return;
                }

                SET_CHAR_BULLETPROOF_VEST(Main.PlayerPed.GetHandle(), true);

                GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out uint currentHealth);

                foreach (eWeaponType weaponType in StrongWeapons)
                {
                    if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(Main.PlayerPed.GetHandle(), (int)weaponType))
                    {
                        int damagePercentage = Main.GenerateRandomNumber(DamageMinimumPercent, DamageMaximumPercent);

                        if (pArmour < ArmourThreshold1)
                            damageFraction = damagePercentage / 50f;
                        else if (pArmour < ArmourThreshold2)
                            damageFraction = damagePercentage / 100f;

                        long reducedHealth = (long)(currentHealth - (currentHealth * damageFraction));

                        SET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), (uint)reducedHealth);
                    }
                }
            }
        }
    }
}

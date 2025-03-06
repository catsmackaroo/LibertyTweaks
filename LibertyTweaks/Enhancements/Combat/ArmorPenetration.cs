using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

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

            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;
                HandlePen(pedHandle);
            }

            HandlePen(Main.PlayerPed.GetHandle());
        }
        private static void HandlePen(int handle)
        {
            GET_CHAR_ARMOUR(handle, out uint pArmour);

            if (pArmour > ArmourThreshold2 || pArmour == 0)
                return;

            GET_CHAR_HEALTH(handle, out uint currentHealth);
            if (currentHealth <= 100)
                return;

            foreach (eWeaponType weaponType in StrongWeapons)
            {
                if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(handle, (int)weaponType))
                {
                    int damagePercentage = Main.GenerateRandomNumber(DamageMinimumPercent, DamageMaximumPercent);

                    if (pArmour < ArmourThreshold1)
                        damageFraction = damagePercentage / 50f;
                    else if (pArmour < ArmourThreshold2)
                        damageFraction = damagePercentage / 100f;

                    long reducedHealth = (long)(currentHealth * (1 - damageFraction));

                    reducedHealth = Math.Max(reducedHealth, 100);

                    SET_CHAR_HEALTH(handle, (uint)reducedHealth);
                    CLEAR_CHAR_LAST_WEAPON_DAMAGE(handle);
                }
            }
        }


    }
}

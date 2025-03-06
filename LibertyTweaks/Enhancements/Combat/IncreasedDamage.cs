using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class IncreasedDamage
    {
        private static bool enable;
        private static bool disableAt25;
        private static bool disableWithArmor;
        public static int DamageMinimumPercent = 0;
        public static int DamageMaximumPercent = 3;

        private static readonly List<eWeaponType> meleeWeapons = new List<eWeaponType>
        {
            eWeaponType.WEAPON_UNARMED, eWeaponType.WEAPON_KNIFE, eWeaponType.WEAPON_BASEBALLBAT
        };


        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Increased Damage", "Enable", true);
            disableAt25 = settings.GetBoolean("Increased Damage", "Disable At 25% Health", true);
            disableWithArmor = settings.GetBoolean("Increased Damage", "Disable With Armor", true);

            DamageMinimumPercent = settings.GetInteger("Increased Damage", "Damage Minimum Percent", 0);
            DamageMaximumPercent = settings.GetInteger("Increased Damage", "Damage Maximum Percent", 3);

            Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out uint health);
            GET_CHAR_ARMOUR(Main.PlayerPed.GetHandle(), out uint armor);
            if ((disableAt25 && health <= 125) || (disableWithArmor && armor > 0))
                return;

            if (PlayerHelper.HasPlayerBeenDamagedHealth())
            {
                if (Main.PlayerPed.RagdollStatus == 6)
                    return;

                foreach (eWeaponType meleeWeaponType in meleeWeapons)
                {
                    if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(Main.PlayerPed.GetHandle(), (int)meleeWeaponType))
                        return;
                }

                GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out uint currentHealth);

                if (DamageMaximumPercent > 0 || DamageMinimumPercent > 0)
                {
                    int damagePercentage = Main.GenerateRandomNumber(DamageMinimumPercent, DamageMaximumPercent);
                    float damageFraction = damagePercentage / 100f;
                    float healthFactor = currentHealth / 200f;

                    damageFraction *= healthFactor;

                    uint damageAmount = (uint)(currentHealth * damageFraction);

                    uint newHealth = currentHealth > damageAmount ? currentHealth - damageAmount : 0;

                    SET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), newHealth);
                }
            }
        }
    }
}

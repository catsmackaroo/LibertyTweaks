using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using System.Collections.Generic;
using IVSDKDotNet.Enums;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class IncreasedDamage
    {
        private static bool enable;
        public static int DamageMinimumPercent = 0;
        public static int DamageMaximumPercent = 3;

        private static readonly List<eWeaponType> meleeWeapons = new List<eWeaponType>
        {
            eWeaponType.WEAPON_UNARMED, eWeaponType.WEAPON_KNIFE, eWeaponType.WEAPON_BASEBALLBAT
        };


        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Increased Damage", "Enable", true);
            DamageMinimumPercent = settings.GetInteger("Increased Damage", "Damage Minimum Percent", 0);
            DamageMaximumPercent = settings.GetInteger("Increased Damage", "Damage Maximum Percent", 3);

            Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (PlayerChecks.HasPlayerBeenDamagedHealth())
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

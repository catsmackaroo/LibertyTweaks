using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo 

namespace LibertyTweaks
{
    internal class HealthRegeneration
    {
        private static bool enable;
        private static bool enableOutOfCombatRegen;

        private static uint combatHealthRegenTo = 126;
        private static uint outOfCombatHealthRegenTo = 150;

        private static DateTime lastRegenTime = DateTime.MinValue;
        private static uint lastKnownHealth = 0;
        private static readonly object lockObject = new object();
        private static int regenHealthMinTimer;
        private static int regenHealthMaxTimer;
        private static int regenHealthMinHeal;
        private static int regenHealthMaxHeal;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Health Regeneration", "Enable", true);
            enableOutOfCombatRegen = settings.GetBoolean("Health Regeneration", "Out of Combat Health Regen", true);

            combatHealthRegenTo = (uint)settings.GetInteger("Health Regeneration", "Combat Health Regen To", 126);
            outOfCombatHealthRegenTo = (uint)settings.GetInteger("Health Regeneration", "Out of Combat Health Regen To", 150);

            regenHealthMinTimer = settings.GetInteger("Health Regeneration", "Regen Timer Minimum", 30);
            regenHealthMaxTimer = settings.GetInteger("Health Regeneration", "Regen Timer Maximum", 60);
            regenHealthMinHeal = settings.GetInteger("Health Regeneration", "Minimum Heal Amount", 5);
            regenHealthMaxHeal = settings.GetInteger("Health Regeneration", "Maximum Heal Amount", 10);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out uint playerHealth);

            if (IS_CHAR_DEAD(Main.PlayerPed.GetHandle()) || IS_PAUSE_MENU_ACTIVE() || playerHealth == 200)
            {
                lock (lockObject)
                {
                    lastRegenTime = DateTime.MinValue;
                }
                return;
            }

            lock (lockObject)
            {
                if (playerHealth < lastKnownHealth)
                {
                    lastRegenTime = DateTime.MinValue;
                }

                if (lastRegenTime == DateTime.MinValue || PlayerHelper.HasPlayerBeenDamagedHealth())
                    lastRegenTime = DateTime.UtcNow;

                if (DateTime.UtcNow > lastRegenTime.AddSeconds(Main.GenerateRandomNumber(regenHealthMinTimer, regenHealthMaxTimer)))
                {
                    uint newHealth;

                    if (!PlayerHelper.IsPlayerInOrNearCombat() && enableOutOfCombatRegen && playerHealth < outOfCombatHealthRegenTo)
                    {
                        newHealth = playerHealth + (uint)regenHealthMinHeal;
                        newHealth = Math.Min(newHealth, outOfCombatHealthRegenTo);
                        SET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), newHealth);
                        Main.Log($"Player health regenerated to {newHealth}");
                        lastRegenTime = DateTime.UtcNow;
                    }
                    else if (playerHealth <= combatHealthRegenTo)
                    {
                        newHealth = (uint)(playerHealth + Main.GenerateRandomNumber(regenHealthMinHeal, regenHealthMaxHeal));
                        newHealth = Math.Min(newHealth, combatHealthRegenTo);
                        SET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), newHealth);
                        Main.Log($"Player health regenerated to {newHealth}");
                        lastRegenTime = DateTime.UtcNow;
                    }
                    else
                    {
                        lock (lockObject)
                        {
                            lastRegenTime = DateTime.MinValue;
                        }
                        return;
                    }
                }
                lastKnownHealth = playerHealth;
            }
        }
    }
}

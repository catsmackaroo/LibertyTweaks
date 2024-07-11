using System;
using System.Collections.Generic;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;

namespace LibertyTweaks
{
    internal class SwitchWeaponReloadFix
    {
        private static bool enable;
        private static Dictionary<int, int> equippedWeapons = new Dictionary<int, int>();
        private static int lastWeaponHash = 0;
        private static int previousWeaponHash = 0;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Switch Weapon Gun Reload", true);

            if (enable)
                Main.Log("Switch Weapon Reload Fix script initialized...");
        }

        public Dictionary<int, int> GetEquippedWeapons()
        {
            return equippedWeapons;
        }

        public static void Tick()
        {
            if (!enable) return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            int currentWeapon;
            GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out currentWeapon);

            // Get current ammo count in clip
            int currentAmmo;
            GET_AMMO_IN_CLIP(playerPed.GetHandle(), currentWeapon, out currentAmmo);

            // Check if the weapon has changed
            if (currentWeapon != lastWeaponHash)
            {
                // Store the current weapon and its ammo
                StoreWeapon(currentWeapon, playerPed);

                // Restore ammo if switching back to previous weapon
                if (currentWeapon == previousWeaponHash)
                {
                    if (equippedWeapons.TryGetValue(previousWeaponHash, out int restoredAmmo))
                    {
                        SET_AMMO_IN_CLIP(playerPed.GetHandle(), previousWeaponHash, restoredAmmo);
                        Main.Log($"Restored ammo for weapon {previousWeaponHash}: {restoredAmmo}");
                    }
                }

                // Update previous weapon hash to last weapon hash
                previousWeaponHash = lastWeaponHash;

                // Update last weapon hash to current weapon
                lastWeaponHash = currentWeapon;

                // Log the weapon change
                Main.Log($"Weapon changed to: {currentWeapon}");
            }
        }

        private static void StoreWeapon(int weaponHash, IVPed playerPed)
        {
            // Get current ammo count in clip
            GET_AMMO_IN_CLIP(playerPed.GetHandle(), weaponHash, out int currentAmmo);

            // Store or update the weapon in the dictionary
            if (equippedWeapons.ContainsKey(weaponHash))
            {
                equippedWeapons[weaponHash] = currentAmmo;
            }
            else
            {
                equippedWeapons.Add(weaponHash, currentAmmo);
            }
        }
    }
}

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using CCL.GTAIV;
using System.Numerics;
using System.Collections.Generic;
using IVSDKDotNet.Enums;

namespace LibertyTweaks
{
    public enum WeaponGroup
    {
        SmallPistol = 5,
        HeavyPistol = 6,
        SMG = 8,
        Shotgun = 9,
        AssaultRifle = 10,
        Sniper = 11,
        Heavy = 12,
        Thrown = 13
    }

    public class WeaponHelpers
    {
        public static bool IsHandgunReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@handgun", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsDeagleReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@deagle", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsCzReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@cz75", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool Is44Reloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@44A", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsBarettaReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@baretta", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsShotgunReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@shotgun", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsAA12Reloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@aa12", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsStreetReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@test_gun", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsSawnReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@sawnoff", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsUziReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@uzi", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsMp5Reloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@mp5k", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsP90Reloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@p90", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsGoldUziReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@gold_uzi", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsAk47Reloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@ak47", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsLmgReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@m249", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsRifleReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@rifle", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsRifle2Reloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@rifle", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsDsrReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@dsr1", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "p_load");
        public static bool IsRPGReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@rocket", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsGLauncherReloading() => Main.PlayerPed.GetAnimationController().IsPlaying("gun@grnde_launch", IS_CHAR_DUCKING(Main.PlayerPed.GetHandle()) ? "reload_crouch" : "reload");
        public static bool IsReloading()
        {
            return IsHandgunReloading() || IsDeagleReloading() || IsCzReloading() || Is44Reloading() ||
                   IsBarettaReloading() || IsShotgunReloading() || IsAA12Reloading() || IsStreetReloading() ||
                   IsSawnReloading() || IsUziReloading() || IsMp5Reloading() || IsP90Reloading() ||
                   IsGoldUziReloading() || IsAk47Reloading() || IsLmgReloading() || IsRifleReloading() ||
                   IsRifle2Reloading() || IsDsrReloading() || IsRPGReloading() || IsGLauncherReloading();
        }
        public static string GetReloadingAnimGroup()
        {
            if (IsHandgunReloading()) return "gun@handgun";
            if (IsDeagleReloading()) return "gun@deagle";
            if (IsCzReloading()) return "gun@cz75";
            if (Is44Reloading()) return "gun@44A";
            if (IsBarettaReloading()) return "gun@baretta";
            if (IsShotgunReloading()) return "gun@shotgun";
            if (IsAA12Reloading()) return "gun@aa12";
            if (IsStreetReloading()) return "gun@test_gun";
            if (IsSawnReloading()) return "gun@sawnoff";
            if (IsUziReloading()) return "gun@uzi";
            if (IsMp5Reloading()) return "gun@mp5k";
            if (IsP90Reloading()) return "gun@p90";
            if (IsGoldUziReloading()) return "gun@gold_uzi";
            if (IsAk47Reloading()) return "gun@ak47";
            if (IsLmgReloading()) return "gun@m249";
            if (IsRifleReloading()) return "gun@rifle";
            if (IsRifle2Reloading()) return "gun@rifle";
            if (IsDsrReloading()) return "gun@dsr1";
            if (IsRPGReloading()) return "gun@rocket";
            if (IsGLauncherReloading()) return "gun@grnde_launch";
            return null;
        }

        public static int GetWeaponType()
        {
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int pWeapon);
            return pWeapon;
        }
        public static IVWeaponInfo GetWeaponInfo()
        {
            return IVWeaponInfo.GetWeaponInfo((uint)GetWeaponType());
        }

        public static WeaponGroup GetWeaponGroup() 
        {
            return (WeaponGroup)GetWeaponInfo().Group;
        }

        public static List<eWeaponType> GetWeaponInventory(bool IncludeMelee)
        {
            List<eWeaponType> inventory = new List<eWeaponType>();

            HashSet<eWeaponType> melee = new HashSet<eWeaponType>
            {
                eWeaponType.WEAPON_UNARMED,
                eWeaponType.WEAPON_BASEBALLBAT,
                eWeaponType.WEAPON_KNIFE,
                eWeaponType.WEAPON_ANYMELEE,
                eWeaponType.WEAPON_ANYWEAPON
            };


            foreach (eWeaponType weapon in Enum.GetValues(typeof(eWeaponType)))
            {
                if (melee.Contains(weapon) && !IncludeMelee)
                    melee.Add(weapon);

                if (HAS_CHAR_GOT_WEAPON(Main.PlayerPed.GetHandle(), (int)weapon) && !melee.Contains(weapon))
                {
                    inventory.Add(weapon);
                }
            }

            return inventory;
        }

        public static void PrintWeaponInventory()
        {
            // Get the player's weapon inventory
            List<eWeaponType> inventory = GetWeaponInventory(true);

            // Print each weapon in the inventory
            Main.Log("Player's Weapon Inventory:");
            foreach (eWeaponType weapon in inventory)
                Main.Log($"- {weapon}");
        }
        public static Dictionary<eWeaponType, int> GetWeaponAmmoCounts()
        {
            Dictionary<eWeaponType, int> ammoCounts = new Dictionary<eWeaponType, int>();

            foreach (eWeaponType weapon in GetWeaponInventory(false))
            {
                GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)weapon, out int ammo);
                ammoCounts[weapon] = ammo;
            }

            return ammoCounts;
        }
        public static int GetSpecificWeaponAmmo(eWeaponType eWeaponType)
        {
            Dictionary<eWeaponType, int> ammoCounts = GetWeaponAmmoCounts();
            if (ammoCounts.ContainsKey(eWeaponType))
                return ammoCounts[eWeaponType];
            else
                return 0;
        }
        public static bool CanReload()
        {
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);
            GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), currentWeapon, out int weaponAmmo);
            GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, out int clipAmmo);
            GET_MAX_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), currentWeapon, out int clipAmmoMax);

            if (clipAmmo < clipAmmoMax 
                && clipAmmo > 0)
                return true;
            else
                return false;
        }
        public static bool IsHoldingGun()
        {
            if (GetWeaponInfo().WeaponFlags.Gun == true)
                return true;
            else
                return false;
        }
        public static eWeaponType GetNextAvailableWeapon()
        {
            eWeaponType currentWeapon = (eWeaponType)GetWeaponType();
            List<eWeaponType> inventory = GetWeaponInventory(false);

            if (inventory.Count == 0)
                return eWeaponType.WEAPON_UNARMED;

            int currentIndex = inventory.IndexOf(currentWeapon);

            int nextIndex = (currentIndex + 1) % inventory.Count;

            return inventory[nextIndex];
        }
        public static eWeaponType GetPreviousAvailableWeapon()
        {
            eWeaponType currentWeapon = (eWeaponType)GetWeaponType();
            List<eWeaponType> inventory = GetWeaponInventory(false);

            if (inventory.Count == 0)
                return eWeaponType.WEAPON_UNARMED;

            int currentIndex = inventory.IndexOf(currentWeapon);

            int previousIndex = (currentIndex - 1 + inventory.Count) % inventory.Count;
            return inventory[previousIndex];
        }

        public static int GetNextWeaponAsInt()
        {
            List<eWeaponType> inventory = GetWeaponInventory(false);

            if (inventory.Count == 0)
                return 0;

            int currentWeapon = GetWeaponType();
            int currentIndex = inventory.FindIndex(weapon => (int)weapon == currentWeapon);
            int nextIndex = (currentIndex + 1) % inventory.Count;

            eWeaponType nextWeapon = inventory[nextIndex];
            
            return (int)nextWeapon;
        }
        public static int GetPreviousWeaponAsInt()
        {
            List<eWeaponType> inventory = GetWeaponInventory(false);
            if (inventory.Count == 0)
                return 0;

            int currentWeapon = GetWeaponType();
            int currentIndex = inventory.FindIndex(weapon => (int)weapon == currentWeapon);
            int previousIndex = (currentIndex - 1 + inventory.Count) % inventory.Count;
            
            eWeaponType previousWeapon = inventory[previousIndex];

            return (int)previousWeapon;
        }


    }
}

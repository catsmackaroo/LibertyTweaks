using CCL.GTAIV;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static IVSDKDotNet.Native.Natives;

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
    public enum VanillaGuns
    {
        WEAPON_GRENADE,
        WEAPON_MOLOTOV,
        WEAPON_ROCKET,
        WEAPON_PISTOL,
        WEAPON_DEAGLE,
        WEAPON_SHOTGUN,
        WEAPON_BARETTA,
        WEAPON_MICRO_UZI,
        WEAPON_MP5,
        WEAPON_AK47,
        WEAPON_M4,
        WEAPON_SNIPERRIFLE,
        WEAPON_M40A1,
        WEAPON_RLAUNCHER,
        WEAPON_MINIGUN,
        WEAPON_EPISODIC_1,
        WEAPON_EPISODIC_2,
        WEAPON_EPISODIC_3,
        WEAPON_EPISODIC_4,
        WEAPON_EPISODIC_5,
        WEAPON_EPISODIC_6,
        WEAPON_EPISODIC_7,
        WEAPON_EPISODIC_8,
        WEAPON_EPISODIC_9,
        WEAPON_EPISODIC_10,
        WEAPON_EPISODIC_11,
        WEAPON_EPISODIC_12,
        WEAPON_EPISODIC_13,
        WEAPON_EPISODIC_14,
        WEAPON_EPISODIC_15,
        WEAPON_EPISODIC_16,
        WEAPON_EPISODIC_17,
        WEAPON_EPISODIC_18,
        WEAPON_EPISODIC_19,
        WEAPON_EPISODIC_20,
        WEAPON_EPISODIC_21,
        WEAPON_EPISODIC_22,
        WEAPON_EPISODIC_23,
        WEAPON_EPISODIC_24,
    }
    public class WeaponHelpers
    {
        private static List<string> addonAnimGroups = new List<string>();
        public static void InitAddonAnims(SettingsFile settings)
        {
            string[] animGroups = settings.GetValue("Addon Animations", "Addon Anim Groups", "").Split(',');

            addonAnimGroups.Clear();
            addonAnimGroups.AddRange(animGroups);

            foreach (var group in addonAnimGroups)
            {
                if (!string.IsNullOrWhiteSpace(group))
                {
                    Main.Log($"Registering {group} to addon anim groups.");
                }
            }
        }
        public static bool IsAddonWeaponReloading()
        {
            bool isPlayerDucking = IS_CHAR_DUCKING(Main.PlayerPed.GetHandle());

            foreach (var group in addonAnimGroups)
            {
                if (Main.PlayerPed.GetAnimationController().IsPlaying(group, isPlayerDucking ? "reload_crouch" : "reload"))
                {
                    return true;
                }
            }

            return false;
        }
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
                   IsRifle2Reloading() || IsDsrReloading() || IsRPGReloading() || IsGLauncherReloading() || IsAddonWeaponReloading();
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
        public static float GetShotgunReloadAnimTime()
        {
            var animController = Main.PlayerPed.GetAnimationController();
            bool isPlayerDucking = IS_CHAR_DUCKING(Main.PlayerPed.GetHandle());

            if (IsBarettaReloading())
                return animController.GetCurrentAnimationTime("gun@baretta", isPlayerDucking ? "reload_crouch" : "reload");
            else if (IsShotgunReloading())
                return animController.GetCurrentAnimationTime("gun@shotgun", isPlayerDucking ? "reload_crouch" : "reload");
            else if (IsAA12Reloading())
                return animController.GetCurrentAnimationTime("gun@aa12", isPlayerDucking ? "reload_crouch" : "reload");
            else if (IsStreetReloading())
                return animController.GetCurrentAnimationTime("gun@test_gun", isPlayerDucking ? "reload_crouch" : "reload");
            else if (IsSawnReloading())
                return animController.GetCurrentAnimationTime("gun@sawnoff", isPlayerDucking ? "reload_crouch" : "reload");

            return 0.0f;
        }
        public static bool IsPlayerAiming()
        {
            string[] predefinedAnimations = new string[]
            {
                "gun@handgun|fire", "gun@handgun|fire_crouch",
                "gun@deagle|fire", "gun@deagle|fire_crouch",
                "gun@uzi|fire", "gun@uzi|fire_crouch",
                "gun@mp5k|fire", "gun@mp5k|fire_crouch",
                "gun@sawnoff|fire", "gun@sawnoff|fire_crouch",
                "gun@shotgun|fire", "gun@shotgun|fire_crouch",
                "gun@baretta|fire", "gun@baretta|fire_crouch",
                "gun@cz75|fire", "gun@cz75|fire_crouch",
                "gun@grnde_launch|fire", "gun@grnde_launch|fire_crouch",
                "gun@p90|fire", "gun@p90|fire_crouch",
                "gun@gold_uzi|fire", "gun@gold_uzi|fire_crouch",
                "gun@aa12|fire", "gun@aa12|fire_crouch",
                "gun@44a|fire", "gun@44a|fire_crouch",
                "gun@ak47|fire", "gun@ak47|fire_crouch", "gun@ak47|fire_up", "gun@ak47|fire_down",
                "gun@test_gun|fire", "gun@test_gun|fire_crouch", "gun@test_gun|fire_up", "gun@test_gun|fire_down",
                "gun@m249|fire", "gun@m249|fire_crouch", "gun@m249|fire_up", "gun@m249|fire_down",
                "gun@rifle|fire", "gun@rifle|fire_crouch", "gun@rifle|fire_alt", "gun@rifle|fire_crouch_alt",
                "gun@dsr1|fire", "gun@dsr1|fire_crouch", "gun@dsr1|fire_alt", "gun@dsr1|fire_crouch_alt",
                "gun@rocket|fire", "gun@rocket|fire_crouch"
            };

            // Combine predefined animations with addon animations
            List<string> combinedAnimations = new List<string>(predefinedAnimations);
            foreach (var group in addonAnimGroups)
            {
                combinedAnimations.Add($"{group}|fire");
                combinedAnimations.Add($"{group}|fire_crouch");
            }

            foreach (var anim in combinedAnimations)
            {
                var parts = anim.Split('|');
                if (IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), parts[0], parts[1]))
                {
                    return true;
                }
            }

            return false;
        }

        // If using this method, call "CLEAR_CAR_LAST_WEAPON_DAMAGE()" after to reset. 
        // Can't get it to work in one method for some reason
        public static bool HasCarBeenDamagedByAnyWeapon(IVVehicle vehicleIV)
        {
            foreach (eWeaponType weapon in Enum.GetValues(typeof(VanillaGuns)))
            {
                if (HAS_CAR_BEEN_DAMAGED_BY_WEAPON(vehicleIV.GetHandle(), (int)weapon))
                {
                    return true;
                }
            }
            return false;
        }
        public static int GetCurrentWeaponType()
        {
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int pWeapon);
            return pWeapon;
        }
        public static IVWeaponInfo GetCurrentWeaponInfo()
        {
            return IVWeaponInfo.GetWeaponInfo((uint)GetCurrentWeaponType());
        }
        public static WeaponGroup GetCurrentWeaponGroup()
        {
            return (WeaponGroup)GetCurrentWeaponInfo().Group;
        }
        public static bool IsTryingToDriveBy()
        {
            if (IS_USING_CONTROLLER() && IsDrivebying()) return true;

            if (!IS_USING_CONTROLLER())
            {
                if (IsHoldingGun() && NativeControls.IsGameKeyPressed(0, GameKey.Aim)
                || IsHoldingGun() && NativeControls.IsGameKeyPressed(0, GameKey.Attack))
                {
                    return true;
                }
            }

            return false;
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
                    inventory.Add(weapon);
            }

            return inventory;
        }
        public static void PrintWeaponInventory()
        {
            // Example to use WeaponInventory 
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
            if (GetCurrentWeaponInfo().WeaponFlags.Gun == true)
                return true;
            else
                return false;
        }
        public static eWeaponType GetNextAvailableWeapon()
        {
            eWeaponType currentWeapon = (eWeaponType)GetCurrentWeaponType();
            List<eWeaponType> inventory = GetWeaponInventory(false);

            if (inventory.Count == 0)
                return eWeaponType.WEAPON_UNARMED;

            int currentIndex = inventory.IndexOf(currentWeapon);

            int nextIndex = (currentIndex + 1) % inventory.Count;

            return inventory[nextIndex];
        }
        public static eWeaponType GetPreviousAvailableWeapon()
        {
            eWeaponType currentWeapon = (eWeaponType)GetCurrentWeaponType();
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

            int currentWeapon = GetCurrentWeaponType();
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

            int currentWeapon = GetCurrentWeaponType();
            int currentIndex = inventory.FindIndex(weapon => (int)weapon == currentWeapon);
            int previousIndex = (currentIndex - 1 + inventory.Count) % inventory.Count;

            eWeaponType previousWeapon = inventory[previousIndex];

            return (int)previousWeapon;
        }
        public static bool IsPlayerBlindfiring()
        {
            if (!IS_PED_IN_COVER(Main.PlayerPed.GetHandle()))
                return false;

            string[] animGroups = new string[]
            {
                "cover_l_high_centre", "cover_l_high_corner", "cover_r_high_corner", "cover_r_high_centre",
                "cover_l_low_centre", "cover_l_low_corner", "cover_r_low_corner", "cover_r_low_centre"

            };

            string[] animNames = new string[]
            {
                "pistol_blindfire", "rifle_blindfire", "ak47_blindfire", 
                "rocket_blindfire", "shotgun_blindfire", "uzi_blindfire"
            };

            foreach (var group in animGroups)
            {
                foreach (var name in animNames)
                {
                    if (IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), group, name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public static (string animGroup, string animName) GetCurrentPlayerWeaponAnim()
        {
            string[] predefinedAnimGroups = new string[]
            {
                "veh@drivebylow", "veh@drivebystd", "veh@drivebytruck", "veh@drivebyvan",
                "gun@handgun", "gun@deagle", "gun@uzi", "gun@mp5k", "gun@sawnoff", "gun@shotgun",
                "gun@baretta", "gun@cz75", "gun@grnde_launch", "gun@p90", "gun@gold_uzi",
                "gun@aa12", "gun@44a", "gun@ak47", "gun@test_gun", "gun@m249", "gun@rifle",
                "gun@dsr1", "cover_l_high_corner", "cover_l_low_corner", "cover_r_high_corner", "cover_r_low_corner",
                "jump_std", "jump_rifle"
            };

            // Combines vanilla anim groups with addon ones
            string[] animGroups = new string[predefinedAnimGroups.Length + addonAnimGroups.Count];
            predefinedAnimGroups.CopyTo(animGroups, 0);
            addonAnimGroups.ToArray().CopyTo(animGroups, predefinedAnimGroups.Length);

            string[] animNames = new string[]
            {
                "ds_aim_in", "ds_aim_loop", "ds_aim_out", "fire", "fire_crouch", "holster", "holster_2_aim", "holster_crouch",
                "unholster", "unholster_crouch", "fire_up", "fire_down", "fire_alt", "fire_crouch_alt", "holster_up", "holster_down",
                "unholster_up", "unholster_down", "unholster_alt", "unholster_crouch_alt", "pistol_blindfire", "rifle_blindfire",
                "ak47_blindfire", "rocket_blindfire", "shotgun_blindfire", "jump_inair_l", "jump_inair_r", "reload", "reload_crouch",
            };

            foreach (var group in animGroups)
            {
                foreach (var name in animNames)
                {
                    if (IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), group, name))
                    {
                        return (group, name);
                    }
                }
            }

            return (null, null);
        }
        public static bool IsDrivebying()
        {
            string[] animations = new string[]
            {
        "veh@drivebylow|ds_aim_in", "veh@drivebylow|ds_aim_loop", "veh@drivebylow|ds_aim_out",
        "veh@drivebystd|ds_aim_in", "veh@drivebystd|ds_aim_loop", "veh@drivebystd|ds_aim_out",
        "veh@drivebytruck|ds_aim_in", "veh@drivebytruck|ds_aim_loop", "veh@drivebytruck|ds_aim_out",
        "veh@drivebyvan|ds_aim_in", "veh@drivebyvan|ds_aim_loop", "veh@drivebyvan|ds_aim_out"
            };

            foreach (var anim in animations)
            {
                var parts = anim.Split('|');
                if (IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), parts[0], parts[1]))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsPlayerHolstering()
        {
            // add addon weapons if ever using this 
            string[] animations = new string[]
            {
                "gun@handgun|holster", "gun@handgun|holster_2_aim", "gun@handgun|holster_crouch",
                "gun@deagle|holster", "gun@deagle|holster_2_aim", "gun@deagle|holster_crouch",
                "gun@uzi|holster", "gun@uzi|holster_2_aim", "gun@uzi|holster_crouch",
                "gun@mp5k|holster", "gun@mp5k|holster_2_aim", "gun@mp5k|holster_crouch",
                "gun@sawnoff|holster", "gun@sawnoff|holster_2_aim", "gun@sawnoff|holster_crouch",
                "gun@shotgun|holster", "gun@shotgun|holster_2_aim", "gun@shotgun|holster_crouch",
                "gun@baretta|holster", "gun@baretta|holster_2_aim", "gun@baretta|holster_crouch",
                "gun@cz75|holster", "gun@cz75|holster_2_aim", "gun@cz75|holster_crouch",
                "gun@grnde_launch|holster", "gun@grnde_launch|holster_2_aim", "gun@grnde_launch|holster_crouch",
                "gun@p90|holster", "gun@p90|holster_2_aim", "gun@p90|holster_crouch",
                "gun@gold_uzi|holster", "gun@gold_uzi|holster_2_aim", "gun@gold_uzi|holster_crouch",
                "gun@aa12|holster", "gun@aa12|holster_2_aim", "gun@aa12|holster_crouch",
                "gun@44a|holster", "gun@44a|holster_2_aim", "gun@44a|holster_crouch",
                "gun@ak47|holster", "gun@ak47|holster_2_aim", "gun@ak47|holster_crouch", "gun@ak47|holster_up", "gun@ak47|holster_down",
                "gun@test_gun|holster", "gun@test_gun|holster_2_aim", "gun@test_gun|holster_crouch", "gun@test_gun|holster_up", "gun@test_gun|holster_down",
                "gun@m249|holster", "gun@m249|holster_2_aim", "gun@m249|holster_crouch", "gun@m249|holster_up", "gun@m249|holster_down",
                "gun@rifle|holster", "gun@rifle|holster_2_aim", "gun@rifle|holster_crouch", "gun@rifle|holster_alt", "gun@rifle|holster_crouch_alt",
                "gun@dsr1|holster", "gun@dsr1|holster_2_aim", "gun@dsr1|holster_crouch", "gun@dsr1|holster_alt", "gun@dsr1|holster_crouch_alt"
            };

            foreach (var anim in animations)
            {
                var parts = anim.Split('|');
                if (IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), parts[0], parts[1]))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsPlayerUnHolstering()
        {
            // add addon weapons if ever using this 
            string[] animations = new string[]
            {
                "gun@handgun|unholster", "gun@handgun|unholster_crouch",
                "gun@deagle|unholster", "gun@deagle|unholster_crouch",
                "gun@uzi|unholster", "gun@uzi|unholster_crouch",
                "gun@mp5k|unholster", "gun@mp5k|unholster_crouch",
                "gun@sawnoff|unholster", "gun@sawnoff|unholster_crouch",
                "gun@shotgun|unholster", "gun@shotgun|unholster_crouch",
                "gun@baretta|unholster", "gun@baretta|unholster_crouch",
                "gun@cz75|unholster", "gun@cz75|unholster_crouch",
                "gun@grnde_launch|unholster", "gun@grnde_launch|unholster_crouch",
                "gun@p90|unholster", "gun@p90|unholster_crouch",
                "gun@gold_uzi|unholster", "gun@gold_uzi|unholster_crouch",
                "gun@aa12|unholster", "gun@aa12|unholster_crouch",
                "gun@44a|unholster", "gun@44a|unholster_crouch",
                "gun@ak47|unholster", "gun@ak47|unholster_crouch", "gun@ak47|unholster_up", "gun@ak47|unholster_down",
                "gun@test_gun|unholster", "gun@test_gun|unholster_crouch", "gun@test_gun|unholster_up", "gun@test_gun|unholster_down",
                "gun@m249|unholster", "gun@m249|unholster_crouch", "gun@m249|unholster_up", "gun@m249|unholster_down",
                "gun@rifle|unholster", "gun@rifle|unholster_crouch", "gun@rifle|unholster_alt", "gun@rifle|unholster_crouch_alt",
                "gun@dsr1|unholster", "gun@dsr1|unholster_crouch", "gun@dsr1|unholster_alt", "gun@dsr1|unholster_crouch_alt"
            };

            foreach (var anim in animations)
            {
                var parts = anim.Split('|');
                if (IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), parts[0], parts[1]))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsTwoHandedGun(int weapon)
        {
            if (weapon == 0) return false;

            if (IVWeaponInfo.GetWeaponInfo((uint)weapon).WeaponFlags.TwoHanded)
                return true;
            else
                return false;
        }
        public static bool IsHeavyGun(int weapon)
        {
            if (weapon == 0) return false;

            if (IVWeaponInfo.GetWeaponInfo((uint)weapon).WeaponFlags.Heavy)
                return true;
            else
                return false;
        }
    }
}

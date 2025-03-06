using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    public class PlayerHelper
    {
        public static float GetTotalSpeedVehicle(IVVehicle vehicleIV)
        {
            var speedVector = vehicleIV.GetSpeedVector(true);
            return (speedVector.Y + speedVector.X + speedVector.Z);
        }
        public static bool IsPlayerInOrNearCombat()
        {
            IVPool pedPool = IVPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    if (ptr == IVPlayerInfo.FindThePlayerPed())
                        continue;

                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    if (IS_PED_IN_COMBAT(pedHandle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsPlayerSeenByPolice()
        {
            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;
                IVPed pedPed = NativeWorld.GetPedInstanceFromHandle(pedHandle);

                if (pedPed == null || IS_CHAR_DEAD(pedHandle))
                    continue;

                GET_CHAR_MODEL(pedHandle, out uint pedModel);
                GET_CURRENT_BASIC_COP_MODEL(out uint copModel);

                if (pedModel != copModel)
                    continue;

                if (pedPed.CanCharSeeChar(Main.PlayerPed, 50, 130))
                    return true;
            }
            return false;
        }

        private static uint currentHealth = 0;
        private static uint previousHealth = 0;
        private static bool damagedTaken = false;
        public static bool HasPlayerBeenDamagedHealth()
        {

            GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out previousHealth);

            if (previousHealth < currentHealth && currentHealth > 0 && previousHealth > 0)
                damagedTaken = true;

            GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out currentHealth);

            if (damagedTaken)
            {
                previousHealth = currentHealth;
                damagedTaken = false;
                return true;
            }
            else
                return false;

        }

        private static uint currentArmor = 0;
        private static uint previousArmor = 0;
        private static bool armorDamageTaken = false;
        public static bool HasPlayerBeenDamagedArmor()
        {
            GET_CHAR_ARMOUR(Main.PlayerPed.GetHandle(), out previousArmor);

            if (previousArmor < currentArmor && currentArmor > 0 && previousArmor > 0)
                armorDamageTaken = true;

            GET_CHAR_ARMOUR(Main.PlayerPed.GetHandle(), out currentArmor);

            if (armorDamageTaken)
            {
                previousArmor = currentArmor;
                armorDamageTaken = false;
                return true;
            }
            else
                return false;
        }

        private static DateTime lastShotTime = DateTime.MinValue;
        private static readonly TimeSpan timeBetweenShots = TimeSpan.FromSeconds(1);

        public static bool HasPlayerShotRecently()
        {
            if (IS_CHAR_SHOOTING(Main.PlayerPed.GetHandle()))
            {
                DateTime currentTime = DateTime.Now;

                TimeSpan elapsed = currentTime - lastShotTime;

                if (elapsed <= timeBetweenShots)
                {
                    lastShotTime = currentTime;
                    return true;
                }

                lastShotTime = currentTime;
            }
            return false;
        }

        private static uint previousVehicleHealth = 0;
        private static uint currentVehicleHealth = 0;

        public static (int damageAmount, int damageLevel, float normalizedDamage) GetVehicleDamage()
        {
            int damageAmount = 0;
            int damageLevel = 0;
            float normalizedDamage = 0f;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
            {
                previousVehicleHealth = 0;
                currentVehicleHealth = 0;
                damageAmount = 0;
                damageLevel = 0;
                normalizedDamage = 0f;
                return (0, 0, 0f);
            }

            GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int currentVehicle);
            GET_CAR_HEALTH(currentVehicle, out uint currentHealth);

            if (previousVehicleHealth == 0 || currentVehicleHealth == 0)
            {
                previousVehicleHealth = currentHealth;
                currentVehicleHealth = currentHealth;
                return (0, 0, 0f);
            }

            currentVehicleHealth = currentHealth;

            if (currentVehicleHealth < previousVehicleHealth)
            {
                damageAmount = (int)(previousVehicleHealth - currentVehicleHealth);
                normalizedDamage = CommonHelpers.Clamp((float)damageAmount / 2f, 0f, 100f);
            }

            previousVehicleHealth = currentVehicleHealth;

            if (damageAmount > 450) damageLevel = 4;
            else if (damageAmount > 250) damageLevel = 3;
            else if (damageAmount > 100) damageLevel = 2;
            else if (damageAmount > 0) damageLevel = 1;

            return (damageAmount, damageLevel, normalizedDamage);
        }
        public static bool IsAiming()
        {
            string[] animations = new string[]
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
                "gun@dsr1|fire", "gun@dsr1|fire_crouch", "gun@dsr1|fire_alt", "gun@dsr1|fire_crouch_alt"
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

        public static bool IsBlindfiring() => IS_PED_IN_COVER(Main.PlayerPed.GetHandle())
            && (IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "pistol_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "rifle_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "ak47_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "rocket_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_high_corner", "shotgun_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "shotgun_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "rocket_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "ak47_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "rifle_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "pistol_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "pistol_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "rifle_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "ak47_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "rocket_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_high_corner", "shotgun_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "shotgun_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "rocket_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "ak47_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_r_low_corner", "rifle_blindfire")
            || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "cover_l_low_corner", "pistol_blindfire"));
        public static bool IsHolstering()
        {
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
        public static bool IsAboutToDriveby()
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
        public static (string animGroup, string animName) GetCurrentPlayingAnim()
        {
            string[] animGroups = new string[]
            {
        "veh@drivebylow", "veh@drivebystd", "veh@drivebytruck", "veh@drivebyvan",
        "gun@handgun", "gun@deagle", "gun@uzi", "gun@mp5k", "gun@sawnoff", "gun@shotgun",
        "gun@baretta", "gun@cz75", "gun@grnde_launch", "gun@p90", "gun@gold_uzi",
        "gun@aa12", "gun@44a", "gun@ak47", "gun@test_gun", "gun@m249", "gun@rifle",
        "gun@dsr1", "cover_l_high_corner", "cover_l_low_corner", "cover_r_high_corner", "cover_r_low_corner",
        "jump_std", "jump_rifle"
            };

            string[] animNames = new string[]
            {
        "ds_aim_in", "ds_aim_loop", "ds_aim_out", "fire", "fire_crouch", "holster", "holster_2_aim", "holster_crouch",
        "unholster", "unholster_crouch", "fire_up", "fire_down", "fire_alt", "fire_crouch_alt", "holster_up", "holster_down",
        "unholster_up", "unholster_down", "unholster_alt", "unholster_crouch_alt", "pistol_blindfire", "rifle_blindfire",
        "ak47_blindfire", "rocket_blindfire", "shotgun_blindfire", "jump_inair_l", "jump_inair_r"
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

        public static bool IsUnHolstering()
        {
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

        public static bool IsJumping() => IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "jump_std", "jump_inair_l") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "jump_std", "jump_inair_r") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "jump_rifle", "jump_inair_l") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "jump_rifle", "jump_inair_r");
        public static bool IsPlayerSkidding()
        {
            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || IS_PAUSE_MENU_ACTIVE()
                || IS_CHAR_IN_ANY_BOAT(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                return false;
            }

            IVVehicle playerVehicle = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            GET_CAR_ROLL(playerVehicle.GetHandle(), out float roll);
            float sideSpeed = playerVehicle.GetSpeedVector(true).X;

            if ((sideSpeed > 8 || sideSpeed < -8)
                && !IS_CAR_IN_AIR_PROPER(playerVehicle.GetHandle())
                && roll < 20 && roll > -20)
            {
                return true;
            }

            return false;
        }
        public static bool isPlayerDucking = IS_CHAR_DUCKING(Main.PlayerPed.GetHandle());

        public static bool IsPlayerDoingBurnout()
        {
            if (Main.PlayerVehicle == null)
                return false;
            float carspeed = Main.PlayerVehicle.GetSpeed();

            if (IS_CAR_IN_AIR_PROPER(Main.PlayerVehicle.GetHandle()) || carspeed > 2f)
                return false;

            bool only2wheels = Main.PlayerVehicle.WheelCount <= 2;
            float combinedSpeed;
            float speed;
            float speed2;
            float speed3 = 0;
            float speed4 = 0;
            IVVehicleWheel vehWheel1 = Main.PlayerVehicle.Wheels[0];
            IVVehicleWheel vehWheel2 = Main.PlayerVehicle.Wheels[1];
            IVVehicleWheel vehWheel3 = null;
            IVVehicleWheel vehWheel4 = null;

            if (!only2wheels)
            {
                vehWheel3 = Main.PlayerVehicle.Wheels[2];
                vehWheel4 = Main.PlayerVehicle.Wheels[3];
            }

            if (!only2wheels && (vehWheel3 != null && vehWheel4 != null))
            {
                speed3 = vehWheel3.Speed;
                speed4 = vehWheel4.Speed;
            }

            speed = vehWheel1.Speed;
            speed2 = vehWheel2.Speed;

            combinedSpeed = speed + speed2 + speed3 + speed4;

            if (combinedSpeed < -70)
                return true;
            else
                return false;
        }

        public static bool IsPlayerAimingAtAnyChar()
        {
            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;
                if (IS_PLAYER_FREE_AIMING_AT_CHAR(Main.PlayerIndex, pedHandle))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

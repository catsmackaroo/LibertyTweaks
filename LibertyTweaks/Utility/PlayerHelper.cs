﻿using CCL.GTAIV;
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
        public static bool IsPlayerAiming()
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
                "gun@dsr1|fire", "gun@dsr1|fire_crouch", "gun@dsr1|fire_alt", "gun@dsr1|fire_crouch_alt",
                "gun@rocket|fire", "gun@rocket|fire_crouch"
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

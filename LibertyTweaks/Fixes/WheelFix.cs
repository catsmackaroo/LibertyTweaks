﻿using CCL.GTAIV;
using IVSDKDotNet;
using System;

// Credits: ClonkAndre

namespace LibertyTweaks
{
    internal class WheelFix
    {

        private static bool enable;
        private static bool canWheelFixCodeBeExecuted;
        private static bool canChangeWheelValue;
        private static float newWheelValue;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            WheelFix.section = section;
            enable = settings.GetBoolean(section, "Wheel Fix", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void PreChecks()
        {
            // If the fix is disabled, return from this method
            if (!enable)
                return;

            // If the player was atleast once in a vehicle we allow the actual wheel fix code to be executed to prevent the error
            if (Main.PlayerPed.GetVehicle() != null)
                canWheelFixCodeBeExecuted = true;
        }

        public static void Process(UIntPtr vehPtr)
        {
            // If the fix is disabled, return from this method
            if (!enable)
                return;

            // Is this code allowed to run yet? Will be true once the player was atleast once in a vehicle
            if (!canWheelFixCodeBeExecuted)
                return;

            // If the vehPtr is zero
            if (vehPtr == UIntPtr.Zero)
                return;

            // Find the player ped pointer
            UIntPtr plyPtr = IVPlayerInfo.FindThePlayerPed();

            // Check if the pointer is not empty
            if (plyPtr == UIntPtr.Zero)
                return;

            // If player is dead then reset values
            if (Main.PlayerPed.Dead)
            {
                newWheelValue = 0f;
                canChangeWheelValue = false;
                return;
            }

            // Get the last/current vehicle of the player ped
            IVVehicle veh = IVVehicle.FromUIntPtr(Main.PlayerPed.Vehicle);

            // Check if the veh is null
            if (veh is null || veh == null)
                return;

            // If the vehPtr is equals to the veh pointer
            if (vehPtr == veh.GetUIntPtr())
            {
                // If the driver of the veh is the player ped
                if (veh.Driver == Main.PlayerPed.GetUIntPtr())
                {
                    // If player pressed the EnterCar key we will store the last "SteerActual" value so when player is no longer in vehicle it will be applied
                    if (NativeControls.IsGameKeyPressed(0, GameKey.EnterCar))
                    {
                        newWheelValue = veh.SteerActual;
                        canChangeWheelValue = false;
                    }
                    else
                    {
                        // Reset value if we can to prevent the player (or NPCs) from not being able to steer anymore
                        if (canChangeWheelValue)
                        {
                            newWheelValue = 0f;
                            canChangeWheelValue = false;
                        }
                    }
                }
                else
                {
                    // If there is no driver in the veh then we set the new steering value we stored before
                    if (veh.Driver == UIntPtr.Zero)
                    {
                        veh.SteerActual = newWheelValue;
                        canChangeWheelValue = true;
                    }
                    else // If there is a driver in the veh but it is not the player ped then reset value
                    {
                        newWheelValue = 0f;
                        canChangeWheelValue = false;
                    }
                }
            }
        }

    }
}

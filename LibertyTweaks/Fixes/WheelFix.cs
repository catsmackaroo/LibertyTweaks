using System;

using CCL.GTAIV;
using IVSDKDotNet;

// Credits: ClonkAndre

namespace LibertyTweaks
{
    internal class WheelFix
    {

        private static bool enable;
        private static bool canWheelFixCodeBeExecuted;
        private static bool canChangeWheelValue;
        private static float newWheelValue;
        
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Wheel Fix", true);
        }

        public static void PreChecks()
        {
            // If the fix is disabled, return from this method
            if (!enable)
                return;

            // Get the player ped
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            // If the player was atleast once in a vehicle we allow the actual wheel fix code to be executed to prevent the error
            if (playerPed.GetVehicle() != null)
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

            // Get the player ped from the pointer above
            IVPed playerPed = IVPed.FromUIntPtr(plyPtr);

            // If player is dead then reset values
            if (playerPed.Dead)
            {
                newWheelValue = 0f;
                canChangeWheelValue = false;
                return;
            }

            // Get the last/current vehicle of the player ped
            IVVehicle veh = IVVehicle.FromUIntPtr(playerPed.Vehicle);
 
            // Check if the veh is null
            if (veh is null || veh == null)
                return;

            // If the vehPtr is equals to the veh pointer
            if (vehPtr == veh.GetUIntPtr())
            {
                // If the driver of the veh is the player ped
                if (veh.Driver == playerPed.GetUIntPtr())
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

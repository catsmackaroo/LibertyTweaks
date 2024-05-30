using CCL.GTAIV;

using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: AssaultKifle47, catsmackaroo & ClonkAndre

namespace LibertyTweaks
{
    internal class NoOvertaking
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Overtaking Fix", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            // Gets the playerPed
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            int playerPedHandle = playerPed.GetHandle();

            // Checks if the player is in any car
            if (!IS_CHAR_IN_ANY_CAR(playerPedHandle))
                return;

            // Gets the car the player is using
            GET_CAR_CHAR_IS_USING(playerPedHandle, out int pVehInt);

            // Gets the speed of the car the player is using
            GET_CAR_SPEED(pVehInt, out float vehSpeed);

            // If speed is below 7.0 then do the overtaking fix logic
            if (vehSpeed < 7.0f)
            {
                // Gets the
                // ition behind the players vehicle by minus 6.0
                GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(pVehInt, 0.0f, -6.0f, 0.0f, out float pOffX, out float pOffY, out float pOffZ);

                // Gets the closest car by the offset Position from above
                int closestCar = GET_CLOSEST_CAR(pOffX, pOffY, pOffZ, 4f, 0, 69);
                
                // If there is no closest car then return
                if (closestCar == 0)
                    return;

                // Gets the driver of the closest car
                GET_DRIVER_OF_CAR(closestCar, out int closeCarPed);

                // If there is no driver in the closest car then return
                if (closeCarPed == 0)
                    return;

                uint playerIndex = GET_PLAYER_ID();
                STORE_WANTED_LEVEL((int)playerIndex, out uint playerWantedLevel);

                if (playerWantedLevel != 0)
                    return;

                // Tell driver of closest car to stand still
                Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(3.5), "Main", () =>
                {
                    _TASK_STAND_STILL(closeCarPed, 3000);
                });


                
            }
        }
    }
}

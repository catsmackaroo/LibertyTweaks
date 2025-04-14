using CCL.GTAIV;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class LessOvertaking
    {
        private static bool enable;
        private static Dictionary<int, DateTime> blockingStartTimes = new Dictionary<int, DateTime>();
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            LessOvertaking.section = section;
            enable = settings.GetBoolean(section, "Less Overtaking", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            int playerPedHandle = Main.PlayerPed.GetHandle();

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
                // Gets the position behind the player's vehicle by minus 6.0
                GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(pVehInt, 0.0f, -6.0f, 0.0f, out float pOffX, out float pOffY, out float pOffZ);

                // Gets the closest car by the offset position from above
                int closestCar = GET_CLOSEST_CAR(pOffX, pOffY, pOffZ, 4f, 0, 69);

                // If there is no closest car then return
                if (closestCar == 0)
                    return;

                GET_CAR_SPEED(closestCar, out float pVehSpeed);

                if (pVehSpeed > 7.0)
                    return;

                // Gets the driver of the closest car
                GET_DRIVER_OF_CAR(closestCar, out int closeCarPed);

                // If there is no driver in the closest car then return
                if (closeCarPed == 0)
                    return;

                STORE_WANTED_LEVEL(Main.PlayerIndex, out uint playerWantedLevel);

                if (playerWantedLevel != 0)
                    return;

                // Only perform script if the player is actually blocking their path
                GET_CAR_BLOCKING_CAR(closestCar, out int blockingCar);

                // Check if the player has been blocking the pedestrian for more than a minute
                if (blockingStartTimes.ContainsKey(closeCarPed))
                {
                    double blockingDuration = (DateTime.Now - blockingStartTimes[closeCarPed]).TotalSeconds;
                    if (blockingDuration > 60)
                    {
                        // Stop calling _TASK_STAND_STILL if blocking time exceeds one minute
                        return;
                    }
                }
                else
                {
                    blockingStartTimes[closeCarPed] = DateTime.Now;
                }

                _TASK_STAND_STILL(closeCarPed, 8000);
            }
        }
    }
}

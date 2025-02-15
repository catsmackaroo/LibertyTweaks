using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using System;
using System.Numerics;

// Credits: ServalEd & catsmackaroo

namespace LibertyTweaks
{
    internal class Downforce
    {
        private static bool enable;
        private static bool dontCrash;
        private static bool gotHandling = false;
        private static Vector3 CoM;
        private static float vehTraction;
        private static bool checkDateTime;
        private static DateTime currentDateTime;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Car Downforce", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (Main.PlayerPed == null || IS_CHAR_DEAD(Main.PlayerPed.GetHandle())) return;

            if (Main.PlayerPed.IsInVehicle())
                dontCrash = true;

            if (dontCrash)
            {
                IVVehicle playerVehicle = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

                if (playerVehicle != null && Main.PlayerPed.IsInVehicle())
                {
                    if (!gotHandling)
                    {
                        CoM = playerVehicle.Handling.CenterOfMass;
                        vehTraction = playerVehicle.Handling.TractionCurveMax;
                        gotHandling = true;
                    }

                    if (!checkDateTime)
                    {
                        currentDateTime = DateTime.Now;
                        checkDateTime = true;
                    }

                    if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > 50.0)
                    {
                        checkDateTime = false;

                        Vector3 speedVector = playerVehicle.GetSpeedVector(true);
                        playerVehicle.Handling.CenterOfMass = new Vector3(CoM.X, CoM.Y, CoM.Z - (speedVector.Y * 0.00125f));
                        playerVehicle.Handling.TractionCurveMax = vehTraction + (speedVector.Y * 0.00125f);
                    }
                }
                else if (playerVehicle != null && !Main.PlayerPed.IsInVehicle() && gotHandling)
                {
                    playerVehicle.Handling.CenterOfMass = CoM;
                    playerVehicle.Handling.TractionCurveMax = vehTraction;
                    gotHandling = false;
                }
            }
        }
    }
}

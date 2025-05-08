using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using System.Windows.Input;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class Hydraulics
    {
        private static bool enable;
        private static bool HasHydraulicsInstalled = false;
        private static bool hydraulics = false;
        private static DateTime lastToggleTime = DateTime.MinValue;
        private static readonly TimeSpan toggleDelay = TimeSpan.FromSeconds(0.75); // 1 second delay

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Car Hydraulics", "Enable", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE()) return;

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || IS_PAUSE_MENU_ACTIVE()
                || IS_CHAR_IN_ANY_BOAT(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_HELI(Main.PlayerPed.GetHandle()))
            {
                return;
            }

            IVVehicle playerVehicle = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            IVVehicle vehicleIV = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            HasHydraulicsInstalled = vehicleIV.HandlingFlags.HydraulicInst;

            if (NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && DateTime.Now - lastToggleTime > toggleDelay
                && !IS_CAR_IN_AIR_PROPER(vehicleIV.GetHandle())
                && vehicleIV.GetSpeed() < 5)
            {
                int[] wheelIndices = null;

                // Check for numpad inputs
                if (Keyboard.IsKeyDown(Key.NumPad8))
                {
                    wheelIndices = new int[] { 0, 1 }; // Wheel 0 & 1
                }
                else if (Keyboard.IsKeyDown(Key.NumPad4))
                {
                    wheelIndices = new int[] { 0, 2 }; // Wheel 0 & 2
                }
                else if (Keyboard.IsKeyDown(Key.NumPad6))
                {
                    wheelIndices = new int[] { 1, 3 }; // Wheel 1 & 3
                }
                else if (Keyboard.IsKeyDown(Key.NumPad2))
                {
                    wheelIndices = new int[] { 2, 3 }; // Wheel 2 & 3
                }
                else if (Keyboard.IsKeyDown(Key.NumPad8) && Keyboard.IsKeyDown(Key.NumPad4))
                {
                    wheelIndices = new int[] { 0 }; // Wheel 0
                }
                else if (Keyboard.IsKeyDown(Key.NumPad2) && Keyboard.IsKeyDown(Key.NumPad6))
                {
                    wheelIndices = new int[] { 1 }; // Wheel 1
                }
                else if (Keyboard.IsKeyDown(Key.NumPad2) && Keyboard.IsKeyDown(Key.NumPad4))
                {
                    wheelIndices = new int[] { 2 }; // Wheel 2
                }
                else if (Keyboard.IsKeyDown(Key.NumPad8) && Keyboard.IsKeyDown(Key.NumPad6))
                {
                    wheelIndices = new int[] { 3 }; // Wheel 3
                }

                if (wheelIndices != null)
                {
                    foreach (int wheelIndex in wheelIndices)
                    {
                        if (HasHydraulicsInstalled == true && !hydraulics)
                        {
                            Vector3 desiredPos = vehicleIV.Wheels[wheelIndex].Position += new Vector3(0, 0, -0.4f);

                            if (vehicleIV.Wheels[wheelIndex].Position != desiredPos)
                                vehicleIV.Wheels[wheelIndex].Position += new Vector3(0, 0, -0.01f);

                            if (vehicleIV.Wheels[wheelIndex].Position == desiredPos)
                                hydraulics = true;
                        }
                        else if (HasHydraulicsInstalled && hydraulics)
                        {
                            Vector3 desiredPos = vehicleIV.Wheels[wheelIndex].Position += new Vector3(0, 0, 0.4f);

                            if (vehicleIV.Wheels[wheelIndex].Position != desiredPos)
                                vehicleIV.Wheels[wheelIndex].Position += new Vector3(0, 0, 0.01f);

                            if (vehicleIV.Wheels[wheelIndex].Position == desiredPos)
                                hydraulics = false;
                        }
                    }
                    lastToggleTime = DateTime.Now;
                }
            }

            //if (vehicleIV.GetSpeed() > 10/* && hydraulics*/)
            //{
            //NativeGame.RadarZoom = 101;
            //Vector3 desiredPos = vehicleIV.Wheels[0].Position += new Vector3(0, 0, 0.4f);

            //if (vehicleIV.Wheels[0].Position != desiredPos)
            //    vehicleIV.Wheels[0].Position += new Vector3(0, 0, 0.01f);

            //if (vehicleIV.Wheels[0].Position == desiredPos)
            //    hydraulics = false;
            //}
        }
    }
}

using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;

// Credits: ClonkAndre

namespace LibertyTweaks
{
    internal class BrakeLights
    {

        public static bool enable;
        private static bool handbrake = false;
        private static DateTime lastSoundTime = DateTime.MinValue;
        private static readonly TimeSpan soundCooldown = TimeSpan.FromSeconds(8);
        public static string section { get; private set; }
        public static void Init(SettingsFile settings,string section)
        {
            BrakeLights.section = section;
            enable = settings.GetBoolean(section, "Brake Lights", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            // Gets the current vehicle of the player
            IVVehicle playerVehicle = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

            if (!IS_CHAR_DEAD(Main.PlayerPed.GetHandle()))
            {
                if (playerVehicle != null)
                {
                    // Gets the speed of the current vehicle of the player
                    GET_CAR_SPEED(playerVehicle.GetHandle(), out float carSpeed);

                    // If speed of the vehicle is below a certain point
                    if (carSpeed < 0.09500f)
                    {
                        // Enable handbrake system, disabling brake lights if desired
                        if (NativeControls.IsGameKeyPressed(0, GameKey.Jump))
                        {
                            if (DateTime.Now - lastSoundTime > soundCooldown)
                            {
                                if (handbrake == true)
                                    return;

                                PLAY_SOUND_FRONTEND(-1, "VEHICLES_EXTRAS_STANDARD_HANDBRAKE");
                                handbrake = true;
                            }

                            return;
                        }


                        // Disable the brake lights if the player presses the gas pedal. Also disables handbrake
                        if (NativeControls.IsGameKeyPressed(0, GameKey.MoveForward) || (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward))
                            || (NativeControls.IsUsingController() && NativeControls.IsGameKeyPressed(0, GameKey.Attack)))
                        {
                            playerVehicle.BrakePedal = 0f;
                            handbrake = false;
                        }
                        else if (handbrake == false) // Activate brake lights
                            playerVehicle.BrakePedal = 0.15f;

                    }
                }
            }
        }
    }
}

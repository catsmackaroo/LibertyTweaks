using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre

namespace LibertyTweaks
{
    internal class BrakeLights
    {

        public static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Brake Lights Fix", true);

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
                        // Disable the brake lights if the player presses the gas pedal
                        if (NativeControls.IsGameKeyPressed(0, GameKey.MoveForward) || (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward)) || (NativeControls.IsUsingController() && NativeControls.IsGameKeyPressed(0, GameKey.Attack)))
                            playerVehicle.BrakePedal = 0f;
                        else // Activate brake lights
                            playerVehicle.BrakePedal = 0.15f;
                    }
                }
            }
        }
    }
}

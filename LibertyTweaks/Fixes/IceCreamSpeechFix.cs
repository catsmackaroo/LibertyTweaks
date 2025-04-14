using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre

namespace LibertyTweaks
{
    internal class IceCreamSpeechFix
    {

        private static bool saidStealingLine;
        private static bool enable;

        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            IceCreamSpeechFix.section = section;
            enable = settings.GetBoolean(section, "Ice Cream Theft Speech", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            // Checks if the player is trying to enter a locked car
            if (IS_CHAR_TRYING_TO_ENTER_A_LOCKED_CAR(Main.PlayerPed.GetHandle()))
            {
                // Gets the car the player would enter
                GET_VEHICLE_PLAYER_WOULD_ENTER(0, out int veh);

                // If there is a car
                if (veh != 0)
                {
                    // Get the car model and check if it's MrTasty
                    GET_CAR_MODEL(veh, out uint model);
                    if (model == (uint)eVehicle.VEHICLE_MRTASTY)
                    {
                        // Say speech if we can but only once
                        if (!saidStealingLine && !PLAYER_IS_PISSED_OFF(0))
                        {
                            Main.PlayerPed.SayAmbientSpeech("JACKING_ICE_CREAM");
                            saidStealingLine = true;
                        }
                    }
                }
            }
            else
            {
                // Reset so we can say it another time when we try to enter a locked MrTasty again
                saidStealingLine = false;
            }
        }

    }
}

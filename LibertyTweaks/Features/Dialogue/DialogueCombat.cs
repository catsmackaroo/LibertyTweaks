using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Native;

namespace LibertyTweaks
{
    internal class DialogueCombat
    {

        private static bool enable; 
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DialogueCombat.section = section;
            enable = settings.GetBoolean(section, "More Dialogue - Combat", false);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
                return;

            bool pCombat;

            pCombat = Natives.IS_CHAR_SHOOTING(Main.PlayerPed.GetHandle());
            if (pCombat == true)
            {
                switch (Main.GenerateRandomNumber(0, 350))
                {
                    case 0:
                        Main.PlayerPed.SayAmbientSpeech("IN_COVER_DODGE_BULLETS");
                        break;

                    case 1:
                        Main.PlayerPed.SayAmbientSpeech("SHOOT");
                        break;

                    case 2:
                        Main.PlayerPed.SayAmbientSpeech("KILLED_ALL");
                        break;

                    case 3:
                        Main.PlayerPed.SayAmbientSpeech("CHASED");
                        break;

                    case 4:
                        Main.PlayerPed.SayAmbientSpeech("GENERIC_INSULT");
                        break;

                    case 5:
                        Main.PlayerPed.SayAmbientSpeech("FIGHT");
                        break;

                    case 6:
                        Main.PlayerPed.SayAmbientSpeech("STAY_DOWN");
                        break;

                    case 7:
                        Main.PlayerPed.SayAmbientSpeech("PULL_GUN");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
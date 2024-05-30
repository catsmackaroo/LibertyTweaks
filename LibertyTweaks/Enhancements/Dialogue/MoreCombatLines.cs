using IVSDKDotNet;
using CCL.GTAIV;
using IVSDKDotNet.Native;

namespace LibertyTweaks
{
    internal class MoreCombatLines
    {

        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("More Dialogue", "Combat", true);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
                return;

            bool pCombat;
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            pCombat = Natives.IS_CHAR_SHOOTING(playerPed.GetHandle());
            if (pCombat == true)
            {
                switch (Main.GenerateRandomNumber(0, 350))
                {
                    case 0:
                        playerPed.SayAmbientSpeech("IN_COVER_DODGE_BULLETS");
                        break;

                    case 1:
                        playerPed.SayAmbientSpeech("SHOOT");
                        break;

                    case 2:
                        playerPed.SayAmbientSpeech("KILLED_ALL");
                        break;

                    case 3:
                        playerPed.SayAmbientSpeech("CHASED");
                        break;

                    case 4:
                        playerPed.SayAmbientSpeech("GENERIC_INSULT");
                        break;

                    case 5:
                        playerPed.SayAmbientSpeech("FIGHT");
                        break;

                    case 6:
                        playerPed.SayAmbientSpeech("STAY_DOWN");
                        break;

                    case 7:
                        playerPed.SayAmbientSpeech("PULL_GUN");
                        break;

                    default:
                        break;
                }
            }
            else
            {

            }
        }
    }
}
using CCL.GTAIV;
using IVSDKDotNet;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class DialogueLooting
    {
        private static bool didSpeak;
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DialogueLooting.section = section;
            enable = settings.GetBoolean(section, "More Dialogue - Looting", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            Vector3 playerGroundPos = NativeWorld.GetGroundPosition(Main.PlayerPed.Matrix.Pos);


            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;

                if (IS_CHAR_DEAD(pedHandle))
                {
                    GET_CHAR_COORDINATES(pedHandle, out Vector3 pedCoords);

                    if (Vector3.Distance(Main.PlayerPed.Matrix.Pos, pedCoords) < 2f)
                    {
                        if (NativePickup.IsAnyPickupAtPos(playerGroundPos))
                        {
                            if (!didSpeak)
                            {
                                Main.PlayerPed.SayAmbientSpeech("SEARCH_BODY_TAKE_ITEM");
                                didSpeak = true;
                            }
                        }
                        else
                            didSpeak = false;
                    }
                }
            }
        }
    }
}
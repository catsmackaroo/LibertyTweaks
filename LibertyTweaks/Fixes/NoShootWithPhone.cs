using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: AssaultKifle47, ItsClonkAndre

namespace LibertyTweaks
{
    internal class NoShootWithPhone
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Disable Driveby During Phone", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            int playerPedHandle = Main.PlayerPed.GetHandle();

            if (!IS_CHAR_IN_ANY_CAR(playerPedHandle))
                return;

            uint state = IVPhoneInfo.ThePhoneInfo.State;

            if (state > 1000)
                Main.PlayerPed.PlayerInfo.CanDoDriveby = 0;
            else if (state == 1000)
                Main.PlayerPed.PlayerInfo.CanDoDriveby = 1;
        }
    }
}

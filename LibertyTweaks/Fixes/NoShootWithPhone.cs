using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class NoShootWithPhone
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            NoShootWithPhone.section = section;
            enable = settings.GetBoolean(section, "Disable Driveby During Phone", false);

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

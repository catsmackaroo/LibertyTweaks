using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class AllowCopsAllMissions
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Allow Cops All Missions", true);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (GET_CREATE_RANDOM_COPS() == false)
                SET_CREATE_RANDOM_COPS(true);
        }
    }
}

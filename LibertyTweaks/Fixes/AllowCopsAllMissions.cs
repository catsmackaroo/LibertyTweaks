using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class AllowCopsAllMissions
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            AllowCopsAllMissions.section = section;
            enable = settings.GetBoolean(section, "Allow Cops All Missions", false);

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

using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class ExtraHospitalSpawn
    {
        private static bool enable;
        private static bool added = false;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            ExtraHospitalSpawn.section = section;
            enable = settings.GetBoolean(section, "Unused Hospital Respawn", false);
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (IS_CHAR_DEAD(Main.PlayerPed.GetHandle()))
            {
                if (!added)
                {
                    Natives.ADD_HOSPITAL_RESTART(-146, -511, 14, 324, 3);
                    added = true;
                }
            }
        }
    }
}
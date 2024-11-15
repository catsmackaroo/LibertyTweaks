using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using IVSDKDotNet.Native;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class ExtraHospitalSpawn
    {
        private static bool enable;
        private static bool added = false;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Unused Hospital Respawn", true);
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
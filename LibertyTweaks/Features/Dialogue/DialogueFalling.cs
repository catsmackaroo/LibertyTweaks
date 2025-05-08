using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;


namespace LibertyTweaks
{
    internal class DialogueFalling
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            DialogueFalling.section = section;
            enable = settings.GetBoolean(section, "More Dialogue - Fall Screaming", false);

            if (enable)
                Main.Log("script initialized...");
        }
        public static void Tick()
        {
            if (!enable)
                return;

            float heightAboveGround;
            heightAboveGround = IVPedExtensions.GetHeightAboveGround(Main.PlayerPed);

            if (heightAboveGround > 6)
            {
                if (IS_PED_RAGDOLL(Main.PlayerPed.GetHandle()))
                {
                    HIGH_FALL_SCREAM(Main.PlayerPed.GetHandle());
                }
            }
        }
    }
}
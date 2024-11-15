using IVSDKDotNet;

using CCL.GTAIV;
using static IVSDKDotNet.Native.Natives;


namespace LibertyTweaks
{
    internal class VLikeScreaming
    {
        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("More Dialogue", "Fall Screaming", true);

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
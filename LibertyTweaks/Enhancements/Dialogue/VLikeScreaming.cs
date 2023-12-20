using IVSDKDotNet;

using CCL.GTAIV;
using static IVSDKDotNet.Native.Natives;


namespace LibertyTweaks
{
    internal class VLikeScreaming
    {
        private static bool enableFix;
        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("More Dialogue", "Fall Screaming", true);
        }
        public static void Tick()
        {
            if (!enableFix)
                return;
            float heightAboveGround;
            int playerId;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            playerId = IVPedExtensions.GetHandle(playerPed);
            heightAboveGround = IVPedExtensions.GetHeightAboveGround(playerPed);

            if (heightAboveGround > 6)
            {
                if (IS_PED_RAGDOLL(playerId))
                {
                    HIGH_FALL_SCREAM(playerId);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using IVSDKDotNet;

using CCL.GTAIV;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;


namespace FallScreaming.VLikeScreaming
{
    internal class VLikeScreaming
    {
        private static bool enableFix;
        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "V-Like Screaming", true);
        }
        public static void Tick()
        {
            float heightAboveGround;
            int playerId;

            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed());

            playerId = CPedExtensions.GetHandle(playerPed);
            heightAboveGround = CPedExtensions.GetHeightAboveGround(playerPed);

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
using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks.QuickSaveFunc
{
    internal class QuickSave
    {
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "Quick Saving", true);
        }

        public static void Process()
        {
            if (!enableFix) 
                return;

            int playerId;
            float heightAboveGround;

            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed());

            playerId = CPedExtensions.GetHandle(playerPed);
            heightAboveGround = CPedExtensions.GetHeightAboveGround(playerPed);

            if (heightAboveGround < 2)
            {
                if (IS_PED_RAGDOLL(playerId))
                {
                    return;
                }
                else
                {
                    NativeGame.ShowSaveMenu();
                }
            }
        }
    }
}

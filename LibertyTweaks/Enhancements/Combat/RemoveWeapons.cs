using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre 

namespace LibertyTweaks
{
    internal class RemoveWeapons
    {
        private static bool enableFix;
        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Remove Weapons On Death", "Enable", true);
        }

        public static void Tick()
        {
            if (!enableFix)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            if (IS_CHAR_DEAD(playerPed.GetHandle()))
                REMOVE_ALL_CHAR_WEAPONS(playerPed.GetHandle()); 
        }
    }
}

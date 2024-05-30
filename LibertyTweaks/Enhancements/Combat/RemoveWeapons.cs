using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre 

namespace LibertyTweaks
{
    internal class RemoveWeapons
    {
        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Remove Weapons On Death", "Enable", true);


            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            if (IS_CHAR_DEAD(playerPed.GetHandle()))
                REMOVE_ALL_CHAR_WEAPONS(playerPed.GetHandle()); 
        }
    }
}

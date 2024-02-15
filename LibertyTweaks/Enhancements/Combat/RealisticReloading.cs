using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Numerics;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class RealisticReloading
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Realistic Reloading", "Enable", true);
        }

        public static void Tick()
        {
            if (!enable)
                return;

            int playerId;
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            playerId = IVPedExtensions.GetHandle(playerPed);

            if (NativeControls.IsGameKeyPressed(0, GameKey.Reload))
            {
                // Get current weapon
                GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out uint currentWeap);
                SET_AMMO_IN_CLIP(playerPed.GetHandle(), (int)currentWeap, 0);
            }
        }
    }
}

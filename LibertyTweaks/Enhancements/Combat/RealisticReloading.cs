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
        private static bool enable2;
        private static uint currentWeapon;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Weapon Recoil", "Enable", true);
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

            }

        }
    }
}

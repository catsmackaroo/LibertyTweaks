using CCL.GTAIV;

using IVSDKDotNet;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class MoveWithSniper
    {
        private static int playerHandle;
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Move With Sniper", "Enable", true);

            if (enableFix)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enableFix)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            if (playerPed == null) return;
            playerHandle = IVPedExtensions.GetHandle(playerPed);

            if (IS_PLAYER_DEAD((int)GET_PLAYER_ID())) return;

            GET_CURRENT_CHAR_WEAPON(playerHandle, out int currentWeapon);

            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1 
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE 
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {
                if (NativeControls.IsGameKeyPressed(0, GameKey.Aim))
                {
                    IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot = 0;
                }
                else
                {
                    IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot = 6;
                }
            }
        }
    }
}
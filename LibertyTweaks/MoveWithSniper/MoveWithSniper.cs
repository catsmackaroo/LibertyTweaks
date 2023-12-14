using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class MoveWithSniper
    {
        private static int playerId;
        private static uint currentWeapon;
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "Move With Sniper", true);
        }

        public static void Tick()
        {
            if (!enableFix)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            playerId = IVPedExtensions.GetHandle(playerPed);
            GET_CURRENT_CHAR_WEAPON(playerId, out currentWeapon);

            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1 
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE 
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {
                if (NativeControls.IsGameKeyPressed(0, GameKey.Aim))
                {
                    IVWeaponInfo.GetWeaponInfo(currentWeapon).WeaponSlot = 16;
                }
                else
                {
                    IVWeaponInfo.GetWeaponInfo(currentWeapon).WeaponSlot = 6;
                }
            }
        }
    }
}
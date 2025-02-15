using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Native;
using System;
using System.Security.Policy;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SniperMovement
    {
        private static bool enable;
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Move With Sniper", "Enable", true);

            Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            int currentWeapon = WeaponHelpers.GetWeaponType();
            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {

                if (IS_USING_CONTROLLER() && (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward) || NativeControls.IsGameKeyPressed(0, GameKey.MoveForward)))
                {
                    Reset();
                    return;
                }
                if (PlayerHelper.IsAiming())
                {
                    if (WeaponHelpers.GetWeaponInfo().WeaponSlot != 9)
                        WeaponHelpers.GetWeaponInfo().WeaponSlot = 9;
                    return;
                }
                if (WeaponHelpers.IsReloading())
                {
                    if (WeaponHelpers.GetWeaponInfo().WeaponSlot != 9)
                        WeaponHelpers.GetWeaponInfo().WeaponSlot = 9;
                    return;
                }
                else
                {
                    Reset();
                    return;
                }
            }
            else
            {
                Reset();
                return;
            }
        }

        private static void Reset()
        {
            if (IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1).WeaponSlot != 6
                             || IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE).WeaponSlot != 6
                             || IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15).WeaponSlot != 6)
            {
                IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1).WeaponSlot = 6;
                IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE).WeaponSlot = 6;
                IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15).WeaponSlot = 6;
            }
        }
    }
}
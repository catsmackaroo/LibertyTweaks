using CCL.GTAIV;
using IVSDKDotNet;
using System;
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

            // If player has sniper equipped
            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {
                WeaponHelpers.GetWeaponInfo().WeaponSlot = 0;

                //if (WeaponHelpers.IsReloading() 
                //    || NativeControls.IsGameKeyPressed(0, GameKey.Reload) 
                //    || !NativeControls.IsGameKeyPressed(0, GameKey.Aim) 
                //    || !NativeControls.IsGameKeyPressed(0, GameKey.Attack))
                //    Reset();

                //if (NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft)
                //    || NativeControls.IsGameKeyPressed(0, GameKey.MoveForward)
                //    || NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward)
                //    || NativeControls.IsGameKeyPressed(0, GameKey.MoveRight))
                //else
                //    Reset();
            }
            else
                Reset();
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
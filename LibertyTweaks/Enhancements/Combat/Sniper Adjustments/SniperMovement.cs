using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SniperMovement
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            SniperMovement.section = section;
            enable = settings.GetBoolean(section, "Move With Sniper", false);

            Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            int currentWeapon = WeaponHelpers.GetCurrentWeaponType();
            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {

                if (IS_USING_CONTROLLER() && (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward) || NativeControls.IsGameKeyPressed(0, GameKey.MoveForward)))
                {
                    Reset();
                    return;
                }
                if (WeaponHelpers.IsPlayerAiming())
                {
                    if (WeaponHelpers.GetCurrentWeaponInfo().WeaponSlot != 9)
                        WeaponHelpers.GetCurrentWeaponInfo().WeaponSlot = 9;
                    return;
                }
                if (WeaponHelpers.IsReloading())
                {
                    if (WeaponHelpers.GetCurrentWeaponInfo().WeaponSlot != 9)
                        WeaponHelpers.GetCurrentWeaponInfo().WeaponSlot = 9;
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
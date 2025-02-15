using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SniperScopeToggle
    {
        private static bool enable;
        private static bool isFpEnabled;
        private static DateTime lastToggleTime;
        private static TimeSpan toggleCooldown = TimeSpan.FromMilliseconds(500);
        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Toggle Sniper Scope", "Enable", true);

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
                if (NativeControls.IsGameKeyPressed(0, GameKey.Aim) || NativeControls.IsGameKeyPressed(0, GameKey.Attack))
                {
                    if (NativeControls.IsGameKeyPressed(0, GameKey.LookBehind))
                    {
                        if (DateTime.Now - lastToggleTime > toggleCooldown)
                        {
                            isFpEnabled = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson;

                            if (isFpEnabled)
                                IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson = false;
                            else
                                IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson = true;

                            SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), false);
                            SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), true);
                            IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).Accuracy = 0;
                            IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).Damage = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).DamageFPS;

                            lastToggleTime = DateTime.Now;
                        }
                    }
                }
            }
        }

    }
}
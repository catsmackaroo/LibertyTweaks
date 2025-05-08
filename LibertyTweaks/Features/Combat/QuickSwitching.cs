using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using static IVSDKDotNet.Native.Natives;

// UNARMED - 1
// MELEE - 2
// HANDGUN - 3
// SHOTGUN - 4
// SMG - 5
// AR - 6
// SNIPER - 7
// HEAVY WEAPON - 8
// GRENADE - 9
// SPECIAL - 0

namespace LibertyTweaks
{
    internal class QuickSwitching
    {
        private static bool enable;
        private static bool enableQuickSwitch;

        private static DateTime lastProcessTime = DateTime.MinValue;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(500);
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            QuickSwitching.section = section;
            enable = settings.GetBoolean(section, "Switch Weapons While Aiming", false);
            enableQuickSwitch = settings.GetBoolean(section, "Switch Weapons While Aiming - Quick Switch", false);

            if (enable)
            {
                Main.Log("script initialized...");

                if (enableQuickSwitch)
                {
                    Main.Log("Quick Switching enabled...");
                }
            }
        }

        public static void Tick()
        {
            var time = Main.PlayerPed.GetAnimationController().GetCurrentAnimationTime("gun@rocket", "fire");
            if (!enable || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()) 
                || !WeaponHelpers.IsPlayerAiming() || time >= 0.8) return;

            if (NativeControls.MouseWheel != 0)
                return;

            if (DateTime.Now - lastProcessTime >= delay)
            {
                if (NativeControls.IsGameKeyPressed(0, GameKey.NextWeapon))
                {
                    SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), false);
                    SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), true);
                    int nextWeapon = WeaponHelpers.GetNextWeaponAsInt();

                    if (enableQuickSwitch)
                    {
                        SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), nextWeapon, true);
                    }
                    else
                    {
                        GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), nextWeapon, 1, true);
                        ADD_AMMO_TO_CHAR(Main.PlayerPed.GetHandle(), nextWeapon, -1);
                    }
                    lastProcessTime = DateTime.Now;
                }
                
                if (NativeControls.IsGameKeyPressed(0, GameKey.LastWeapon))
                {
                    SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), false);
                    SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), true);
                    int lastWeapon = WeaponHelpers.GetPreviousWeaponAsInt();

                    if (enableQuickSwitch)
                    {
                        SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), lastWeapon, true);
                    }
                    else
                    {
                        GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), lastWeapon, 1, true);
                        ADD_AMMO_TO_CHAR(Main.PlayerPed.GetHandle(), lastWeapon, -1);
                    }
                    lastProcessTime = DateTime.Now;
                }
            }
        }
    }
}

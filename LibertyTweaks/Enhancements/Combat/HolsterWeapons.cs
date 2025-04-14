using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class HolsterWeapons
    {
        private static int lastWeapon;
        private static bool enable;
        public static Keys key;

        // Controller Support
        private static uint padIndex = 0;
        private static ControllerButton controllerKey1;
        private static ControllerButton controllerKey2;
        private static DateTime lastProcessTime = DateTime.MinValue;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(500);
        public static string section { get; private set; }

        public static void Init(SettingsFile settings, string section)
        {
            HolsterWeapons.section = section;
            enable = settings.GetBoolean(section, "Weapon Holstering", false);
            key = settings.GetKey(section, "Weapon Holstering - Key", Keys.H);
            controllerKey1 = (ControllerButton)settings.GetInteger(section, "Weapon Holstering - Controller Key", (int)ControllerButton.BUTTON_DPAD_DOWN);
            controllerKey2 = (ControllerButton)settings.GetInteger(section, "Weapon Holstering - Controller Key 2", (int)ControllerButton.BUTTON_A);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (IS_USING_CONTROLLER())
            {
                bool bothKeysPressed = NativeControls.IsControllerButtonPressed(padIndex, controllerKey1)
                                    && NativeControls.IsControllerButtonPressed(padIndex, controllerKey2);

                if (bothKeysPressed && DateTime.Now - lastProcessTime >= delay)
                {
                    Process();
                    lastProcessTime = DateTime.Now;
                }
            }
        }
        public static void Process()
        {
            if (!enable || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            if (IS_PLAYER_PLAYING(Main.PlayerIndex))
            {
                if (WeaponHelpers.GetCurrentWeaponType() != 0)
                {
                    lastWeapon = WeaponHelpers.GetCurrentWeaponType(); 
                    GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), 0, 1, true);
                }
                else if (WeaponHelpers.GetWeaponInventory(true).Contains((IVSDKDotNet.Enums.eWeaponType)lastWeapon))
                {
                    GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), lastWeapon, 1, true);
                    ADD_AMMO_TO_CHAR(Main.PlayerPed.GetHandle(), lastWeapon, -1);
                }
            }
        }

    }
}

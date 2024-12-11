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

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Weapon Holstering", "Enable", true);
            key = settings.GetKey("Weapon Holstering", "Key", Keys.H);
            controllerKey1 = (ControllerButton)settings.GetInteger("Weapon Holstering", "Controller Key 1", (int)ControllerButton.BUTTON_DPAD_DOWN);
            controllerKey2 = (ControllerButton)settings.GetInteger("Weapon Holstering", "Controller Key 2", (int)ControllerButton.BUTTON_A);


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
                // If the player has a valid weapon and hasn't holstered it yet
                if (WeaponHelpers.GetWeaponType() != 0)
                {
                    lastWeapon = WeaponHelpers.GetWeaponType(); // Initialize lastWeapon
                    GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), 0, 1, true);
                }
                // Only go back to last weapon if player has last weapon still
                else if (WeaponHelpers.GetWeaponInventory().Contains((IVSDKDotNet.Enums.eWeaponType)lastWeapon))
                {
                    // The player is restoring the last weapon
                    GIVE_DELAYED_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), lastWeapon, 1, true);
                    ADD_AMMO_TO_CHAR(Main.PlayerPed.GetHandle(), lastWeapon, -1);
                }
            }
        }

    }
}

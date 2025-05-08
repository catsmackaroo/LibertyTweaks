using CCL.GTAIV;
using IVSDKDotNet;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class ImprovedRolling
    {
        public static bool enable;
        private static bool tried2crouch = false;

        private static float desiredTimeToCancel;
        private static float desiredCombatRollCancel = 0.65f;
        private static readonly float defaultCancelTime = 0.94f;
        private static readonly float crouchedCancelTime = 0.86f;
        private static readonly float rollTimeThreshold = 0.8f;

        private static readonly float defaultRollSpeed = 1.45f;
        private static readonly float defaultCombatRollSpeed = 1.85f;

        private static bool isRollingLeft;
        private static bool isRollingRight;
        private static bool isCombatRollingForward;
        private static bool isCombatRollingBackward;

        private static bool isRolling;

        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            ImprovedRolling.section = section;
            enable = settings.GetBoolean(section, "Improved Rolling", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE() || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            isRollingLeft = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_left") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_l");
            isRollingRight = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_right") || IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_r");
            isCombatRollingForward = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_fwd");
            isCombatRollingBackward = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_bwd");
            isRolling = isRollingLeft || isRollingRight || isCombatRollingForward || isCombatRollingBackward;

            HandleRolls();
            HandleForceInRolls();

            IVGame.ShowSubtitleMessage(isRolling.ToString());

            // Sniper rolling
            int currentWeapon = WeaponHelpers.GetCurrentWeaponType();
            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {
                if (WeaponHelpers.IsPlayerAiming())
                {
                    if (NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft) && NativeControls.IsGameKeyPressed(0, GameKey.Jump))
                    {
                        CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                        _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 0);
                    }

                    if (NativeControls.IsGameKeyPressed(0, GameKey.MoveRight) && NativeControls.IsGameKeyPressed(0, GameKey.Jump))
                    {

                        CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                        _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 1);
                    }
                }
            }
        }
        private static void HandleReloadInRolls()
        {
            // Reload in roll
            GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out int totalAmmo);
            GET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out int magAmmo);
            GET_MAX_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), out int maxAmmo);
            if ((isRollingLeft || isRollingRight || isCombatRollingForward) && magAmmo != maxAmmo)
            {
                if (WeaponHelpers.CanReload())
                {
                    int ammoToReload = maxAmmo - magAmmo;
                    totalAmmo -= ammoToReload;
                    SET_AMMO_IN_CLIP(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), maxAmmo);
                    SET_CHAR_AMMO(Main.PlayerPed.GetHandle(), WeaponHelpers.GetCurrentWeaponType(), totalAmmo);
                }
            }
        }

        private static void HandleCancelRolls()
        {
            if (tried2crouch)
                desiredTimeToCancel = crouchedCancelTime;
            else
                desiredTimeToCancel = defaultCancelTime;

            if (isRollingLeft)
            {
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_left", defaultRollSpeed);
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_left", out float time);

                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_l", defaultCombatRollSpeed);
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_l", out float time2);

                if (time > desiredTimeToCancel)
                    SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_left", 1);

                if (time2 > desiredCombatRollCancel)
                    SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_l", 1);

                if (NativeControls.IsGameKeyPressed(0, GameKey.Crouch))
                    tried2crouch = true;
            }
            else if (isRollingRight)
            {
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_right", defaultRollSpeed);
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_right", out float time);

                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_r", defaultCombatRollSpeed);
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_r", out float time2);

                if (time > desiredTimeToCancel)
                    SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "ev_dives", "plyr_roll_right", 1);

                if (time2 > desiredCombatRollCancel)
                    SET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_l", 1);

                if (NativeControls.IsGameKeyPressed(0, GameKey.Crouch))
                    tried2crouch = true;

            }
            else if (isCombatRollingBackward || isCombatRollingForward)
            {
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_fwd", defaultCombatRollSpeed);
                SET_CHAR_ANIM_SPEED(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_bwd", defaultCombatRollSpeed);
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerPed.GetHandle(), "move_crouch_rifle", "crouch_roll_fwd", out float time);

                if (time == 0.65)
                    CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());

                if (NativeControls.IsGameKeyPressed(0, GameKey.Crouch))
                {
                    if (time > 0.6)
                    {
                        CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                    }

                    tried2crouch = true;
                }
            }
            else if (tried2crouch)
            {
                SET_CHAR_DUCKING_TIMED(Main.PlayerPed.GetHandle(), 9999);
                tried2crouch = false;
            }
        }
        private static void HandleForceInRolls()
        {
            if (isRolling)
            {
                if (NativeControls.IsGameKeyPressed(0, GameKey.MoveForward))
                    Main.PlayerPed.ApplyForceRelative(new Vector3(0, 0.35f, 0));
                else if (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward))
                    Main.PlayerPed.ApplyForceRelative(new Vector3(0, -0.35f, 0));
                else if (NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
                    Main.PlayerPed.ApplyForceRelative(new Vector3(-0.35f, 0, 0));
                else if (NativeControls.IsGameKeyPressed(0, GameKey.MoveRight))
                    Main.PlayerPed.ApplyForceRelative(new Vector3(0.35f, 0, 0));
            }
        }
        private static void HandleRolls()
        {
            if (isRolling) return;

            // Straight Roll
            if (NativeControls.IsGameKeyPressed(0, GameKey.MoveForward)
                && NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 2);
            }

            // Backward Roll
            if (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward)
                && NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 3);
            }

            // Diagonal Roll
            if (NativeControls.IsGameKeyPressed(0, GameKey.MoveForward)
                && NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                Main.PlayerPed.Matrix.RotateLocalY(3);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.MoveForward)
                && NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                Main.PlayerPed.Matrix.RotateLocalY(-3);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward)
                && NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                float heading = Main.PlayerPed.GetHeading();
                float adjustedHeading = heading - 4;
                Main.PlayerPed.SetHeading(adjustedHeading);
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 1);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.MoveBackward)
                && NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                float heading = Main.PlayerPed.GetHeading();
                float adjustedHeading = heading + 4;
                Main.PlayerPed.SetHeading(adjustedHeading);
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 2);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                float heading = Main.PlayerPed.GetHeading();
                float adjustedHeading = heading + 4;
                Main.PlayerPed.SetHeading(adjustedHeading);
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 0);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.Jump)
                && WeaponHelpers.IsPlayerAiming()
                && NativeControls.IsGameKeyPressed(0, GameKey.MoveRight)
                && !NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft))
            {
                float heading = Main.PlayerPed.GetHeading();
                float adjustedHeading = heading + 4;
                Main.PlayerPed.SetHeading(adjustedHeading);
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
                _TASK_COMBAT_ROLL(Main.PlayerPed.GetHandle(), 1);
            }
        }
    }
}

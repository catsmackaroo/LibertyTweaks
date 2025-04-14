using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SprintInInteriors
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            SprintInInteriors.section = section;
            enable = settings.GetBoolean(section, "Sprint in Interiors", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (IS_PED_RAGDOLL(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || IVPhoneInfo.ThePhoneInfo.State > 1000
                || WeaponHelpers.IsPlayerAiming())
                return;

            if (IS_INTERIOR_SCENE() && Main.PlayerPed.PlayerInfo.Stamina > 0)
            {
                if (DynamicMovement.enableSprintFix)
                {
                    if (DynamicMovement.IsSprintEnabled == false)
                        return;

                    if (!DynamicMovement.IsCapsLockActive())
                        return;
                }
                int playerPedHandle = Main.PlayerPed.GetHandle();
                uint state = IVPhoneInfo.ThePhoneInfo.State;
                float moveState = Main.PlayerPed.PedMoveBlendOnFoot.MoveState;

                if (NativeControls.IsGameKeyPressed(0, GameKey.Sprint) && NativeControls.IsGameKeyPressed(0, GameKey.MoveForward))
                {
                    if (moveState < 3.0f && moveState != 0)
                        moveState += 0.05f;

                    moveState = CommonHelpers.Clamp(moveState, 0.0f, 3.0f);
                    Main.PlayerPed.PedMoveBlendOnFoot.MoveState = moveState;
                }
            }
        }
    }
}

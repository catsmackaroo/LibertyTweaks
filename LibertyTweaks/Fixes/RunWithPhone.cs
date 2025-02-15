using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: AssaultKifle47, ItsClonkAndre

namespace LibertyTweaks
{
    internal class RunWithPhone
    {
        private static bool enable;
        private static bool enableRun;
        private static float moveStateMax = 2f;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "Jog With Phone", true);
            enableRun = settings.GetBoolean("Fixes", "Run With Phone", false);

            if (enable)
                Main.Log("script initialized...");

            if (enableRun)
                moveStateMax = 3.0f;
            else if (enable)
                moveStateMax = 2.0f;
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (IS_PED_RAGDOLL(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || WeaponHelpers.GetWeaponType() != 46)
                return;

            int playerPedHandle = Main.PlayerPed.GetHandle();
            uint state = IVPhoneInfo.ThePhoneInfo.State;

            if (state > 1000)
            {
                float moveState = Main.PlayerPed.PedMoveBlendOnFoot.MoveState;

                if (NativeControls.IsGameKeyPressed(0, GameKey.Sprint))
                {
                    if (moveState < moveStateMax && moveState != 0)
                        moveState += 0.05f;

                    moveState = CommonHelpers.Clamp(moveState, 0.0f, moveStateMax);
                    Main.PlayerPed.PedMoveBlendOnFoot.MoveState = moveState;
                }
            }
        }
    }
}

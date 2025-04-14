using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class RunWithPhone
    {
        private static bool enable;
        private static bool enableRun;
        private static float moveStateMax = 2f;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            RunWithPhone.section = section;
            enable = settings.GetBoolean(section, "Jog With Phone", false);
            enableRun = settings.GetBoolean(section, "Run With Phone", false);

            if (enable)
                Main.Log("script initialized...");

            if (enable && enableRun)
                Main.Log("Both 'Jog With Phone' & 'Run With Phone' enabled. It's recommended to use one or the other to avoid possible issues. Default: Jog With Phone");

            if (enableRun)
                moveStateMax = 3.0f;
            else if (enable)
                moveStateMax = 2.0f;
        }

        public static void Tick()
        {
            if (!enable && !enableRun)
                return;

            if (IS_PED_RAGDOLL(Main.PlayerPed.GetHandle())
                || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle())
                || WeaponHelpers.GetCurrentWeaponType() != 46)
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

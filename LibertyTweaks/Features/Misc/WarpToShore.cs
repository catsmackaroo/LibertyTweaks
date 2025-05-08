using CCL.GTAIV;
using IVSDKDotNet;
using System.Numerics;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class WarpToShore
    {
        private static bool enable;

        private static bool canTeleport = false;
        private static bool hasPlayerBeenTold = false;

        // Controller Support
        //private static uint padIndex = 0;
        //private static ControllerButton controllerKey1;
        //private static ControllerButton controllerKey2;
        //private static DateTime lastProcessTime = DateTime.MinValue;
        //private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(500);
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            WarpToShore.section = section;
            enable = settings.GetBoolean(section, "Warp To Shore", false);

            //controllerKey1 = (ControllerButton)settings.GetInteger("Warp To Shore", "Controller Key 1", (int)ControllerButton.BUTTON_DPAD_DOWN);
            //controllerKey2 = (ControllerButton)settings.GetInteger("Warp To Shore", "Controller Key 2", (int)ControllerButton.BUTTON_Y);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            if (IS_CHAR_SWIMMING(Main.PlayerPed.GetHandle()))
            {
                // Distance Checks
                float dist = 0f;
                int closestCar = GET_CLOSEST_CAR(Main.PlayerPos, 500f, 0, 70);
                if (closestCar != 0)
                {
                    GET_CAR_COORDINATES(closestCar, out Vector3 carPos);
                    dist = Vector3.Distance(Main.PlayerPos, carPos);
                }

                //IVGame.ShowSubtitleMessage($"Distance1: {dist1} Distance2: {dist2} CanTeleport: {canTeleport.ToString()}");

                if (Main.PlayerWantedLevel == 0
                    && PlayerHelper.IsPlayerInOrNearCombat() == false
                    && (dist > 20 || dist == 0)
                    && !IVTheScripts.IsPlayerOnAMission()
                    && !IS_SCREEN_FADING())
                {
                    canTeleport = true;
                }
                else
                {
                    canTeleport = false;
                }

                if (canTeleport && hasPlayerBeenTold == false)
                {
                    IVGame.ShowSubtitleMessage($"Press ~INPUT_PICKUP~ to warp to shore.");
                    hasPlayerBeenTold = true;
                }
            }
            else
            {
                canTeleport = false;
            }

            if (canTeleport && NativeControls.IsGameKeyPressed(0, GameKey.Action))
            {
                Process();
            }
        }
        public static void Process()
        {
            if (!enable || Main.PlayerWantedLevel > 0 || PlayerHelper.IsPlayerInOrNearCombat()) return;

            if (canTeleport)
            {
                WarpScript warpScript = new WarpScript();
                warpScript.GetNearestTeleportLocation(Main.PlayerPos);
                WarpLocation nearestLocation = warpScript.GetNearestTeleportLocation(Main.PlayerPed.Matrix.Pos);

                if (nearestLocation != null)
                {
                    CommonHelpers.HandleScreenFade(4500, true, () =>
                    {
                        Main.PlayerPed.Teleport(nearestLocation.ToVector3(), false, true);
                        Main.PlayerPed.SetHeading(nearestLocation.Heading);
                        REQUEST_ANIMS("playidles_wet_1h");
                        _TASK_PLAY_ANIM(Main.PlayerPed.GetHandle(), "brush_off_stand", "playidles_wet_1h", 8, 0, 0, 0, 0, -1);
                        return;
                    });
                }
            }
        }
    }
}

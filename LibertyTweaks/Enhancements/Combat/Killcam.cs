using System;
using System.Numerics;
using System.Diagnostics;

using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;

// Credits: ItsClonkAndre

namespace LibertyTweaks.Enhancements.Combat
{
    internal class Killcam
    {
        public static bool enable;
        private static bool enableOnlyForMissions;
        private static int targetedPed;
        private static int missionChance;
        private static int freeroamChance;
        public static NativeCamera cam;
        private static bool wasSetToShowPlayer;
        private static bool wasHudOn;
        private static uint whatRadarMode;
        //private static bool canStillActivate;
        private static Stopwatch watch = new Stopwatch();
        private static Stopwatch cooldownWatch = new Stopwatch(); 
        private static bool firstFrame = true;

        private const double standardKillcamDuration = 8.0;
        private const double shortKillcamDuration = 3.0;
        private const double killcamCooldown = 20.0;
        private static bool useQuickKillcam = false;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Killcam", "Enable", true);
            enableOnlyForMissions = settings.GetBoolean("Killcam", "Only During Missions", true);
            missionChance = settings.GetInteger("Killcam", "Mission Chance", 80);
            freeroamChance = settings.GetInteger("Killcam", "Freeroam Chance", 40);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void IngameStartup()
        {
            if (!enable)
                return;
            
            firstFrame = true;
        }

        // todo: task bug when player is ragdolled during killcam || add regular kill check with higher rarity
        public static void Tick()
        {
            if (!enable)
                return;

            if (firstFrame)
            {
                if (cam == null)
                {
                    cam = NativeCamera.Create();
                    cam.Deactivate();
                }
                firstFrame = false;
            }

            //IVGame.ShowSubtitleMessage(canStillActivate.ToString());

            if (cooldownWatch.IsRunning && cooldownWatch.Elapsed.TotalSeconds >= killcamCooldown)
            {
                useQuickKillcam = false;
                cooldownWatch.Stop(); // Stop the cooldown timer since it's no longer needed
            }

            if (enableOnlyForMissions && !IVTheScripts.IsPlayerOnAMission())
                return;

            int activePedsCount = PedHelper.GetActivePedsCount(Main.PlayerPos, 35f, true, false);
            if (activePedsCount > 2)
            {
                ResetTarget();
                return;
            }

            // Target acquisition logic
            FindTarget();

            if (targetedPed != 0)
            {
                int chance = IVTheScripts.IsPlayerOnAMission() ? missionChance : freeroamChance;
                if (!ShouldActivateKillcam(chance))
                {
                    ResetTarget();
                    return;
                }

                if (DOES_CHAR_EXIST(targetedPed) && !watch.IsRunning)
                    SetupKillcam();
            }

            HandleKillcamPlayback();
        }

        private static void FindTarget()
        {
            foreach (var kvp in PedHelper.PedHandles)
            {
                int handle = kvp.Value;

                if (IS_CHAR_DEAD(handle))
                    continue;

                // Disabling car KillCam as it's a bit buggy atm
                //if (Main.PlayerPed.IsInVehicle())
                //{
                //    GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int playerCar);
                //    if (HAS_CHAR_BEEN_DAMAGED_BY_CAR(handle, playerCar))
                //    {
                //        targetedPed = handle;
                //        canStillActivate = true;
                //        break;
                //    }
                //}

                if (IS_PLAYER_FREE_AIMING_AT_CHAR((int)GET_PLAYER_ID(), handle))
                {
                    targetedPed = handle;
                    break;
                }
            }
        }

        private static bool ShouldActivateKillcam(int chance)
        {
            int rnd = Main.GenerateRandomNumber(1, 101);
            return rnd <= chance;
        }

        private static void SetupKillcam()
        {
            GET_CHAR_COORDINATES(targetedPed, out Vector3 pos);
            cam.SetTargetPed(targetedPed);

            if (IS_CHAR_IN_ANY_CAR(targetedPed) || Main.PlayerPed.IsInVehicle())
                cam.PointAtPed(targetedPed);
            else
                cam.PointAtPed(Main.PlayerPed.GetHandle());

            if (WAS_PED_KILLED_BY_HEADSHOT(targetedPed)/* || canStillActivate*/)
                ActivateKillcam(pos);
            else
                PositionCameraForDynamicView();
        }

        private static void ActivateKillcam(Vector3 pos)
        {
            ResetTarget();
            wasSetToShowPlayer = false;

            // Set time scaling
            IVTimer.TimeScale = 0.2f;
            IVTimer.TimeScale2 = 0.2f;
            IVTimer.TimeScale3 = 0.2f;

            // Simulate aim task
            //if (!Main.PlayerPed.IsInVehicle())
            _TASK_AIM_GUN_AT_COORD(Main.PlayerPed.GetHandle(), pos.X, pos.Y, pos.Z, 6000);

            // Apply visual effects
            SET_TIMECYCLE_MODIFIER("church");
            SaveHudAndRadarState();
            IVMenuManager.HudOn = false;
            IVMenuManager.RadarMode = 0;

            TRIGGER_PTFX_ON_PED_BONE("blood_stun_punch", targetedPed, 0f, 0f, 0f, 90f, 0f, 0f, (int)eBone.BONE_HEAD, 1065353216);

            // Activate the camera
            cam.Activate();
            watch.Reset();
            watch.Start();
            cooldownWatch.Reset();
        }

        private static void PositionCameraForDynamicView()
        {
            GET_OFFSET_FROM_CHAR_IN_WORLD_COORDS(targetedPed, new Vector3(GENERATE_RANDOM_FLOAT_IN_RANGE(-1f, 1f), -1.25f, 0.3f), out Vector3 offset);
            cam.Position = offset;
        }

        private static void HandleKillcamPlayback()
        {
            if (watch.IsRunning)
            {

                double duration = useQuickKillcam ? shortKillcamDuration : standardKillcamDuration;

                if (NativeControls.IsGameKeyPressed(0, GameKey.Attack)
                || NativeControls.IsGameKeyPressed(0, GameKey.Jump))
                {
                    if (/*!Main.PlayerPed.IsInVehicle() && */watch.Elapsed.TotalSeconds > 1)
                        EndKillcam();
                }

                // Transition to player camera if enough time has passed and the player hasn't been shown yet
                if (watch.Elapsed.TotalSeconds > duration - 2.0 && !wasSetToShowPlayer && !useQuickKillcam /*&& !Main.PlayerPed.IsInVehicle()*/)
                    TransitionToPlayerCam();

                // End the killcam when the duration has passed
                if (watch.Elapsed.TotalSeconds > duration)
                    EndKillcam();
            }
        }


        private static void TransitionToPlayerCam()
        {
            GET_PED_BONE_POSITION(Main.PlayerPed.GetHandle(), (uint)eBone.BONE_HEAD, Vector3.Zero, out Vector3 headPos);
            GET_OFFSET_FROM_CHAR_IN_WORLD_COORDS(Main.PlayerPed.GetHandle(), new Vector3(0f, 1.2f, 0.5f), out Vector3 offset);

            cam.Unpoint();
            cam.SetTargetPed(Main.PlayerPed.GetHandle());
            cam.Position = offset;
            cam.PointAtCoord(headPos);

            // Set flag to prevent re-triggering this transition
            wasSetToShowPlayer = true;
            // Check if cooldown has passed to determine the type of killcam
            // issue: the usequickkillcam is enabled during activekillcam and thus needs to be moved so that it won't show the player during quickkillcams only
        }


        private static void EndKillcam()
        {
            ResetTarget();

            IVTimer.TimeScale = 1f;
            IVTimer.TimeScale2 = 1f;
            IVTimer.TimeScale3 = 1f;

            CLEAR_CHAR_TASKS(Main.PlayerPed.GetHandle());
            CLEAR_TIMECYCLE_MODIFIER();

            RestoreHudAndRadarState();
            cam.Deactivate();

            // Reset flags for the next killcam
            watch.Stop();
            cooldownWatch.Start();  // Start cooldown timer after killcam ends
            wasSetToShowPlayer = false;  // Reset this flag
            useQuickKillcam = cooldownWatch.Elapsed.TotalSeconds < killcamCooldown;
        }

        private static void SaveHudAndRadarState()
        {
            wasHudOn = IVMenuManager.HudOn;
            whatRadarMode = IVMenuManager.RadarMode;
        }

        private static void RestoreHudAndRadarState()
        {
            IVMenuManager.HudOn = wasHudOn;
            IVMenuManager.RadarMode = whatRadarMode;
        }

        private static void ResetTarget()
        {
            targetedPed = 0;
            //canStillActivate = false;
        }
    }
}

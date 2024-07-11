//using IVSDKDotNet;
//using static IVSDKDotNet.Native.Natives;
//using CCL;
//using CCL.GTAIV;
//using System;
//using System.Numerics;
//using System.Windows.Forms;
//using IVSDKDotNet.Enums;

//namespace LibertyTweaks
//{
//    internal class Freecam
//    {
//        private static bool enableFreecam;
//        private static NativeCamera freeCam;
//        private static IVVehicle attachedVehicle;
//        private static Vector3 currentPos;
//        private static Vector3 moveDir;
//        private static float moveSpeed;
//        private static bool isCamActive;
//        private static bool isCamAttached;
//        private static Keys toggleKey;

//        public static void Init(SettingsFile settings)
//        {
//            enableFreecam = settings.GetBoolean("Freecam", "Enable", true);
//            toggleKey = settings.GetKey("Freecam", "Key", Keys.P);

//            if (enableFreecam)
//                Main.Log("Freecam script initialized...");
//        }

//        public static void Tick()
//        {
//            if (enableFreecam && ImGuiIV.IsKeyPressed(eImGuiKey.ImGuiKey_P))
//            {
//                isCamActive = !isCamActive;
//                if (isCamActive)
//                {
//                    ActivateFreecam();
//                }
//                else
//                {
//                    DeactivateFreecam();
//                }
//            }

//            if (isCamActive)
//            {
//                HandleFreecamMovement();
//            }
//        }

//        private static void ActivateFreecam()
//        {
//            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
//            if (playerPed == null) return;

//            currentPos = playerPed.Matrix.Pos;
//            freeCam = NativeCamera.Create(eCamType.CAM_FREE);
//            isCamActive = true;
//            freeCam.Position = currentPos;
//        }

//        private static void DeactivateFreecam()
//        {
//            if (freeCam != null && freeCam.IsActive)
//            {
//                IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
//                freeCam.AttachToPed(playerPed.GetHandle());
//                freeCam.Dispose();
//            }
//            isCamActive = false;
//        }

//        private static void HandleFreecamMovement()
//        {
//            if (freeCam == null) return;

//            moveDir = new Vector3(NativeControls.IsGameKeyPressed(0, GameKey.MoveLeft) * moveSpeed,
//                NativeControls.IsGameKeyPressed(0, GameKey.MoveUpDown) * moveSpeed,
//                NativeControls.IsGameKeyPressed(0, GameKey.MoveForwardBackward) * moveSpeed
//            );

//            currentPos += moveDir;
//            freeCam.Position = currentPos;

//            if (NativeControls.IsGameKeyPressed(0, GameKey.Crouch))
//            {
//                AttachToVehicle();
//            }

//            if (isCamAttached && attachedVehicle != null)
//            {
//                currentPos = attachedVehicle.Position;
//                freeCam.Position = currentPos;
//            }
//        }

//        private static void AttachToVehicle()
//        {
//            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
//            if (playerPed == null) return;

//            GET_CAR_CHAR_IS_USING(playerPed.GetHandle(), out int vehicleHandle);
//            if (vehicleHandle == 0) return;

//            attachedVehicle = IVVehicle.FromUIntPtr((IntPtr)vehicleHandle);
//            isCamAttached = true;
//        }
//    }
//}

using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Numerics;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: ServalEd & catsmackaroo

namespace LibertyTweaks
{
    internal class ShoulderSwap
    {
        private static bool enable;
        private static bool enableResetWhenNotAiming;
        public static Keys key;

        // Controller Support
        private static uint padIndex = 0;
        private static ControllerButton controllerKey1;
        private static DateTime lastProcessTime = DateTime.MinValue;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(500);

        private static bool IsSwapped = false;
        private static int obj1 = 0;
        private static int obj2 = 0;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            ShoulderSwap.section = section;
            enable = settings.GetBoolean(section, "Shoulder Swap", false);
            enableResetWhenNotAiming = settings.GetBoolean(section, "Shoulder Swap - Reset When Not Aiming", false);
            key = settings.GetKey(section, "Shoulder Swap - Key", Keys.B);
            controllerKey1 = (ControllerButton)settings.GetInteger(section, "Shoulder Swap - Controller Key", (int)ControllerButton.BUTTON_BUMPER_LEFT);


            if (enable)
            {
                Main.Log("script initialized...");
                Main.Log($"Key: {key} | Controller Key: {controllerKey1} | Reset When Not Aiming: {enableResetWhenNotAiming}");

            }
        }

        public static void Process()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE() || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            IsSwapped = !IsSwapped;
        }
        public static IVObject GetObjectInstFromHandle(int objectHandle)
        {
            UIntPtr at = IVPools.GetObjectPool().GetAt((uint)objectHandle);
            if (at != UIntPtr.Zero)
            {
                return IVObject.FromUIntPtr(at);
            }

            return null;
        }
        public static void Tick()
        {
            if (!enable || IS_PAUSE_MENU_ACTIVE() || IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            if (IS_USING_CONTROLLER() && WeaponHelpers.IsPlayerAiming())
            {
                if (NativeControls.IsControllerButtonPressed(padIndex, controllerKey1))
                {
                    if (DateTime.Now - lastProcessTime >= delay)
                    {
                        Process();
                        lastProcessTime = DateTime.Now;
                    }
                }
            }

            NativeCamera cam = NativeCamera.GetGameCam();
            if (cam == null) return;

            if (WeaponHelpers.IsPlayerAiming() 
                || (NativeControls.IsGameKeyPressed(0, GameKey.Aim) || NativeControls.IsGameKeyPressed(0, GameKey.Attack) && WeaponHelpers.IsHoldingGun()))
            {
                if (IsSwapped == true)
                {
                    if (obj2 == 0)
                    {
                        CREATE_OBJECT(GET_HASH_KEY("bm_poolcue"), Main.PlayerPed.Matrix.Pos + new Vector3(0f, 0f, 20f), out obj2, true);
                        SET_OBJECT_DYNAMIC(obj2, false);
                        ATTACH_OBJECT_TO_PED(obj2, Main.PlayerPed.GetHandle(), (uint)eBone.BONE_ROOT, 0.45f, 0.1f, 0.2f, 0f, 0f, 0f, 0);
                        SET_OBJECT_VISIBLE(obj2, false);
                    }
                    else
                    {
                        SET_OBJECT_COLLISION(obj2, true);
                        SET_OBJECT_ALPHA(obj2, 0);
                        GET_CHAR_HEADING(Main.PlayerPed.GetHandle(), out float pHeading);
                        SET_OBJECT_HEADING(obj2, pHeading);
                    }
                }
                else
                {
                    if (obj2 != 0 && IsSwapped == false)
                    {
                        DELETE_OBJECT(ref obj2);
                    }
                }
            }
            else
            {
                if (enableResetWhenNotAiming)
                    IsSwapped = false;

                if (obj1 != 0 && IsSwapped == false)
                {
                    DELETE_OBJECT(ref obj1);
                }
                if (obj2 != 0 && IsSwapped == false)
                {
                    DELETE_OBJECT(ref obj2);
                }
            }
        }
    }
}

using CCL.GTAIV;
using DocumentFormat.OpenXml.Presentation;
using IVSDKDotNet;
using IVSDKDotNet.Native;
using System;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo, ItsClonkAndre

namespace LibertyTweaks
{
    internal class QuickSave
    {
        private static bool enable;
        private static bool location;
        private static bool quickOrSelected;
        public static Keys key;


        // Controller Support
        private static uint padIndex = 0;
        private static ControllerButton controllerKey1;
        private static ControllerButton controllerKey2;
        private static DateTime lastProcessTime = DateTime.MinValue;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(500);

        private static Vector3 lastSavedPosition;
        private static bool firstFrame = true;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            QuickSave.section = section;
            enable = settings.GetBoolean(section, "Quick-Saving", false);
            location = settings.GetBoolean(section, "Quick-Saving - Save Location", false);
            quickOrSelected = settings.GetBoolean(section, "Quick-Saving - Select Saves", false);
            key = settings.GetKey(section, "Quick-Saving - Key", Keys.F9);
            controllerKey1 = (ControllerButton)settings.GetInteger(section, "Quick-Saving - Controller Key", (int)ControllerButton.BUTTON_DPAD_DOWN);
            controllerKey2 = (ControllerButton)settings.GetInteger(section, "Quick-Saving - Controller Key 2", (int)ControllerButton.BUTTON_START);

            if (enable)
            {
                Main.Log("script initialized...");
                Main.Log($"Save Location: {location} | Select Saves: {quickOrSelected} | Key: {key} | Controller Keys: {controllerKey1} + {controllerKey2}");
            }
        }


        public static void Tick()
        {
            if (!enable)
                return;

            if (!location)
                return;

            if (lastSavedPosition == null)
                return;

            // Only teleport player on very first frame
            if (firstFrame)
            {
                // Teleport player to last saved position if there is a last saved position
                lastSavedPosition = Main.GetTheSaveGame().GetVector3("PlayerPosition");
                float lastSavedPlayerHeading = Main.GetTheSaveGame().GetFloat("PlayerHeading");

                if (lastSavedPosition != Vector3.Zero)
                {
                    if (lastSavedPlayerHeading != 0)
                    {
                        Main.PlayerPed.Teleport(lastSavedPosition, false, true);
                        SET_CHAR_HEADING(Main.PlayerPed.GetHandle(), lastSavedPlayerHeading);
                        CLEAR_ROOM_FOR_CHAR(Main.PlayerPed.GetHandle());
                    }
                }

                firstFrame = false;
            }

            // Save last player position if game is saving
            if (Main.GetTheSaveGame().IsGameSaving())
            {
                Main.GetTheSaveGame().SetVector3("PlayerPosition", Main.PlayerPed.Matrix.Pos);
                Main.GetTheSaveGame().SetFloat("PlayerHeading", Main.PlayerPed.GetHeading());
                Main.GetTheSaveGame().Save();
                Main.Log("Saved player position: " + Main.PlayerPed.Matrix.Pos);
            }

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
        public static void IngameStartup()
        {
            if (!enable)
                return;

            if (!location)
                return;

            firstFrame = true;
        }

        public static void Process()
        {
            if (!enable)
                return;

            float heightAboveGround;
            bool autoSaveStatus = Natives.GET_IS_AUTOSAVE_OFF();

            heightAboveGround = IVPedExtensions.GetHeightAboveGround(Main.PlayerPed);

            if (heightAboveGround < 2)
            {
                if (IS_PED_RAGDOLL(Main.PlayerPed.GetHandle()))
                    return;

                if (IVTheScripts.IsPlayerOnAMission())
                    return;

                if (quickOrSelected == false)
                {
                    if (autoSaveStatus == true)
                    {
                        IVGame.ShowSubtitleMessage("Auto-save is currently disabled.");
                        return;
                    }
                    else
                    {
                        NativeGame.DoAutoSave();
                    }
                }
                else
                {
                    NativeGame.ShowSaveMenu();
                }
            }
        }
    }
}
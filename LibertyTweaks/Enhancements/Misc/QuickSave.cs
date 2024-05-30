using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Native;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo & ItsClonkAndre

namespace LibertyTweaks
{
    internal class QuickSave
    {
        private static bool enable;
        private static bool saveLocation;
        private static bool quickOrSelected;
        private static bool firstFrame = true;
        private static Vector3 lastSavedPosition;
        private static float lastSavedPlayerHeading;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Quick-Saving", "Enable", true);
            saveLocation = settings.GetBoolean("Quick-Saving", "Save Location", true);
            quickOrSelected = settings.GetBoolean("Quick-Saving", "Select Saves", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (!saveLocation)
                return;

            if (lastSavedPosition == null)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            // Only teleport player on very first frame
            if (firstFrame)
            {
                try
                {
                    // Teleport player to last saved position if there is a last saved position
                    Vector3 lastSavedPosition = Main.GetTheSaveGame().GetVector3("PlayerPosition");
                    float lastSavedPlayerHeading = Main.GetTheSaveGame().GetFloat("PlayerHeading");

                    if (lastSavedPosition != Vector3.Zero)
                    {
                        if (lastSavedPlayerHeading != 0)
                        {
                            playerPed.Teleport(lastSavedPosition, false, true);
                            SET_CHAR_HEADING(playerPed.GetHandle(), lastSavedPlayerHeading);
                            CLEAR_ROOM_FOR_CHAR(playerPed.GetHandle());
                        }
                    }
                }
                catch (System.Exception) 
                {

                }

                firstFrame = false;
            }

            // Save last player position if game is saving
            if (Main.GetTheSaveGame().IsGameSaving())
            {
                Main.GetTheSaveGame().SetVector3("PlayerPosition", playerPed.Matrix.Pos);
                Main.GetTheSaveGame().SetFloat("PlayerHeading", playerPed.GetHeading());
                Main.GetTheSaveGame().Save();
            }
        }

        public static void IngameStartup()
        {
            if (!enable)
                return;

            if (!saveLocation)
                return;

            firstFrame = true;
        }

        public static void Process()
        {
            if (!enable)
                return;

            int playerId;
            float heightAboveGround;
            bool autoSaveStatus = Natives.GET_IS_AUTOSAVE_OFF();

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            playerId = IVPedExtensions.GetHandle(playerPed);
            heightAboveGround = IVPedExtensions.GetHeightAboveGround(playerPed);

            if (!IS_CHAR_IN_ANY_CAR(playerId))
            {
                if (heightAboveGround < 2)
                {
                    if (IS_PED_RAGDOLL(playerId))
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
}
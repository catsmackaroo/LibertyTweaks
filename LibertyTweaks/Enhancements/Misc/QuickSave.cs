using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Native;
using LibertyTweaks;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class QuickSave
    {
        private static bool enable;
        private static bool saveLocation;
        private static bool quickOrSelected;
        private static bool firstFrame = true;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Quick-Saving", "Enable", true);
            saveLocation = settings.GetBoolean("Quick-Saving", "Save Location", true);
            quickOrSelected = settings.GetBoolean("Quick-Saving", "Select Saves", true);
        }

        public static void Tick()
        {
            if (!enable)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            // Only teleport player on very first frame
            if (firstFrame)
            {
                // Teleport player to last saved position if there is a last saved position
                Vector3 lastSavedPosition = Main.GetTheSaveGame().GetVector3("LastPosition");

                if (lastSavedPosition != Vector3.Zero)
                    playerPed.Teleport(lastSavedPosition, false, true);

                firstFrame = false;
            }

            // Save last player position if game is saving
            if (Main.GetTheSaveGame().IsGameSaving())
            {
                Main.GetTheSaveGame().SetVector3("LastPosition", playerPed.Matrix.Pos);
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
                        if (!IS_CHAR_IN_ANY_CAR((int)playerId))
                        {
                            NativeGame.DoAutoSave();
                        }
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
using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Native;
using LibertyTweaks;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class QuickSave
    {
        private static bool enableFix;
        private static bool quickOrSelected;

        public void Init(SettingsFile settings, CustomIVSave saveGame)
        {
            enableFix = settings.GetBoolean("Quick-Saving", "Enable", true);
            quickOrSelected = settings.GetBoolean("Quick-Saving", "Select Saves", true);
        }

        public static void Process()
        {
            if (!enableFix)
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
        //public static void Spawn()
        //{
        //    if (saveName == IVGenericGameStorage.ValidSaveName)
        //    {

        //    }
        //}
        //}
    }
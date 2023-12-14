using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class QuickSave
    {
        private static bool enableFix;
        private static bool quickOrSelected;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "Quick Saving", true);
            quickOrSelected = settings.GetBoolean("Main", "Selected Saves", true);
        }

        public static void Process()
        {
            if (!enableFix) 
                return;

            int playerId;
            float heightAboveGround;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            playerId = IVPedExtensions.GetHandle(playerPed); 
            heightAboveGround = IVPedExtensions.GetHeightAboveGround(playerPed);

            if (heightAboveGround < 2)
            {
                if (IS_PED_RAGDOLL(playerId))
                    return;

                bool autoSaveStatus = Natives.GET_IS_AUTOSAVE_OFF();

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
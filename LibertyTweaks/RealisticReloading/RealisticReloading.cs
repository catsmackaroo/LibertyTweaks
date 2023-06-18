using CCL.GTAIV;
using CCL.GTAIV.AnimationController;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre

namespace LibertyTweaks.RealisticReloading
{
    internal class RealisticReloading
    {
        public static void Init(SettingsFile settings)
        {

        }

        public static void Tick()
        {
            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed()); // Grab Player CPed
            PedAnimationController animController = playerPed.GetAnimationController();
            bool isPlayerDucking = IS_CHAR_DUCKING(playerPed.GetHandle());

            uint ammoTotal, ammoRest, ammoClip, ammoReduced;    // UINTs for Max Ammo, Max Ammo Minus Clip, Ammo Clip, Ammo Max Minus Clip Minus Reduced
            uint currentWeapon; // UINT for currentWeapon

            GET_CURRENT_CHAR_WEAPON(playerPed.GetHandle(), out currentWeapon);
            GET_AMMO_IN_CHAR_WEAPON(playerPed.GetHandle(), currentWeapon, out ammoTotal);
            GET_AMMO_IN_CLIP(playerPed.GetHandle(), currentWeapon, out ammoClip);

            ammoRest = ammoTotal -= ammoClip;
            ammoReduced = ammoRest -= ammoClip;

            CGame.ShowSubtitleMessage(ammoReduced.ToString());

            bool isReloading = animController.IsPlaying("gun@handgun", isPlayerDucking ? "reload_crouch" : "reload")
                || animController.IsPlaying("gun@deagle", isPlayerDucking ? "reload_crouch" : "reload")
                || animController.IsPlaying("gun@uzi", isPlayerDucking ? "reload_crouch" : "reload")
                || animController.IsPlaying("gun@mp5k", isPlayerDucking ? "reload_crouch" : "p_load")
                || animController.IsPlaying("gun@ak47", isPlayerDucking ? "reload_crouch" : "p_load")
                || animController.IsPlaying("gun@rifle", isPlayerDucking ? "reload_crouch" : "p_load")
                || animController.IsPlaying("gun@rifle", isPlayerDucking ? "reload_crouch" : "reload");

            if (isReloading == true) 
            {
                SET_CHAR_AMMO(playerPed.GetHandle(), currentWeapon, ammoReduced);
            }
        }
    }
}

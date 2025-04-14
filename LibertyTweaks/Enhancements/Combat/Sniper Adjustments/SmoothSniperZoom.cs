using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SmoothSniperZoom
    {
        private static bool enable;
        private static float currentFOV;
        private static float targetFOV;
        private static readonly float lerpSpeed = 0.3f;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            SmoothSniperZoom.section = section;
            enable = settings.GetBoolean(section, "Smooth Sniper Zoom", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable) return;

            var cam = NativeCamera.GetGameCam();
            if (Main.PlayerPed == null || cam == null) return;
            if (Main.PlayerPed.GetHandle() == 0) return;

            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);
            uint currentWeapSlot = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot;

            if (IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson == true && WeaponHelpers.IsPlayerAiming())
            {
                float newFOV = cam.FOV;

                if (newFOV != targetFOV)
                    targetFOV = newFOV;

                currentFOV = CommonHelpers.SmoothStep(currentFOV, targetFOV, lerpSpeed);
                cam.FOV = currentFOV;
            }
        }
    }
}

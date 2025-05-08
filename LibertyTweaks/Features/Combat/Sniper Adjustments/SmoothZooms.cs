using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SmoothZooms
    {
        private static bool enable;
        private static float currentFOV;
        private static float targetFOV;
        private static readonly float lerpSpeed = 0.35f;
        public static string section { get; private set; }

        public static void Init(SettingsFile settings, string section)
        {
            SmoothZooms.section = section;
            enable = settings.GetBoolean(section, "Smoother Zoom", false);

            if (enable)
            {
                Main.Log("script initialized...");
                Main.Log("WARNING: This feature seems incompatible with late versions of FusionFix...if you encounter issues, disable this feature.");
            }
        }

        public static void Tick()
        {
            if (!enable) return;

            var cam = NativeCamera.GetGameCam();
            if (Main.PlayerPed == null || cam == null) return;
            if (Main.PlayerPed.GetHandle() == 0) return;

            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);
            uint currentWeapSlot = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot;

            float newFOV = cam.FOV;

            if (newFOV != targetFOV)
            {
                targetFOV = newFOV;
            }

            currentFOV = CommonHelpers.SmoothStep(currentFOV, targetFOV, lerpSpeed);
            cam.FOV = currentFOV;
        }
    }
}

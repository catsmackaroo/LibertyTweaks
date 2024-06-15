using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using CCL;
using CCL.GTAIV;

// Credits: catsmackaroo, ClonkAndre

namespace LibertyTweaks
{
    internal class FOV
    {
        private static bool enableIncreasedFov;
        private static bool enableDynamicFov;
        private static float targetFOV = 1.1f;
        private static float currentFOV = 1.0f;
        private static float lerpSpeed = 0.05f;
        private static float maxFOVMultiplier = 1.22f;
        private static float adjustableMultiplier;

        public static void Init(SettingsFile settings)
        {
            enableIncreasedFov = settings.GetBoolean("Field of View", "Increased Field of View", true);
            enableDynamicFov = settings.GetBoolean("Field of View", "Dynamic Field of View", true);
            adjustableMultiplier = settings.GetFloat("Field of View", "Increased Multiplier", 1.1f); 

            if (enableIncreasedFov)
                Main.Log("- Increased Field of View script initialized...");

            if (enableDynamicFov)
                Main.Log("- Dynamic Field of View script initialized...");
        }

        public static void Tick()
        {
            IVCam cam = IVCamera.TheFinalCam;
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            int playerPedHandle = playerPed.GetHandle();

            if (enableIncreasedFov)
                IncreasedFOV(cam, adjustableMultiplier);

            if (enableDynamicFov)
                DynamicFOV(playerPedHandle, cam);
        }

        private static void IncreasedFOV(IVCam cam, float adjustableMultiplier)
        {
            if (cam != null)
                cam.FOV = cam.FOV * adjustableMultiplier;
        }

        private static void DynamicFOV(int playerPedHandle, IVCam cam)
        {
            if (IS_CHAR_IN_ANY_CAR(playerPedHandle))
            {
                GET_CAR_CHAR_IS_USING(playerPedHandle, out int pVehInt);
                GET_CAR_SPEED(pVehInt, out float vehSpeed);
                if (IS_CHAR_IN_ANY_HELI(playerPedHandle))
                {
                    targetFOV = 1.0f;
                }

                if (vehSpeed > 2)
                {
                    targetFOV = 1.0f;
                }

                targetFOV = 1.0f + vehSpeed / 300.0f;
                if (targetFOV > maxFOVMultiplier)
                {
                    targetFOV = maxFOVMultiplier;
                }
            }
            else
            {
                targetFOV = 1.0f;
            }

            currentFOV = Lerp(currentFOV, targetFOV, lerpSpeed);

            if (cam != null)
            {
                cam.FOV = cam.FOV * currentFOV;
            }
        }

        private static float Lerp(float start, float end, float amount)
        {
            return start + (end - start) * amount;
        }
    }
}

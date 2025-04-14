using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal static class VehicleHelpers
    {
        public static float GetCameraCarMisalignment(int vehicleHandle, NativeCamera cam)
        {
            if (vehicleHandle == 0 || cam == null) return 0.0f;

            GET_CAR_HEADING(vehicleHandle, out float carHeading);
            float camHeading = cam.Rotation.Z;

            float headingDifference = Math.Abs(carHeading - camHeading);
            if (headingDifference > 180.0f)
                headingDifference = 360.0f - headingDifference;

            return headingDifference;
        }
    }
}

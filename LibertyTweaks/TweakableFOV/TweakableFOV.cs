using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo, ClonkAndre

namespace LibertyTweaks
{
    internal class TweakableFOV
    {
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "Tweakable Field of View", true);
        }

        public static void Tick(float fovMulti)
        {
            if (!enableFix)
                return;

            CCam cam = CCamera.GetFinalCam();
            if (cam != null)
            {
                cam.FOV = cam.FOV * fovMulti;
            }
        }
    }
}

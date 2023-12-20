using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credit: catsmackaroo

namespace LibertyTweaks
{
    internal class CarFireBreakdown
    {
        private static bool enable;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Vehicles Break on Fire", "Enable", true);
        }

        public static void Tick()
        {
            if (!enable)
                return;

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            if (IS_CHAR_IN_ANY_CAR(playerPed.GetHandle()))
            {

                GET_CAR_CHAR_IS_USING(playerPed.GetHandle(), out int pVeh); 

                if (IS_CAR_ON_FIRE(pVeh))
                {
                    SET_CAR_ENGINE_ON(pVeh, false, false);
                }
            }

        }
    }
}


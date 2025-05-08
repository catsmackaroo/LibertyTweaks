using CCL.GTAIV;
using IVSDKDotNet;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;


// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class NooseCruiserWithNoose
    {
        private static bool enable;
        private static uint noosecruiser = 148777611;
        private static uint nooseswat = 3290204350;
        private static uint cop = 4111764146;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Fixes", "NOoSE Cruisers with NOoSE", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;



            foreach (var kvp in PedHelper.VehHandles)
            {
                int carhandle = kvp.Value;

                GET_CAR_MODEL(carhandle, out uint model);
                if (model == noosecruiser)
                {
                    GET_DRIVER_OF_CAR(carhandle, out var driver);
                    GET_CHAR_MODEL(driver, out uint drivermodel);
                    if (drivermodel == cop)
                    {
                        DELETE_CHAR(ref driver);
                        GET_CAR_COORDINATES(carhandle, out Vector3 carpos);
                        NativeWorld.SpawnPed(nooseswat, carpos, out int newdriver);

                    }

                }
            }
        }
    }
}


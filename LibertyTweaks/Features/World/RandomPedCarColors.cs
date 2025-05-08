using IVSDKDotNet;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class RandomPedCarColors
    {
        private static bool enable;
        private static HashSet<int> ignoredVehicles = new HashSet<int>();

        private static readonly HashSet<uint> gangs = new HashSet<uint>
        {
            3993744036,
            3151056432,
            869501081,
            632613980,
            3791037286,
            4059382627,
            207714363,
            514268366,
            43005364,
            1346668127,
            2617712099,
            2833685951,
            1574850459,
            2341677824,
            280474699,
            4275703952,
            1844702918,
            1609755055,
            3964469865,
            1117105909,
            2794569427,
            3413608606,
            1540383669,
            764249904,
            492147228,
            2368926169,
            1168388225,
            2548192516,
            3992604899,
            2678076464,
            64730935,
            510389335,
            2458961059,
            2206803240,
            1976502708,
            1543404628,
            1865532596,
            431692232,
            1724587620,
            3114292481,
            871281791,
            683712035,
            3210959519,
            4130031670
        };
        private static readonly HashSet<uint> folk = new HashSet<uint>
        {
            2932525255,
            52357603,
            575808580,
            812112483,
            4165724716,
            54114008,
            4002254208,
            1743814728,
            1158569407,
            1969438324,
            1621955848,
            2488080944,
            3272046500,
            2968572791,
            3072003881,
            2548814027,
            2104499156,
            26615298,
        };

        private static readonly HashSet<uint> taxis = new HashSet<uint>
        {
            3338918751,
            1208856469,
            1884962369
        };

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Random Ped Car Colors", "Enable", true);
            ignoredVehicles = new HashSet<int>();

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            try
            {
                foreach (var kvp in PedHelper.VehHandles)
                {
                    int vehHandle = kvp.Value;

                    if (!ignoredVehicles.Contains(vehHandle))
                    {
                        GET_DRIVER_OF_CAR(vehHandle, out int driverHandle);

                        if (driverHandle == 0)
                        {
                            ignoredVehicles.Add(vehHandle);
                            continue;
                        }

                        GET_CHAR_MODEL(driverHandle, out uint model);
                        if (!gangs.Contains(model) && !folk.Contains(model))
                        {
                            ignoredVehicles.Add(vehHandle);
                            continue;
                        }

                        GET_CAR_MODEL(vehHandle, out uint carModel);
                        if (taxis.Contains(carModel))
                        {
                            ignoredVehicles.Add(vehHandle);
                            continue;
                        }

                        GET_CAR_COLOURS(vehHandle, out int color1, out int color2);
                        GET_EXTRA_CAR_COLOURS(vehHandle, out int extraColor1, out int extraColor2);

                        int chance = Main.GenerateRandomNumber(0, 99);
                        if (chance <= 20)
                        {
                            ignoredVehicles.Add(vehHandle);
                            continue;
                        }

                        int selectionRng = Main.GenerateRandomNumber(0, 3);
                        int randomColor1 = Main.GenerateRandomNumber(color1 - 10, color1 + 10);
                        int randomColor2 = Main.GenerateRandomNumber(color2 - 10, color2 + 10);

                        switch (selectionRng)
                        {
                            case 0:
                                int blackedRng = Main.GenerateRandomNumber(0, 99);
                                if (blackedRng <= 10)
                                {
                                    randomColor1 = 0;
                                    randomColor2 = 0;
                                }
                                break;
                            case 1:
                                int sameColorRng = Main.GenerateRandomNumber(0, 99);
                                if (sameColorRng <= 50)
                                    randomColor1 = randomColor2;
                                break;
                            case 2:
                                int wackedOutRng = Main.GenerateRandomNumber(0, 99);
                                if (wackedOutRng <= 5)
                                {
                                    randomColor1 = Main.GenerateRandomNumber(0, 133);
                                    randomColor2 = Main.GenerateRandomNumber(0, 133);
                                }
                                break;
                            default:
                                break;
                        }

                        CHANGE_CAR_COLOUR(vehHandle, randomColor1, randomColor2);
                        ignoredVehicles.Add(vehHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                Main.Log($"Error in Tick: {ex.Message}");
            }
        }
    }
}

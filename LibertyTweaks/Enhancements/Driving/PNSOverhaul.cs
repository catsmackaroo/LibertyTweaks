using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class PNSOverhaul
    {
        public static bool enable;
        public static int CarInitCost;
        public static int EngInitCost;
        public static float CarCostMult;
        public static float EngCostMult;
        public static int ColorCost;

        private static bool gotColor;
        private static int PrimColor1;
        private static int PrimColor2;
        private static int SecColor1;
        private static int SecColor2;
        private static int NumOfResprays;
        private static bool CheckDateTime;
        private static DateTime currentDateTime;
        public static IVVehicle playerVehicle;
        public static IVVehicle pVehicle;
        private static int vehHandle;
        private static int pVeh;
        private static int cams;
        private static bool inMenu;
        private static bool sprayCar;
        private static bool FastScroll;
        private static bool changeColor;
        private static bool isCamKeyDown;
        private static bool isLeftKeyDown;
        private static bool isRightKeyDown;
        private static bool isUpKeyDown;
        private static bool isDownKeyDown;
        private static bool isEnterKeyDown;
        private static bool isCancelKeyDown;
        private static bool Broke;
        private static bool HasWantedLvl;
        private static bool hasGotWanted;
        private static bool GoToConfirm;
        private static bool GoBack;
        private static bool confirmation;
        private static int colorType;
        private static int pColor1;
        private static int pColor2;
        private static int sColor1;
        private static int sColor2;
        private static int damageCost;
        private static int engineCost;
        private static uint episode;
        private static uint pMoney;
        private static uint pCarHealth;
        private static float pEngineHealth;
        private static uint cHealth;
        private static float eHealth;
        private static string colorTypeString;
        private static Vector3 offset;
        private static NativeCamera cam;
        private static int randomNum;
        private static int vehHash;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            PNSOverhaul.section = section;
            enable = settings.GetBoolean(section, "Pay 'n' Spray Overhaul", false);
            CarInitCost = settings.GetInteger(section, "Pay 'n' Spray Overhaul - Initial Deformation Cost", 100);
            EngInitCost = settings.GetInteger(section, "Pay 'n' Spray Overhaul - Initial Engine Cost", 100);
            CarCostMult = settings.GetFloat(section, "Pay 'n' Spray Overhaul - Deformation Cost Multiplier", 2.0f);
            EngCostMult = settings.GetFloat(section, "Pay 'n' Spray Overhaul - Engine Cost Multiplier", 2.0f);
            ColorCost = settings.GetInteger(section, "Pay 'n' Spray Overhaul - Color Cost", 200);

            cHealth = 1000;
            eHealth = 1000;
            colorType = 4;
            cams = 1;
            colorTypeString = "Primary Color";

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            episode = GET_CURRENT_EPISODE();

            if (cam == null)
            {
                cam = NativeCamera.Create();
                cam.Deactivate();
            }

            playerVehicle = Main.PlayerVehicle;
            vehHandle = Main.PlayerVehicle.GetHandle();

            if (!IS_CHAR_DEAD(Main.PlayerPed.GetHandle()))
            {
                if (playerVehicle != null)
                {
                    if (!gotColor)
                    {
                        GET_CAR_COLOURS(vehHandle, out PrimColor1, out PrimColor2);
                        GET_EXTRA_CAR_COLOURS(vehHandle, out SecColor1, out SecColor2);
                        NumOfResprays = GET_INT_STAT(282);
                        gotColor = true;
                    }

                    if (PLAYER_IS_INTERACTING_WITH_GARAGE())
                    {
                        if (IS_WANTED_LEVEL_GREATER(Main.PlayerIndex, 0) && !hasGotWanted)
                        {
                            HasWantedLvl = true;
                            hasGotWanted = true;
                        }
                        else if (!hasGotWanted)
                            HasWantedLvl = false;

                        GET_CAR_HEALTH(vehHandle, out pCarHealth);
                        pEngineHealth = GET_ENGINE_HEALTH(vehHandle);
                        pMoney = IVPlayerInfoExtensions.GetMoney(Main.PlayerPed.PlayerInfo);
                        if (pCarHealth < 1000 && pCarHealth >= 0)
                            cHealth = pCarHealth;
                        if (pEngineHealth < 1000 && pEngineHealth >= 0)
                            eHealth = pEngineHealth;

                        else if (pCarHealth >= 1000 && !IS_SCREEN_FADING())
                        {
                            cHealth = 1000;
                            eHealth = 1000;
                        }

                        if (1000 - (int)cHealth > (CarInitCost / CarCostMult))
                            damageCost = (int)((1000 - (int)cHealth) * CarCostMult);
                        else
                            damageCost = CarInitCost;

                        if (1000 - (int)eHealth > (EngInitCost / EngCostMult))
                            engineCost = (int)((1000 - (int)eHealth) * EngCostMult);
                        else
                            engineCost = EngInitCost;

                        if (pMoney < (int)(damageCost + engineCost))
                        {
                            IVGame.ShowSubtitleMessage("You need $" + ((int)(damageCost + engineCost)).ToString() + " to pay for the repairs");

                            IVGarages.NoResprays = true;
                        }
                        else
                            IVGarages.NoResprays = false;

                        pVeh = vehHandle;
                        pVehicle = playerVehicle;
                    }

                    else if (!PLAYER_IS_INTERACTING_WITH_GARAGE())
                        hasGotWanted = false;

                    if (NumOfResprays < GET_INT_STAT(282) && !inMenu)
                    {
                        CHANGE_CAR_COLOUR(vehHandle, PrimColor1, PrimColor2);
                        SET_EXTRA_CAR_COLOURS(vehHandle, SecColor1, SecColor2);
                        gotColor = false;

                        pColor1 = PrimColor1;
                        pColor2 = PrimColor2;
                        sColor1 = SecColor1;
                        sColor2 = SecColor2;

                        FREEZE_CAR_POSITION(pVeh, true);
                        WARP_CHAR_INTO_CAR(Main.PlayerPed.GetHandle(), pVeh);
                        LOCK_CAR_DOORS(pVeh, 4);

                        IVPlayerInfoExtensions.RemoveMoney(Main.PlayerPed.PlayerInfo, (int)(damageCost + engineCost));
                        inMenu = true;
                    }
                }
                if (isLeftKeyDown || isRightKeyDown)
                {
                    if (CheckDateTime == false)
                    {
                        currentDateTime = DateTime.Now;
                        CheckDateTime = true;
                    }
                    if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > 500)
                    {
                        CheckDateTime = false;

                        FastScroll = true;
                    }
                }
                else if (!isLeftKeyDown && !isRightKeyDown)
                {
                    CheckDateTime = false;
                    FastScroll = false;
                }
                if (FastScroll)
                {
                    if (CheckDateTime == false)
                    {
                        currentDateTime = DateTime.Now;
                        CheckDateTime = true;
                    }
                    if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > 50)
                    {
                        CheckDateTime = false;
                        PickColors();
                    }
                }

                if (inMenu)
                {
                    if (IS_CHAR_VISIBLE(Main.PlayerPed.GetHandle()))
                    {
                        SET_CHAR_VISIBLE(Main.PlayerPed.GetHandle(), false);
                    }
                    Main.PlayerVehicle.SteerActual = 3;
                    Main.PlayerVehicle.SteerDesired = 3;

                    ColorMenu();
                    if (!confirmation)
                    {
                        if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                        {
                            isLeftKeyDown = true;
                            PickColors();
                        }
                        if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                        {
                            isRightKeyDown = true;
                            PickColors();
                        }
                        if (NativeControls.IsGameKeyPressed(0, GameKey.NavUp) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                        {
                            if (colorType < 4)
                                colorType += 1;
                            else
                                colorType = 1;

                            isUpKeyDown = true;
                            PickColors();
                        }
                        if (NativeControls.IsGameKeyPressed(0, GameKey.NavDown) && !isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                        {
                            if (colorType > 1)
                                colorType -= 1;
                            else
                                colorType = 4;
                            isDownKeyDown = true;
                            PickColors();
                        }
                    }
                    if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && !isCamKeyDown)
                    {
                        if (cams < 4)
                            cams += 1;
                        else
                            cams = 1;

                        isCamKeyDown = true;
                    }
                    if (NativeControls.IsGameKeyPressed(0, GameKey.NavEnter) && !isEnterKeyDown && !isCancelKeyDown)
                    {
                        GoToConfirm = true;
                        isEnterKeyDown = true;
                    }

                    if (NativeControls.IsGameKeyPressed(0, GameKey.NavBack) && !isEnterKeyDown && !isCancelKeyDown)
                    {
                        GoBack = true;
                        isCancelKeyDown = true;
                    }

                    if (!NativeControls.IsGameKeyPressed(0, GameKey.NavLeft) && isLeftKeyDown)
                        isLeftKeyDown = false;

                    if (!NativeControls.IsGameKeyPressed(0, GameKey.NavRight) && isRightKeyDown)
                        isRightKeyDown = false;

                    if (!NativeControls.IsGameKeyPressed(0, GameKey.NavUp) && isUpKeyDown)
                        isUpKeyDown = false;

                    if (!NativeControls.IsGameKeyPressed(0, GameKey.NavDown) && isDownKeyDown)
                        isDownKeyDown = false;

                    if (!NativeControls.IsGameKeyPressed(0, GameKey.Action) && isCamKeyDown)
                        isCamKeyDown = false;

                    if (!NativeControls.IsGameKeyPressed(0, GameKey.NavEnter) && isEnterKeyDown)
                        isEnterKeyDown = false;

                    if (!NativeControls.IsGameKeyPressed(0, GameKey.NavBack) && isCancelKeyDown)
                        isCancelKeyDown = false;
                }
            }
            if (playerVehicle != null && !Main.PlayerPed.IsInVehicle() && gotColor)
            {
                gotColor = false;
                NumOfResprays = GET_INT_STAT(282);
            }
        }
        private static void ColorMenu()
        {
            if (!sprayCar)
            {
                pMoney = IVPlayerInfoExtensions.GetMoney(Main.PlayerPed.PlayerInfo);
                if (confirmation)
                    IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Press ~INPUT_PICKUP~ to change cycle through cameras.~n~Press ~INPUT_FRONTEND_ACCEPT~ to confirm.~n~Press ~INPUT_PHONE_CANCEL~ to go back.");

                else if (!Broke)
                {
                    if (!IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Use ~INPUT_KB_UP~ and ~INPUT_KB_DOWN~ to change color types.~n~Use ~INPUT_KB_LEFT~ and ~INPUT_KB_RIGHT~ to browse colors.~n~Press ~INPUT_PICKUP~ to change cycle through cameras.~n~Press ~INPUT_FRONTEND_ACCEPT~ to accept.~n~Press ~INPUT_PHONE_CANCEL~ to cancel.~n~Changing colors will cost an extra $" + ColorCost.ToString() + ".");
                    else if (IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Use ~PAD_DPAD_UP~ and ~PAD_DPAD_DOWN~ to change color types.~n~Use ~PAD_DPAD_LEFT~ and ~PAD_DPAD_RIGHT~ to browse colors.~n~Press ~INPUT_PICKUP~ to change cycle through cameras.~n~Press ~INPUT_FRONTEND_ACCEPT~ to accept.~n~Press ~INPUT_PHONE_CANCEL~ to cancel.~n~Changing colors will cost an extra $" + ColorCost.ToString() + ".");
                }
                else
                {
                    if (!IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "You cannot afford a respray!~n~Press ~INPUT_PHONE_CANCEL~ to continue.");
                    else if (IS_USING_CONTROLLER())
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "You cannot afford a respray!~n~Press ~INPUT_PHONE_CANCEL~ to continue.");
                }

                PRINT_HELP_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "");
                cam.PointAtVehicle(vehHandle);
                cam.Activate();
                if (cams == 1)
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(0, 2.5f, 2.5f), out offset);
                else if (cams == 2)
                {
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(2.5f, 0f, 2.5f), out offset);
                    offset += new Vector3(0, 0, -1.55f);
                }
                else if (cams == 3)
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(-2.5f, 0f, 2.5f), out offset);
                else if (cams == 4)
                    GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(vehHandle, new Vector3(0, -2.5f, 2.5f), out offset);
                cam.Position = offset;

                string colorName;
                if (colorType == 4)
                {
                    colorName = colorMap.ContainsKey(pColor1) ? colorMap[pColor1] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({pColor1})");
                }
                else if (colorType == 3)
                {
                    colorName = colorMap.ContainsKey(pColor2) ? colorMap[pColor2] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({pColor2})");
                }
                else if (colorType == 2)
                {
                    colorName = colorMap.ContainsKey(sColor1) ? colorMap[sColor1] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({sColor1})");
                }
                else
                {
                    colorName = colorMap.ContainsKey(sColor2) ? colorMap[sColor2] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({sColor2})");
                }

                if (!isLeftKeyDown && !isRightKeyDown && !isUpKeyDown && !isDownKeyDown)
                {
                    if (pMoney >= ColorCost && !confirmation && GoToConfirm)
                    {
                        confirmation = true;
                        GoToConfirm = false;
                    }

                    else if (pMoney >= ColorCost && confirmation && GoToConfirm)
                    {
                        DO_SCREEN_FADE_OUT(1000);
                        changeColor = true;
                        sprayCar = true;
                        GoToConfirm = false;
                    }
                    else if (pMoney < ColorCost && GoToConfirm)
                    {
                        Broke = true;
                        GoToConfirm = false;
                    }

                    else if (confirmation && GoBack)
                    {
                        confirmation = false;
                        GoBack = false;
                    }

                    else if (!confirmation && GoBack)
                    {
                        DO_SCREEN_FADE_OUT(1000);
                        sprayCar = true;
                        GoBack = false;
                    }
                }
            }

            else if (sprayCar && IS_SCREEN_FADED_OUT())
            {
                vehHash = (pVehicle.ModelIndex.GetHashCode());
                randomNum = Main.GenerateRandomNumber(1, 6);
                DO_SCREEN_FADE_IN(1000);

                if (!changeColor || (PrimColor1 == pColor1 && PrimColor2 == pColor2 && SecColor1 == sColor1 && SecColor2 == sColor2))
                {
                    CHANGE_CAR_COLOUR(pVeh, PrimColor1, PrimColor2);
                    SET_EXTRA_CAR_COLOURS(pVeh, SecColor1, SecColor2);
                    if (((episode == 0 || episode == 2) && (vehHash == 10616994 || vehHash == 7077996 || vehHash == 11927734)) || (episode == 1 && (vehHash == 35389980 || vehHash == 38928978 || vehHash == 40239718)))
                        IVGame.ShowSubtitleMessage("This is the best I can do with this wreck.", 4000);
                    else if (((episode == 0 || episode == 2) && vehHash == 10682531 && pColor1 == 51 && pColor2 == 51) || (episode == 1 && vehHash == 38994515 && pColor1 == 51 && pColor2 == 51))
                        IVGame.ShowSubtitleMessage("That's the motherfucking green sabre!", 4000);
                    else if (HasWantedLvl && randomNum > 3)
                        IVGame.ShowSubtitleMessage("They won't be looking for these plates.", 4000);
                    else if (HasWantedLvl && randomNum <= 3)
                        IVGame.ShowSubtitleMessage("These plates will throw them off the scent.", 4000);
                    else if (randomNum > 3)
                        IVGame.ShowSubtitleMessage("As good as new.", 4000);
                    else if (randomNum <= 3)
                        IVGame.ShowSubtitleMessage("Not a scratch.", 4000);
                }
                else
                {
                    if (((episode == 0 || episode == 2) && (vehHash == 10616994 || vehHash == 7077996 || vehHash == 11927734)) || (episode == 1 && (vehHash == 35389980 || vehHash == 38928978 || vehHash == 40239718)))
                        IVGame.ShowSubtitleMessage("This is the best I can do with this wreck.", 4000);
                    else if (((episode == 0 || episode == 2) && vehHash == 10682531 && pColor1 == 51 && pColor2 == 51) || (episode == 1 && vehHash == 38994515 && pColor1 == 51 && pColor2 == 51))
                        IVGame.ShowSubtitleMessage("That's the motherfucking green sabre!", 4000);
                    else if (HasWantedLvl && randomNum <= 2)
                        IVGame.ShowSubtitleMessage("They won't be looking for this color.", 4000);
                    else if (HasWantedLvl && randomNum > 4)
                        IVGame.ShowSubtitleMessage("People aren't going to screw with this paintjob.", 4000);
                    else if (HasWantedLvl && randomNum > 2)
                        IVGame.ShowSubtitleMessage("They'll be looking for a different color.", 4000);
                    else if (randomNum <= 2)
                        IVGame.ShowSubtitleMessage("Nice color.", 4000);
                    else if (randomNum > 4)
                        IVGame.ShowSubtitleMessage("The new color is much better.", 4000);
                    else if (randomNum > 2)
                        IVGame.ShowSubtitleMessage("Hope you like the new color.", 4000);
                    IVPlayerInfoExtensions.RemoveMoney(Main.PlayerPed.PlayerInfo, ColorCost);
                }

                cam.Deactivate();
                CLEAR_HELP();
                Broke = false;
                FREEZE_CAR_POSITION(pVeh, false);
                LOCK_CAR_DOORS(pVeh, 0);
                colorType = 4;
                cams = 1;

                if (!IS_CHAR_VISIBLE(Main.PlayerPed.GetHandle()))
                {
                    SET_CHAR_VISIBLE(Main.PlayerPed.GetHandle(), true);
                }
                changeColor = false;
                sprayCar = false;
                inMenu = false;
                gotColor = false;
                hasGotWanted = false;
                confirmation = false;
            }
        }


        private static readonly Dictionary<int, string> colorMap = new Dictionary<int, string>
        {
            { 0, "Black" },
            { 1, "Black Poly" },
            { 2, "Concord Blue Poly" },
            { 3, "Pewter Gray Poly" },
            { 4, "Silver Stone Poly" },
            { 5, "Winning Silver Poly" },
            { 6, "Steel Gray Poly" },
            { 7, "Shadow Silver Poly" },
            { 8, "Silver Stone Poly" },
            { 9, "Porcelain Silver Poly" },
            { 10, "Gray Poly" },
            { 11, "Anthracite Gray Poly" },
            { 12, "Astra Silver Poly" },
            { 13, "Ascot Gray" },
            { 14, "Clear Crystal Blue Frost Poly" },
            { 15, "Silver Poly" },
            { 16, "Dark Titanium Poly" },
            { 17, "Titanium Frost Poly" },
            { 18, "Police White" },
            { 19, "Medium Gray Poly" },
            { 20, "Medium Gray Poly" },
            { 21, "Steel Gray Poly" },
            { 22, "Slate Gray" },
            { 23, "Gun Metal Poly" },
            { 24, "Light Blue Grey" },
            { 25, "Securicor Light Gray" },
            { 26, "Arctic White" },
            { 27, "Very Red" },
            { 28, "Torino Red Pearl" },
            { 29, "Formula Red" },
            { 30, "Blaze Red" },
            { 31, "Graceful Red Mica" },
            { 32, "Garnet Red Poly" },
            { 33, "Desert Red" },
            { 34, "Cabernet Red Poly" },
            { 35, "Turismo Red" },
            { 36, "Desert Red" },
            { 37, "Currant Red Solid" },
            { 38, "Bright Currant Red Poly" },
            { 39, "Electric Currant Red Poly" },
            { 40, "Medium Cabernet Solid" },
            { 41, "Wild Strawberry Poly" },
            { 42, "Medium Red Solid" },
            { 43, "Bright Red" },
            { 44, "Bright Red" },
            { 45, "Medium Garnet Red Poly" },
            { 46, "Brilliant Red Poly" },
            { 47, "Brilliant Red Poly 2" },
            { 48, "Alabaster Solid" },
            { 49, "Twilight Blue Poly" },
            { 50, "Torch Red" },
            { 51, "Green" },
            { 52, "Deep Jewel Green" },
            { 53, "Agate Green" },
            { 54, "Petrol Blue Green Poly" },
            { 55, "Hoods" },
            { 56, "Green" },
            { 57, "Dark Green Poly" },
            { 58, "Rio Red" },
            { 59, "Securicor Dark Green" },
            { 60, "Seafoam Poly" },
            { 61, "Pastel Alabaster Solid" },
            { 62, "Midnight Blue" },
            { 63, "Striking Blue" },
            { 64, "Saxony Blue Poly" },
            { 65, "Jasper Green Poly" },
            { 66, "Mariner Blue" },
            { 67, "Harbor Blue Poly" },
            { 68, "Diamond Blue Poly" },
            { 69, "Surf Blue" },
            { 70, "Nautical Blue Poly" },
            { 71, "Light Crystal Blue Poly" },
            { 72, "Medium Regatta Blue Poly" },
            { 73, "Spinnaker Blue Solid" },
            { 74, "Ultra Blue Poly" },
            { 75, "Bright Blue Poly" },
            { 76, "Nassau Blue Poly" },
            { 77, "Medium Sapphire Blue Poly" },
            { 78, "Steel Blue Poly" },
            { 79, "Light Sapphire Blue Poly" },
            { 80, "Malachite Poly" },
            { 81, "Medium Maui Blue Poly" },
            { 82, "Bright Blue Poly" },
            { 83, "Bright Blue Poly" },
            { 84, "Blue" },
            { 85, "Dark Sapphire Blue Poly" },
            { 86, "Light Sapphire Blue Poly" },
            { 87, "Medium Sapphire Blue Firemist" },
            { 88, "Twilight Blue Poly" },
            { 89, "Taxi Yellow" },
            { 90, "Race Yellow Solid" },
            { 91, "Pastel Alabaster" },
            { 92, "Oxford White Solid" },
            { 93, "Flax" },
            { 94, "Medium Flax" },
            { 95, "Pueblo Beige" },
            { 96, "Light Ivory" },
            { 97, "Smoke Silver Poly" },
            { 98, "Bisque Frost Poly" },
            { 99, "Classic Red" },
            { 100, "Vermilion Solid" },
            { 101, "Vermillion Solid" },
            { 102, "Biston Brown Poly" },
            { 103, "Light Beechwood Poly" },
            { 104, "Dark Beechwood Poly" },
            { 105, "Dark Sable Poly" },
            { 106, "Medium Beechwood Poly" },
            { 107, "Woodrose Poly" },
            { 108, "Sandalwood Frost Poly" },
            { 109, "Medium Sandalwood Poly" },
            { 110, "Copper Beige" },
            { 111, "Warm Grey Mica" },
            { 112, "White" },
            { 113, "Frost White" },
            { 114, "Honey Beige Poly" },
            { 115, "Seafoam Poly" },
            { 116, "Light Titanium Poly" },
            { 117, "Light Champagne Poly" },
            { 118, "Arctic Pearl" },
            { 119, "Light Driftwood Poly" },
            { 120, "White Diamond Pearl" },
            { 121, "Antelope Beige" },
            { 122, "Currant Blue Poly" },
            { 123, "Crystal Blue Poly" },
            { 124, "Temple Curtain Purple" },
            { 125, "Cherry Red" },
            { 126, "Securicor Dark Green" },
            { 127, "Taxi Yellow" },
            { 128, "Police Car Blue" },
            { 129, "Mellow Burgundy" },
            { 130, "Desert Taupe Poly" },
            { 131, "Lammy Orange" },
            { 132, "Lammy Yellow" },
            { 133, "Very White" }
        };

        private static void PickColors()
        {
            if (inMenu)
            {
                if (colorType == 4)
                {
                    colorTypeString = "Primary Color";
                    if (isLeftKeyDown)
                    {
                        if (pColor1 > 0)
                            pColor1 -= 1;
                        else
                            pColor1 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (pColor1 < 133)
                            pColor1 += 1;
                        else
                            pColor1 = 0;
                    }
                }
                else if (colorType == 3)
                {
                    colorTypeString = "Secondary Color";
                    if (isLeftKeyDown)
                    {
                        if (pColor2 > 0)
                            pColor2 -= 1;
                        else
                            pColor2 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (pColor2 < 133)
                            pColor2 += 1;
                        else
                            pColor2 = 0;
                    }
                }
                else if (colorType == 2)
                {
                    colorTypeString = "Pearlescent Color";
                    if (isLeftKeyDown)
                    {
                        if (sColor1 > 0)
                            sColor1 -= 1;
                        else
                            sColor1 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (sColor1 < 133)
                            sColor1 += 1;
                        else
                            sColor1 = 0;
                    }
                }
                else if (colorType == 1)
                {
                    colorTypeString = "Tertiary Color";
                    if (isLeftKeyDown)
                    {
                        if (sColor2 > 0)
                            sColor2 -= 1;
                        else
                            sColor2 = 133;
                    }
                    else if (isRightKeyDown)
                    {
                        if (sColor2 < 133)
                            sColor2 += 1;
                        else
                            sColor2 = 0;
                    }
                }

                CHANGE_CAR_COLOUR(pVeh, pColor1, pColor2);
                SET_EXTRA_CAR_COLOURS(pVeh, sColor1, sColor2);

                string colorName;
                if (colorType == 4)
                {
                    colorName = colorMap.ContainsKey(pColor1) ? colorMap[pColor1] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({pColor1})");
                }
                else if (colorType == 3)
                {
                    colorName = colorMap.ContainsKey(pColor2) ? colorMap[pColor2] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({pColor2})");
                }
                else if (colorType == 2)
                {
                    colorName = colorMap.ContainsKey(sColor1) ? colorMap[sColor1] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({sColor1})");
                }
                else
                {
                    colorName = colorMap.ContainsKey(sColor2) ? colorMap[sColor2] : "Unknown";
                    IVGame.ShowSubtitleMessage($"{colorTypeString}~n~{colorName} ({sColor2})");
                }
            }
        }


    }
}
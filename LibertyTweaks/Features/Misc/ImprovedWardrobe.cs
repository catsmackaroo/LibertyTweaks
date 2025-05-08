using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Linq;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using IVSDKDotNet.Enums;

// the main reason for this script's existence is so that the player can actually use addon clothing naturally. also because the vanilla system sucks...
// but how can I make it feel right? 
// Thinking of making it so the LibertyTweaks.save will keep track of each item of clothing the player has worn,
// and if it fits within the range of vanilla clothing, you can use it in wardrobe.. essentially unlocking it. otherwise, you can't access said clothing

// if the clothing is NOT within the range of vanilla, then it will already be in the wardrobe. maybe this would only apply through story progress?

// this logic could just apply to the models themselves, though. so if a mod has extra textures for specific clothing, you can wear it.

// default max models:
// upper body = 17
// lower body = 8
// feet = 7
// hair = 3


namespace LibertyTweaks
{
    public class PedComponents
    {
        public const int Torso = 1;
        public const int Legs = 2;
        public const int Feet = 5;
        public const int Head = 0;
        public const int Hair = 7;
    }
    internal class ImprovedWardrobe
    {
        // Configuration
        private static bool enable;
        private static bool firstFrame = true;
        private static List<(Vector3 Position, float Heading)> wardrobeLocations = new List<(Vector3, float)>();
        public static string Section { get; private set; }

        // Wardrobe state
        private static bool wardrobeActive;
        private static NativeCamera cam;
        private static int currentComponentIndex = 0;
        private static readonly int[] pedComponentsOrder =
        {
           PedComponents.Hair,
           PedComponents.Torso,
           PedComponents.Legs,
           PedComponents.Feet,
        };

        // HUD and radar state
        private const uint RadarOn = 1;
        private const uint RadarOff = 0;
        private const uint RadarBlipsOnly = 2;
        private static uint originalRadarMode = RadarOn;
        private static bool originalHud = true;

        // sounds
        private const string navRightTexture = "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_RIGHT";
        private const string navLeftTexture = "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_LEFT";
        private const string navClothing = "FRONTEND_MENU_MP_VIEW_VEHICLE";

        // Toggle delay
        private static DateTime lastToggleTime = DateTime.MinValue;
        private static readonly TimeSpan toggleDelay = TimeSpan.FromMilliseconds(250);

        // Misc
        private static bool messageShown = false;
        private static bool unlockAllClothing = false;
        private static bool unlockAllAddonClothing = false;
        public static bool enableClothingVans = false;
        public static bool enableClothingVansAlwaysBlips = false;
        
        public static void Init(SettingsFile settings, string section)
        {
            Section = section;
            enable = settings.GetBoolean(section, "Improved Wardrobe", false);
            unlockAllClothing = settings.GetBoolean(section, "Improved Wardrobe - Unlock All Clothing", false);
            unlockAllAddonClothing = settings.GetBoolean(section, "Improved Wardrobe - Unlock All Addon Clothing", false);
            enableClothingVans = settings.GetBoolean(section, "Improved Wardrobe - Enable Clothing Vans", false);
            enableClothingVansAlwaysBlips = settings.GetBoolean(section, "Improved Wardrobe - Clothing Van Show Blip Always", false);

            if (enable)
            {
                Main.Log("script initialized...");
                Main.Log($"Unlock All Clothing: {unlockAllClothing}");
                Main.Log($"Unlock All Addon Clothing: {unlockAllAddonClothing}");
                LoadWardrobeSpots(settings);
            }

            if (enableClothingVans)
            {
                ImprovedWardrobeClothingVan.Init();
            }
        }
        public static void IngameStartup()
        {
            firstFrame = true;
        }
        private static void InitializeFirstFrame()
        {
            ImprovedWardrobeClothingTracker.Load();
            ImprovedWardrobeClothingTracker.AddVanillaClothingToOwnedForTBoGT();

            if (enableClothingVansAlwaysBlips)
                ImprovedWardrobeClothingVan.InitializeFarBlip();

            firstFrame = false;
        }
        private static void LoadWardrobeSpots(SettingsFile settings)
        {
            string[] lines = System.IO.File.ReadAllLines(settings.FilePath);
            bool inWardrobeSection = false;

            foreach (var line in lines)
            {
                if (line.Trim().Equals("[Wardrobe Locations]", StringComparison.OrdinalIgnoreCase))
                {
                    inWardrobeSection = true;
                    continue;
                }

                if (inWardrobeSection)
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                        break;

                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith(";"))
                        continue;

                    var parts = line.Split(',');
                    if (parts.Length == 4 &&
                        float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float heading))
                    {
                        wardrobeLocations.Add((new Vector3(x, y, z), heading));
                        Main.Log($"Wardrobe location added: {x}, {y}, {z}, heading: {heading}");
                    }
                }
            }
        }
        public static void Tick()
        {
            if (!enable) return;

            if (firstFrame)
                InitializeFirstFrame();

            // Terminate conflicting scripts
            if (NativeGame.IsScriptRunning("ambwardrobe"))
                NativeGame.TerminateScriptsWithThisName("ambwardrobe");

            // Tracks owned clothes for player
            ImprovedWardrobeClothingTracker.Tick();
            ImprovedWardrobeClothingTracker.Save();

            // Handle Wardrobe Van
            if (enableClothingVans)
                ImprovedWardrobeClothingVan.Tick();

            // Check if the player is near a wardrobe
            if (IsPlayerNearWardrobe(out var nearestWardrobe))
            {
                if (!messageShown)
                    ShowWardrobeMessage();

                if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && CanToggleWardrobe())
                {
                    _TASK_STAND_STILL(Main.PlayerPed.GetHandle(), 2000);

                    CommonHelpers.HandleScreenFade(500, true, () =>
                    {
                        ToggleWardrobe(nearestWardrobe);
                    });
                }
            }

            if (wardrobeActive)
            {
                HandleWardrobe();
                HandleCameraZoom();
            }
        }
        private static bool CanToggleWardrobe()
        {
            if (DateTime.UtcNow - lastToggleTime >= toggleDelay)
            {
                lastToggleTime = DateTime.UtcNow;
                return true;
            }
            return false;
        }
        private static bool IsPlayerNearWardrobe(out (Vector3 Position, float Heading) nearestWardrobe)
        {
            nearestWardrobe = wardrobeLocations
                .FirstOrDefault(location => Vector3.Distance(Main.PlayerPos, location.Position) < 3f);

            if (nearestWardrobe.Position != default)
            {
                return true;
            }

            messageShown = false;
            return false;
        }
        private static void ShowWardrobeMessage()
        {
            Main.ShowMessage("Press ~INPUT_PICKUP~ to access your wardrobe.");
            messageShown = true;
        }
        private static void ToggleWardrobe((Vector3 Position, float Heading) wardrobe)
        {
            wardrobeActive = !wardrobeActive;
            currentComponentIndex = PedComponents.Torso;

            if (wardrobeActive)
            {
                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_ON");
                ActivateWardrobe(wardrobe);
                DisableHUD();
            }
            else
            {
                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_BACK");
                ResetWardrobe();
                RestoreHUD();
            }
        }
        private static void DisableHUD()
        {
            originalRadarMode = IVMenuManager.RadarMode;
            originalHud = IVMenuManager.HudOn;
            IVMenuManager.RadarMode = RadarOff;
            IVMenuManager.HudOn = false;
        }
        private static void RestoreHUD()
        {
            IVMenuManager.RadarMode = originalRadarMode;
            IVMenuManager.HudOn = originalHud;
        }
        private static void ActivateWardrobe((Vector3 Position, float Heading) wardrobe)
        {
            if (cam == null)
            {
                cam = NativeCamera.Create();
                cam.Activate();

                SET_CHAR_HEADING(Main.PlayerPed.GetHandle(), wardrobe.Heading);
                Main.PlayerPed.Teleport(wardrobe.Position, false, true);

                // Position the camera
                GET_OFFSET_FROM_CHAR_IN_WORLD_COORDS(Main.PlayerPed.GetHandle(), new Vector3(0f, 5f, 0f), out Vector3 offset);
                cam.SetTargetPed(Main.PlayerPed.GetHandle());
                cam.PointAtPed(Main.PlayerPed.GetHandle());
                cam.FOV *= 0.5f;
                cam.Position = new Vector3(offset.X, offset.Y, offset.Z + 1.25f);
            }
        }
        private static float? originalFOV = null;
        private static DateTime zoomStartTime;
        private static bool isZoomingIn = false;
        private static readonly TimeSpan zoomDuration = TimeSpan.FromMilliseconds(300);
        private static void HandleCameraZoom()
        {
            if (cam == null || !wardrobeActive) return;

            bool isAttackKeyPressed = NativeControls.IsGameKeyPressed(0, GameKey.Attack);

            if (isAttackKeyPressed)
            {
                if (originalFOV == null || !isZoomingIn)
                {
                    originalFOV = cam.FOV;
                    zoomStartTime = DateTime.UtcNow;
                    isZoomingIn = true;
                }

                float t = (float)(DateTime.UtcNow - zoomStartTime).TotalMilliseconds / (float)zoomDuration.TotalMilliseconds;
                t = CommonHelpers.Clamp(t, 0f, 1f);

                float targetFOV = originalFOV.Value / 1.5f;
                cam.FOV = CommonHelpers.SmoothStep(originalFOV.Value, targetFOV, t);
            }
            else
            {
                if (originalFOV != null && isZoomingIn)
                {
                    zoomStartTime = DateTime.UtcNow;
                    isZoomingIn = false;
                }

                if (originalFOV != null)
                {
                    float t = (float)(DateTime.UtcNow - zoomStartTime).TotalMilliseconds / (float)zoomDuration.TotalMilliseconds;
                    t = CommonHelpers.Clamp(t, 0f, 1f);

                    float targetFOV = originalFOV.Value;
                    cam.FOV = CommonHelpers.SmoothStep(originalFOV.Value / 1.5f, targetFOV, t);

                    if (t >= 1f)
                    {
                        originalFOV = null;
                    }
                }
            }
        }
        private static void ResetWardrobe()
        {
            if (cam != null)
            {
                cam.Deactivate();
                cam.Delete();
                cam = null;
            }
            CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
            CLEAR_HELP();
            wardrobeActive = false;
            SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_UNARMED, true);
        }
        private static void HandleWardrobe()
        {
            HandleComponentNavigation();

            _TASK_STAND_STILL(Main.PlayerPed.GetHandle(), 2000);

            switch (pedComponentsOrder[currentComponentIndex])
            {
                case PedComponents.Hair:
                    HandleHair();
                    break;
                case PedComponents.Torso:
                    HandleTorso();
                    break;
                case PedComponents.Legs:
                    HandleLegs();
                    break;
                case PedComponents.Feet:
                    HandleFeet();
                    break;
            }
        }
        private static void HandleComponentNavigation()
        {
            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavUp))
            {
                lastNavTime = DateTime.UtcNow;
                currentComponentIndex = (currentComponentIndex - 1 + pedComponentsOrder.Length) % pedComponentsOrder.Length;
                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_MT");
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavDown))
            {
                lastNavTime = DateTime.UtcNow;
                currentComponentIndex = (currentComponentIndex + 1) % pedComponentsOrder.Length;
                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_MT");
            }
        }
        private static DateTime lastNavTime = DateTime.MinValue;
        private static readonly TimeSpan navDelay = TimeSpan.FromMilliseconds(200);
        private static void HandleTorso()
        {
            var currentTorso = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso);
            var maxTorso = GET_NUMBER_OF_CHAR_DRAWABLE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Torso);
            var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso);
            var maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso) - 1;

            const int defaultMaxTorso = 17; // Default maximum for upper body

            var message = $"Upper Body~n~Type: {currentTorso + 1}/{maxTorso}, Style: {currentTexture + 1}/{maxTexture + 1}~n~Press ~PAD_DPAD_LEFT~ or ~PAD_DPAD_RIGHT~ to change clothing.~n~Press ~INPUT_PICKUP~ to accept.";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP_FOREVER_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "PLACEHOLDER_1");

            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the next owned torso
                do
                {
                    currentTexture++;
                    if (currentTexture > maxTexture)
                    {
                        currentTorso = (currentTorso + 1) % maxTorso;
                        maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso) - 1;
                        currentTexture = 0;
                    }
                }
                while (!unlockAllClothing &&
                       !(unlockAllAddonClothing && currentTorso >= defaultMaxTorso) && // Allow all addon clothing if enabled
                       !ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Torso, currentTorso, currentTexture));

                PLAY_SOUND_FRONTEND(-1, navRightTexture);
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso, currentTexture);
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the previous owned torso
                do
                {
                    if (currentTexture == 0)
                    {
                        currentTorso = (currentTorso - 1 + maxTorso) % maxTorso;
                        maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso) - 1;
                        currentTexture = maxTexture;
                    }
                    else
                    {
                        currentTexture--;
                    }
                }
                while (!unlockAllClothing &&
                       !(unlockAllAddonClothing && currentTorso >= defaultMaxTorso) && // Allow all addon clothing if enabled
                       !ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Torso, currentTorso, currentTexture));

                PLAY_SOUND_FRONTEND(-1, navLeftTexture);
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso, currentTexture);
            }
        }
        private static void HandleLegs()
        {
            var currentLegs = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs);
            var maxLegs = GET_NUMBER_OF_CHAR_DRAWABLE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Legs);
            var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs);
            var maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs) - 1;

            const int defaultMaxLegs = 8; // Default maximum for lower body

            var message = $"Lower Body~n~Legs: {currentLegs + 1}/{maxLegs}, Style: {currentTexture + 1}/{maxTexture + 1}~n~Press ~PAD_DPAD_LEFT~ or ~PAD_DPAD_RIGHT~ to change clothing.~n~Press ~INPUT_PICKUP~ to accept.";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP_FOREVER_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "PLACEHOLDER_1");

            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the next owned legs
                do
                {
                    currentTexture++;
                    if (currentTexture > maxTexture)
                    {
                        currentLegs = (currentLegs + 1) % maxLegs;
                        maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs) - 1;
                        currentTexture = 0;
                    }
                }
                while (!unlockAllClothing &&
                       !(unlockAllAddonClothing && currentLegs >= defaultMaxLegs) && // Allow all addon clothing if enabled
                       !ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Legs, currentLegs, currentTexture));

                PLAY_SOUND_FRONTEND(-1, navRightTexture);
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs, currentTexture);
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the previous owned legs
                do
                {
                    if (currentTexture == 0)
                    {
                        currentLegs = (currentLegs - 1 + maxLegs) % maxLegs;
                        maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs) - 1;
                        currentTexture = maxTexture;
                    }
                    else
                    {
                        currentTexture--;
                    }
                }
                while (!unlockAllClothing &&
                       !(unlockAllAddonClothing && currentLegs >= defaultMaxLegs) && // Allow all addon clothing if enabled
                       !ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Legs, currentLegs, currentTexture));

                PLAY_SOUND_FRONTEND(-1, navLeftTexture);
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs, currentTexture);
            }
        }
        private static void HandleFeet()
        {
            var currentFeet = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet);
            var maxFeet = GET_NUMBER_OF_CHAR_DRAWABLE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Feet);
            var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet);
            var maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet) - 1;

            const int defaultMaxFeet = 7; // Default maximum for feet

            var message = $"Footwear~n~Feet: {currentFeet + 1}/{maxFeet}, Style: {currentTexture + 1}/{maxTexture + 1}~n~Press ~PAD_DPAD_LEFT~ or ~PAD_DPAD_RIGHT~ to change clothing.~n~Press ~INPUT_PICKUP~ to accept.";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP_FOREVER_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "PLACEHOLDER_1");

            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the next owned feet
                do
                {
                    currentTexture++;
                    if (currentTexture > maxTexture)
                    {
                        currentFeet = (currentFeet + 1) % maxFeet;
                        maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet) - 1;
                        currentTexture = 0;
                    }
                }
                while (!unlockAllClothing &&
                       !(unlockAllAddonClothing && currentFeet >= defaultMaxFeet) && // Allow all addon clothing if enabled
                       !ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Feet, currentFeet, currentTexture));

                PLAY_SOUND_FRONTEND(-1, navRightTexture);
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet, currentTexture);
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the previous owned feet
                do
                {
                    if (currentTexture == 0)
                    {
                        currentFeet = (currentFeet - 1 + maxFeet) % maxFeet;
                        maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet) - 1;
                        currentTexture = maxTexture;
                    }
                    else
                    {
                        currentTexture--;
                    }
                }
                while (!unlockAllClothing &&
                       !(unlockAllAddonClothing && currentFeet >= defaultMaxFeet) && // Allow all addon clothing if enabled
                       !ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Feet, currentFeet, currentTexture));

                PLAY_SOUND_FRONTEND(-1, navLeftTexture);
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet, currentTexture);
            }
        }
        private static void HandleHair()
        {
            var currentHair = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Hair);
            var maxHair = GET_NUMBER_OF_CHAR_DRAWABLE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Hair);
            var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Hair);
            var maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Hair, currentHair) - 1;

            var message = $"Hairstyle~n~Hair: {currentHair + 1}/{maxHair}, Style: {currentTexture + 1}/{maxTexture + 1}~n~Press ~PAD_DPAD_LEFT~ or ~PAD_DPAD_RIGHT~ to change hair.~n~Press ~INPUT_PICKUP~ to accept.";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP_FOREVER_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "PLACEHOLDER_1");

            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight))
            {
                lastNavTime = DateTime.UtcNow;

                if (currentTexture + 1 <= maxTexture)
                {
                    PLAY_SOUND_FRONTEND(-1, navRightTexture);
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Hair, currentHair, currentTexture + 1);
                }
                else
                {
                    PLAY_SOUND_FRONTEND(-1, navClothing);
                    var newHair = (currentHair + 1) % maxHair;
                    var newMaxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Hair, newHair) - 1;
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Hair, newHair, 0);
                }
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft))
            {
                lastNavTime = DateTime.UtcNow;

                if (currentTexture - 1 >= 0 && currentTexture - 1 <= maxTexture)
                {
                    PLAY_SOUND_FRONTEND(-1, navLeftTexture);
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Hair, currentHair, currentTexture - 1);
                }
                else
                {
                    PLAY_SOUND_FRONTEND(-1, navClothing);
                    var newHair = (currentHair - 1 + maxHair) % maxHair;
                    var newMaxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Hair, newHair) - 1;
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Hair, newHair, newMaxTexture);
                }
            }
        }
        private static void HandleHats()
        {
            // propType = 0 for hats I believe
            SET_CHAR_PROP_INDEX(Main.PlayerPed.GetHandle(), 0, 0);
            SET_CHAR_PROP_INDEX_TEXTURE(Main.PlayerPed.GetHandle(), 0, 0, 0);
        }
        private static void HandleGlasses()
        {
            // propType = 1 for glasses I believe
            SET_CHAR_PROP_INDEX(Main.PlayerPed.GetHandle(), 1, 0);
            SET_CHAR_PROP_INDEX_TEXTURE(Main.PlayerPed.GetHandle(), 1, 0, 0);
        }

        private static void HandleMisc()
        {
            // suse
        }
        
        private static void HandleMisc2()
        {
            // suse
        }
    }
}

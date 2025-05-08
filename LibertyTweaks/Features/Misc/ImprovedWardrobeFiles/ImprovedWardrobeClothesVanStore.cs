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

namespace LibertyTweaks
{
    internal class ImprovedWardrobeClothesVanStore
    {
        // vanilla clothing
        //torso 0 | textures max 0-2
        //torso 1 | textures max 0-2
        //torso 2 | textures max 0-2
        //torso 3 | textures max 0-2
        //torso 4 | textures max 0-2
        //torso 5 | textures max 0-2
        //torso 6 | textures max 0
        //torso 7 | textures max 0
        //torso 8 | textures max 0
        //torso 9 | textures max 0-2
        //torso 10 | textures max 0-2
        //torso 11 | textures max 0-2
        //torso 12 | textures max 0-3
        //torso 13 | textures max 0-2
        //torso 14 | textures max 0-2
        //torso 15 | textures max 0-3
        //torso 16 | textures max 0-1

        private static readonly Dictionary<uint, uint> VanillaClothingLimitsTorsoIV = new Dictionary<uint, uint>
{
    { 0, 2 }, 
    { 1, 2 }, 
    { 2, 2 },  
    { 3, 2 },  
    { 4, 2 }, 
    { 5, 2 }, 
    { 6, 0 },  
    { 7, 0 },  
    { 8, 0 },  
    { 9, 2 },  
    { 10, 2 }, 
    { 11, 2 }, 
    { 12, 3 }, 
    { 13, 2 }, 
    { 14, 2 }, 
    { 15, 3 },
    { 16, 1 } 
};
        private static readonly Dictionary<uint, uint> VanillaClothingLimitsLegsIV = new Dictionary<uint, uint>
{
    { 0, 3 },
    { 1, 0 },  
    { 2, 4 }, 
    { 3, 0 },
    { 4, 4 }, 
    { 5, 2 },  
    { 6, 2 }, 
    { 7, 3 }
};
        private static readonly Dictionary<uint, uint> VanillaClothingLimitsFeetIV = new Dictionary<uint, uint>
{
    { 0, 1 },
    { 1, 3 },
    { 2, 2 },
    { 3, 1 },
    { 4, 2 },
    { 5, 2 },
    { 6, 2 }
};

        public static readonly Dictionary<uint, uint> VanillaClothingLimitsTorsoTBoGT = new Dictionary<uint, uint>
{
    { 0, 0 },
    { 1, 0 },
    { 2, 1 },
    { 3, 1 },
    { 4, 1 }
};
        public static readonly Dictionary<uint, uint> VanillaClothingLimitsLegsTBoGT = new Dictionary<uint, uint>
{
    { 0, 0 },
    { 1, 1 }
};
        public static readonly Dictionary<uint, uint> VanillaClothingLimitsFeetTBoGT = new Dictionary<uint, uint>
{
    { 0, 0 },
    { 1, 0 }
};

        private static readonly Dictionary<uint, uint> VanillaClothingLimitsTorsoTLAD = new Dictionary<uint, uint>
{
    { 0, 1 }
};
        private static readonly Dictionary<uint, uint> VanillaClothingLimitsLegsTLAD = new Dictionary<uint, uint>
{
    { 0, 0 }
};
        private static readonly Dictionary<uint, uint> VanillaClothingLimitsFeetTLAD = new Dictionary<uint, uint>
{
    { 0, 0 }
};

        // Wardrobe state
        public static bool storeActive;
        private static NativeCamera cam;
        private static int currentComponentIndex = 0;
        private static readonly int[] pedComponentsOrder =
        {
           PedComponents.Torso,
           PedComponents.Legs,
           PedComponents.Feet,
        };

        private static Dictionary<int, (int Drawable, int Texture)> lastKnownOwnedClothing = new Dictionary<int, (int Drawable, int Texture)>();

        // HUD and radar state
        private const uint RadarOn = 1;
        private const uint RadarOff = 0;
        private static uint originalRadarMode = RadarOn;
        private static bool originalHud = true;

        // Toggle delay
        private static DateTime lastToggleTime = DateTime.MinValue;
        private static readonly TimeSpan toggleDelay = TimeSpan.FromMilliseconds(250);

        // Pricing
        public static int price = 400;

        // Previous clothing state
        private static Dictionary<int, (int Drawable, int Texture)> previousClothing = new Dictionary<int, (int, int)>();
        private static Dictionary<uint, (uint Drawable, uint Texture)> currentSelectedClothing = new Dictionary<uint, (uint Drawable, uint Texture)>();
        private static bool hasPurchasedClothing = false;
        public static void HandleStore()
        {
            // If the player exits the store using INPUT_ACTION, restore previous clothing if no purchase was made
            if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && storeActive && CanToggleStore())
            {
                CommonHelpers.HandleScreenFade(500, true, () =>
                {
                    if (!hasPurchasedClothing)
                    {
                        RestoreLastKnownOwnedClothing();
                    }
                    ResetStore();
                });
                
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && CanToggleStore())
            {
                CommonHelpers.HandleScreenFade(500, true, () =>
                {
                    ToggleStore();
                });
            }

            if (storeActive)
            {
                ImprovedWardrobeClothingVan.vanSpawnTime = DateTime.UtcNow;
                SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_UNARMED, true);
                HandleStoreComponent();
                HandleCameraZoom();
            }
        }
        private static bool CanToggleStore()
        {
            if (DateTime.UtcNow - lastToggleTime >= toggleDelay)
            {
                lastToggleTime = DateTime.UtcNow;
                return true;
            }
            return false;
        }
        public static void ToggleStore()
        {
            storeActive = !storeActive;

            if (storeActive)
            {
                // Save the last known owned clothes before entering the store
                SaveLastKnownOwnedClothing();

                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_ON");
                DisableHUD();
                SaveCurrentClothing();
                ActivateStore();
            }
            else
            {
                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_BACK");
                RestoreHUD();
                ResetStore();
            }
        }
        private static void SaveLastKnownOwnedClothing()
        {
            lastKnownOwnedClothing.Clear();

            foreach (var component in pedComponentsOrder)
            {
                var drawable = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), component);
                var texture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), (uint)component);

                // Only save clothes that the player owns
                if (ImprovedWardrobeClothingTracker.PlayerOwnsClothing((uint)component, (uint)drawable, (uint)texture))
                {
                    lastKnownOwnedClothing[component] = ((int)drawable, (int)texture);
                }
            }
        }
        private static void DisableHUD()
        {
            IVMenuManager.RadarMode = RadarOff;
            IVMenuManager.HudOn = false;
        }
        private static void RestoreHUD()
        {
            IVMenuManager.RadarMode = originalRadarMode;
            IVMenuManager.HudOn = originalHud;
        }
        private static void ActivateStore()
        {
            if (cam == null)
            {
                cam = NativeCamera.Create();
                cam.Activate();

                // Set player location
                SET_CHAR_HEADING(Main.PlayerPed.GetHandle(), ImprovedWardrobeClothingVan.selectedVanLocation.Heading - 180);
                if (ImprovedWardrobeClothingVan.purchaseLocation != Vector3.Zero)
                    Main.PlayerPed.Teleport(ImprovedWardrobeClothingVan.purchaseLocation, false, true);

                // Position the camera
                GET_OFFSET_FROM_CHAR_IN_WORLD_COORDS(Main.PlayerPed.GetHandle(), new Vector3(0f, 5f, 0.0f), out Vector3 offset);
                cam.Position = offset;
                cam.SetTargetPed(Main.PlayerPed.GetHandle());
                cam.PointAtPed(Main.PlayerPed.GetHandle());
                cam.FOV *= 0.4f;

                Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(0.5), "Main", () =>
                {
                    cam.Unpoint();
                });
            }
        }
        private static float? originalFOV = null;
        private static DateTime zoomStartTime;
        private static bool isZoomingIn = false;
        private static readonly TimeSpan zoomDuration = TimeSpan.FromMilliseconds(300);
        private static void HandleCameraZoom()
        {
            if (cam == null || !storeActive) return;

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
        private static void ResetStore()
        {
            // Disable storeActive early to prevent the tracker from adding clothing during the reset process
            storeActive = false;

            if (cam != null)
            {
                cam.Deactivate();
                cam.Delete();
                cam = null;
            }

            // Apply all purchased or selected clothing
            if (hasPurchasedClothing)
            {
                ApplyAllPurchasedClothing();
            }
            else
            {
                ApplyCurrentSelectedClothing();
            }

            // Reset the purchase flag for the next store session
            hasPurchasedClothing = false;

            CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerPed.GetHandle());
            CLEAR_HELP();
            IVMenuManager.RadarMode = originalRadarMode;
            IVMenuManager.HudOn = originalHud;
            SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_UNARMED, true);
        }
        private static void HandleStoreComponent()
        {
            HandleComponentNavigation();

            switch (pedComponentsOrder[currentComponentIndex])
            {
                case PedComponents.Torso:
                    HandleTorsoPurchase();
                    break;
                case PedComponents.Legs:
                    HandleLegsPurchase();
                    break;
                case PedComponents.Feet:
                    HandleFeetPurchase();
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
        private static void SaveCurrentClothing()
        {
            previousClothing.Clear();
            foreach (var component in pedComponentsOrder)
            {
                var drawable = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), component);
                var texture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), (uint)component);
                previousClothing[component] = ((int)drawable, (int)texture);
            }
        }
        private static void RestoreLastKnownOwnedClothing()
        {
            foreach (var component in pedComponentsOrder)
            {
                if (lastKnownOwnedClothing.TryGetValue(component, out var clothing))
                {
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), (uint)component, (uint)clothing.Drawable, (uint)clothing.Texture);
                }
            }

            Main.Log("Restored last known owned clothing.");
        }
        private static bool IsVanillaClothing(int component, uint drawable, uint texture)
        {
            Dictionary<uint, uint> vanillaLimits;

            // Determine the correct vanilla limits dictionary based on the component and episode
            if (component == PedComponents.Torso)
            {
                if (Main.Episode == (uint)Episode.IV)
                    vanillaLimits = VanillaClothingLimitsTorsoIV;
                else if (Main.Episode == (uint)Episode.TBoGT)
                    vanillaLimits = VanillaClothingLimitsTorsoTBoGT;
                else 
                    vanillaLimits = VanillaClothingLimitsTorsoTLAD;
            }
            else if (component == PedComponents.Legs)
            {
                if (Main.Episode == (uint)Episode.IV)
                    vanillaLimits = VanillaClothingLimitsLegsIV;
                else if (Main.Episode == (uint)Episode.TBoGT)
                    vanillaLimits = VanillaClothingLimitsLegsTBoGT;
                else 
                    vanillaLimits = VanillaClothingLimitsLegsTLAD;
            }
            else if (component == PedComponents.Feet)
            {
                if (Main.Episode == (uint)Episode.IV)
                    vanillaLimits = VanillaClothingLimitsFeetIV;
                else if (Main.Episode == (uint)Episode.TBoGT)
                    vanillaLimits = VanillaClothingLimitsFeetTBoGT;
                else 
                    vanillaLimits = VanillaClothingLimitsFeetTLAD;
            }
            else
            {
                // If the component is not Torso, Legs, or Feet, return false (not vanilla)
                return false;
            }

            // Check if the drawable and texture combination is within vanilla limits
            return vanillaLimits.TryGetValue(drawable, out uint maxTexture) && texture <= maxTexture;
        }
        private static void HandleTorsoPurchase()
        {
            var currentTorso = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso);
            var maxTorso = GET_NUMBER_OF_CHAR_DRAWABLE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Torso);
            var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso);
            var maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso) - 1;

            var ownedMessage = "";
            var purchaseOrWear = "~n~Press ~INPUT_SPRINT~ to purchase.";
            var isOwned = false;

            // Determine if the current clothing is vanilla or addon based on the episode
            bool isVanilla;
            if (Main.Episode == (uint)Episode.IV)
            {
                isVanilla = VanillaClothingLimitsTorsoIV.TryGetValue((uint)currentTorso, out uint vanillaMaxTexture) && currentTexture <= vanillaMaxTexture;
            }
            else if (Main.Episode == (uint)Episode.TBoGT)
            {
                isVanilla = VanillaClothingLimitsTorsoTBoGT.TryGetValue((uint)currentTorso, out uint vanillaMaxTexture) && currentTexture <= vanillaMaxTexture;
            }
            else // Assume TLAD
            {
                isVanilla = VanillaClothingLimitsTorsoTLAD.TryGetValue((uint)currentTorso, out uint vanillaMaxTexture) && currentTexture <= vanillaMaxTexture;
            }

            // Check if the clothing is owned
            if (ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Torso, (uint)currentTorso, (uint)currentTexture))
            {
                isOwned = true;
                ownedMessage = "~W~ - ~g~Owned~s~";
                purchaseOrWear = ""; // Remove purchase prompt if owned
            }

            var message = $"Upper Body {ownedMessage}~n~Type: {currentTorso + 1}/{maxTorso}, Style: {currentTexture + 1}/{maxTexture + 1}~n~Press ~PAD_DPAD_LEFT~ or ~PAD_DPAD_RIGHT~ to browse clothing.~n~Press ~INPUT_PICKUP~ to exit.{purchaseOrWear}";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP_FOREVER_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "PLACEHOLDER_1");

            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight))
            {
                lastNavTime = DateTime.UtcNow;

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
                while (IsVanillaClothing(PedComponents.Torso ,currentTorso, currentTexture)); // Skip vanilla clothing

                PLAY_SOUND_FRONTEND(-1, "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_RIGHT");
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso, currentTexture);
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft))
            {
                lastNavTime = DateTime.UtcNow;

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
                while (IsVanillaClothing(PedComponents.Torso, currentTorso, currentTexture)); // Skip vanilla clothing

                PLAY_SOUND_FRONTEND(-1, "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_LEFT");
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Torso, currentTorso, currentTexture);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.Sprint))
            {
                if (!isOwned)
                {
                    lastNavTime = DateTime.UtcNow;
                    PurchaseClothing();
                }
            }

            HandleTorsoAnims();
        }
        private static void HandleTorsoAnims()
        {
            var isAnimPlaying = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "clothing", "examine shirt");
            REQUEST_ANIMS("clothing");
            if (!isAnimPlaying) 
                _TASK_PLAY_ANIM_NON_INTERRUPTABLE(Main.PlayerPed.GetHandle(),"examine shirt", "clothing", 8, 0, 0, 0, 0, -1);
        }
        private static void HandleLegsPurchase()
        {
            var currentLegs = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs);
            var maxLegs = GET_NUMBER_OF_CHAR_DRAWABLE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Legs);
            var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs);
            var maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs) - 1;

            var ownedMessage = "";
            var purchaseOrWear = "~n~Press ~INPUT_SPRINT~ to purchase.";
            var isOwned = false;

            // Determine if the current clothing is vanilla or addon based on the episode
            bool isVanilla = IsVanillaClothing(PedComponents.Legs, (uint)currentLegs, (uint)currentTexture);

            // Check if the clothing is owned
            if (ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Legs, (uint)currentLegs, (uint)currentTexture))
            {
                isOwned = true;
                ownedMessage = "~W~ - ~g~Owned~s~";
                purchaseOrWear = ""; // Remove purchase prompt if owned
            }

            var message = $"Lower Body {ownedMessage}~n~Type: {currentLegs + 1}/{maxLegs}, Style: {currentTexture + 1}/{maxTexture + 1}~n~Press ~PAD_DPAD_LEFT~ or ~PAD_DPAD_RIGHT~ to browse clothing.~n~Press ~INPUT_PICKUP~ to exit.{purchaseOrWear}";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP_FOREVER_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "PLACEHOLDER_1");

            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the next addon legs
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
                while (IsVanillaClothing(PedComponents.Legs, (uint)currentLegs, (uint)currentTexture)); // Skip vanilla clothing

                PLAY_SOUND_FRONTEND(-1, "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_RIGHT");
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs, currentTexture);
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the previous addon legs
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
                while (IsVanillaClothing(PedComponents.Legs, (uint)currentLegs, (uint)currentTexture)); // Skip vanilla clothing

                PLAY_SOUND_FRONTEND(-1, "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_LEFT");
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Legs, currentLegs, currentTexture);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.Sprint))
            {
                if (!isOwned)
                {
                    lastNavTime = DateTime.UtcNow;
                    PurchaseClothing();
                }
            }

            HandleLegsAnims();
        }
        private static void HandleLegsAnims()
        {
            var isAnimPlaying = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "clothing", "examine legs");
            REQUEST_ANIMS("clothing");
            if (!isAnimPlaying)
                _TASK_PLAY_ANIM_NON_INTERRUPTABLE(Main.PlayerPed.GetHandle(), "examine legs", "clothing", 8, 0, 0, 0, 0, -1);
        }
        private static void HandleFeetPurchase()
        {
            var currentFeet = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet);
            var maxFeet = GET_NUMBER_OF_CHAR_DRAWABLE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Feet);
            var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet);
            var maxTexture = GET_NUMBER_OF_CHAR_TEXTURE_VARIATIONS(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet) - 1;

            var ownedMessage = "";
            var purchaseOrWear = "~n~Press ~INPUT_SPRINT~ to purchase.";
            var isOwned = false;

            // Determine if the current clothing is vanilla or addon based on the episode
            bool isVanilla = IsVanillaClothing(PedComponents.Feet, (uint)currentFeet, (uint)currentTexture);

            // Check if the clothing is owned
            if (ImprovedWardrobeClothingTracker.PlayerOwnsClothing(PedComponents.Feet, (uint)currentFeet, (uint)currentTexture))
            {
                isOwned = true;
                ownedMessage = "~W~ - ~g~Owned~s~";
                purchaseOrWear = ""; // Remove purchase prompt if owned
            }

            var message = $"Footwear {ownedMessage}~n~Type: {currentFeet + 1}/{maxFeet}, Style: {currentTexture + 1}/{maxTexture + 1}~n~Press ~PAD_DPAD_LEFT~ or ~PAD_DPAD_RIGHT~ to browse clothing.~n~Press ~INPUT_PICKUP~ to exit.{purchaseOrWear}";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP_FOREVER_WITH_STRING_NO_SOUND("PLACEHOLDER_1", "PLACEHOLDER_1");

            if (DateTime.UtcNow - lastNavTime < navDelay)
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.NavRight))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the next addon feet
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
                while (IsVanillaClothing(PedComponents.Feet, (uint)currentFeet, (uint)currentTexture)); // Skip vanilla clothing

                PLAY_SOUND_FRONTEND(-1, "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_RIGHT");
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet, currentTexture);
            }
            else if (NativeControls.IsGameKeyPressed(0, GameKey.NavLeft))
            {
                lastNavTime = DateTime.UtcNow;

                // Navigate to the previous addon feet
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
                while (IsVanillaClothing(PedComponents.Feet, (uint)currentFeet, (uint)currentTexture)); // Skip vanilla clothing

                PLAY_SOUND_FRONTEND(-1, "GENERAL_FRONTEND_MENU_SCROLL_ALT_1_LEFT");
                SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), PedComponents.Feet, currentFeet, currentTexture);
            }

            if (NativeControls.IsGameKeyPressed(0, GameKey.Sprint))
            {
                if (!isOwned)
                {
                    lastNavTime = DateTime.UtcNow;
                    PurchaseClothing();
                }
            }

            HandleFeetAnims();
        }
        private static void HandleFeetAnims()
        {
            var isAnimPlaying = IS_CHAR_PLAYING_ANIM(Main.PlayerPed.GetHandle(), "clothing", "examine shoes");
            REQUEST_ANIMS("clothing");
            if (!isAnimPlaying)
                _TASK_PLAY_ANIM_NON_INTERRUPTABLE(Main.PlayerPed.GetHandle(), "examine shoes", "clothing", 8, 0, 0, 0, 0, -1);
        }
        private static void PurchaseClothing()
        {
            int totalCost = 0;
            int newComponentCount = 0;

            foreach (var component in pedComponentsOrder)
            {
                var currentDrawable = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), component);
                var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), (uint)component);

                // Check if the player already owns the clothing
                if (ImprovedWardrobeClothingTracker.PlayerOwnsClothing((uint)component, (uint)currentDrawable, (uint)currentTexture))
                {
                    continue; // Skip owned clothing
                }

                // Add the cost for this new clothing item
                totalCost += price;
                newComponentCount++;

                // Add the purchased clothing to the tracker
                ImprovedWardrobeClothingTracker.AddClothingWithTexture((uint)component, (uint)currentDrawable, (uint)currentTexture);

                // Update the current selected clothing
                currentSelectedClothing[(uint)component] = ((uint)currentDrawable, (uint)currentTexture);
            }

            // If no new clothing is found, show a message and return
            if (newComponentCount == 0)
            {
                IVGame.ShowSubtitleMessage("You already own all the clothing items.");
                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_ERROR");
                return;
            }

            // Check if the player has enough money
            if (Main.PlayerPed.PlayerInfo.GetMoney() < totalCost)
            {
                IVGame.ShowSubtitleMessage("You don't have enough money to purchase all items.");
                PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_ERROR");
                return;
            }

            // Deduct the money and finalize the purchase
            Main.PlayerPed.PlayerInfo.RemoveMoney(totalCost);
            PLAY_SOUND_FRONTEND(-1, "WARDROBE_BUY");
            IVGame.ShowSubtitleMessage($"Purchased all new clothing for $~g~{totalCost}~s~");
            hasPurchasedClothing = true;

            if (ImprovedWardrobeClothingVan.drivePed != null)
            {
                ImprovedWardrobeClothingVan.drivePed.SayAmbientSpeech("GENERIC_YES_PLEASE");
            }

            Main.Log($"Purchased {newComponentCount} new clothing items for a total of ${totalCost}.");
        }
        private static void ApplyAllPurchasedClothing()
        {
            foreach (var component in pedComponentsOrder)
            {
                // Get the current drawable and texture for the component
                var currentDrawable = GET_CHAR_DRAWABLE_VARIATION(Main.PlayerPed.GetHandle(), component);
                var currentTexture = GET_CHAR_TEXTURE_VARIATION(Main.PlayerPed.GetHandle(), (uint)component);

                // Check if the player owns the clothing and apply it
                if (ImprovedWardrobeClothingTracker.PlayerOwnsClothing((uint)component, (uint)currentDrawable, (uint)currentTexture))
                {
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), (uint)component, (uint)currentDrawable, (uint)currentTexture);
                }
                else if (lastKnownOwnedClothing.TryGetValue(component, out var ownedClothing))
                {
                    // Fallback to the last known owned clothing if the purchased clothing is not valid
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), (uint)component, (uint)ownedClothing.Drawable, (uint)ownedClothing.Texture);
                }
            }

            Main.Log("Applied all purchased clothing.");
        }
        private static void ApplyCurrentSelectedClothing()
        {
            foreach (var component in pedComponentsOrder)
            {
                if (currentSelectedClothing.TryGetValue((uint)component, out var clothing))
                {
                    SET_CHAR_COMPONENT_VARIATION(Main.PlayerPed.GetHandle(), (uint)component, (uint)clothing.Drawable, (uint)clothing.Texture);
                }
            }

            Main.Log("Applied currently selected clothing.");
        }
    }
}

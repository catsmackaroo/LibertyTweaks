using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal class ImprovedWardrobeClothingVan
    {
        private static ImprovedWardrobeLoadVanLocations vanLocationLoader;
        public static VanLocation selectedVanLocation;
        private const float VanSpawnProximityThreshold = 50.0f; 
        private const float VanDespawnProximityThreshold = 90.0f; 
        private const float ChecksProximityThreshold = 40.0f; 
        private const float PedProximityThreshold = 10.0f; 
        private const float StoreProximityThreshold = 2f;
        private static IVVehicle van = null;
        private static NativeBlip blip = null;

        public static IVPed passengerPed = null;
        public static IVPed drivePed = null;
        public static DateTime vanSpawnTime;
        public static bool isVanLeaving = false;

        private static uint episode;

        public static Vector3 purchaseLocation = new Vector3(0, 0, 0);

        public static void Init()
        {
            vanLocationLoader = new ImprovedWardrobeLoadVanLocations();
            Random random = new Random();
            var vanLocations = vanLocationLoader.GetVanLocations();
            if (vanLocations.Count > 0)
            {
                selectedVanLocation = vanLocations[random.Next(vanLocations.Count)];
                Main.Log($"Selected Van Location: {selectedVanLocation}");
            }
            else
            {
                Main.Log("No van locations available to select.");
            }
        }

        private static bool foundLocation = false; 
        private static bool pedsSpawned = false;
        public static void Tick()
        {
            if (!IS_PLAYER_PLAYING(Main.PlayerIndex) || IS_INTERIOR_SCENE())
                return;

            episode = Main.Episode;

            if (HasPlayerFoundVan() && !PlayerHelper.IsPlayerInOrNearCombat())
            {
                if (ImprovedWardrobe.enableClothingVansAlwaysBlips)
                    ResetFarBlip();

                SpawnVan();
                PrintTutorial();
            }

            if (IsPlayerNearVan() && !IS_SCREEN_FADING() && !IS_SCREEN_FADED_OUT() && !IS_CHAR_IN_TAXI(Main.PlayerPed.GetHandle()))
            {
                if (van != null && !pedsSpawned && !isVanLeaving)
                {
                    var distance = Vector3.Distance(Main.PlayerPos, van.Matrix.Pos);
                    if (distance < PedProximityThreshold)
                    {
                        HandlePeds();
                    }
                }

                if (IsPlayerReadyToPurchase())
                {
                    ImprovedWardrobeClothesVanStore.HandleStore();
                }

                if (drivePed != null && passengerPed != null && van != null && !isVanLeaving)
                {
                    RemoveForeignDrivers();
                    HandleVanLeaving();

                    if (DOES_CHAR_EXIST(drivePed.GetHandle()) && DOES_CHAR_EXIST(passengerPed.GetHandle()))
                    {
                        if (!PlayerHelper.IsPlayerInOrNearCombat())
                        {
                            if (IS_PED_FLEEING(drivePed.GetHandle()) || IS_PED_FLEEING(passengerPed.GetHandle()))
                            {
                                if (!isVanLeaving)
                                    isVanLeaving = true;

                                return;
                            }

                            UpdatePedAnimations();
                        }
                    }
                }
            }

            if (IsPlayerFarFromVan() && van != null && blip != null)
            {
                if (drivePed != null)
                    drivePed.MarkAsNoLongerNeeded();

                if (passengerPed != null)
                    passengerPed.MarkAsNoLongerNeeded();

                blip.Delete();
                blip = null;
                isVanLeaving = true;
                van.MarkAsNoLongerNeeded();
                van.Delete();
                van = null;
            }
        }
        private static bool HasPlayerFoundVan()
        {
            if (selectedVanLocation != null && foundLocation == false)
            {
                Vector3 playerPosition = Main.PlayerPos;
                float distance = Vector3.Distance(playerPosition, selectedVanLocation.ToVector3());

                if (distance <= VanSpawnProximityThreshold)
                {
                    foundLocation = true;
                    return true;
                }
            }

            return false;
        }
        private static bool IsPlayerNearVan()
        {
            if (van != null)
            {
                float distance = Vector3.Distance(Main.PlayerPos, selectedVanLocation.ToVector3());
                if (distance <= ChecksProximityThreshold)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsPlayerFarFromVan()
        {
            if (van != null)
            {
                float distance = Vector3.Distance(Main.PlayerPos, selectedVanLocation.ToVector3());
                if (distance >= VanDespawnProximityThreshold)
                {
                    return true;
                }
            }
            return false;
        }
        private static void SpawnVan()
        {
            try
            {
                NativeWorld.SpawnVehicle("PONY", selectedVanLocation.ToVector3(), out int handle, false);
                van.SetAsMissionVehicle();
                van = NativeWorld.GetVehicleInstanceFromHandle(handle);
                van.SetHeading(selectedVanLocation.Heading);
                TURN_OFF_VEHICLE_EXTRA(van.GetHandle(), 1, false);
                TURN_OFF_VEHICLE_EXTRA(van.GetHandle(), 0, false);
                SET_CAR_ON_GROUND_PROPERLY(van.GetHandle());
                OPEN_CAR_DOOR(handle, 3);
                OPEN_CAR_DOOR(handle, 2);
                LOCK_CAR_DOORS(van.GetHandle(), 3);
                SET_CAR_ENGINE_ON(van.GetHandle(), true, true);
                SET_HAS_BEEN_OWNED_BY_PLAYER(van.GetHandle(), false);

                InitializeBlip();

                vanSpawnTime = DateTime.UtcNow;
                isVanLeaving = false;
            }
            catch (Exception ex)
            {
                Main.Log($"Error spawning van: {ex.Message}");
            }
        }
        private static void HandleVanLeaving(bool skipCheck = false)
        {
            if (!isVanLeaving && DateTime.UtcNow - vanSpawnTime >= TimeSpan.FromMinutes(4) || PlayerHelper.IsPlayerInOrNearCombat() || skipCheck == true 
                || IS_PED_RAGDOLL(passengerPed.GetHandle()) || IS_CHAR_TRYING_TO_ENTER_A_LOCKED_CAR(Main.PlayerPed.GetHandle()))
            {
                if (ImprovedWardrobeClothesVanStore.storeActive) return;

                if (blip != null)
                {
                    blip.Delete();
                    blip = null;
                }
                
                if (van != null)
                {
                    van.MarkAsNoLongerNeeded();
                    CLOSE_ALL_CAR_DOORS(van.GetHandle());
                }

                if (drivePed.GetHandle() != 0)
                {
                    SET_BLOCKING_OF_NON_TEMPORARY_EVENTS(drivePed.GetHandle(), true);
                    _TASK_CAR_DRIVE_WANDER(drivePed.GetHandle(), van.GetHandle(), 30f, 3);
                    SET_CHAR_WILL_COWER_INSTEAD_OF_FLEEING(drivePed.GetHandle(), true);
                }
                
                if (passengerPed.GetHandle() != 0)
                {
                    SET_BLOCKING_OF_NON_TEMPORARY_EVENTS(passengerPed.GetHandle(), true);
                    _TASK_ENTER_CAR_AS_PASSENGER(passengerPed.GetHandle(), van.GetHandle(), 5000, 0);
                    SET_CHAR_WILL_COWER_INSTEAD_OF_FLEEING(passengerPed.GetHandle(), true);
                }

                isVanLeaving = true;
            }
        }
        private static void InitializeBlip()
        {
            if (blip == null && van != null)
            {
                blip = NativeBlip.AddBlip(van);
                blip.FlashBlip = true;
                blip.Icon = BlipIcon.Building_ClothShop;
                blip.Name = "Wardrobe Van";

                Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(2), "Main", () =>
                {
                    blip.FlashBlip = false;
                });
            }
        }
        public static void InitializeFarBlip()
        {
            ResetFarBlip();

            if (blip == null)
            {
                blip = NativeBlip.AddBlipContact(selectedVanLocation.ToVector3());
                blip.Icon = BlipIcon.Building_ClothShop;
                blip.Name = "Wardrobe Van";
                blip.Display = eBlipDisplay.BLIP_DISPLAY_MAP_ONLY;
                blip.ShowOnlyWhenNear = true;
            }
        }
        private static void ResetFarBlip()
        {
            if (blip != null)
            {
                blip.Delete();
                blip = null;
            }
        }
        private static void PrintTutorial()
        {
            var message = $"There is a nearby Wardrobe Van. You can purchase imported clothing not seen in other clothing stores across the map.";
            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP("PLACEHOLDER_1");
        }

        private static bool storeMessage = false;
        private static bool IsPlayerReadyToPurchase()
        {
            if (isVanLeaving || PlayerHelper.IsPlayerInOrNearCombat())
                return false;

            if (van != null)
            {
                GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(van.GetHandle(), 0.0f, -3.0f, 0.0f, out float pOffX, out float pOffY, out float pOffZ);
                purchaseLocation = new Vector3(pOffX, pOffY, pOffZ);

                float distance = Vector3.Distance(Main.PlayerPos, new Vector3(pOffX, pOffY, pOffZ));
                if (distance <= StoreProximityThreshold)
                {
                    if (storeMessage == false)
                    {
                        IVGame.ShowSubtitleMessage("Press ~INPUT_PICKUP~ to access the Wardrobe Van.");
                        drivePed.SayAmbientSpeech("GENERIC_HI");
                        _TASK_TURN_CHAR_TO_FACE_CHAR(drivePed.GetHandle(), Main.PlayerPed.GetHandle());
                        storeMessage = true;
                    }
                    return true;
                }
                else
                {
                    storeMessage = false;
                }
            }
            return false;
        }
        private static void HandlePeds()
        {
            SpawnPedScenario1();
        }
        private static void RemoveForeignDrivers()
        {
            GET_DRIVER_OF_CAR(van.GetHandle(), out int driver);

            if (driver == 0) return;
            else
            {
                if (driver == Main.PlayerPed.GetHandle())
                {
                    SET_CAR_ENGINE_ON(van.GetHandle(), false, false);
                    SET_NEEDS_TO_BE_HOTWIRED(van.GetHandle(), true);
                    HandleVanLeaving(true);
                    return;
                }
                if (!isVanLeaving && driver != drivePed.GetHandle() && driver != passengerPed.GetHandle())
                {
                    DELETE_CHAR(ref driver);
                }
            }
        }
        private static void SpawnPedScenario1()
        {
            var passengerPreset = GetPedPresetByName("Passenger");
            if (passengerPreset == null)
            {
                Main.Log("Passenger preset not found. Skipping passenger spawn.");
                return;
            }

            GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(van.GetHandle(), 1f, 0f, 0.0f, out float besideX, out float besideY, out float besideZ);
            passengerPed = NativeWorld.SpawnPed(passengerPreset.ModelName, new Vector3(besideX, besideY, besideZ), out int pedHandle, true);
            passengerPed.SetHeading(van.GetHeading() - 90f);
            ApplyPedPreset(passengerPed, passengerPreset);

            if (episode == (uint)Episode.IV)
                SET_AMBIENT_VOICE_NAME(passengerPed.GetHandle(), "M_M_PORIENT_01");

            //SET_CHAR_MONEY(passengerPed.GetHandle(), (uint)Main.GenerateRandomNumber(200, 5000));
            RegisterPedAnimation(passengerPed.GetHandle(), new List<(string AnimationName, string AnimationDict)>
    {
        ("sit_idle_a", "amb@wall_idles"),
        ("sit_idle_b", "amb@wall_idles"),
        ("sit_idle_c", "amb@wall_idles")
    }, useDelay: true, delayBetweenAnimations: TimeSpan.FromSeconds(Main.GenerateRandomNumber(1, 6)));

            Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(1), "Main", () =>
            {
                // Get the "Driver" preset
                var driverPreset = GetPedPresetByName("Driver");
                if (driverPreset == null)
                {
                    Main.Log("Driver preset not found. Skipping driver spawn.");
                    return;
                }

                GET_OFFSET_FROM_CAR_IN_WORLD_COORDS(van.GetHandle(), -1f, -2.5f, 0.0f, out float behindX, out float behindY, out float behindZ);
                drivePed = NativeWorld.SpawnPed(driverPreset.ModelName, new Vector3(behindX, behindY, behindZ), out int pedHandle2, true);
                drivePed.SetHeading(van.GetHeading() - -180);
                ApplyPedPreset(drivePed, driverPreset);
                SET_AMBIENT_VOICE_NAME(drivePed.GetHandle(), "M_Y_BRONX_01");
                RegisterPedAnimation(pedHandle2, new List<(string AnimationName, string AnimationDict)>
    {
        ("idle_lookaround_b", "cop_wander_idles"),
        ("idle_lookaround_a", "cop_wander_idles"),
        ("idle_look_at_watch", "cop_wander_idles")
    }, useDelay: true, delayBetweenAnimations: TimeSpan.FromSeconds(Main.GenerateRandomNumber(5, 20)));

            });

            pedsSpawned = true;

        }

        #region Ped Animation
        private class PedAnimation
        {
            public int PedHandle { get; }
            public List<(string AnimationName, string AnimationDict)> Animations { get; }
            public bool IsSecondary { get; }
            public bool UseDelay { get; }
            public TimeSpan DelayBetweenAnimations { get; }
            public DateTime LastAnimationTime { get; private set; }
            private int currentAnimationIndex;

            public PedAnimation(int pedHandle, List<(string AnimationName, string AnimationDict)> animations, bool isSecondary = false, bool useDelay = false, TimeSpan? delayBetweenAnimations = null)
            {
                PedHandle = pedHandle;
                Animations = animations;
                IsSecondary = isSecondary;
                UseDelay = useDelay;
                DelayBetweenAnimations = delayBetweenAnimations ?? TimeSpan.FromSeconds(1); // Default delay of 1 second
                LastAnimationTime = DateTime.MinValue;
                currentAnimationIndex = 0;
            }

            public (string AnimationName, string AnimationDict) GetCurrentAnimation()
            {
                return Animations[currentAnimationIndex];
            }

            public void SelectRandomAnimation()
            {
                Random random = new Random();
                currentAnimationIndex = random.Next(Animations.Count);
            }

            public void UpdateLastAnimationTime()
            {
                LastAnimationTime = DateTime.UtcNow;
            }
        }

        private static List<PedAnimation> pedAnimations = new List<PedAnimation>();
        private static void RegisterPedAnimation(int pedHandle, List<(string AnimationName, string AnimationDict)> animations, bool isSecondary = false, bool useDelay = false, TimeSpan? delayBetweenAnimations = null)
        {
            pedAnimations.Add(new PedAnimation(pedHandle, animations, isSecondary, useDelay, delayBetweenAnimations));
        }
        private static void UpdatePedAnimations()
        {
            foreach (var pedAnimation in pedAnimations)
            {
                var (animationName, animationDict) = pedAnimation.GetCurrentAnimation();
                REQUEST_ANIMS(animationDict);

                // Check if the animation is playing
                if (IS_CHAR_PLAYING_ANIM(pedAnimation.PedHandle, animationDict, animationName))
                {
                    // Get the current animation time
                    GET_CHAR_ANIM_CURRENT_TIME(pedAnimation.PedHandle, animationDict, animationName, out float animTime);

                    // Replay the animation if the current time is 0.90 or greater
                    if (animTime >= 0.90f)
                    {
                        pedAnimation.SelectRandomAnimation();
                        (animationName, animationDict) = pedAnimation.GetCurrentAnimation();
                        pedAnimation.UpdateLastAnimationTime();

                        if (pedAnimation.IsSecondary)
                        {
                            _TASK_PLAY_ANIM_SECONDARY_IN_CAR(
                                pedAnimation.PedHandle,
                                animationName,
                                animationDict,
                                8, 0, 0, 1, 0, -1
                            );
                        }
                        else
                        {
                            _TASK_PLAY_ANIM_NON_INTERRUPTABLE(
                                pedAnimation.PedHandle,
                                animationName,
                                animationDict,
                                8, 0, 0, 1, 0, -1
                            );
                        }
                    }
                }
                else
                {
                    // Replay the animation if it's not playing
                    if (pedAnimation.UseDelay && DateTime.UtcNow - pedAnimation.LastAnimationTime < pedAnimation.DelayBetweenAnimations)
                    {
                        continue; // Skip if the delay hasn't passed
                    }

                    pedAnimation.SelectRandomAnimation();
                    (animationName, animationDict) = pedAnimation.GetCurrentAnimation();
                    pedAnimation.UpdateLastAnimationTime();

                    if (pedAnimation.IsSecondary)
                    {
                        _TASK_PLAY_ANIM_SECONDARY_IN_CAR(
                            pedAnimation.PedHandle,
                            animationName,
                            animationDict,
                            8, 0, 0, 1, 0, -1
                        );
                    }
                    else
                    {
                        _TASK_PLAY_ANIM_NON_INTERRUPTABLE(
                            pedAnimation.PedHandle,
                            animationName,
                            animationDict,
                            8, 0, 0, 1, 0, -1
                        );
                    }
                }
            }
        }

        #endregion

        #region Ped Presets

        private class PedPreset
        {
            public string Name { get; }
            public string ModelName { get; }
            public Dictionary<int, (int Drawable, int Texture)> Components { get; }
            public Dictionary<int, (int PropIndex, int PropTexture)> Props { get; }

            public PedPreset(string name, string modelName, Dictionary<int, (int Drawable, int Texture)> components, Dictionary<int, (int PropIndex, int PropTexture)> props = null)
            {
                Name = name;
                ModelName = modelName;
                Components = components;
                Props = props ?? new Dictionary<int, (int PropIndex, int PropTexture)>();
            }
        }

        private static void ApplyPedPreset(IVPed ped, PedPreset preset)
        {
            // Apply components
            foreach (var component in preset.Components)
            {
                SET_CHAR_COMPONENT_VARIATION(ped.GetHandle(), (uint)component.Key, (uint)component.Value.Drawable, (uint)component.Value.Texture);
            }

            // Apply props
            foreach (var prop in preset.Props)
            {
                SET_CHAR_PROP_INDEX(ped.GetHandle(), (uint)prop.Value.PropIndex, (uint)prop.Value.PropTexture);
            }
        }

        private static readonly List<PedPreset> PedPresets = new List<PedPreset>
{
    new PedPreset("Passenger", "m_y_multiplayer", new Dictionary<int, (int Drawable, int Texture)>
    {
        { PedComponents.Head, (2, 6) },
        { PedComponents.Torso, (Main.GenerateRandomNumber(0, 2), Main.GenerateRandomNumber(0, 2)) },
        { PedComponents.Legs, (Main.GenerateRandomNumber(0, 2), Main.GenerateRandomNumber(0, 2)) },
        { PedComponents.Feet, (0, 0) },
        { PedComponents.Hair, (0, 0) },
    },
    new Dictionary<int, (int PropIndex, int PropTexture)>
    {
        { 0, (0, 0) }, 
    }),
    new PedPreset("Driver", "m_y_multiplayer", new Dictionary<int, (int Drawable, int Texture)>
    {
        { PedComponents.Head, (1, 4) },
        { PedComponents.Torso, (Main.GenerateRandomNumber(0, 2), Main.GenerateRandomNumber(0, 2)) },
        { PedComponents.Legs, (Main.GenerateRandomNumber(0, 2), Main.GenerateRandomNumber(0, 2)) },
        { PedComponents.Feet, (0, 0) },
        { PedComponents.Hair, (0, 0) },
    },
    new Dictionary<int, (int PropIndex, int PropTexture)>
    {
        { 0, (0, 0) },
    })
};

        private static PedPreset GetPedPresetByName(string name)
        {
            return PedPresets.Find(preset => preset.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


        #endregion



    }
}

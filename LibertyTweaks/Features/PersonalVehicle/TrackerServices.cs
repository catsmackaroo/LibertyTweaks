using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace LibertyTweaks
{
    internal class TrackerServices
    {
        private static bool messageShown = false;
        internal class TrackerServiceLocation
        {
            public Vector3 Location { get; set; }
            public bool Spawned { get; set; }
            public bool Unlocked { get; set; }

            public TrackerServiceLocation(Vector3 location)
            {
                Location = location;
                Spawned = false;
                Unlocked = false;
            }
        }
        public static List<TrackerServiceLocation> TrackerServiceLocations { get; private set; } = new List<TrackerServiceLocation>();

        /// <summary>
        /// This method initializes the tracker service locations from LibertyTweaks.ini, allowing users to create their own.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="section"></param>
        public static void Init(SettingsFile settings, string section)
        {
            int index = 1;

            while (true)
            {
                string key = $"TrackerServiceLocation{index}";
                string value = settings.GetValue(section, key, null);

                if (string.IsNullOrEmpty(value))
                    break;

                Vector3 location = CommonHelpers.ParseVector3(value);
                if (location != Vector3.Zero)
                {
                    TrackerServiceLocations.Add(new TrackerServiceLocation(location));
                }
                else
                {
                    Main.Log($"Invalid Vector3 format for {key}: {value}");
                }

                index++;
            }

            Main.Log($"Loaded {TrackerServiceLocations.Count} tracker service locations.");
        }
        public static void ManageBlips()
        {
            foreach (var trackerService in TrackerServiceLocations)
            {
                if (!trackerService.Spawned && trackerService.Unlocked)
                {
                    var blip = NativeBlip.AddBlip(trackerService.Location);
                    blip.Icon = BlipIcon.Building_Garage;
                    blip.ShowOnlyWhenNear = true;
                    blip.Name = "Tracker Service";
                    blip.FlashBlip2 = true;

                    Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(10), "Main", () =>
                    {
                        blip.FlashBlip2 = false; 
                    });
                    trackerService.Spawned = true; // Mark as spawned
                }
            }
        }
        /// <summary>
        /// This method checks if the player has found the tracker services. If so, they become unlocked and appear on the map.
        /// </summary>
        public static void HasFoundTrackerLocation()
        {
            foreach (var trackerService in TrackerServiceLocations)
            {
                if (!trackerService.Unlocked && Vector3.Distance(Main.PlayerPos, trackerService.Location) < 50f)
                {
                    trackerService.Unlocked = true;
                    Main.Log($"Unlocked tracker service '{trackerService.Location}'!");

                    if (!TrackerServiceLocations.Any(ts => ts.Unlocked && ts != trackerService))
                    {
                        PlayTutorial();
                    }
                }
            }
        }
        private static void PlayTutorial()
        {
            Main.ShowMessage("A nearby Tracker Service is available. Tracker Services let you keep track of your personal vehicle.");

            Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(8), "Main", () =>
            {
                Main.ShowMessage("Tracked Vehicles will also be delivered to you after missions or death. You may also store your weapons in them.");
            });
        }
        /// <summary>
        /// Checks if the player is at a tracker service location. If so, it shows a message and allows the player to use the tracker service.
        /// </summary>
        public static bool IsPlayerAtTrackerService()
        {
            if (!CanPlayerUseTrackerService())
                return false;

            foreach (var service in TrackerServices.TrackerServiceLocations)
            {
                var distance = Vector3.Distance(service.Location, Main.PlayerPed.Matrix.Pos);

                if (service.Unlocked && service.Spawned && distance < 5)
                {
                    if (PersonalVehicleHandler.CanVehicleBePersonal(true))
                    {
                        if (WouldTrackerServiceBeAccepted())
                        {
                            if (NativeControls.IsGameKeyPressed(0, GameKey.SoundHorn))
                            {
                                UseTrackerService();
                            }
                        }

                        return true; // Found a valid location, return true.
                    }
                }
            }

            // If no valid location was found, reset the messageShown flag and return false.
            if (messageShown)
                messageShown = false;

            return false;
        }
        /// <summary>
        /// Handles the tracker service logic. It sets the current vehicle as a tracked vehicle. Called only from above method.
        /// </summary>
        public static void UseTrackerService()
        {
            TrackedVehicle.SetCurrentVehicle();

            Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(1), "Main", () =>
            {
                CommonHelpers.HandleScreenFade(5000, false, () =>
                {
                    SetWorldEvents();
                    TrackedVehicle.SetCurrentVehicle();
                });
            });
        }

        /// <summary>
        /// Determines whether the tracker service would be denied based on vehicle state & player circumstance.
        /// </summary>
        private static bool WouldTrackerServiceBeAccepted()
        {
            if (PersonalVehicleHandler.trackerVehicle.GetHandle() == Main.PlayerVehicle.GetHandle())
            {
                ShowMessageOnce("You already have a tracker on this vehicle.");
                return false;
            }
            if (Main.PlayerPed.PlayerInfo.GetMoney() < TrackerServices.DeterminePrice())
            {
                ShowMessageOnce($"You need $~r~{TrackerServices.DeterminePrice()}~s~ to use this tracker service.");
                return false;
            }
            if (Main.PlayerWantedLevel > 0)
            {
                ShowMessageOnce("You cannot use the tracker service while ~r~wanted~s~.");
                return false;
            }
            if (Main.PlayerVehicle.GetHealth() < 600)
            {
                ShowMessageOnce("Your vehicle is ~r~too damaged~s~ to be tracked.");
                return false;
            }
            if (IVTheScripts.IsPlayerOnAMission())
            {
                ShowMessageOnce("You cannot use the tracker service while ~r~on a mission~s~.");
                return false;
            }

            ShowMessageOnce($"Press ~INPUT_VEH_HORN~ to track your vehicle for $~g~{TrackerServices.DeterminePrice()}");
            return true;
        }

        /// <summary>
        /// Determine if the player can actually use the tracker service based off circumstance.
        /// </summary>
        private static bool CanPlayerUseTrackerService()
        {
            return IS_PLAYER_PLAYING(Main.PlayerIndex) &&
                   IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()) &&
                   !IS_CAR_DEAD(Main.PlayerVehicle.GetHandle()) &&
                   !IVTheScripts.IsPlayerOnAMission() &&
                   PersonalVehicleHandler.trackerVehicle.GetHandle() != Main.PlayerVehicle.GetHandle();
        }
        /// <summary>
        /// Plays sound effects for setting the tracker.
        /// </summary>
        public static void PlaySFX()
        {
            PLAY_SOUND_FRONTEND(-1, "BREAK_SPARK_FIZZ");
            PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_MP_READY");

            Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(0.5), "Main", () =>
            {
                PLAY_SOUND_FRONTEND(-1, "BREAK_SPARK_FIZZ");
                PLAY_SOUND_FRONTEND(-1, "DEST_SPARKING_WIRES_MT");
                PLAY_SOUND_FRONTEND(-1, "ELECTRICAL_SPARK_RND");
            });
        }

        /// <summary>
        /// Sets the world events for the tracker service. This includes time of day and removing money from the player.
        /// </summary>
        public static void SetWorldEvents()
        {
            GET_TIME_OF_DAY(out int beforeTime, out int beforeTimeMinute);
            uint afterTime = (uint)(beforeTime + 3);
            uint afterTimeMinute = (uint)(beforeTimeMinute + 30);
            SET_TIME_OF_DAY(afterTime, afterTimeMinute);
            SKIP_RADIO_FORWARD();

            Main.PlayerPed.PlayerInfo.RemoveMoney(TrackerServices.DeterminePrice());
        }
        public static int DeterminePrice()
        {
            var price = Main.PlayerVehicle.Handling.MonetaryValue / 4;
            price = (uint)CommonHelpers.Clamp(price, 1000, 50000);

            var finalPrice = Math.Ceiling((double)price); 

            return (int)finalPrice;
        }
        private static void ShowMessageOnce(string message)
        {
            if (!messageShown)
            {
                IVGame.ShowSubtitleMessage(message, 5000);
                messageShown = true;
            }
        }
        /// <summary>
        /// This method saves the tracker service locations to the LibertyTweaks.save file.
        /// </summary>
        public static void Save()
        {
            if (Main.GameSaved)
            {
                for (int i = 0; i < TrackerServiceLocations.Count; i++)
                {
                    var trackerService = TrackerServiceLocations[i];
                    Main.GetTheSaveGame().SetBoolean($"TrackerServiceUnlocked{i}", trackerService.Unlocked);
                }

                Main.GetTheSaveGame().Save();
                Main.Log("Tracker service locations saved successfully.");
            }
        }

        /// <summary>
        /// This method loads the tracker service locations from the LibertyTweaks.save file, upon game load.
        /// </summary>
        public static void Load()
        {
            for (int i = 0; i < TrackerServiceLocations.Count; i++)
            {
                var trackerService = TrackerServiceLocations[i];
                trackerService.Unlocked = Main.GetTheSaveGame().GetBoolean($"TrackerServiceUnlocked{i}", false);
            }
        }
    }
}

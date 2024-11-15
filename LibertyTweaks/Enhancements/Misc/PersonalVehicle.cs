using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;
using CCL.GTAIV;
using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using DocumentFormat.OpenXml;
using System.Linq;

// Credits: catsmackaroo

namespace LibertyTweaks
{

    internal class PersonalVehicle
    {
        #region Variables

        // Main
        public static bool enable;
        public static bool enableReplaceParkedVeh;
        public static bool enableImpound;
        public static bool enableAlwaysUnlocked;

        // Keys 
        public static Keys personalVehicleKey;

        // Personal Vehicle
        private static NativeBlip vehBlip;
        private static IVVehicle savedVehicle;
        private static string savedVehicleName;
        private static uint savedVehicleModelId;
        private static byte savedVehiclePrimaryColor;
        private static byte savedVehicleSecondaryColor;
        private static byte savedVehicleQuaternaryColor;
        private static byte savedVehicleTertiaryColor;
        private static float savedVehicleEngineHealth;
        private static float savedVehiclePetrolTankHealth;
        private static float savedVehicleHeading;
        private static float savedVehicleDirt;
        private static readonly bool[] savedVehicleExtras = new bool[11];
        private static Vector3 savedVehiclePosition;
        private static bool firstFrame = true;
        private static bool isBlipAttached;
        private static bool hasSaved;
        private static bool newGameCleanup;

        // Tracker Service
        private static bool blipsSpawned = false;
        private static IVVehicle checkVehicle;
        private static NativeBlip trackerBlip;
        private static readonly List<Vector3> serviceLocations = new List<Vector3>();
        private static Vector3 northAlgonquinPNS = new Vector3(-335, 1531, 19);
        private static Vector3 southAlgonquinPNS = new Vector3(-481, 350, 6);
        private static Vector3 dukesPNS = new Vector3(1044, -332, 18);
        private static Vector3 northAlderneyPNS = new Vector3(-1125, 1185, 16);
        private static Vector3 southAlderneyPNS = new Vector3(-1308, 272, 10);
        private static Vector3 stevieLocation = new Vector3(722, 1392, 14);
        private static bool canBeTracked = false;
        private static bool canShowTrackerGuide = false;
        private static uint priceForTracking = 1000;
        private static Dictionary<Vector3, bool> messageShown = new Dictionary<Vector3, bool>();
        private static readonly string trackerName = "Tracker Service";

        // Impound Stuff
        private static bool isVehicleImpounded;
        private static readonly List<Vector3> policeStations = new List<Vector3>();
        private static Vector3 algonquinImpound = new Vector3(68, 1248, 17);
        private static Vector3 southAlgonquinImpound = new Vector3(-421, 316, 13);
        private static Vector3 southWestAlgonquinImpound = new Vector3(-415, -262, 13);
        private static Vector3 airportImpound = new Vector3(2138, 465, 6);
        private static Vector3 northAlderneyImpound = new Vector3(-845, 1314, 23);
        private static Vector3 southAlderneyImpound = new Vector3(-1251, -252, 4);
        private static Vector3 bohanImpound = new Vector3(993, 1894, 24);
        private static Vector3 brokerImpound = new Vector3(1211, -100, 29);

        // Teleports
        private static bool hasVehicleBeenReplaced = false;
        private static bool canTeleportPostDeathOrFail = false;

        // Player Stats
        //private static readonly int delayInMilliseconds = 5000;
        private static int islandsUnlocked;
        private static int islandsUnlockedInitial;
        private static int romanMissionProgress;
        private static int romanMissionProgressInitial;
        private static int brucieMissionProgress;
        private static int brucieMissionProgressInitial;
        private static int missionsCompleted;
        private static uint currentEpisode;
        private static int totalDeathsInitial;
        private static int totalDeaths;
        private static int totalFailedMissionsInitial;
        private static int totalFailedMissions;
        private static int totalArrestsInitial;
        private static int totalArrests;
        #endregion

        #region Initialization Methods
        public static void Init(Script instance, SettingsFile settings)
        {
            enable = settings.GetBoolean("Personal Vehicle", "Enable", true);
            enableReplaceParkedVeh = settings.GetBoolean("Personal Vehicle", "Deliveries", true);
            enableImpound = settings.GetBoolean("Personal Vehicle", "Impound", true);
            enableAlwaysUnlocked = settings.GetBoolean("Personal Vehicle", "Service Always Unlocked", true);

            personalVehicleKey = settings.GetKey("Personal Vehicle", "Tracker Key", Keys.E);

            if (enable)
            {
                Main.Log("script initialized...");
                instance.RegisterPhoneNumber("3345550100", () =>
                {
                    FindNearestPossibleTeleport();
                    IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "~S~Cheat activated");
                    PRINT_HELP("PLACEHOLDER_1");
                    IVCheat.HasPlayerCheated = true;
                });
            }
        }

        public static void IngameStartup()
        {
            if (!enable)
                return;

            Cleanup();
            firstFrame = true;
        }
        #endregion

        #region Main Methods
        public static void Process()
        {
            if (!enable)
                return;

            uint playerMoney = IVPlayerInfoExtensions.GetMoney(Main.PlayerPed.PlayerInfo);

            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()) || canBeTracked == false || playerMoney < priceForTracking)
                return;

            if (IS_CHAR_IN_CAR(Main.PlayerPed.GetHandle(), savedVehicle.GetHandle()))
            {
                IVGame.ShowSubtitleMessage("This vehicle already has a tracker.");
                return;
            }

            Cleanup();
            //isDestroyed = false;

            savedVehicle = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

            Main.Log("Set Unsaved Vehicle: " + savedVehicle.Handling.Name);

            GET_TIME_OF_DAY(out int beforeTime, out int beforeTimeMinute);
            uint afterTime = (uint)(beforeTime + 3);
            uint afterTimeMinute = (uint)(beforeTimeMinute + 30);
            IVGame.ShowSubtitleMessage("");

            CommonHelpers.HandleScreenFade(2000, false, () =>
            {
                canShowTrackerGuide = true;
                SET_TIME_OF_DAY(afterTime, afterTimeMinute);
                SKIP_RADIO_FORWARD();
                IVPlayerInfoExtensions.RemoveMoney(Main.PlayerPed.PlayerInfo, (int)priceForTracking);
            });
        }
        public static void Tick()
        {
            if (!enable)
                return;

            if (firstFrame)
                InitializeFirstFrame();

            if (Main.gxtEntries && IS_THIS_HELP_MESSAGE_WITH_NUMBER_BEING_DISPLAYED("INT4_P2", 0))
                IVText.TheIVText.ReplaceTextOfTextLabel("INT4_P3", "~S~Additionally, services around all Pay 'n' Sprays can add a tracker to your vehicle.");

            uint playerMoney = IVPlayerInfoExtensions.GetMoney(Main.PlayerPed.PlayerInfo);

            STORE_WANTED_LEVEL(Main.PlayerIndex, out uint playerWantedLevel);

            CheckPlayerStats();
            HandleSaving();
            HandleBlipSpawning();
            HandleMissionBlipVisibility();
            HandleTrackerService(serviceLocations, playerMoney);
            HandleImpoundLogic(playerWantedLevel);

            if (missionsCompleted == 0 && !newGameCleanup)
            {
                Cleanup();
                newGameCleanup = true;
            }

            if (HasMissionProgressChanged())
            {
                if (currentEpisode != 0)
                    return;

                ManageServiceBlips();
                UpdateMissionProgress();
            }

            if (HasPlayerDiedOrFailMission() && savedVehicle != null)
            {
                canTeleportPostDeathOrFail = true;
            }

            if (canTeleportPostDeathOrFail)
            {
                GET_CHAR_HEALTH(Main.PlayerPed.GetHandle(), out uint pHealth);

                if (pHealth == 200)
                {
                    Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(1), "Main", () =>
                    {
                        if (Vector3.Distance(Main.PlayerPed.Matrix.Pos, savedVehicle.Matrix.Pos) > 100f)
                        {
                            if (!isVehicleImpounded && !hasVehicleBeenReplaced)
                            {
                                FindNearestPossibleTeleport();
                                UpdateDeathsAndFails();
                                canTeleportPostDeathOrFail = false;
                            }
                        }
                    });
                }
            }

            if (savedVehicle != null)
            {
                savedVehicle.SetAsMissionVehicle();

                ManageVehicleBlip();
                HandleMissionCompletion();
                HandleVehicleStatus();

                if (!isVehicleImpounded)
                    LOCK_CAR_DOORS(savedVehicle.GetHandle(), 1);
            }
        }
        private static void InitializeFirstFrame()
        {
            islandsUnlockedInitial = GET_INT_STAT(363);
            romanMissionProgressInitial = GET_INT_STAT(3);
            brucieMissionProgressInitial = GET_INT_STAT(16);
            totalFailedMissionsInitial = GET_INT_STAT(254);
            totalDeathsInitial = GET_INT_STAT(261);
            totalArrestsInitial = GET_INT_STAT(419);
            currentEpisode = GET_CURRENT_EPISODE();

            blipsSpawned = false;
            newGameCleanup = false;
            SpawnSavedVehicle();
            firstFrame = false;
        }
        private static void CheckPlayerStats()
        {
            totalFailedMissions = GET_INT_STAT(254);
            totalArrests = GET_INT_STAT(419);
            islandsUnlocked = GET_INT_STAT(363);
            romanMissionProgress = GET_INT_STAT(3);
            brucieMissionProgress = GET_INT_STAT(16);
            missionsCompleted = GET_INT_STAT(253);
            totalDeaths = GET_INT_STAT(261);
        }
        private static bool HasMissionProgressChanged()
        {
            return islandsUnlockedInitial != islandsUnlocked ||
                   romanMissionProgress != romanMissionProgressInitial ||
                   brucieMissionProgress != brucieMissionProgressInitial;
        }
        private static void UpdateMissionProgress()
        {
            islandsUnlockedInitial = islandsUnlocked;
            romanMissionProgressInitial = romanMissionProgress;
            brucieMissionProgressInitial = brucieMissionProgress;
        }
        private static bool HasPlayerDiedOrFailMission()
        {
            return totalFailedMissionsInitial != totalFailedMissions || totalDeathsInitial != totalDeaths;
        }
        private static void UpdateDeathsAndFails()
        {
            totalDeathsInitial = totalDeaths;
            totalFailedMissionsInitial = totalFailedMissions;
        }
        private static bool HasPlayerBeenBusted()
        {
            return totalArrestsInitial != totalArrests;
        }
        private static void UpdateArrests()
        {
            totalArrestsInitial = totalArrests;
        }
        private static void HandleMissionCompletion()
        {
            if (IS_MISSION_COMPLETE_PLAYING())
            {
                if (!enableReplaceParkedVeh)
                    return;

                if (Vector3.Distance(Main.PlayerPed.Matrix.Pos, savedVehicle.Matrix.Pos) > 100f)
                {
                    if (isVehicleImpounded)
                        return;

                    if (!hasVehicleBeenReplaced)
                        FindNearestPossibleTeleport();
                }
            }
            else
            {
                hasVehicleBeenReplaced = false;
            }
        }
        private static void HandleVehicleStatus()
        {
            if (IS_CAR_DEAD(savedVehicle.GetHandle()))
            {
                Cleanup();
            }
        }
        private static void HandleSaving()
        {
            if (GET_IS_DISPLAYINGSAVEMESSAGE() && !hasSaved)
            {
                hasSaved = true;
            }
            else if (!GET_IS_DISPLAYINGSAVEMESSAGE() && hasSaved)
            {
                SaveVehicleData();
                hasSaved = false;
            }
        }
        private static void HandleBlipSpawning()
        {
            if (!blipsSpawned)
            {
                AddServiceLocations();
                ManageServiceBlips();
            }
        }
        private static void HandleMissionBlipVisibility()
        {
            if (IVTheScripts.IsPlayerOnAMission())
            {
                if (vehBlip != null)
                    vehBlip.ShowOnlyWhenNear = true;
            }
            else
            {
                if (vehBlip != null)
                    vehBlip.ShowOnlyWhenNear = false;
            }
        }
        //private static void HandleScreenFade()
        //{
        //    if (canFade)
        //    {
        //        DO_SCREEN_FADE_OUT(2000);
        //        SET_PLAYER_CONTROL(Main.PlayerIndex, false);
        //        if (IS_SCREEN_FADING())
        //        {
        //            Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(2), "Main", () =>
        //            {
        //                DO_SCREEN_FADE_IN(2000);
        //                SET_PLAYER_CONTROL(Main.PlayerIndex, true);
        //            });
        //        }
        //        canFade = false;
        //    }
        //}
        private static (Vector3 nearestLocation, int nearestIndex) FindNearestLocation(Vector3 currentPos, List<Vector3> locations)
        {
            float minDistance = float.MaxValue;
            Vector3 nearestLocation = Vector3.Zero;
            int nearestIndex = -1;

            for (int i = 0; i < locations.Count; i++)
            {
                float distance = Vector3.Distance(currentPos, locations[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestLocation = locations[i];
                    nearestIndex = i;
                }
            }

            return (nearestLocation, nearestIndex);
        }
        private static void AddServiceLocations()
        {
            Main.Log("Entering AddServiceLocations method...");

            serviceLocations.Clear();
            
            if (enableAlwaysUnlocked || islandsUnlocked == 3)
            {
                serviceLocations.Add(dukesPNS);
                serviceLocations.Add(northAlgonquinPNS);
                serviceLocations.Add(southAlgonquinPNS);
                serviceLocations.Add(northAlderneyPNS);
                serviceLocations.Add(southAlderneyPNS);
            }
            else
            {
                if (islandsUnlocked == 2)
                {
                    serviceLocations.Add(dukesPNS);
                    serviceLocations.Add(northAlgonquinPNS);
                    serviceLocations.Add(southAlgonquinPNS);
                }

                if (romanMissionProgress >= 33)
                {
                    serviceLocations.Add(dukesPNS);
                }

                if (brucieMissionProgress == 100)
                {
                    serviceLocations.Add(stevieLocation);
                }
            }
        }
        private static void ManageServiceBlips()
        {
            Main.Log("Entering ManageServiceBlips method...");

            if (trackerBlip != null)
            {
                trackerBlip.Delete();
                trackerBlip = null;
                blipsSpawned = false;
            }

            if (serviceLocations.Count == 0)
            {
                Main.Log("Service locations are not populated.");
                return;
            }

            foreach (Vector3 location in serviceLocations)
            {
                try
                {
                    trackerBlip = NativeBlip.AddBlip(location);
                    trackerBlip.ShowOnlyWhenNear = true;
                    trackerBlip.Icon = BlipIcon.Building_Garage;
                    trackerBlip.Name = trackerName;
                }
                catch
                {
                    Main.Log($"Error adding blips at {location}. Restarting the game may fix this issue.");
                }
                blipsSpawned = true;
            }
        }
        public static void FindNearestPossibleTeleport()
        {
            Main.Log("Entering FindNearestPossibleTeleport method...");

            if (isVehicleImpounded)
                return;

            if (savedVehicle == null)
                return;

            float searchRadius = 1000f;
            List<int> nearbyVehicles = new List<int>();
            IVVehicle closestCarVeh = null;
            float closestDistance = float.MaxValue;
            float carHeading = 0;
            IVPool vehPool = IVPools.GetVehiclePool();

            try
            {
                for (int i = 0; i < vehPool.Count; i++)
                {
                    int vehicle = GET_CLOSEST_CAR(Main.PlayerPed.Matrix.Pos, searchRadius, 0, (uint)i);
                    if (vehicle == 0 || nearbyVehicles.Contains(vehicle))
                        continue;

                    GET_DRIVER_OF_CAR(vehicle, out int carDriver);
                    GET_CAR_HEADING(vehicle, out float vehicleHeading);
                    IVVehicle currentCarVeh = NativeWorld.GetVehicleInstanceFromHandle(vehicle);

                    if (currentCarVeh != null && !currentCarVeh.VehicleFlags.EngineOn && carDriver == 0 &&
                        vehicle != savedVehicle.GetHandle() &&
                        !IS_CAR_A_MISSION_CAR(vehicle) && !IS_THIS_MODEL_A_BOAT((uint)currentCarVeh.ModelIndex) && !IS_THIS_MODEL_A_HELI((uint)currentCarVeh.ModelIndex))
                    {
                        nearbyVehicles.Add(vehicle);
                        float distanceToPlayer = (currentCarVeh.Matrix.Pos - Main.PlayerPed.Matrix.Pos).Length();

                        if (distanceToPlayer < closestDistance)
                        {
                            closestDistance = distanceToPlayer;
                            closestCarVeh = currentCarVeh;
                            carHeading = vehicleHeading;
                        }
                    }
                }

                TeleportationScript teleportScript = new TeleportationScript();
                Vector3 savedVehiclePos = savedVehicle.Matrix.Pos;
                teleportScript.GetNearestTeleportLocation(savedVehiclePos);
                TeleportLocation nearestLocation = teleportScript.GetNearestTeleportLocation(Main.PlayerPed.Matrix.Pos);

                if (nearestLocation != null)
                {
                    float distanceToTeleport = Vector3.Distance(Main.PlayerPed.Matrix.Pos, nearestLocation.ToVector3());
                    if (distanceToTeleport < 150f)
                    {
                        Main.Log($"Teleporting saved vehicle to nearest location: {nearestLocation} at distance {distanceToTeleport}");

                        savedVehicle.Teleport(nearestLocation.ToVector3(), true, true);
                        SET_CAR_HEADING(savedVehicle.GetHandle(), nearestLocation.Heading);
                        SET_CAR_ON_GROUND_PROPERLY(savedVehicle.GetHandle());
                        SET_CAR_ENGINE_ON(savedVehicle.GetHandle(), false, false);
                        CLOSE_ALL_CAR_DOORS(savedVehicle.GetHandle());

                        return; 
                    }
                }

                if (closestCarVeh != null)
                {
                    Main.Log($"Closest vehicle found: {closestCarVeh.Handling.Name} at distance {closestDistance}");

                    ReplaceParkedVehicle(closestCarVeh, carHeading);
                }
            }
            catch (Exception ex)
            {
                Main.LogError($"Error in FindNearestPossibleTeleport: {ex.Message}");
            }
        }
        private static void ReplaceParkedVehicle(IVVehicle targetVehicle, float heading)
        {
            try
            {
                savedVehicle.Teleport(targetVehicle.Matrix.Pos, false, true);
                SET_CAR_HEADING(savedVehicle.GetHandle(), heading);
                SET_CAR_ON_GROUND_PROPERLY(savedVehicle.GetHandle());
                SET_CAR_ENGINE_ON(savedVehicle.GetHandle(), false, false);
                CLOSE_ALL_CAR_DOORS(savedVehicle.GetHandle());

                Main.Log($"Vehicle replaced successfully at {targetVehicle.Matrix.Pos}");

                targetVehicle.MarkAsNoLongerNeeded();
                targetVehicle.Delete();
                hasVehicleBeenReplaced = true;
            }
            catch (Exception ex)
            {
                Main.LogError($"Error in ReplaceParkedVehicle: {ex.Message}");
            }
        }
        private static void DisplayTrackerServiceTutorial()
        {
            if (canShowTrackerGuide)
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "You have tracked this vehicle. When tracking a vehicle, you can find it on the map. It will be saved, similar to vehicles parked in-front of safehouses. You may only have one tracked vehicle at a time.");
                PRINT_HELP("PLACEHOLDER_1");
                canShowTrackerGuide = false;
            }
        }
        private static void HandleTrackerService(List<Vector3> locations, uint playerMoney)
        {
            if (!IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            string[] emergencyVehicles = { "POL", "FBI", "NOOSE", "AMBUL", "FIRE" };

            if (messageShown == null)
            {
                messageShown = new Dictionary<Vector3, bool>();
                foreach (var location in locations)
                {
                    messageShown[location] = false;
                }
            }

            foreach (Vector3 location in locations)
            {
                if (Vector3.Distance(Main.PlayerPed.Matrix.Pos, location) < 5f)
                {
                    STORE_WANTED_LEVEL(Main.PlayerIndex, out uint currentWantedLevel);

                    if (currentWantedLevel > 0)
                        return;

                    if (canShowTrackerGuide)
                    {
                        DisplayTrackerServiceTutorial();
                        return;
                    }


                    if (!IS_CHAR_IN_CAR(Main.PlayerPed.GetHandle(), savedVehicle.GetHandle()))
                    {
                        if (messageShown[location])
                            return;

                        GET_CAR_CHAR_IS_USING(Main.PlayerPed.GetHandle(), out int currentCar);
                        GET_CAR_HEALTH(currentCar, out uint currentCarHealth);
                        GET_CAR_MODEL(currentCar, out uint currentCarModel);
                        checkVehicle = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());

                        if (emergencyVehicles.Any(identifier => checkVehicle.Handling.Name.Contains(identifier)) || currentCarModel == 1911513875)
                        {
                            priceForTracking = (uint)(checkVehicle.Handling.MonetaryValue * (1.0 / 2.0));
                        }
                        else
                        {
                            priceForTracking = (uint)(checkVehicle.Handling.MonetaryValue * (1.0 / 6.0));
                        }

                        if (Vector3.Distance(Main.PlayerPed.Matrix.Pos, stevieLocation) < 5f)
                        {
                            priceForTracking = (uint)(priceForTracking * (1.0 / 2.0));
                        }

                        if (currentCarHealth < 800)
                        {
                            IVGame.ShowSubtitleMessage("This vehicle is too damaged to be given a tracker.");
                            messageShown[location] = true;
                            return;
                        }

                        if (IS_CAR_A_MISSION_CAR(currentCar))
                        {
                            IVGame.ShowSubtitleMessage("You cannot add trackers to mission vehicles.");
                            messageShown[location] = true;
                            return;
                        }

                        if (playerMoney < priceForTracking)
                        {
                            IVGame.ShowSubtitleMessage("You have insufficient funds. Tracker $" + priceForTracking);
                            messageShown[location] = true;
                            return;
                        }

                        if (PlayerChecks.IsPlayerSeenByPolice())
                        {
                            IVGame.ShowSubtitleMessage("The police have sight of you.");
                            messageShown[location] = true;
                            return;
                        }

                        IVGame.ShowSubtitleMessage("Press " + personalVehicleKey + " to add a tracker to this vehicle. " + "Price: $" + priceForTracking);
                        messageShown[location] = true;
                    }

                    canBeTracked = true;
                    return;
                }
                else
                    messageShown[location] = false;
            }

            canBeTracked = false;
        }
        private static void ManageVehicleBlip()
        {
            if (savedVehicle == null)
                return;

            if (IS_CHAR_IN_CAR(Main.PlayerPed.GetHandle(), savedVehicle.GetHandle()))
            {
                if (vehBlip != null)
                {
                    vehBlip.Delete();
                    vehBlip = null;
                    isBlipAttached = false;
                    Main.Log("Blip detached; player in saved vehicle.");
                }
            }
            else if (!isBlipAttached)
            {
                SET_CAR_AS_MISSION_CAR(savedVehicle.GetHandle());
                try
                {
                    vehBlip = savedVehicle.AttachBlip();
                    vehBlip.Icon = BlipIcon.Building_Garage;
                    vehBlip.Name = "Personal Vehicle";
                    isBlipAttached = true;
                    Main.Log("Blip attached; player not in saved vehicle.");
                }
                catch
                {
                    Main.Log($"Error attaching blip to saved vehicle. Restarting the game may fix this issue.");
                    isBlipAttached = true;
                }

            }
        }
        private static void HandleImpoundLogic(uint playerWantedLevel)
        {
            if (savedVehicle == null || !enableImpound)
                return;

            if (Vector3.Distance(Main.PlayerPed.Matrix.Pos, savedVehicle.Matrix.Pos) > 200f)
                return;

            if (isVehicleImpounded && Vector3.Distance(Main.PlayerPed.Matrix.Pos, savedVehicle.Matrix.Pos) < 10f && playerWantedLevel < 4)
            {
                Main.Log("Player is near impounded car; giving 4 stars.");
                ALTER_WANTED_LEVEL(Main.PlayerIndex, 4);
                APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                isVehicleImpounded = false;
            }

            if (!isVehicleImpounded)
            {
                if (playerWantedLevel >= 1 && (IS_PLAYER_DEAD(Main.PlayerIndex)) || HasPlayerBeenBusted())
                {

                    Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(12), "Main", () =>
                    {
                        ImpoundVehicle();
                        UpdateArrests();
                    });
                }
            }
        }
        private static void ImpoundVehicle()
        {
            if (!isVehicleImpounded && savedVehicle != null)
            {
                Main.Log("Vehicle has been impounded.");

                if (policeStations.Count == 0)
                {
                    policeStations.Add(airportImpound);
                    policeStations.Add(algonquinImpound);
                    policeStations.Add(northAlderneyImpound);
                    policeStations.Add(southAlderneyImpound);
                    policeStations.Add(bohanImpound);
                    policeStations.Add(brokerImpound);
                    policeStations.Add(southAlgonquinImpound);
                    policeStations.Add(southWestAlgonquinImpound);
                }

                var (nearestImpoundLocation, nearestIndex) = FindNearestLocation(Main.PlayerPed.Matrix.Pos, policeStations);

                    savedVehicle.Teleport(nearestImpoundLocation, false, true);
                    SET_CAR_ON_GROUND_PROPERLY(savedVehicle.GetHandle());
                    LOCK_CAR_DOORS(savedVehicle.GetHandle(), 7);
                    SET_CAR_ENGINE_ON(savedVehicle.GetHandle(), false, false);
                    CLOSE_ALL_CAR_DOORS(savedVehicle.GetHandle());

                    string impoundMessage;
                    switch (nearestIndex)
                    {
                        case 0:
                            impoundMessage = "Your tracked vehicle has been impounded at the Francis International Airport Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 268);
                            break;
                        case 1:
                            impoundMessage = "Your tracked vehicle has been impounded at the East Holland Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 89);
                            break;
                        case 2:
                            impoundMessage = "Your tracked vehicle has been impounded at the Leftwood Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 268);
                            break;
                        case 3:
                            impoundMessage = "Your tracked vehicle has been impounded at the Acter Industrial Park Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 268);
                            break;
                        case 4:
                            impoundMessage = "Your tracked vehicle has been impounded at the Northern Gardens Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 359);
                            break;
                        case 5:
                            impoundMessage = "Your tracked vehicle has been impounded at the South Slopes Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 180);
                            break;
                        case 6:
                            impoundMessage = "Your tracked vehicle has been impounded at the Westminster Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 180);
                            break;
                        case 7:
                            impoundMessage = "Your tracked vehicle has been impounded at the Suffolk Police Station.";
                            SET_CAR_HEADING(savedVehicle.GetHandle(), 358);
                            break;
                        default:
                            impoundMessage = "Your tracked vehicle has been impounded.";
                            break;
                    }
                    IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", impoundMessage);
                    PRINT_HELP("PLACEHOLDER_1");

                isVehicleImpounded = true;
            }
        }
        #endregion

        #region Data Saving Methods
        private static void SaveVehicleData()
        {
            Main.Log("Entering SaveVehicleData method...");

            try
            {
                if (savedVehicle  != null)
                {
                    GET_CAR_MODEL(savedVehicle.GetHandle(), out uint savedVehicleModel);
                    savedVehicleModelId = savedVehicleModel;
                    savedVehicleName = savedVehicle.Handling.Name;
                    savedVehiclePrimaryColor = savedVehicle.PrimaryColor;
                    savedVehicleSecondaryColor = savedVehicle.SecondaryColor;
                    savedVehicleQuaternaryColor = savedVehicle.QuaternaryColor;
                    savedVehicleTertiaryColor = savedVehicle.TertiaryColor;
                    savedVehicleEngineHealth = savedVehicle.EngineHealth;
                    savedVehiclePetrolTankHealth = savedVehicle.PetrolTankHealth;
                    savedVehicleHeading = savedVehicle.GetHeading();
                    savedVehiclePosition = savedVehicle.Matrix.Pos;
                    savedVehicleDirt = savedVehicle.DirtLevel;

                    for (int i = 1; i < savedVehicleExtras.Length; i++)
                    {
                        savedVehicleExtras[i] = IS_VEHICLE_EXTRA_TURNED_ON(savedVehicle.GetHandle(), (uint)i);
                    }
                }
            }
            catch (Exception ex)
            {
                Cleanup();
                Main.LogError("Error updating vehicle data: " + ex.Message);
            }

            Main.GetTheSaveGame().SetValue("VehicleName", savedVehicleName);
            Main.GetTheSaveGame().SetInteger("VehicleModel", (int)savedVehicleModelId);
            Main.GetTheSaveGame().SetInteger("VehicleColor1", savedVehiclePrimaryColor);
            Main.GetTheSaveGame().SetInteger("VehicleColor2", savedVehicleSecondaryColor);
            Main.GetTheSaveGame().SetInteger("VehicleColor3", savedVehicleQuaternaryColor);
            Main.GetTheSaveGame().SetInteger("VehicleColor4", savedVehicleTertiaryColor);
            Main.GetTheSaveGame().SetFloat("VehicleEngineHealth", savedVehicleEngineHealth);
            Main.GetTheSaveGame().SetFloat("VehiclePetrolTankHealth", savedVehiclePetrolTankHealth);
            Main.GetTheSaveGame().SetFloat("VehicleHeading", savedVehicleHeading);
            Main.GetTheSaveGame().SetVector3("VehiclePosition", savedVehiclePosition);
            Main.GetTheSaveGame().SetFloat("VehicleDirt", savedVehicleDirt);
            //Main.GetTheSaveGame().SetBoolean("IsVehicleDestroyed", isDestroyed);
            Main.GetTheSaveGame().SetBoolean("IsVehicleImpounded", isVehicleImpounded);

            for (int i = 1; i < savedVehicleExtras.Length; i++)
            {
                Main.GetTheSaveGame().SetBoolean($"VehicleExtra{i}", savedVehicleExtras[i]);
            }

            Main.GetTheSaveGame().Save();
        }
        private static void SpawnSavedVehicle()
        {
            Main.Log("Entering SpawnSavedVehicle method...");

            string lastSavedVehicleName = Main.GetTheSaveGame().GetValue("VehicleName");
            uint lastSavedVehicleModel = (uint)Main.GetTheSaveGame().GetInteger("VehicleModel");
            Vector3 lastSavedVehiclePosition = Main.GetTheSaveGame().GetVector3("VehiclePosition");
            byte lastSavedVehiclePrimaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor1");
            byte lastSavedVehicleSecondaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor2");
            byte lastSavedVehicleQuaternaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor3");
            byte lastSavedVehicleTertiaryColor = (byte)Main.GetTheSaveGame().GetInteger("VehicleColor4");
            float lastSavedVehicleEngineHealth = Main.GetTheSaveGame().GetFloat("VehicleEngineHealth");
            float lastSavedVehiclePetrolTankHealth = Main.GetTheSaveGame().GetFloat("VehiclePetrolTankHealth");
            float lastSavedVehicleHeading = Main.GetTheSaveGame().GetFloat("VehicleHeading");
            float lastSavedVehicleDirt = Main.GetTheSaveGame().GetFloat("VehicleDirt");
            bool[] lastSavedVehicleExtras = new bool[savedVehicleExtras.Length];
            bool wasVehicleImpounded = Main.GetTheSaveGame().GetBoolean("IsVehicleImpounded");
            for (int i = 0; i < lastSavedVehicleExtras.Length; i++)
            {
                lastSavedVehicleExtras[i] = Main.GetTheSaveGame().GetBoolean($"VehicleExtra{i}");
            }

            if (!string.IsNullOrEmpty(lastSavedVehicleName))
            {
                try
                {
                    int closestCar = GET_CLOSEST_CAR(lastSavedVehiclePosition, 10f, 0, 70);
                    if (closestCar != 0)
                    {
                        MARK_CAR_AS_NO_LONGER_NEEDED(closestCar);
                        DELETE_CAR(ref closestCar);
                    }

                    savedVehicle = NativeWorld.SpawnVehicle(lastSavedVehicleModel, lastSavedVehiclePosition, out int savedVehicleHandle, true);
                    CHANGE_CAR_COLOUR(savedVehicleHandle, lastSavedVehiclePrimaryColor, lastSavedVehicleSecondaryColor);
                    SET_EXTRA_CAR_COLOURS(savedVehicleHandle, lastSavedVehicleQuaternaryColor, lastSavedVehicleTertiaryColor);
                    SET_CAR_ON_GROUND_PROPERLY(savedVehicleHandle);
                    SET_CAR_HEADING(savedVehicleHandle, lastSavedVehicleHeading);
                    SET_ENGINE_HEALTH(savedVehicleHandle, (uint)lastSavedVehicleEngineHealth);
                    SET_PETROL_TANK_HEALTH(savedVehicleHandle, (uint)lastSavedVehiclePetrolTankHealth);
                    SET_VEHICLE_DIRT_LEVEL(savedVehicleHandle, lastSavedVehicleDirt);
                    SET_HAS_BEEN_OWNED_BY_PLAYER(savedVehicleHandle, true);
                    savedVehicle.VehicleFlags.NeedsToBeHotWired = false;
                    isBlipAttached = false;
                    //isDestroyed = false;
                    if (wasVehicleImpounded == true)
                        isVehicleImpounded = true;

                    for (int i = 0; i < lastSavedVehicleExtras.Length; i++)
                    {
                        if (lastSavedVehicleExtras[i])
                        {
                            TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, false);
                        }
                        else
                        {
                            TURN_OFF_VEHICLE_EXTRA(savedVehicleHandle, i, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Main.LogError("Error spawning vehicle: " + ex.Message);
                }
            }
            else
            {
                Main.Log("No saved vehicle found.");
            }
        }
        #endregion

        #region Cleanup Methods
        private static void Cleanup()
        {
            Main.Log("Entering Cleanup method...");

            if (vehBlip != null)
            {
                vehBlip.Delete();
                vehBlip = null;
                isBlipAttached = false;
            }

            ResetSavedVehicleState();
        }
        private static void ResetSavedVehicleState()
        {
            Main.Log("Entering ResetSavedVehicleState method...");

            if (savedVehicle != null)
            {
                savedVehicleName = "";
                savedVehiclePrimaryColor = 0;
                savedVehicleSecondaryColor = 0;
                savedVehicleQuaternaryColor = 0;
                savedVehicleTertiaryColor = 0;
                savedVehicleEngineHealth = 0;
                savedVehiclePetrolTankHealth = 0;
                savedVehicleHeading = 0;
                savedVehiclePosition = Vector3.Zero;
                savedVehicleDirt = 0;
                savedVehicle.MarkAsNoLongerNeeded();
                Array.Clear(savedVehicleExtras, 0, savedVehicleExtras.Length);
                savedVehicle = null;
                isVehicleImpounded = false;
            }
        }
        #endregion
    }
}

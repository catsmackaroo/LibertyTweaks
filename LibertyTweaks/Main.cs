using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    public class Main : Script
    {
        #region Variables
        private static readonly Random rnd = new Random();
        public static bool verboseLogging;
        public static bool gxtEntries;
        public static bool debugMode;
        public static Keys debugMenuKey;
        public DateTime timer;
        private static CustomIVSave saveGame;
        public static DelayedCalling TheDelayedCaller;

        private double lastFixedTickTime = 0;
        private const double fixedTickInterval = 0.5;

        public static IVPed PlayerPed { get; private set; }
        public static IVVehicle PlayerVehicle { get; private set; }
        public static int PlayerIndex { get; private set; }
        public static int CarCrashLevel { get; set; }
        public static float CarCrashDamageNormalized { get; private set; }
        public static int PlayerWantedLevel { get; private set; }
        public static float PlayerHealth { get; private set; }
        public static Vector3 PlayerPos { get; private set; }
        public static uint Episode { get; private set; }
        public static bool GameSaved { get; private set; }
        #endregion

        #region Functions
        public static int GenerateRandomNumber(int x, int y)
        {
            if (y < x)
            {
                y = x;
            }
            return rnd.Next(x, y + 1);
        }
        public static float GenerateRandomNumberFloat(float x, float y)
        {
            if (y < x)
            {
                y = x;
            }
            return (float)(rnd.NextDouble() * (y - x) + x);
        }
        internal static CustomIVSave GetTheSaveGame()
        {
            return saveGame;
        }
        #endregion

        #region Constructor
        public Main()
        {
            Initialized += Main_Initialized;
            Tick += Main_Tick;
            Drawing += Main_Drawing;
            KeyDown += Main_KeyDown;
            ProcessAutomobile += Main_ProcessAutomobile;
            ProcessCamera += Main_ProcessCamera;
            IngameStartup += Main_IngameStartup;
            GameLoadPriority += Main_GameLoadPriority;
            GameLoad += Main_GameLoad;
            Uninitialize += Main_Uninitialize;
            TheDelayedCaller = new DelayedCalling();
        }

        private void Main_Uninitialize(object sender, EventArgs e)
        {
            if (TheDelayedCaller != null)
            {
                TheDelayedCaller.ClearAll();
                TheDelayedCaller = null;
            }
        }
        private void Main_GameLoad(object sender, EventArgs e)
        {
            if (WeaponMagazines.enable == true)
                WeaponMagazines.LoadFiles();
        }

        private void Main_Drawing(object sender, EventArgs e)
        {
            NoCursorEscape.Process();
        }

        private void Main_GameLoadPriority(object sender, EventArgs e)
        {
            ArmoredCops.LoadFiles();
        }
        #endregion
        private void Main_IngameStartup(object sender, EventArgs e)
        {
            QuickSave.IngameStartup();
            PersonalVehicleOld.IngameStartup();
            LoadingFadeIn.IngameStartup();
            WeaponMagazines.IngameStartup();
            ImprovedCrashes.IngameStartup();

            Episode = GET_CURRENT_EPISODE();
            // 1.7 Stuff
            //Killcam.IngameStartup();
            //PersonalVehicleHandler.IngameStartup();
            ImprovedWardrobe.IngameStartup();
        }
        private void Main_Initialized(object sender, EventArgs e)
        {
            OnlyRaiseKeyEventsWhenInGame = true;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            verboseLogging = Settings.GetBoolean("Liberty Tweaks", "Verbose Logging", true);
            gxtEntries = Settings.GetBoolean("Liberty Tweaks", "GXT Edits", true);
            saveGame = CustomIVSave.CreateOrLoadSaveGameData(this);

            // MAIN
            Log($"LibertyTweaks version {version} has loaded.");
            WeaponHelpers.InitAddonAnims(Settings);

            var generalSection = "General";
            var progressionSection = "Progression";
            var pedsSection = "Peds";
            var combatSection = "Weapons & Combat";
            var vehiclesSection = "Vehicles";
            var fixesSection = "Fixes";

            Log($"Loading Liberty Tweaks configuration...");
            Log($"Loading category [General]...");
            LoadGeneralFeatures(Settings, generalSection);

            Log($"Loading category [Progression]...");
            LoadProgressionFeatures(Settings, progressionSection);

            Log($"Loading [Peds]...");
            LoadPedsFeatures(Settings, pedsSection);

            Log($"Loading [Combat]...");
            LoadCombatFeatures(Settings, combatSection);

            Log($"Loading [Vehicles]...");
            LoadVehicleFeatures(Settings, vehiclesSection);

            Log($"Loading [Fixes]...");
            LoadFixes(Settings, fixesSection);

            //CopyLocationToClipboard.Init(Settings);
            // Unreleased & unfinished
            //Killcam.Init(Settings);
            //QuickSwitching.Init(Settings); 
            //WarpToShore.Init(Settings);
            //DisableSprintWithHeavyWeapons.Init(Settings);
            //RunAndGun.Init(Settings);
            //DynamicCrosshair.Init(Settings);
            //RandomPedCarColors.Init(Settings);
            //ImprovedRolling.Init(Settings);
            //Hydraulics.Init(Settings);
            //Hydraulics.Init(Settings);
            //RandomizedPedRagdoll.Init(Settings);
            //FixedShotgunReload.Init(Settings);

        }
        private void LoadGeneralFeatures(SettingsFile settings, string section)
        {
            DynamicFOV.Init(settings, section);
            QuickSave.Init(settings, section);
            AutosaveOnCollectibles.Init(settings, section);
            DialogueCombat.Init(settings, section);
            DialogueLooting.Init(settings, section);
            DialogueFalling.Init(settings, section);
            ToggleHUD.Init(settings, section);
            SkipRadioSegments.Init(settings, section);
            LoseStarsWhileUnseen.Init(settings, section);

            WarpToShore.Init(settings, section);
            ImprovedWardrobe.Init(settings, section);
        }
        private void LoadProgressionFeatures(SettingsFile settings, string section)
        {
            StaminaProgression.Init(settings, section);
            WeaponProgression.Init(settings, section);
        }
        private void LoadPedsFeatures(SettingsFile settings, string section)
        {
            PedsLockDoors.Init(settings, section);
            ImprovedAIAccuracyAndFirerates.Init(settings, section);
            ArmoredCops.Init(settings, section);
            ExtendedPedWeaponPool.Init(settings, section);

            RandomizedPedEuphoria.Init(settings, section);
        }

        private void LoadCombatFeatures(SettingsFile settings, string section)
        {
            HolsterWeapons.Init(settings, section);
            WeaponMagazines.Init(settings, section);
            SniperMovement.Init(settings, section);
            RemoveWeaponsOnDeath.Init(settings, section);
            HealthRegeneration.Init(settings, section);
            DynamicMovement.Init(settings, section);
            SniperScopeToggle.Init(settings, section);
            Recoil.Init(settings, section);
            RealisticReloading.Init(settings, section);
            ArmorPenetration.Init(settings, section);
            ShoulderSwap.Init(settings, section);

            //ImprovedRolling.Init(settings, section);
            QuickSwitching.Init(settings, section);
            DisableSprintWithHeavyWeapons.Init(settings, section);
        }

        private void LoadVehicleFeatures(SettingsFile settings, string section)
        {
            PersonalVehicleOld.Init(this, settings, section);
            //PersonalVehicleHandler.Init(settings, section);
            VehiclesBreakOnFire.Init(settings, section);
            CarExplosionsRandomized.Init(settings, section);
            CameraTiltAndRotation.Init(settings, section);
            CameraShake.Init(settings, section);
            CarCrashShake.Init(settings, section);
            CameraDynamicHeight.Init(settings, section);
            CarRollover.Init(settings, section);
            HighRPMShaking.Init(settings, section);
            ImprovedCrashes.Init(settings, section);
            PNSOverhaul.Init(settings, section);
        }

        private void LoadFixes(SettingsFile settings, string section)
        {
            LessOvertaking.Init(settings, section);
            IceCreamSpeechFix.Init(settings, section);
            WheelFix.Init(settings, section);
            BrakeLights.Init(settings, section);
            ExtraHospitalSpawn.Init(settings, section);
            CopShotgunFix.Init(settings, section);
            NoCursorEscape.Init(settings, section);
            LoadingFadeIn.Init(settings, section);
            AllowCopsAllMissions.Init(settings, section);
            DrivebyInPolice.Init(settings, section);
            NoWantedInVigilante.Init(settings, section);
            NoShootWithPhone.Init(settings, section);
            DisableHUDOnBlindfire.Init(settings, section);
            DisableHUDOnCinematic.Init(settings, section);
            DisableCrosshairWithNoHUD.Init(settings, section);
            RunWithPhone.Init(settings, section);
            TimeSkipFix.Init(settings, section);
            SprintInInteriors.Init(settings, section);
            SmoothZooms.Init(settings, section);
            UnholsteredGunFix.Init(settings, section);

            RollInAirFix.Init(settings, section);
            DeathTimescaleFix.Init(settings, section);
        }

        private void Main_ProcessCamera(object sender, EventArgs e)
        {
            DynamicFOV.Tick();
            CameraShake.Tick();
            CameraTiltAndRotation.Tick();
            CameraDynamicHeight.Tick();
            SmoothZooms.Tick();
        }

        private void Main_ProcessAutomobile(UIntPtr vehPtr)
        {
            WheelFix.Process(vehPtr);
        }

        private void Main_Tick(object sender, EventArgs e)
        {

            PlayerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            PlayerPos = PlayerPed.Matrix.Pos;
            PlayerIndex = (int)GET_PLAYER_ID();
            PlayerHealth = PlayerPed.Health;
            STORE_WANTED_LEVEL(PlayerIndex, out uint playerWantedLevel);
            PlayerWantedLevel = (int)playerWantedLevel;

            PedHelper.GrabAllPeds();
            PedHelper.GrabAllVehicles();

            PlayerVehicle = IS_CHAR_IN_ANY_CAR(PlayerPed.GetHandle()) ? IVVehicle.FromUIntPtr(PlayerPed.GetVehicle()) : null;
            var (_, damageLevel, normalizedDamage) = PlayerHelper.GetVehicleDamage();
            CarCrashLevel = damageLevel;
            CarCrashDamageNormalized = normalizedDamage;

            GameSaved = CommonHelpers.HasGameSaved();
            Episode = GET_CURRENT_EPISODE();

            GET_GAME_TIMER(out uint gameTimer);
            gameTimer = (uint)(gameTimer / 1000.0);
            if (gameTimer - lastFixedTickTime >= fixedTickInterval)
            {
                lastFixedTickTime = gameTimer;
                FixedTickFeatures();
            }

            TickFeatures();
            //UnreleasedFeatures();
            TheDelayedCaller.Process();
        }

        private void TickFeatures()
        {
            ImprovedAIAccuracyAndFirerates.Tick();
            WeaponMagazines.Tick();
            SniperMovement.Tick();
            DialogueCombat.Tick();
            DialogueLooting.Tick();
            ArmoredCops.Tick();
            HealthRegeneration.Tick();
            Recoil.Tick();
            RealisticReloading.Tick();
            QuickSave.Tick();
            ExtendedPedWeaponPool.Tick();
            PedsLockDoors.Tick();
            ArmorPenetration.Tick();
            RemoveWeaponsOnDeath.Tick();
            CarExplosionsRandomized.Tick();
            StaminaProgression.Tick();
            WeaponProgression.Tick();
            ToggleHUD.Tick();
            BrakeLights.Tick();
            ExtraHospitalSpawn.Tick();
            IceCreamSpeechFix.Tick();
            WheelFix.PreChecks();
            LoadingFadeIn.Tick();
            DynamicMovement.Tick();
            HolsterWeapons.Tick();
            HighRPMShaking.Tick();
            ImprovedCrashes.Tick();
            DrivebyInPolice.Tick();
            PersonalVehicleOld.Tick();
            NoShootWithPhone.Tick();
            DisableHUDOnBlindfire.Tick();
            DisableCrosshairWithNoHUD.Tick();
            CameraShake.Tick();
            CarCrashShake.Tick();
            LessOvertaking.Tick();
            CarRollover.Tick();
            ShoulderSwap.Tick();
            PNSOverhaul.Tick();
            RunWithPhone.Tick();
            TimeSkipFix.Tick();
            SprintInInteriors.Tick();
            DisableHUDOnCinematic.Tick();
            CameraDynamicHeight.Tick();
            SmoothZooms.Tick();
            
            // 1.7
            RollInAirFix.Tick();
            RandomizedPedEuphoria.Tick();
            DisableSprintWithHeavyWeapons.Tick();
            QuickSwitching.Tick();
            WarpToShore.Tick();
            //PersonalVehicleHandler.Tick();
            ImprovedWardrobe.Tick();
            DeathTimescaleFix.Tick();
            //ImprovedRolling.Tick();
            //FixedShotgunReload.Tick();
        }

        private void FixedTickFeatures()
        {
            AutosaveOnCollectibles.Tick();
            LoseStarsWhileUnseen.Tick();
            DialogueFalling.Tick();
            VehiclesBreakOnFire.Tick();
            CopShotgunFix.Tick();
            AllowCopsAllMissions.Tick();
            UnholsteredGunFix.Tick();
            NoWantedInVigilante.Tick();
        }

        private void UnreleasedFeatures()
        {
            // 1.6 - More gameplay focused
            Killcam.Tick();
            RandomPedCarColors.Tick();

            // 1.7 - World focused
            Hydraulics.Tick();
        }
        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == CopyLocationToClipboard.key)
            //    CopyLocationToClipboard.Process();

            if (e.KeyCode == ShoulderSwap.key)
                ShoulderSwap.Process();

            if (e.KeyCode == SkipRadioSegments.key)
                SkipRadioSegments.Process();

            if (e.KeyCode == ToggleHUD.key)
                ToggleHUD.Process();

            if (e.KeyCode == QuickSave.key)
                QuickSave.Process();

            if (e.KeyCode == HolsterWeapons.key)
                HolsterWeapons.Process();

            if (e.KeyCode == Keys.LShiftKey)
                DynamicMovement.Process();

            if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && PersonalVehicleOld.canBeTracked)
                PersonalVehicleOld.Process();

            if (e.KeyCode == SniperScopeToggle.key)
                SniperScopeToggle.Process();
        }
        public static void Log(string message, [CallerFilePath] string filePath = "")
        {
            if (verboseLogging == true)
            {
                var fileName = System.IO.Path.GetFileName(filePath);

                if (fileName == "Main.cs")
                    fileName = string.Empty;
                else
                    fileName = $"[{fileName}]";

                IVGame.Console.Print($"Liberty Tweaks - {fileName} {message}");
            }
        }
        public static void LogError(string message)
        {
            IVGame.Console.Print("Liberty Tweaks - Error: " + message);
        }

        internal static int GenerateRandomNumber(int v, uint wheelCount)
        {
            throw new NotImplementedException();
        }

        public static void ShowMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
                PRINT_HELP("PLACEHOLDER_1");
            }
        }
    }
}

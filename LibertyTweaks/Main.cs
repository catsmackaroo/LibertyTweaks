using System;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using IVSDKDotNet;
using System.Reflection;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;
using LibertyTweaks.Enhancements.Combat;
using System.Numerics;
using LibertyTweaks.Enhancements.Driving;
using CCL.GTAIV;
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
        public static int CarCrashLevel { get; private set; }
        public static float CarCrashDamageAmountNormalized { get; private set; }
        public static int PlayerWantedLevel { get; private set; }
        public static float PlayerHealth { get; private set; }
        public static Vector3 PlayerPos {  get; private set; }
        public static List<UIntPtr> Peds { get; private set; } = new List<UIntPtr>();

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
            if (ArmoredCops.enableVests == true)
                ArmoredCops.LoadFiles();
        }
        #endregion

        private void Main_IngameStartup(object sender, EventArgs e)
        {
            QuickSave.IngameStartup();
            PersonalVehicle.IngameStartup();
            LoadingFadeIn.IngameStartup();
            WeaponMagazines.IngameStartup();

            // 1.7 Stuff
            //Killcam.IngameStartup();
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
            HolsterWeapons.Init(Settings);
            ImprovedAI.Init(Settings);
            WeaponMagazines.Init(Settings);
            SniperMovement.Init(Settings);
            SniperScopeToggle.Init(Settings);
            RemoveWeaponsOnDeath.Init(Settings);
            DynamicFOV.Init(Settings);
            QuickSave.Init(Settings);
            AutosaveOnCollectibles.Init(Settings);
            MoreCombatLines.Init(Settings);
            SearchBody.Init(Settings);
            VLikeScreaming.Init(Settings);
            ArmoredCops.Init(Settings);
            LoseStarsWhileUnseen.Init(Settings);
            HealthRegeneration.Init(Settings);
            ToggleHUD.Init(Settings);
            Recoil.Init(Settings);
            CarFireBreakdown.Init(Settings);
            RealisticReloading.Init(Settings);
            PersonalVehicle.Init(this, Settings);
            ExtendedPedWeaponPool.Init(Settings);
            PedsLockDoors.Init(Settings);
            ArmorPenetration.Init(Settings);
            IncreasedDamage.Init(Settings);
            CarExplosionsRandomized.Init(Settings);
            StaminaProgression.Init(Settings);
            WeaponProgression.Init(Settings);
            NoOvertaking.Init(Settings);
            NoCursorEscape.Init(Settings);
            IceCreamSpeechFix.Init(Settings);
            WheelFix.Init(Settings);
            UnholsteredGunFix.Init(Settings);
            BrakeLights.Init(Settings);
            ExtraHospitalSpawn.Init(Settings);
            CopShotgunFix.Init(Settings);
            LoadingFadeIn.Init(Settings);
            DynamicMovement.Init(Settings);
            AllowCopsAllMissions.Init(Settings);
            CameraShake.Init(Settings);
            CameraRotation.Init(Settings);
            HighRPMShaking.Init(Settings);
            ImprovedCrashes.Init(Settings);
            DrivebyInPolice.Init(Settings);
            NoWantedInVigilante.Init(Settings);
            NoShootWithPhone.Init(Settings);
            DisableHUDOnBlindfire.Init(Settings);
            DisableCrosshairWithNoHUD.Init(Settings);
            CarCrashShake.Init(Settings);
            CarRollover.Init(Settings);
            SkipRadioTrack.Init(Settings);
            ShoulderSwap.Init(Settings);
            PNSOverhaul.Init(Settings);
            RunWithPhone.Init(Settings);
            TimeSkipFix.Init(Settings);

            // 1.7 Stuff
            //Killcam.Init(Settings);
            //QuickSwitching.Init(Settings); 
            //WarpToShore.Init(Settings);
            //DisableSprintWithHeavyWeapons.Init(Settings);
            //RunAndGun.Init(Settings);
            //DynamicCrosshair.Init(Settings);
            //RandomPedCarColors.Init(Settings);
            //ImprovedRolling.Init(Settings);
        }

        private void Main_ProcessCamera(object sender, EventArgs e)
        {
            DynamicFOV.Tick();
            CameraShake.Tick();
            CameraRotation.Tick();
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
            PedHelper.GrabAllPeds();
            PedHelper.GrabAllVehicles();

            if (CarCrashLevel != 0)
                CarCrashLevel = 0;

            if (CarCrashDamageAmountNormalized != 0)
                CarCrashDamageAmountNormalized = 0;

            PlayerVehicle = IS_CHAR_IN_ANY_CAR(PlayerPed.GetHandle()) ? IVVehicle.FromUIntPtr(PlayerPed.GetVehicle()) : null;

            var (_, damageLevel, normalizedDamage) = PlayerHelper.GetVehicleDamage();
            CarCrashLevel = damageLevel;
            CarCrashDamageAmountNormalized = normalizedDamage;

            STORE_WANTED_LEVEL(PlayerIndex, out uint playerWantedLevel);
            PlayerWantedLevel = (int)playerWantedLevel;

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
            ImprovedAI.Tick();
            WeaponMagazines.Tick();
            SniperMovement.Tick();
            SniperScopeToggle.Tick();
            MoreCombatLines.Tick();
            SearchBody.Tick();
            ArmoredCops.Tick();
            HealthRegeneration.Tick();
            Recoil.Tick();
            RealisticReloading.Tick();
            QuickSave.Tick();
            ExtendedPedWeaponPool.Tick();
            PedsLockDoors.Tick();
            ArmorPenetration.Tick();
            IncreasedDamage.Tick();
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
            PersonalVehicle.Tick();
            NoShootWithPhone.Tick();
            DisableHUDOnBlindfire.Tick();
            DisableCrosshairWithNoHUD.Tick();
            CameraShake.Tick();
            CarCrashShake.Tick();
            NoOvertaking.Tick();
            CarRollover.Tick();
            ShoulderSwap.Tick();
            PNSOverhaul.Tick();
            RunWithPhone.Tick();
            TimeSkipFix.Tick();
            //ImprovedRolling.Tick();
        }

        private void FixedTickFeatures()
        {
            AutosaveOnCollectibles.Tick();
            LoseStarsWhileUnseen.Tick();
            VLikeScreaming.Tick();
            CarFireBreakdown.Tick();
            CopShotgunFix.Tick();
            AllowCopsAllMissions.Tick();
            UnholsteredGunFix.Tick();
            NoWantedInVigilante.Tick();
        }

        private void UnreleasedFeatures()
        {
            Killcam.Tick();
            QuickSwitching.Tick();
            WarpToShore.Tick();
            DisableSprintWithHeavyWeapons.Tick();
            RunAndGun.Tick();
            RandomPedCarColors.Tick();
        }
        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == WarpToShore.key)
            //    WarpToShore.Process();

            if (e.KeyCode == ShoulderSwap.key)
                ShoulderSwap.Process();

            if (e.KeyCode == SkipRadioTrack.key)
                SkipRadioTrack.Process();

            if (e.KeyCode == ToggleHUD.key)
                ToggleHUD.Process();

            if (e.KeyCode == QuickSave.key)
                QuickSave.Process();

            if (e.KeyCode == HolsterWeapons.key)
                HolsterWeapons.Process();

            if (NativeControls.IsGameKeyPressed(0, GameKey.Sprint))
                DynamicMovement.Process();
            
            if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && PersonalVehicle.canBeTracked)
                PersonalVehicle.Process();
        }
        public static void Log(string message, [CallerFilePath] string filePath = "")
        {
            if (verboseLogging == true)
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                IVGame.Console.Print($"Liberty Tweaks - [{fileName}] {message}");
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
    }
}

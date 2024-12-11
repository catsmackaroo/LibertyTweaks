using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using IVSDKDotNet;
using DocumentFormat.OpenXml.Office.CustomUI;
using System.Reflection;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;
using CCL.GTAIV;
using LibertyTweaks.Enhancements.Combat;
using System.Numerics;

namespace LibertyTweaks
{
    public class Main : Script
    {
        #region Variables
        private static Random rnd; 
        public static bool verboseLogging;
        public static bool gxtEntries;
        public static bool debugMode;
        public static Keys debugMenuKey;
        public DateTime timer;
        private static CustomIVSave saveGame;
        public static DelayedCalling TheDelayedCaller;

        public static IVPed PlayerPed { get; private set; }
        public static int PlayerIndex { get; private set; }
        public static int PlayerWantedLevel { get; private set; }
        public static Vector3 PlayerPos {  get; private set; }
        public static List<UIntPtr> Peds { get; private set; } = new List<UIntPtr>();

        #endregion

        #region Functions
        public static int GenerateRandomNumber(int x, int y)
        {
            return rnd.Next(x, y);
        }
        internal static CustomIVSave GetTheSaveGame()
        {
            return saveGame;
        }
        #endregion

        #region Constructor
        public Main()
        {
            rnd = new Random();

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
            Killcam.IngameStartup();
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            // Misc
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
            RemoveWeapons.Init(Settings);
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
            //WeaponProgression.Init(Settings);
            Killcam.Init(Settings);
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
            //DebugTests.Init(Settings);
        }

        private void Main_ProcessCamera(object sender, EventArgs e)
        {
            DynamicFOV.Tick();
        }

        private void Main_ProcessAutomobile(UIntPtr vehPtr)
        {
            WheelFix.Process(vehPtr);
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            // Helpers & Player Data
            PlayerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            PlayerPos = Main.PlayerPed.Matrix.Pos;
            PlayerIndex = (int)GET_PLAYER_ID();
            STORE_WANTED_LEVEL(PlayerIndex, out uint PlayerWantedLevel);
            PedHelper.GrabAllPeds();
            PedHelper.GrabAllVehicles();

            // Main
            NoOvertaking.Tick();
            RemoveWeapons.Tick();
            ImprovedAI.Tick();
            WeaponMagazines.Tick();
            SniperMovement.Tick();  
            SniperScopeToggle.Tick();
            MoreCombatLines.Tick();
            SearchBody.Tick();
            VLikeScreaming.Tick();
            ArmoredCops.Tick();
            LoseStarsWhileUnseen.Tick();
            HealthRegeneration.Tick();
            CarFireBreakdown.Tick();
            Recoil.Tick();
            RealisticReloading.Tick();
            QuickSave.Tick();
            AutosaveOnCollectibles.Tick();
            PersonalVehicle.Tick();
            ExtendedPedWeaponPool.Tick();
            PedsLockDoors.Tick();
            ArmorPenetration.Tick();
            IncreasedDamage.Tick();
            CarExplosionsRandomized.Tick();
            StaminaProgression.Tick();
            //WeaponProgression.Tick();
            Killcam.Tick();
            ToggleHUD.Tick();
            BrakeLights.Tick();
            CopShotgunFix.Tick();
            ExtraHospitalSpawn.Tick();
            IceCreamSpeechFix.Tick();
            WheelFix.PreChecks();
            LoadingFadeIn.Tick();
            DynamicMovement.Tick();
            UnholsteredGunFix.Tick();
            AllowCopsAllMissions.Tick();
            HolsterWeapons.Tick();

            // Scrapped Ideas
            //WorkingPhoneCamera.Tick();
            //AppropriatePoliceScanner.Tick();

            // Other
            TheDelayedCaller.Process();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.B)
            //    StaminaProgression.Process();

            //if (e.KeyCode == DebugTests.key)
            //    DebugTests.Process();

            if (e.KeyCode == ToggleHUD.key)
                ToggleHUD.Process();

            if (e.KeyCode == QuickSave.key)
                QuickSave.Process();

            if (e.KeyCode == HolsterWeapons.key)
                HolsterWeapons.Process();

            if (e.KeyCode == Keys.LShiftKey)
                DynamicMovement.Process();
            
            if (e.KeyCode == PersonalVehicle.personalVehicleKey)
                PersonalVehicle.Process();
        }
        public static void Log(string message, [CallerFilePath] string filePath = "")
        {
            if (verboseLogging == true)
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                IVGame.Console.Print($"Liberty Tweaks - [{fileName}] {message}");
            }
        }
        public static void LogError(string message)
        {
            IVGame.Console.Print("Liberty Tweaks - Error: " + message);
        }
    }
}

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using IVSDKDotNet;

namespace LibertyTweaks
{
    public class Main : Script
    {

        #region Variables
        private static Random rnd; 
        public static bool verboseLogging;
        public static bool gxtEntries;
        public DateTime timer;
        private static CustomIVSave saveGame;
        public static DelayedCalling TheDelayedCaller;
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
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            OnlyRaiseKeyEventsWhenInGame = true;

            // Misc
            verboseLogging = Settings.GetBoolean("Liberty Tweaks", "Verbose Logging", true);
            gxtEntries = Settings.GetBoolean("Liberty Tweaks", "GXT Edits", true);
            saveGame = CustomIVSave.CreateOrLoadSaveGameData(this);

            // MAIN
            HolsterWeapons.Init(Settings);
            ImprovedAI.Init(Settings);
            WeaponMagazines.Init(Settings);
            MoveWithSniper.Init(Settings);
            RemoveWeapons.Init(Settings);
            FOV.Init(Settings);
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
            PersonalVehicle.Init(Settings);
            PedsLockDoors.Init(Settings);

            // FIXES
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
        }

        private void Main_ProcessCamera(object sender, EventArgs e)
        {
            FOV.Tick();
        }

        private void Main_ProcessAutomobile(UIntPtr vehPtr)
        {
            WheelFix.Process(vehPtr);
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            // Main
            NoOvertaking.Tick();
            RemoveWeapons.Tick();
            ImprovedAI.Tick();
            WeaponMagazines.Tick();
            MoveWithSniper.Tick();
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
            PedsLockDoors.Tick();

            // Fixes
            BrakeLights.Tick();
            CopShotgunFix.Tick();
            ExtraHospitalSpawn.Tick();
            IceCreamSpeechFix.Tick();
            WheelFix.PreChecks();
            LoadingFadeIn.Tick();
            DynamicMovement.Tick();
            UnholsteredGunFix.Tick();

            // Other
            TheDelayedCaller.Process();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {         
            if (e.KeyCode == ToggleHUD.toggleHudKey)
            {
                ToggleHUD.Process();
            }

            if (e.KeyCode == QuickSave.quickSaveKey)
            {
                QuickSave.Process();
            }

            if (e.KeyCode == HolsterWeapons.holsterKey)
            {
                HolsterWeapons.Process();
            }

            if (e.KeyCode == Keys.LShiftKey)
            {
                DynamicMovement.Process();
            }
            
            if (e.KeyCode == PersonalVehicle.personalVehicleKey)
            {
                PersonalVehicle.Process();
            }
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

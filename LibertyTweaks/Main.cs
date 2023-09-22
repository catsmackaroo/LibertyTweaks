using System;
using System.Windows.Forms;
using CorpseThief.SearchBody;
using FallScreaming.VLikeScreaming;
using ImprovedDialogue.CombatVoices;
using IVSDKDotNet;
using LibertyTweaks.GunMags;
using LibertyTweaks.QuickSaveFunc;

// DONE:
// HolsterWeapons
// NoOvertaking
// WheelFix
// QuickSave

// todo: C Smack - Death Blips, RealisticReloading, Weapon Variety, Auto Run, TrainFix, PersonalVehicle/Mechanic, & Fix Holster for MoveWithSniper

namespace LibertyTweaks
{
    public class Main : Script 
    {

        #region Variables
        private static Random rnd; 

        public float fovMulti;
        public int pedAccuracy;
        public int pedFirerate;
        private Keys quickSaveKey;
        private Keys holsterKey;
        #endregion

        #region Functions
        public static int GenerateRandomNumber(int x, int y)
        {
            return rnd.Next(x, y);
        }
        #endregion

        #region Constructor
        public Main() 
        {
            // Mod stuff
            rnd = new Random();

            // IV-SDK .NET Stuff
            Initialized += Main_Initialized;
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            ProcessAutomobile += Main_ProcessAutomobile;
            ProcessCamera += Main_ProcessCamera;

            // Below was removed as results were inconsistent. IV Tweaker is required for now
            //  GameLoad += Main_GameLoad;
        }
        #endregion

        private void Main_Initialized(object sender, EventArgs e)
        {
            // Check .INI
                // MAIN
            HolsterWeapons.Init(Settings);
            HigherPedAccuracy.Init(Settings);
            WeaponMagazines.Init(Settings);
            MoveWithSniper.Init(Settings);
            RemoveWeapons.Init(Settings);
            TweakableFOV.Init(Settings);
            QuickSave.Init(Settings);
            BrakeLights.Init(Settings);
            MoreCombatLines.Init(Settings);
            SearchBody.Init(Settings);
            VLikeScreaming.Init(Settings);

                // FIXES
            NoOvertaking.Init(Settings);
            IceCreamSpeechFix.Init(Settings);
            WheelFix.Init(Settings);

                // HOTKEYS & CONFIG
            quickSaveKey = Settings.GetKey("HOTKEYS", "Quick Save Key", Keys.F9);
            holsterKey = Settings.GetKey("HOTKEYS", "Holster Key", Keys.H);
            fovMulti = Settings.GetFloat("SETTINGS", "Field of View Modifier", 1.07f);
            pedAccuracy = Settings.GetInteger("SETTINGS", "Ped Accuracy", 85);
            pedFirerate = Settings.GetInteger("SETTINGS", "Ped Firerate", 85);

        }

        // Below was removed as results were inconsistent. IV Tweaker is required for now
        //private void Main_GameLoad(object sender, EventArgs e)
        //{
        //    WeaponMagazines.LoadFiles();
        //}

        private void Main_ProcessCamera(object sender, EventArgs e)
        {
            // Here we can override camera things like fov
            TweakableFOV.Tick(fovMulti);
        }
        private void Main_ProcessAutomobile(UIntPtr vehPtr)
        {
            // Here we can override vehicle things like steering
            WheelFix.Process(vehPtr);
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            NoOvertaking.Tick();
            WheelFix.PreChecks();
            RemoveWeapons.Tick();
            HigherPedAccuracy.Tick(pedAccuracy, pedFirerate);
            WeaponMagazines.Tick();
            IceCreamSpeechFix.Tick();
            MoveWithSniper.Tick();
            BrakeLights.Tick();
            MoreCombatLines.Tick();
            SearchBody.Tick();
            VLikeScreaming.Tick();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == quickSaveKey)
            {
                QuickSave.Process();
            }

            if (e.KeyCode == holsterKey)
            {
                HolsterWeapons.Process();
            }
        }

    }
}

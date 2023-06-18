using System;
using System.Windows.Forms;

using IVSDKDotNet;
using LibertyTweaks.GunMags;
using LibertyTweaks.HolsterWeapons;
using LibertyTweaks.QuickSaveFunc;

// DONE:
// HolsterWeapons
// NoOvertaking
// WheelFix
// QuickSave

// todo: C Smack - Death Blips, RealisticReloading, Weapon Variety, Auto Run, PersonalVehicle/Mechanic, & Fix Holster for MoveWithSniper

namespace LibertyTweaks
{
    public class Main : Script 
    {

        #region Variables
        private static Random rnd; 

        public float fovMulti;
        private Keys quickSaveKey;
        private Keys holsterKey;
        private Keys vehicleLightsKey;
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
            GameLoad += Main_GameLoad;
            GameLoadPriority += Main_GameLoadPriority;
        }
        #endregion

        private void Main_Initialized(object sender, EventArgs e)
        {
            // Check .INI Enable/Disable Section
                // MAIN
            HolsterWeapons.HolsterWeapons.Init(Settings);
            HigherPedAccuracy.HigherPedAccuracy.Init(Settings);
            WeaponMagazines.Init(Settings);
            MoveWithSniper.MoveWithSniper.Init(Settings);
            RemoveWeapons.RemoveWeapons.Init(Settings);
            TweakableFOV.TweakableFOV.Init(Settings);
            QuickSave.Init(Settings);
                // FIXES
            NoOvertaking.NoOvertaking.Init(Settings);
            IceCreamSpeech.IceCreamSpeechFix.Init(Settings);
            WheelFix.WheelFix.Init(Settings);
            

            // HOTKEYS
            // Quick-Save
            quickSaveKey = Settings.GetKey("Hotkeys", "Quick Save Key", Keys.F9);

            // Holstering
            holsterKey = Settings.GetKey("Hotkeys", "Holster Key", Keys.H);

            // Field of View Multiplier
            fovMulti = Settings.GetFloat("Hotkeys", "Field of View Modifier", 1.07f);
        }

        private void Main_GameLoad(object sender, EventArgs e)
        {
            GunMags.WeaponMagazines.LoadFiles();
        }
        private void Main_GameLoadPriority(object sender, EventArgs e)
        {
            GunMags.WeaponMagazines.LoadPriorityFiles();
        }

        private void Main_ProcessCamera(object sender, EventArgs e)
        {
            // Here we can override camera things like fov
            TweakableFOV.TweakableFOV.Tick(fovMulti);
        }
        private void Main_ProcessAutomobile(UIntPtr vehPtr)
        {
            // Here we can override vehicle things like steering
            WheelFix.WheelFix.Process(vehPtr);
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            NoOvertaking.NoOvertaking.Tick();
            WheelFix.WheelFix.PreChecks();
            RemoveWeapons.RemoveWeapons.Tick();
            HigherPedAccuracy.HigherPedAccuracy.Tick();
            GunMags.WeaponMagazines.Tick();
            IceCreamSpeech.IceCreamSpeechFix.Tick();
            MoveWithSniper.MoveWithSniper.Tick();
            //RealisticReloading.RealisticReloading.Tick();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == quickSaveKey)
            {
                QuickSaveFunc.QuickSave.Process();
            }

            if (e.KeyCode == holsterKey)
            {
                HolsterWeapons.HolsterWeapons.Process();
            }
        }

    }
}

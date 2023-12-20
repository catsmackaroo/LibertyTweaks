using System;
using System.Windows.Forms;
using IVSDKDotNet;

namespace LibertyTweaks
{
    public class Main : Script 
    {

        #region Variables
        private static Random rnd; 

        public float fovMulti;
        public int pedAccuracy;
        public int pedFirerate;
        public int armoredCopsStars;
        public int unseenSlipAwayMinTimer;
        public int unseenSlipAwayMaxTimer;
        public int regenHealthMinTimer;
        public int regenHealthMaxTimer;
        public int regenHealthMinHeal;
        public int regenHealthMaxHeal;
        public DateTime timer;
        private Keys quickSaveKey;
        private Keys holsterKey;
        private Keys toggleHudKey;
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
            rnd = new Random();

            Initialized += Main_Initialized;
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            ProcessAutomobile += Main_ProcessAutomobile;
            ProcessCamera += Main_ProcessCamera;

            GameLoad += Main_GameLoad;
        }

        private void Main_GameLoad(object sender, EventArgs e)
        {
            WeaponMagazines.LoadFiles();
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
            ArmoredCops.Init(Settings);
            UnseenSlipAway.Init(Settings);
            RegenerateHP.Init(Settings);
            ToggleHUD.Init(Settings);
            //StunPunch.Init(Settings);
            CarFireBreakdown.Init(Settings);
            //DeathBlips.Init(Settings);

                // FIXES
            NoOvertaking.Init(Settings);
            IceCreamSpeechFix.Init(Settings);
            WheelFix.Init(Settings);
            UnholsteredGunFix.Init(Settings);

                // HOTKEYS & CONFIG
            quickSaveKey = Settings.GetKey("Quick-Saving", "Key", Keys.F9);
            holsterKey = Settings.GetKey("Weapon Holstering", "Key", Keys.H);
            fovMulti = Settings.GetFloat("Tweakable FOV", "Multiplier", 1.07f);
            pedAccuracy = Settings.GetInteger("Improved AI", "Accuracy", 85);
            pedFirerate = Settings.GetInteger("Improved AI", "Firerate", 85);
            armoredCopsStars = Settings.GetInteger("Improved Police", "Armored Cops Start At", 4);
            unseenSlipAwayMinTimer = Settings.GetInteger("Improved Police", "Lose Stars While Unseen Minimum Count", 60);
            unseenSlipAwayMaxTimer = Settings.GetInteger("Improved Police", "Lose Stars While Unseen Maximum Count", 120);
            regenHealthMinTimer = Settings.GetInteger("Health Regeneration", "Regen Timer Minimum", 30);
            regenHealthMaxTimer = Settings.GetInteger("Health Regeneration", "Regen Timer Maximum", 60);
            regenHealthMinHeal = Settings.GetInteger("Health Regeneration", "Minimum Heal Amount", 5);
            regenHealthMaxHeal = Settings.GetInteger("Health Regeneration", "Maximum Heal Amount", 10);
            toggleHudKey = Settings.GetKey("Toggle HUD", "Key", Keys.K);
        }

        private void Main_ProcessCamera(object sender, EventArgs e)
        {
            TweakableFOV.Tick(fovMulti);
        }
        private void Main_ProcessAutomobile(UIntPtr vehPtr)
        {
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
            UnholsteredGunFix.Tick();
            ArmoredCops.Tick(armoredCopsStars);
            UnseenSlipAway.Tick(timer, unseenSlipAwayMinTimer, unseenSlipAwayMaxTimer);
            RegenerateHP.Tick(timer, regenHealthMinTimer, regenHealthMaxTimer, regenHealthMinHeal, regenHealthMaxHeal);
            //StunPunch.Tick();
            CarFireBreakdown.Tick();
            //DeathBlips.Tick();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == toggleHudKey)
            {
                ToggleHUD.Process();
            }

            if (e.KeyCode == quickSaveKey)
            {
                QuickSave.Process();
            }

            if (e.KeyCode == holsterKey)
            {
                HolsterWeapons.Process();
            }

            //if (e.KeyCode == Keys.LButton)
            //{
            //    StunPunch.Process();
            //}
        }
    }
}

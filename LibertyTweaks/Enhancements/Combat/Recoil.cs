using CCL.GTAIV;
using DocumentFormat.OpenXml.Wordprocessing;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credit: catsmackaroo

namespace LibertyTweaks
{
    internal class Recoil
    {
        private static bool enable;
        public static float recoilSmallPistolAmp1;
        public static float recoilSmallPistolAmp2;
        public static float recoilSmallPistolFreq1;
        public static float recoilSmallPistolFreq2;

        public static float recoilHeavyPistolAmp1;
        public static float recoilHeavyPistolAmp2;
        public static float recoilHeavyPistolFreq1;
        public static float recoilHeavyPistolFreq2;

        public static float recoilShotgunsAmp1;
        public static float recoilShotgunsAmp2;
        public static float recoilShotgunsFreq1;
        public static float recoilShotgunsFreq2;

        public static float recoilSMGAmp1;
        public static float recoilSMGAmp2;
        public static float recoilSMGFreq1;
        public static float recoilSMGFreq2;

        public static float recoilAssaultRiflesAmp1;
        public static float recoilAssaultRiflesAmp2;
        public static float recoilAssaultRiflesFreq1;
        public static float recoilAssaultRiflesFreq2;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Immersive Recoil", "Enable", true);

            recoilSmallPistolAmp1 = settings.GetFloat("Extensive Settings", "Pistol Amplitude 1", 0.4f);
            recoilSmallPistolAmp2 = settings.GetFloat("Extensive Settings", "Pistol Amplitude 2", 0.6f);
            recoilSmallPistolFreq1 = settings.GetFloat("Extensive Settings", "Pistol Frequency 1", 0.1f);
            recoilSmallPistolFreq2 = settings.GetFloat("Extensive Settings", "Pistol Frequency 2", 0.3f);

            recoilHeavyPistolAmp1 = settings.GetFloat("Extensive Settings", "Heavy Pistol Amplitude 2", 0.2f);
            recoilHeavyPistolAmp2 = settings.GetFloat("Extensive Settings", "Heavy Pistol Ampltitude 2", 0.4f);
            recoilHeavyPistolFreq1 = settings.GetFloat("Extensive Settings", "Heavy Pistol Frequency 1", 0.3f);
            recoilHeavyPistolFreq2 = settings.GetFloat("Extensive Settings", "Heavy Pistol Frequency 2", 0.5f);

            recoilShotgunsAmp1 = settings.GetFloat("Extensive Settings", "Shotgun Amplitude 1", 0.3f);
            recoilShotgunsAmp2 = settings.GetFloat("Extensive Settings", "Shotgun Amplitude 2", 0.7f);
            recoilShotgunsFreq1 = settings.GetFloat("Extensive Settings", "Shotgun Frequency 1", 0.4f);
            recoilShotgunsFreq2 = settings.GetFloat("Extensive Settings", "Shotgun Frequency 2", 0.7f);

            recoilSMGAmp1 = settings.GetFloat("Extensive Settings", "SMG Amplitude 1", 0.4f);
            recoilSMGAmp2 = settings.GetFloat("Extensive Settings", "SMG Amplitude 2", 0.6f);
            recoilSMGFreq1 = settings.GetFloat("Extensive Settings", "SMG Frequency 1", 0.1f);
            recoilSMGFreq2 = settings.GetFloat("Extensive Settings", "SMG Frequency 2", 0.3f);

            recoilAssaultRiflesAmp1 = settings.GetFloat("Extensive Settings", "Assault Rifle Amplitude 1", 0.4f);
            recoilAssaultRiflesAmp2 = settings.GetFloat("Extensive Settings", "Assault Rifle Amplitude 2", 0.6f);
            recoilAssaultRiflesFreq1 = settings.GetFloat("Extensive Settings", "Assault Rifle Frequency 1", 0.1f);
            recoilAssaultRiflesFreq2 = settings.GetFloat("Extensive Settings", "Assault Rifle Frequency 2", 0.6f);

            if (enable)
                Main.Log("script initialized...");
        }


        public static void Tick()
        {
            if (!enable)
                return;

            int playerId;
            NativeCamera cam;
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            playerId = IVPedExtensions.GetHandle(playerPed);

            if (IS_CHAR_SHOOTING(playerId))
            {
                cam = NativeCamera.GetGameCam();

                GET_CURRENT_CHAR_WEAPON(playerId, out int currentWeapon);

                // Small Pistols (Pistol & Automatic Pistol)
                if (currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_7
                    || currentWeapon == (int)eWeaponType.WEAPON_PISTOL)
                {
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 140, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSmallPistolAmp1, recoilSmallPistolAmp2), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSmallPistolFreq1, recoilSmallPistolFreq2), 0f);
                    cam.Shake(CameraShakeType.ROLL_LEFT_RIGHT, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 140, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSmallPistolAmp1 + -0.15f, recoilSmallPistolAmp2 + -0.15f), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSmallPistolFreq1, recoilSmallPistolFreq2), 0f);
                }

                // Heavy Pistols (Deagle & .44 Automag)
                if (currentWeapon == (int)eWeaponType.WEAPON_DEAGLE || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_9)
                { // 0.2 0.3
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 160, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilHeavyPistolAmp1, recoilHeavyPistolAmp2), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilHeavyPistolFreq1, recoilHeavyPistolFreq2), 0f);
                    cam.Shake(CameraShakeType.ROLL_LEFT_RIGHT, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 160, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilHeavyPistolAmp1 + -0.15f, recoilHeavyPistolAmp2 + -0.15f), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilHeavyPistolFreq1, recoilHeavyPistolFreq2), 0f);
                }

                // Shotguns (Pump Shotgun, Combat Shotgun, AA12 (Explosive), AA12 (Regular), Street Sweeper, Sawn-off) 
                if (currentWeapon == (int)eWeaponType.WEAPON_SHOTGUN || currentWeapon == (int)eWeaponType.WEAPON_BARETTA
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_11 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_10
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_2 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_6)
                {
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 200, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilShotgunsAmp1, recoilShotgunsAmp2), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilShotgunsFreq1, recoilShotgunsFreq2), 0f);
                    cam.Shake(CameraShakeType.ROLL_LEFT_RIGHT, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 200, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilShotgunsAmp1 + -0.15f, recoilShotgunsAmp2 + -0.15f), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilShotgunsFreq1, recoilShotgunsFreq2), 0f);
                }

                // SMGs (UZI, MP5, P90, & Gold UZI)
                if (currentWeapon == (int)eWeaponType.WEAPON_MICRO_UZI || currentWeapon == (int)eWeaponType.WEAPON_MP5
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_12 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_13)
                {
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 140, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSMGAmp1, recoilSMGAmp2), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSMGFreq1, recoilSMGFreq2), 0f);
                    cam.Shake(CameraShakeType.ROLL_LEFT_RIGHT, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 140, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSMGAmp1 + -0.15f, recoilSMGAmp2 + -0.15f), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilSMGFreq1, recoilSMGFreq2), 0f);
                }

                // Assault Rifles (AK47, M4, & LMG
                if (currentWeapon == (int)eWeaponType.WEAPON_AK47 || currentWeapon == (int)eWeaponType.WEAPON_M4
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_14)
                {
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 140, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilAssaultRiflesAmp1, recoilAssaultRiflesAmp2), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilAssaultRiflesFreq1, recoilAssaultRiflesFreq2), 0f);
                    cam.Shake(CameraShakeType.ROLL_LEFT_RIGHT, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 140, GENERATE_RANDOM_FLOAT_IN_RANGE(recoilAssaultRiflesAmp1 + -0.15f, recoilAssaultRiflesAmp2 + -0.15f), GENERATE_RANDOM_FLOAT_IN_RANGE(recoilAssaultRiflesFreq1, recoilAssaultRiflesFreq2 + -0.15f), 0f);
                }
            }
        }
    }
}

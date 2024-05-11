using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credit: catsmackaroo

namespace LibertyTweaks
{
    internal class Recoil
    {
        private static bool enable;
        private static uint currentWeapon;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Immersive Recoil", "Enable", true);
        }


        public static void Tick(float recoilSmallPistolAmp1, float recoilSmallPistolAmp2, float recoilSmallPistolFreq1, float recoilSmallPistolFreq2,
             float recoilHeavyPistolAmp1, float recoilHeavyPistolAmp2, float recoilHeavyPistolFreq1, float recoilHeavyPistolFreq2,
             float recoilShotgunsAmp1, float recoilShotgunsAmp2, float recoilShotgunsFreq1, float recoilShotgunsFreq2,
             float recoilSMGAmp1, float recoilSMGAmp2, float recoilSMGFreq1, float recoilSMGFreq2,
             float recoilAssaultRiflesAmp1, float recoilAssaultRiflesAmp2, float recoilAssaultRiflesFreq1, float recoilAssaultRiflesFreq2)
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

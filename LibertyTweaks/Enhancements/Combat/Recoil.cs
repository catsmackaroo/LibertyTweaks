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
            enable = settings.GetBoolean("Weapon Recoil", "Enable", true);
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

                GET_CURRENT_CHAR_WEAPON(playerId, out currentWeapon);

                if (currentWeapon == (int)eWeaponType.WEAPON_AK47 || currentWeapon == (int)eWeaponType.WEAPON_M4 
                    || currentWeapon == (int)eWeaponType.WEAPON_MICRO_UZI || currentWeapon == (int)eWeaponType.WEAPON_MP5
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_12 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_13
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_14|| currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_7
                    || currentWeapon == (int)eWeaponType.WEAPON_PISTOL)
                {
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 140, GENERATE_RANDOM_FLOAT_IN_RANGE(0.4f, 0.6f), GENERATE_RANDOM_FLOAT_IN_RANGE(0.1f, 0.3f), 0f);
                    cam.Shake(CameraShakeType.TRACK_LEFT_RIGHT, CameraShakeBehaviour.MEDIUM_FAST_EXPONENTIAL_PLUS_FADE_IN_OUT, 160, 0.2f, 0.11f, 0f);
                }

                if (currentWeapon == (int)eWeaponType.WEAPON_RLAUNCHER)
                {
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 300, GENERATE_RANDOM_FLOAT_IN_RANGE(0.6f, 0.8f), 0.2f, 0f);
                    cam.Shake(CameraShakeType.TRACK_LEFT_RIGHT, CameraShakeBehaviour.MEDIUM_FAST_EXPONENTIAL_PLUS_FADE_IN_OUT, 260, 0.5f, 0.6f, 0f);
                }

                if (currentWeapon == (int)eWeaponType.WEAPON_SHOTGUN || currentWeapon == (int)eWeaponType.WEAPON_BARETTA
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_11 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_10
                    || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_2 || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_6)
                {
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 200, GENERATE_RANDOM_FLOAT_IN_RANGE(0.3f, 0.7f), 0.4f, 0f);
                    cam.Shake(CameraShakeType.TRACK_LEFT_RIGHT, CameraShakeBehaviour.MEDIUM_FAST_EXPONENTIAL_PLUS_FADE_IN_OUT, 160, GENERATE_RANDOM_FLOAT_IN_RANGE(0.4f, 0.6f), 0.11f, 0f);
                }

                if (currentWeapon == (int)eWeaponType.WEAPON_DEAGLE || currentWeapon == (int)eWeaponType.WEAPON_EPISODIC_9)
                { // 0.2 0.3
                    cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 160, GENERATE_RANDOM_FLOAT_IN_RANGE(0.2f, 0.4f), 0.3f, 0f);
                    cam.Shake(CameraShakeType.TRACK_LEFT_RIGHT, CameraShakeBehaviour.MEDIUM_FAST_EXPONENTIAL_PLUS_FADE_IN_OUT, 160, GENERATE_RANDOM_FLOAT_IN_RANGE(0.4f, 0.8f), 0.10f, 0f);
                }
            }
        }
    }
}

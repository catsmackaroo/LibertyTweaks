using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    public enum WeaponGroup
    {
        SmallPistol = 5,
        HeavyPistol = 6,
        SMG = 8,
        Shotgun = 9,
        AssaultRifle = 10,
        Sniper = 11
    }

    [ShowStaticFieldsInInspector]

    internal class Recoil
    {
        // General Recoil Settings
        private static bool enable;
        public static float SmallPistolAmplitude;
        public static float SmallPistolAmplitude2;
        public static float SmallPistolFrequency;
        public static float SmallPistolFrequency2;

        public static float HeavyPistolAmplitude;
        public static float HeavyPistolAmplitude2;
        public static float HeavyPistolFrequency;
        public static float HeavyPistolFrequency2;

        public static float ShotgunsAmplitude1;
        public static float ShotgunsAmplitude2;
        public static float ShotgunsFrequency;
        public static float ShotgunsFrequency2;

        public static float SMGAmplitude;
        public static float SMGAmplitude2;
        public static float SMGFrequency;
        public static float SMGFrequency2;

        public static float AssaultRiflesAmplitude;
        public static float AssaultRiflesAmplitude2;
        public static float AssaultRiflesFrequency;
        public static float AssaultRiflesFrequency2;

        // Increased Recoil Settings
        private static bool enableIncrease;
        public static float BaseRecoil;
        public static float AdditionalRecoil;
        public static float CurrentRecoil;
        public static float DecayRate;
        public static float MaximumRecoil;
        public static float SmallPistolAdditional;
        public static float HeavyPistolAdditional;
        public static float ShotgunsAdditional;
        public static float SMGAdditional;
        public static float RifleAdditional;
        public static float CrouchMultiplier;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Immersive Recoil", "Enable", true);
            enableIncrease = settings.GetBoolean("Immersive Recoil", "Increasing Recoil", true);

            // Small Pistol Settings
            SmallPistolAmplitude = settings.GetFloat("Extensive Settings", "Pistol Amplitude Range 1", 0.4f);
            SmallPistolAmplitude2 = settings.GetFloat("Extensive Settings", "Pistol Amplitude Range 2", 0.6f);
            SmallPistolFrequency = settings.GetFloat("Extensive Settings", "Pistol Frequency Range 1", 0.1f);
            SmallPistolFrequency2 = settings.GetFloat("Extensive Settings", "Pistol Frequency Range 2", 0.3f);

            // Heavy Pistol Settings
            HeavyPistolAmplitude = settings.GetFloat("Extensive Settings", "Heavy Pistol Amplitude Range 1", 0.2f);
            HeavyPistolAmplitude2 = settings.GetFloat("Extensive Settings", "Heavy Pistol Amplitude Range 2", 0.4f);
            HeavyPistolFrequency = settings.GetFloat("Extensive Settings", "Heavy Pistol Frequency Range 1", 0.3f);
            HeavyPistolFrequency2 = settings.GetFloat("Extensive Settings", "Heavy Pistol Frequency Range 2", 0.5f);

            // Shotgun Settings
            ShotgunsAmplitude1 = settings.GetFloat("Extensive Settings", "Shotgun Amplitude Range 1", 0.3f);
            ShotgunsAmplitude2 = settings.GetFloat("Extensive Settings", "Shotgun Amplitude Range 2", 0.7f);
            ShotgunsFrequency = settings.GetFloat("Extensive Settings", "Shotgun Frequency Range 1", 0.4f);
            ShotgunsFrequency2 = settings.GetFloat("Extensive Settings", "Shotgun Frequency Range 2", 0.7f);

            // SMG Settings
            SMGAmplitude = settings.GetFloat("Extensive Settings", "SMG Amplitude Range 1", 0.4f);
            SMGAmplitude2 = settings.GetFloat("Extensive Settings", "SMG Amplitude Range 2", 0.6f);
            SMGFrequency = settings.GetFloat("Extensive Settings", "SMG Frequency Range 1", 0.1f);
            SMGFrequency2 = settings.GetFloat("Extensive Settings", "SMG Frequency Range 2", 0.3f);

            // Assault Rifle Settings
            AssaultRiflesAmplitude = settings.GetFloat("Extensive Settings", "Assault Rifle Amplitude Range 1", 0.4f);
            AssaultRiflesAmplitude2 = settings.GetFloat("Extensive Settings", "Assault Rifle Amplitude Range 2", 0.6f);
            AssaultRiflesFrequency = settings.GetFloat("Extensive Settings", "Assault Rifle Frequency Range 1", 0.1f);
            AssaultRiflesFrequency2 = settings.GetFloat("Extensive Settings", "Assault Rifle Frequency Range 2", 0.6f);

            // Increased Recoil Settings
            BaseRecoil = settings.GetFloat("Extensive Settings", "Base Recoil", 0.0f);
            AdditionalRecoil = settings.GetFloat("Extensive Settings", "Additional Recoil", 0.0f);
            DecayRate = settings.GetFloat("Extensive Settings", "Decay Rate", 0.02f);
            MaximumRecoil = settings.GetFloat("Extensive Settings", "Maximum Recoil", 1.0f);

            // Weapon-specific Additional Recoil
            SmallPistolAdditional = settings.GetFloat("Extensive Settings", "Small Pistol Additional", 0.3f);
            HeavyPistolAdditional = settings.GetFloat("Extensive Settings", "Heavy Pistol Additional", 0.4f);
            ShotgunsAdditional = settings.GetFloat("Extensive Settings", "Shotgun Additional", 0.55f);
            SMGAdditional = settings.GetFloat("Extensive Settings", "SMG Additional", 0.13f);
            RifleAdditional = settings.GetFloat("Extensive Settings", "Rifle Additional", 0.12f);
            CrouchMultiplier = settings.GetFloat("Extensive Settings", "Crouch Multiplier", 0.5f);

            if (enable)
                Main.Log("Recoil script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            NativeCamera cam = NativeCamera.GetGameCam();
            bool isPlayerCrouched = IS_CHAR_DUCKING(Main.PlayerPed.GetHandle());
            float appliedRecoil = CurrentRecoil;

            if (enableIncrease)
            {
                if (PlayerChecks.HasPlayerShotRecently())
                {
                    CurrentRecoil = Math.Min(CurrentRecoil + AdditionalRecoil, MaximumRecoil);
                }
                else
                {
                    CurrentRecoil = Math.Max(CurrentRecoil - DecayRate, BaseRecoil);
                }
            }

            if (isPlayerCrouched)
            {
                appliedRecoil *= CrouchMultiplier;
            }

            if (IS_CHAR_SHOOTING(Main.PlayerPed.GetHandle()) && !IS_CAM_SHAKING())
            {
                GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);
                IVWeaponInfo currentWeaponInfo = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon);
                WeaponGroup weaponGroup = (WeaponGroup)currentWeaponInfo.Group;

                switch (weaponGroup)
                {
                    case WeaponGroup.SmallPistol:
                        AdditionalRecoil = SmallPistolAdditional;
                        ApplyCameraShake(cam, SmallPistolAmplitude, SmallPistolAmplitude2, SmallPistolFrequency, SmallPistolFrequency2, appliedRecoil, 160);
                        break;

                    case WeaponGroup.HeavyPistol:
                        AdditionalRecoil = HeavyPistolAdditional;
                        ApplyCameraShake(cam, HeavyPistolAmplitude, HeavyPistolAmplitude2, HeavyPistolFrequency, HeavyPistolFrequency2, appliedRecoil, 200);
                        break;

                    case WeaponGroup.SMG:
                        AdditionalRecoil = SMGAdditional;
                        ApplyCameraShake(cam, SMGAmplitude, SMGAmplitude2, SMGFrequency, SMGFrequency2, appliedRecoil, 200);
                        break;

                    case WeaponGroup.Shotgun:
                        AdditionalRecoil = ShotgunsAdditional;
                        ApplyCameraShake(cam, ShotgunsAmplitude1, ShotgunsAmplitude2, ShotgunsFrequency, ShotgunsFrequency2, appliedRecoil, 250);
                        break;

                    case WeaponGroup.AssaultRifle:
                    case WeaponGroup.Sniper:
                        AdditionalRecoil = RifleAdditional;
                        ApplyCameraShake(cam, AssaultRiflesAmplitude, AssaultRiflesAmplitude2, AssaultRiflesFrequency, AssaultRiflesFrequency2, appliedRecoil, 175);
                        break;
                }
            }
        }

        private static void ApplyCameraShake(NativeCamera cam, float amplitude1, float amplitude2, float frequency1, float frequency2, float appliedRecoil, int duration)
        {
            cam.Shake(CameraShakeType.PITCH_UP_DOWN, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, duration,
                GENERATE_RANDOM_FLOAT_IN_RANGE(amplitude1, amplitude2) + appliedRecoil,
                GENERATE_RANDOM_FLOAT_IN_RANGE(frequency1, frequency2), 0f);

            float randomLeftRightAmplitude = GENERATE_RANDOM_FLOAT_IN_RANGE(-amplitude1, amplitude1);
            float randomIncreasingleftRightRecoil = GENERATE_RANDOM_FLOAT_IN_RANGE(-appliedRecoil, appliedRecoil);

            cam.Shake(CameraShakeType.ROLL_LEFT_RIGHT, CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, duration,
                randomLeftRightAmplitude + randomIncreasingleftRightRecoil,
                GENERATE_RANDOM_FLOAT_IN_RANGE(frequency1, frequency2), 0f);
        }


    }
}

using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{

    [ShowStaticFieldsInInspector]
    internal class Recoil
    {
        // General Recoil Settings
        private static bool enable;
        public static float SmallPistolAmplitude { get; private set; }
        public static float SmallPistolAmplitude2 { get; private set; }
        public static float SmallPistolFrequency { get; private set; }
        public static float SmallPistolFrequency2 { get; private set; }

        public static float HeavyPistolAmplitude { get; private set; }
        public static float HeavyPistolAmplitude2 { get; private set; }
        public static float HeavyPistolFrequency { get; private set; }
        public static float HeavyPistolFrequency2 { get; private set; }

        public static float ShotgunsAmplitude1 { get; private set; }
        public static float ShotgunsAmplitude2 { get; private set; }
        public static float ShotgunsFrequency { get; private set; }
        public static float ShotgunsFrequency2 { get; private set; }

        public static float SMGAmplitude { get; private set; }
        public static float SMGAmplitude2 { get; private set; }
        public static float SMGFrequency { get; private set; }
        public static float SMGFrequency2 { get; private set; }

        public static float AssaultRiflesAmplitude { get; private set; }
        public static float AssaultRiflesAmplitude2 { get; private set; }
        public static float AssaultRiflesFrequency { get; private set; }
        public static float AssaultRiflesFrequency2 { get; private set; }

        // Increased Recoil Settings
        private static bool enableIncrease;
        public static float BaseRecoil { get; private set; }
        public static float AdditionalRecoil { get; private set; }
        public static float CurrentRecoil { get; private set; }
        public static float DecayRate { get; private set; }
        public static float MaximumRecoil { get; private set; }
        public static float SmallPistolAdditional { get; private set; }
        public static float HeavyPistolAdditional { get; private set; }
        public static float ShotgunsAdditional { get; private set; }
        public static float SMGAdditional { get; private set; }
        public static float RifleAdditional { get; private set; }
        public static float CrouchMultiplier { get; private set; }

        private const string ExtensiveSettings = "Extensive Settings";
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            Recoil.section = section;
            enable = settings.GetBoolean(section, "Gun Recoil", false);
            enableIncrease = settings.GetBoolean(section, "Gun Recoil - Increases Overtime", false);

            // Small Pistol Settings
            SmallPistolAmplitude = settings.GetFloat(ExtensiveSettings, "Pistol Amplitude Range 1", 0.4f);
            SmallPistolAmplitude2 = settings.GetFloat(ExtensiveSettings, "Pistol Amplitude Range 2", 0.6f);
            SmallPistolFrequency = settings.GetFloat(ExtensiveSettings, "Pistol Frequency Range 1", 0.1f);
            SmallPistolFrequency2 = settings.GetFloat(ExtensiveSettings, "Pistol Frequency Range 2", 0.3f);

            // Heavy Pistol Settings
            HeavyPistolAmplitude = settings.GetFloat(ExtensiveSettings, "Heavy Pistol Amplitude Range 1", 0.2f);
            HeavyPistolAmplitude2 = settings.GetFloat(ExtensiveSettings, "Heavy Pistol Amplitude Range 2", 0.4f);
            HeavyPistolFrequency = settings.GetFloat(ExtensiveSettings, "Heavy Pistol Frequency Range 1", 0.3f);
            HeavyPistolFrequency2 = settings.GetFloat(ExtensiveSettings, "Heavy Pistol Frequency Range 2", 0.5f);

            // Shotgun Settings
            ShotgunsAmplitude1 = settings.GetFloat(ExtensiveSettings, "Shotgun Amplitude Range 1", 0.3f);
            ShotgunsAmplitude2 = settings.GetFloat(ExtensiveSettings, "Shotgun Amplitude Range 2", 0.7f);
            ShotgunsFrequency = settings.GetFloat(ExtensiveSettings, "Shotgun Frequency Range 1", 0.4f);
            ShotgunsFrequency2 = settings.GetFloat(ExtensiveSettings, "Shotgun Frequency Range 2", 0.7f);

            // SMG Settings
            SMGAmplitude = settings.GetFloat(ExtensiveSettings, "SMG Amplitude Range 1", 0.4f);
            SMGAmplitude2 = settings.GetFloat(ExtensiveSettings, "SMG Amplitude Range 2", 0.6f);
            SMGFrequency = settings.GetFloat(ExtensiveSettings, "SMG Frequency Range 1", 0.1f);
            SMGFrequency2 = settings.GetFloat(ExtensiveSettings, "SMG Frequency Range 2", 0.3f);

            // Assault Rifle Settings
            AssaultRiflesAmplitude = settings.GetFloat(ExtensiveSettings, "Assault Rifle Amplitude Range 1", 0.4f);
            AssaultRiflesAmplitude2 = settings.GetFloat(ExtensiveSettings, "Assault Rifle Amplitude Range 2", 0.6f);
            AssaultRiflesFrequency = settings.GetFloat(ExtensiveSettings, "Assault Rifle Frequency Range 1", 0.1f);
            AssaultRiflesFrequency2 = settings.GetFloat(ExtensiveSettings, "Assault Rifle Frequency Range 2", 0.6f);

            // Increased Recoil Settings
            BaseRecoil = settings.GetFloat(ExtensiveSettings, "Base Recoil", 0.0f);
            AdditionalRecoil = settings.GetFloat(ExtensiveSettings, "Additional Recoil", 0.0f);
            DecayRate = settings.GetFloat(ExtensiveSettings, "Decay Rate", 0.02f);
            MaximumRecoil = settings.GetFloat(ExtensiveSettings, "Maximum Recoil", 1.0f);

            // Weapon-specific Additional Recoil
            SmallPistolAdditional = settings.GetFloat(ExtensiveSettings, "Small Pistol Additional", 0.3f);
            HeavyPistolAdditional = settings.GetFloat(ExtensiveSettings, "Heavy Pistol Additional", 0.4f);
            ShotgunsAdditional = settings.GetFloat(ExtensiveSettings, "Shotgun Additional", 0.55f);
            SMGAdditional = settings.GetFloat(ExtensiveSettings, "SMG Additional", 0.13f);
            RifleAdditional = settings.GetFloat(ExtensiveSettings, "Rifle Additional", 0.12f);
            CrouchMultiplier = settings.GetFloat(ExtensiveSettings, "Crouch Multiplier", 0.5f);

            if (enable)
                Main.Log("script initialized...");
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
                if (PlayerHelper.HasPlayerShotRecently())
                    CurrentRecoil = Math.Min(CurrentRecoil + AdditionalRecoil, MaximumRecoil);
                else
                    CurrentRecoil = Math.Max(CurrentRecoil - DecayRate, BaseRecoil);
            }

            if (isPlayerCrouched)
                appliedRecoil *= CrouchMultiplier;

            if (IS_CHAR_SHOOTING(Main.PlayerPed.GetHandle()))
            {
                GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);
                IVWeaponInfo currentWeaponInfo = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon);
                WeaponGroup weaponGroup = (WeaponGroup)currentWeaponInfo.Group;
                ApplyRecoil(cam, weaponGroup, appliedRecoil);
            }
        }

        private static void ApplyRecoil(NativeCamera cam, WeaponGroup weaponGroup, float appliedRecoil)
        {
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

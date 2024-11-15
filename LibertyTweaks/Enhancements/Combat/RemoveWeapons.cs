using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre 

namespace LibertyTweaks
{
    internal class RemoveWeapons
    {
        private static bool enable;
        private static bool hadBat = false;
        private static bool hadKnife = false;
        private static bool hadParachute = false;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Remove Weapons On Death", "Enable", true);


            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            if (Main.PlayerPed == null)
                return;

            if (IS_CHAR_DEAD(Main.PlayerPed.GetHandle()))
            {
                if (HAS_CHAR_GOT_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_BASEBALLBAT))
                {
                    hadBat = true;
                }

                if (HAS_CHAR_GOT_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_KNIFE))
                {
                    hadKnife = true;
                }

                if (HAS_CHAR_GOT_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_EPISODIC_21))
                {
                    hadParachute = true;
                }

                REMOVE_ALL_CHAR_WEAPONS(Main.PlayerPed.GetHandle());

                if (hadBat)
                    GIVE_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_BASEBALLBAT, 1, true);

                if (hadKnife)
                    GIVE_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_KNIFE, 1, true);

                if (hadParachute)
                    GIVE_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_EPISODIC_21, 1, true);

                hadBat = false;
                hadKnife = false;
                hadParachute = false;
            }
        }
    }
}

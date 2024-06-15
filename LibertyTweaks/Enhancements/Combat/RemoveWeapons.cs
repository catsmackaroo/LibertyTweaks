using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Numerics;
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

            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());

            if (playerPed == null)
                return;

            if (IS_CHAR_DEAD(playerPed.GetHandle()))
            {
                if (HAS_CHAR_GOT_WEAPON(playerPed.GetHandle(), (int)eWeaponType.WEAPON_BASEBALLBAT))
                {
                    hadBat = true;
                }

                if (HAS_CHAR_GOT_WEAPON(playerPed.GetHandle(), (int)eWeaponType.WEAPON_KNIFE))
                {
                    hadKnife = true;
                }

                if (HAS_CHAR_GOT_WEAPON(playerPed.GetHandle(), (int)eWeaponType.WEAPON_EPISODIC_21))
                {
                    hadParachute = true;
                }

                REMOVE_ALL_CHAR_WEAPONS(playerPed.GetHandle());

                if (hadBat)
                    GIVE_WEAPON_TO_CHAR(playerPed.GetHandle(), (int)eWeaponType.WEAPON_BASEBALLBAT, 1, true);

                if (hadKnife)
                    GIVE_WEAPON_TO_CHAR(playerPed.GetHandle(), (int)eWeaponType.WEAPON_KNIFE, 1, true);

                if (hadKnife)
                    GIVE_WEAPON_TO_CHAR(playerPed.GetHandle(), (int)eWeaponType.WEAPON_EPISODIC_21, 1, true);

                hadBat = false;
                hadKnife = false;
                hadParachute = false;

                //SpawnGunVan();
            }
        }

        //private static void SpawnGunVan()
        //{
        //    Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(10), "Main", () =>
        //    {
        //        REQUEST_SCRIPT("jacob_gun_car");
        //        START_NEW_SCRIPT("jacob_gun_car", 1024);
        //    });
        //}
    }
}

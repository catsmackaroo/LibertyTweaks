using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

// Credits: ClonkAndre & catsmackaroo

namespace LibertyTweaks
{
    internal class RemoveWeaponsOnDeath
    {
        private static bool enable;

        private static bool hadBat = false;
        private static bool hadKnife = false;
        private static bool hadParachute = false;

        private static bool inventoryRemoved = false;
        private static bool messageShown = false;

        private static List<eWeaponType> inventory = new List<eWeaponType>();
        private static Dictionary<eWeaponType, int> ammo = new Dictionary<eWeaponType, int>();

        private static int SmallPistolPrice;
        private static int HeavyPistolPrice;
        private static int SMGPrice;
        private static int ShotgunPrice;
        private static int AssaultRiflePrice;
        private static int SniperPrice;
        private static int HeavyPrice;
        private static int ThrownPrice;
        private static readonly int DefaultPrice;
        private static int CompletePrice = 0;

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Remove Weapons On Death", "Enable", true);

            SmallPistolPrice = settings.GetInteger("Remove Weapons On Death", "Small Pistol Price", 500);
            HeavyPistolPrice = settings.GetInteger("Remove Weapons On Death", "Heavy Pistol Price", 1000);
            SMGPrice = settings.GetInteger("Remove Weapons On Death", "SMG Price", 300);
            ShotgunPrice = settings.GetInteger("Remove Weapons On Death", "Shotgun Price", 1500);
            AssaultRiflePrice = settings.GetInteger("Remove Weapons On Death", "Assault Rifle Price", 4000);
            SniperPrice = settings.GetInteger("Remove Weapons On Death", "Sniper Price", 5000);
            HeavyPrice = settings.GetInteger("Remove Weapons On Death", "Heavy Price", 6000);
            ThrownPrice = settings.GetInteger("Remove Weapons On Death", "Thrown Price", 250);

            if (enable)
                Main.Log("script initialized...");
        }


        public static void Tick()
        {
            if (!enable || Main.PlayerPed == null)
                return;

            if (IS_CHAR_DEAD(Main.PlayerPed.GetHandle()) && !inventoryRemoved)
            {
                GrabAndRemoveInventory();
                inventoryRemoved = true;
            }

            int missionsComplete = GET_INT_STAT(253);

            if (missionsComplete == 0)
                return;

            if (!IS_CHAR_DEAD(Main.PlayerPed.GetHandle()) && inventoryRemoved)
            {
                ShowBribeNotification();
                inventoryRemoved = false;
            }

            if (messageShown && NativeControls.IsGameKeyPressed(0, GameKey.Action) && IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1"))
            {
                RestoreWeapons();
            }
        }

        private static void GrabAndRemoveInventory()
        {
            inventory = WeaponHelpers.GetWeaponInventory(false);
            ammo = WeaponHelpers.GetWeaponAmmoCounts();

            hadBat = HAS_CHAR_GOT_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_BASEBALLBAT);
            hadKnife = HAS_CHAR_GOT_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_KNIFE);
            hadParachute = HAS_CHAR_GOT_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_EPISODIC_21);

            REMOVE_ALL_CHAR_WEAPONS(Main.PlayerPed.GetHandle());
            SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)eWeaponType.WEAPON_UNARMED, true);

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

        private static void ShowBribeNotification()
        {
            if (!IS_HELP_MESSAGE_BEING_DISPLAYED())
            {
                Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(10), "Main", () =>
                {
                    BribeForWeaponsNotification(inventory);
                    messageShown = true;
                });
            }
        }

        private static void RestoreWeapons()
        {
            if (Main.PlayerPed.PlayerInfo.GetMoney() < CompletePrice)
                return;

            int oldWeap = WeaponHelpers.GetWeaponType();
            PLAY_SOUND_FRONTEND(-1, "FRONTEND_OTHER_INFO");

            foreach (var weapon in inventory)
            {
                int ammoToGive = ammo.ContainsKey(weapon) ? ammo[weapon] : 0;
                GIVE_WEAPON_TO_CHAR(Main.PlayerPed.GetHandle(), (int)weapon, ammoToGive, true);
                Main.Log($"Restored weapon: {weapon} with ammo: {ammoToGive}");
            }

            IVPlayerInfoExtensions.RemoveMoney(Main.PlayerPed.PlayerInfo, CompletePrice);
            SET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), oldWeap, true);
            CompletePrice = 0;

            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("PLACEHOLDER_1"))
            {
                CLEAR_HELP();
                messageShown = false;
            }
        }

        private static void BribeForWeaponsNotification(List<eWeaponType> inventory)
        {
            CalculateBribePrice(inventory);
            string message;

            if (Main.PlayerPed.PlayerInfo.GetMoney() < CompletePrice)
            {
                message = $"You don't have enough money to bribe.";
            }
            else
            {
                message = $"Pay ~g~${CompletePrice} ~s~bribe to get back your weapons?" +
                $"~n~Press ~INPUT_PICKUP~ to pay.";
            }


            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", message);
            PRINT_HELP("PLACEHOLDER_1");
        }

        private static void CalculateBribePrice(List<eWeaponType> inventory)
        {
            CompletePrice = 0;
            var weaponGroupPrices = new Dictionary<WeaponGroup, int>
            {
                { WeaponGroup.SmallPistol, SmallPistolPrice },
                { WeaponGroup.HeavyPistol, HeavyPistolPrice },
                { WeaponGroup.SMG, SMGPrice },
                { WeaponGroup.Shotgun, ShotgunPrice },
                { WeaponGroup.AssaultRifle, AssaultRiflePrice },
                { WeaponGroup.Sniper, SniperPrice },
                { WeaponGroup.Heavy, HeavyPrice},
                { WeaponGroup.Thrown, ThrownPrice}
            };

            foreach (eWeaponType weapon in inventory)
            {
                IVWeaponInfo weaponInfo = IVWeaponInfo.GetWeaponInfo((uint)weapon);
                WeaponGroup group = (WeaponGroup)weaponInfo.Group;

                if (weaponGroupPrices.ContainsKey(group))
                {
                    CompletePrice += weaponGroupPrices[group];
                }
                else
                {
                    CompletePrice += DefaultPrice;
                }
            }

            foreach (var ammoCount in ammo.Values)
            {
                CompletePrice += ammoCount * 2;
            }

            CompletePrice = (int)Math.Ceiling(CompletePrice / 100.0) * 100;
        }
    }
}

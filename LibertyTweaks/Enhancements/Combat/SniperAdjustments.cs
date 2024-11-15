using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class SniperAdjustments
    {
        private static bool enableFix;
        private static bool enableToggleScope;
        private static bool shallDelete;
        private static bool isFpEnabled;
        private static DateTime lastToggleTime;
        private static TimeSpan toggleCooldown = TimeSpan.FromMilliseconds(500);


        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Move With Sniper", "Enable", true);
            enableToggleScope = settings.GetBoolean("Toggle Sniper Scope", "Enable", true);
        }

        public static void Tick()
        {
            // TODO: fix messy ass 
            GET_CURRENT_CHAR_WEAPON(Main.PlayerPed.GetHandle(), out int currentWeapon);

            if (currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE
                || currentWeapon == (int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15)
            {

                if (NativeControls.IsGameKeyPressed(0, GameKey.Aim))
                {
                    if (enableToggleScope)
                    {
                        if (NativeControls.IsGameKeyPressed(0, GameKey.Jump))
                        {
                            if (DateTime.Now - lastToggleTime > toggleCooldown)
                            {
                                isFpEnabled = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson;

                                if (isFpEnabled)
                                    IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson = false;
                                else
                                    IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponFlags.FirstPerson = true;

                                SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), false);
                                SET_PLAYER_CONTROL((int)GET_PLAYER_ID(), true);

                                IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).Accuracy = 0;
                                IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).Damage = IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).DamageFPS;


                                lastToggleTime = DateTime.Now;
                            }
                        }
                    }

                    if (enableFix)
                    {
                        IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot = 16;
                        GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), currentWeapon, out int currentAmmo);

                        if (currentAmmo >= 1)
                        {
                            shallDelete = true;
                        }
                    }
                    else
                    {
                        if (enableFix)
                        {
                            IVWeaponInfo.GetWeaponInfo((uint)currentWeapon).WeaponSlot = 6;

                            if (shallDelete == true)
                            {
                                REMOVE_WEAPON_FROM_CHAR(Main.PlayerPed.GetHandle(), currentWeapon);
                                shallDelete = false;
                            }
                        }
                    }
                }
                else
                {
                    if (enableFix)
                    {
                        if (IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1).WeaponSlot != 6
                            || IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE).WeaponSlot != 6
                            || IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15).WeaponSlot != 6)
                        {
                            IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_M40A1).WeaponSlot = 6;
                            IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_SNIPERRIFLE).WeaponSlot = 6;
                            IVWeaponInfo.GetWeaponInfo((int)IVSDKDotNet.Enums.eWeaponType.WEAPON_EPISODIC_15).WeaponSlot = 6;
                        }
                    }
                }
            }
        }
    }
}
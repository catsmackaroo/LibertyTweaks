using System;
using System.Collections.Generic;
using System.Numerics;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

// Credits: ItsClonkAndre, catsmackaroo

namespace LibertyTweaks
{
    internal class ExtendedPedWeaponPool
    {
        private static bool enable;
        private static readonly string[] splitChar = { "," };
        private static readonly Random rnd = new Random();
        private static readonly List<WeaponOverride> weaponOverrides = new List<WeaponOverride>();
        private static int executeIn;
        private static bool showAreaName;
        private static DateTime lastExecutionTime = DateTime.MinValue;

        private static readonly HashSet<int> processedPeds = new HashSet<int>();

        private class WeaponOverride
        {
            public int Chance;
            public bool ForMissionPed;
            public uint Episode;
            public string Area;
            public List<int> PedTypes;
            public List<string> PedModels;
            public List<int> Weapons;
            public int AmmoMin, AmmoMax;

            public WeaponOverride(int chance, uint episode, string area, List<int> pedTypes, List<string> pedModels, List<int> weapons, int ammoMin, int ammoMax, bool isMissionPed)
            {
                Chance = chance;
                Episode = episode;
                Area = area;
                PedTypes = pedTypes;
                PedModels = pedModels;
                Weapons = weapons;
                AmmoMin = ammoMin;
                AmmoMax = ammoMax;
                ForMissionPed = isMissionPed;
            }

            public override string ToString()
            {
                return $"Episode: {Episode}, Area: {Area}, PedTypes: [{string.Join(", ", PedTypes)}], Weapons: [{string.Join(", ", Weapons)}]";
            }
        }

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Extended Ped Weapon Pool", "Enable", true);

            if (enable)
                Main.Log("script initialized...");

            executeIn = settings.GetInteger("General", "ExecuteIn", 1000);
            showAreaName = settings.GetBoolean("General", "ShowAreaName", false);

            int overrideCount = settings.GetInteger("General", "OverrideCount", 0);
            for (int i = 0; i < overrideCount; i++)
            {
                string section = i.ToString();
                int chance = settings.GetInteger(section, "ChancePercentage", 50);
                uint episode = (uint)settings.GetInteger(section, "Episode", 3);
                string area = settings.GetValue(section, "Area", "any");
                int ammoMin = settings.GetInteger(section, "AmmoMin", 100);
                int ammoMax = settings.GetInteger(section, "AmmoMax", 999);
                bool forMissionPed = settings.GetBoolean(section, "MissionPeds", true);

                List<string> pedModelsList = new List<string>();
                string pedModels = settings.GetValue(section, "PedModels", "");
                if (!string.IsNullOrWhiteSpace(pedModels))
                {
                    pedModelsList.AddRange(pedModels.Split(splitChar, StringSplitOptions.RemoveEmptyEntries));
                }

                string weaponsString = settings.GetValue(section, "Weapons", "");
                List<int> weaponsList = new List<int>();
                if (!string.IsNullOrEmpty(weaponsString))
                {
                    string[] weaponValues = weaponsString.Split(',');
                    foreach (var weapon in weaponValues)
                    {
                        if (int.TryParse(weapon.Trim(), out int weap))
                        {
                            weaponsList.Add(weap);
                        }
                    }
                }

                List<int> pedTypes = new List<int>();
                string pedTypeString = settings.GetValue(section, "PedType", "");
                if (!string.IsNullOrEmpty(pedTypeString))
                {
                    string[] pedTypeValues = pedTypeString.Split(',');
                    foreach (var pedType in pedTypeValues)
                    {
                        if (int.TryParse(pedType.Trim(), out int type))
                        {
                            pedTypes.Add(type);
                        }
                    }
                }

                weaponOverrides.Add(new WeaponOverride(chance, episode, area, pedTypes, pedModelsList, weaponsList, ammoMin, ammoMax, forMissionPed));
            }
        }

        public static void Tick()
        {
            if (!enable) return;

            int playerID = CONVERT_INT_TO_PLAYERINDEX(GET_PLAYER_ID());
            GET_PLAYER_CHAR(playerID, out int pPed);

            if (showAreaName)
            {
                GET_CHAR_COORDINATES(pPed, out Vector3 pos);
                IVGame.ShowSubtitleMessage($"Area: {GET_NAME_OF_ZONE(pos.X, pos.Y, pos.Z)}");
            }

            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - lastExecutionTime;

            if (elapsed.TotalMilliseconds >= executeIn)
            {
                ExecuteWeaponOverrides();
                lastExecutionTime = now;
            }
        }

        private static void ExecuteWeaponOverrides()
        {
            if (weaponOverrides.Count == 0) return;

            uint currentEpisode = GET_CURRENT_EPISODE();
            IVPool pedPool = IVPools.GetPedPool();

            for (int p = 0; p < pedPool.Count; p++)
            {
                UIntPtr pedPtr = pedPool.Get(p);
                if (pedPtr == UIntPtr.Zero) continue;

                int ped = (int)pedPool.GetIndex(pedPtr);

                if (processedPeds.Contains(ped)) continue;

                GET_PED_TYPE(ped, out uint pType);
                GET_CHAR_MODEL(ped, out int pModel);
                GET_CHAR_COORDINATES(ped, out Vector3 pos);
                string currentPedArea = GET_NAME_OF_ZONE(pos.X, pos.Y, pos.Z);

                foreach (var wO in weaponOverrides)
                {
                    if (!wO.ForMissionPed && IS_PED_A_MISSION_PED(ped))
                        continue;

                    if (wO.PedTypes.Count > 0 && !wO.PedTypes.Contains((int)pType))
                        continue;

                    if (wO.Area != currentPedArea && wO.Area.ToLower() != "any")
                        continue;

                    if (wO.Episode != currentEpisode && wO.Episode != 3)
                        continue;

                    if (wO.PedModels.Count > 0 && !wO.PedModels.Contains($"0x{pModel:X}"))
                        continue;

                    if (wO.ForMissionPed && !IS_PED_A_MISSION_PED(ped)) 
                        continue;

                    int randomValue = rnd.Next(0, 101);
                    int totalChance = 0;
                    bool weaponAssigned = false;
                    foreach (var weapon in wO.Weapons)
                    {
                        totalChance += wO.Chance;
                        if (randomValue <= totalChance)
                        {
                            if (!HAS_CHAR_GOT_WEAPON(ped, weapon))
                            {
                                REMOVE_ALL_CHAR_WEAPONS(ped);
                                GIVE_WEAPON_TO_CHAR(ped, weapon, rnd.Next(wO.AmmoMin, wO.AmmoMax), false);

                                processedPeds.Add(ped);
                                weaponAssigned = true;
                            }
                            break;
                        }
                    }

                    if (weaponAssigned)
                        break;
                }
            }
        }
    }
}

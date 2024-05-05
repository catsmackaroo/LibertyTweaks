using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using LibertyTweaks;
using System.Collections.Generic;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;
using System.Windows.Forms;
using System.Windows.Input;

// Credits: Gillian

namespace LibertyTweaks
{
    internal class QuickGPS
    {
        private static bool enable;
        private static bool heldkey;
        private static int switchblipid;
        private static Vector3 playercoord;
        private static int currentBlip;

        // Burger Shot, Cluckin' Bell and Restaurants
        private static List<int> eatyumyum = new List<int>()
        {
            21, 22, 57
        };

        // Missions
        private static List<int> missions = new List<int>()
        {
            23, 25, 26, 27, 28, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 53, 55, 63, 64, 69, 74, 76, 77, 78, 80, 92
        };

        // Entertainment
        private static List<int> entertainment = new List<int>()
        {
            46, 47, 48, 49, 51, 52, 57, 66, 70, 71
        };

        private static int FindClosestByType(int type)
        {
            int closestblipid = GET_FIRST_BLIP_INFO_ID(type);
            int firstblipid = closestblipid;
            int blipid;
            float closestdistance;
            float distance2;
            Vector3 blipcoord1;
            Vector3 blipcoord2;

            GET_BLIP_COORDS(closestblipid, out blipcoord1);
            GET_DISTANCE_BETWEEN_COORDS_2D(blipcoord1.X, blipcoord1.Y, playercoord.X, playercoord.Y, out closestdistance);

            while (true)
            {
                blipid = GET_NEXT_BLIP_INFO_ID(type);
                if (blipid == 0)
                {
                    break;
                }
                GET_BLIP_COORDS(blipid, out blipcoord2);
                GET_DISTANCE_BETWEEN_COORDS_2D(blipcoord2.X, blipcoord2.Y, playercoord.X, playercoord.Y, out distance2);
                if (distance2 < closestdistance)
                {
                    closestdistance = distance2;
                    closestblipid = blipid;
                }
            }
            return closestblipid;
        }

        private static int FindClosest(params int[] blipids)
        {
            Vector3 blipcoord;
            float closestDistance = float.MaxValue;
            int closestBlipId = -1;

            foreach (int blipid in blipids)
            {
                GET_BLIP_COORDS(blipid, out blipcoord);
                GET_DISTANCE_BETWEEN_COORDS_2D(blipcoord.X, blipcoord.Y, playercoord.X, playercoord.Y, out float distance);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlipId = blipid;
                }
            }

            return closestBlipId;
        }


        private static int FindClosestByTypes(List<int> types)
        {
            List<int> closestBlipIds = new List<int>();

            foreach (int type in types)
            {
                int closestBlipId = FindClosestByType(type);
                closestBlipIds.Add(closestBlipId);
            }

            int closestBlip = FindClosest(closestBlipIds.ToArray());
            return closestBlip;
        }

        public static void Init(SettingsFile settings)
        {
            enable = settings.GetBoolean("Quick GPS", "Enable", true);
        }
        public static void Process(int index, Keys gpskey)
        {
            if (!enable)
                return;
            if (!Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey((int)gpskey)))
            {
                return;
            }
            IVPed playerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            playercoord = playerPed.Matrix.Pos;

            if (IVTheScripts.IsPlayerOnAMission())
            {
                return;
            }
            switch (index)
            {
                // Internet 24; Safehouse 29; Clothes 50; Helitour 56; Station 58; Weapons 59; Pay'n'Spray 75
                case 1:
                    switchblipid = FindClosestByTypes(eatyumyum);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest restaurant!");
                    break;
                case 2:
                    switchblipid = FindClosestByType(29);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest safehouse!");
                    break;
                case 3:
                    switchblipid = FindClosestByType(59);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest weapons shop!");
                    break;
                case 4:
                    switchblipid = FindClosestByType(75);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest Pay'N'Spray!");
                    break;
                case 5:
                    switchblipid = FindClosestByType(24);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest internet cafe!");
                    break;
                case 6:
                    switchblipid = FindClosestByType(50);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest clothing shop!");
                    break;
                case 7:
                    switchblipid = FindClosestByTypes(missions);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest mission!");
                    break;
                case 8:
                    switchblipid = FindClosestByType(56);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to the helitour!");
                    break;
                case 9:
                    switchblipid = FindClosestByTypes(entertainment);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest entertianment!");
                    break;
                /*case 10:
                    switchblipid = FindClosestByType(58);
                    SET_ROUTE(switchblipid, true);
                    IVGame.ShowSubtitleMessage("Set route to closest station!");
                    break;*/
                case 0:
                    SET_ROUTE(switchblipid, false);
                    IVGame.ShowSubtitleMessage("Removed route.");
                    break;
            }
        }
    }
}

/*
                    switch (selectedIndex)
                    {
                        // Internet 24; Safehouse 29; Clothes 50; Helitour 56; Station 58; Weapons 59; Pay'n'Spray 75
                        case 0:
                            SET_ROUTE(FindClosestByTypes(eatyumyum), true);
                            break;
                        case 1:
                            SET_ROUTE(FindClosestByType(29), true);
                            break;
                        case 2:
                            SET_ROUTE(FindClosestByType(59), true);
                            break;
                        case 3:
                            SET_ROUTE(FindClosestByType(75), true);
                            break;
                        case 4:
                            SET_ROUTE(FindClosestByType(24), true);
                            break;
                        case 5:
                            SET_ROUTE(FindClosestByType(50), true);
                            break;
                        case 6:
                            SET_ROUTE(FindClosestByTypes(missions), true);
                            break;
                        case 7:
                            SET_ROUTE(FindClosestByType(56), true);
                            break;
                        case 8:
                            SET_ROUTE(FindClosestByTypes(entertainment), true);
                            break;
                        case 9:
                            SET_ROUTE(FindClosestByType(58), true);
                            break;
                        case 10:
                            _TASK_FLUSH_ROUTE();
                            break;
                    }
*/
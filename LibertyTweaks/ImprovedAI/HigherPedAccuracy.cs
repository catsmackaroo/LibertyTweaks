using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks.HigherPedAccuracy
{
    internal class HigherPedAccuracy
    {
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "Improved AI", true);
        }

        public static void Tick()
        {
            if (!enableFix)
                return;

            //var
            int playerHandle;
            uint playerId;

            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed());
            playerHandle = CPedExtensions.GetHandle(playerPed);
            playerId = GET_PLAYER_ID();

            GET_CURRENT_BASIC_COP_MODEL(out uint copModel);

            //grab all peds
            CPool pedPool = CPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    // Get the handle (ID) of the ped 
                    int pedHandle = (int)pedPool.GetIndex(ptr);
                    CPed pedsCPeds = CPed.FromPointer(ptr);

                    GET_CHAR_COORDINATES(pedHandle, out Vector3 pedCoords);
                    GET_CHAR_MODEL(pedHandle, out uint pedModel);

                    // CPED METHOD - For some reason can result in negative numbers
                    //pedsCPeds.Accuracy = (byte)Main.GenerateRandomNumber(175, 255);
                    //pedsCPeds.ShootRate = (byte)Main.GenerateRandomNumber(175, 255);

                    // Set pedHandle accuracy
                    SET_CHAR_ACCURACY(pedHandle, (uint)Main.GenerateRandomNumber(75, 90));
                    SET_CHAR_SHOOT_RATE(pedHandle, Main.GenerateRandomNumber(75, 90));

                    if (pedModel == copModel)
                    {
                        SET_CHAR_ACCURACY(pedHandle, (uint)Main.GenerateRandomNumber(80, 100));
                        SET_CHAR_SHOOT_RATE(pedHandle, Main.GenerateRandomNumber(95, 100));
                    }
                }
            }
        }
    }
}

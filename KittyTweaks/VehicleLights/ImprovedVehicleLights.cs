using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CCL.GTAIV;

using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks.ImprovedVehicleLights
{
    internal class ImprovedVehicleLights
    {
        public static void Init(SettingsFile settings)
        {

        }

        public static void Tick()
        {
            int pVehInt;
            int playerHandle;
            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed());
            playerHandle = CPedExtensions.GetHandle(playerPed);

            GET_CAR_CHAR_IS_USING(playerHandle, out pVehInt);
            CVehicle pVeh = CVehicle.FromPointer((UIntPtr)pVehInt);

            
            //CVehicle pVeh = CVehicle.FromPointer(CPlayerInfo.FindPlayerVehicle());
            //pVehInt = CVehicleExtensions.GetHandle(pVeh);

            
        }
    }
}

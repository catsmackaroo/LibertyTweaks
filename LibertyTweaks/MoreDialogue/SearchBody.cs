using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

using IVSDKDotNet;

using CCL.GTAIV;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;

namespace CorpseThief.SearchBody
{
    internal class SearchBody
    {

        private static bool didSpeak;
        private static bool enableFix;

        public static void Init(SettingsFile settings)
        {
            enableFix = settings.GetBoolean("Main", "Search Body Lines", true);
        }

        public static void Tick()
        {
            if (!enableFix)
                return;

            // Grab the player CPed, then the player handle (ID)
            CPed playerPed = CPed.FromPointer(CPlayerInfo.FindPlayerPed());
            Vector3 playerGroundPos = NativeWorld.GetGroundPosition(playerPed.Matrix.pos);

            // Grab all peds in world (looped) then grab ped ID
            CPool pedPool = CPools.GetPedPool();
            for (int i = 0; i < pedPool.Count; i++)
            {
                UIntPtr ptr = pedPool.Get(i);
                if (ptr != UIntPtr.Zero)
                {
                    // Get the handle (ID) of the ped 
                    int pedHandle = (int)pedPool.GetIndex(ptr);

                    // Check if ped is in any police vehicle or if the ped model is equals to the current basic cop model
                    if (IS_CHAR_DEAD(pedHandle))
                    {
                        // Get ped coordinates
                        GET_CHAR_COORDINATES(pedHandle, out Vector3 pedCoords);

                        // Check distance between the player and the ped
                        if (Vector3.Distance(playerPed.Matrix.pos, pedCoords) < 2f)
                        {
                            if (NativePickup.IsAnyPickupAtPos(playerGroundPos))
                            {

                                if (!didSpeak)
                                {
                                    playerPed.SayAmbientSpeech("SEARCH_BODY_TAKE_ITEM");
                                    //CGame.ShowSubtitleMessage("Corpse Item");
                                    didSpeak = true;
                                }
                            }
                            else
                            {
                                didSpeak = false;
                            }
                        }
                    }
                }
            }

        }

    }
}
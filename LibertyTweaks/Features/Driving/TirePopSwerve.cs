using CCL.GTAIV;
using IVSDKDotNet;
using System;
using System.Collections.Generic;
using static IVSDKDotNet.Native.Natives;

// credits: catsmackaroo
// doesn't seem to work with all the time with NPCs, not sure why. seems wheel stuff is always adjusted by the game

namespace LibertyTweaks
{
    internal class TirePopSwerve
    {
        private static bool enable;
        private static DateTime biasChangeTime;
        private static readonly TimeSpan biasDuration = TimeSpan.FromSeconds(1);
        private static Dictionary<int, bool> leftTireBiasChanged = new Dictionary<int, bool>();
        private static Dictionary<int, bool> rightTireBiasChanged = new Dictionary<int, bool>();

        public static string section { get; private set; }

        public static void Init(SettingsFile settings, string section)
        {
            TirePopSwerve.section = section;
            enable = settings.GetBoolean(section, "Car Tire Pop Swerve", true);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable || !IS_CHAR_IN_ANY_CAR(Main.PlayerPed.GetHandle()))
                return;

            if (NativeControls.IsGameKeyPressed(0, GameKey.Reload))
            {
                BurstRandomTire();
            }

            HandleTireBurst(0, leftTireBiasChanged, 7f, "bopped");
            HandleTireBurst(1, rightTireBiasChanged, -7f, "bopped 2");

            ResetSteerBias(leftTireBiasChanged, "reset");
            ResetSteerBias(rightTireBiasChanged, "reset 2");
        }

        private static void BurstRandomTire()
        {
            BURST_CAR_TYRE(Main.PlayerVehicle.GetHandle(), (uint)Main.GenerateRandomNumber(0, 3));
        }

        private static void HandleTireBurst(uint tireIndex, Dictionary<int, bool> biasChangedDict, float biasChange, string message)
        {
            foreach (var kvp in PedHelper.VehHandles)
            {
                int car = kvp.Value;
                IVVehicle carVehicle = NativeWorld.GetVehicleInstanceFromHandle(car);

                if (IS_CAR_TYRE_BURST(car, tireIndex))
                {
                    if (!biasChangedDict.ContainsKey(car) || !biasChangedDict[car])
                    {
                        carVehicle.SteerBias += biasChange;
                        biasChangeTime = DateTime.Now;
                        IVGame.ShowSubtitleMessage($"~r~{message}");
                        biasChangedDict[car] = true;
                    }
                }
                else if (biasChangedDict.ContainsKey(car) && biasChangedDict[car])
                {
                    HardResetSteerBias(leftTireBiasChanged, "hard reset");
                    HardResetSteerBias(rightTireBiasChanged, "hard reset 2");
                    biasChangedDict[car] = false;
                }
            }
        }

        private static void ResetSteerBias(Dictionary<int, bool> biasChangedDict, string message)
        {
            foreach (var kvp in PedHelper.VehHandles)
            {
                int car = kvp.Value;
                if (biasChangedDict.ContainsKey(car) && biasChangedDict[car] && DateTime.Now - biasChangeTime > biasDuration)
                {
                    IVVehicle carVehicle = NativeWorld.GetVehicleInstanceFromHandle(car);
                    carVehicle.SteerBias = 0;
                    IVGame.ShowSubtitleMessage($"~g~{message}");
                }
            }
        }
        
        private static void HardResetSteerBias(Dictionary<int, bool> biasChangedDict, string message)
        {
            foreach (var kvp in PedHelper.VehHandles)
            {
                int car = kvp.Value;
                if (biasChangedDict.ContainsKey(car) && biasChangedDict[car])
                {
                    IVVehicle carVehicle = NativeWorld.GetVehicleInstanceFromHandle(car);
                    carVehicle.SteerBias = 0;
                    IVGame.ShowSubtitleMessage($"~g~{message}");
                }
            }
        }
    }
}

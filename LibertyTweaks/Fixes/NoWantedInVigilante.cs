using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class NoWantedInVigilante
    {
        private static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            NoWantedInVigilante.section = section;
            enable = settings.GetBoolean(section, "Never Wanted in Vigilante", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            SET_PLAYER_FAST_RELOAD(Main.PlayerIndex, true);

            try
            {
                if (NativeGame.IsScriptRunning("vigilante") || NativeGame.IsScriptRunning("mostwanted"))
                {
                    STORE_WANTED_LEVEL(Main.PlayerIndex, out uint wantedLevel);

                    if (wantedLevel != 0 && wantedLevel <= 3)
                    {
                        ALTER_WANTED_LEVEL(Main.PlayerIndex, 0);
                        APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Main.Log($"Error: {ex.Message}");
            }
        }
    }
}

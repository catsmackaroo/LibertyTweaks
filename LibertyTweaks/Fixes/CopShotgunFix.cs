using IVSDKDotNet;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;

// Credits: catsmackaroo

namespace LibertyTweaks
{
    internal class CopShotgunFix
    {
        public static bool enable;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            CopShotgunFix.section = section;
            enable = settings.GetBoolean(section, "Cop Shotgun Fix", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (!enable)
                return;

            foreach (var kvp in PedHelper.PedHandles)
            {
                int pedHandle = kvp.Value;

                if (IS_CHAR_DEAD(pedHandle))
                    continue;

                GET_CHAR_MODEL(pedHandle, out uint pedModel);
                Natives.GET_CURRENT_CHAR_WEAPON(pedHandle, out int currentPedWeapon);

                if (pedModel == 4111764146 || pedModel == 2776029317 || pedModel == 4205665177 || pedModel == 3295460374 || pedModel == 148777611)
                {
                    if (currentPedWeapon == 10)
                    {
                        GIVE_WEAPON_TO_CHAR(pedHandle, 11, 30, false);
                    }
                }
            }
        }
    }
}
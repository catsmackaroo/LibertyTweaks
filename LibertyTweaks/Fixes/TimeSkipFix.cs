using CCL.GTAIV;
using IVSDKDotNet;
using System;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    public class TimeSkipFix
    {
        private static bool enable;

        private static bool CheckDateTime;
        private static DateTime currentDateTime;
        private static bool changeTime;
        private static uint day;
        private static uint newDay;
        private static int hour;
        public static string section { get; private set; }
        public static void Init(SettingsFile settings, string section)
        {
            TimeSkipFix.section = section;
            enable = settings.GetBoolean(section, "Time Skip Fix", false);

            if (enable)
                Main.Log("script initialized...");
        }

        public static void Tick()
        {
            if (CheckDateTime == false)
            {
                currentDateTime = DateTime.Now;
                CheckDateTime = true;
            }

            if (DateTime.Now.Subtract(currentDateTime).TotalMilliseconds > 250.0)
            {
                CheckDateTime = false;
                if (IS_CHAR_DEAD(Main.PlayerPed.GetHandle()) && !changeTime)
                {
                    changeTime = true;
                    day = GET_CURRENT_DAY_OF_WEEK();
                }
                if (changeTime && !IS_CHAR_DEAD(Main.PlayerPed.GetHandle()) && IS_SCREEN_FADING_IN())
                {
                    GET_TIME_OF_DAY(out hour, out int minute);
                    newDay = GET_CURRENT_DAY_OF_WEEK();

                    if ((hour < 12) && day == newDay)
                    {
                        SET_TIME_ONE_DAY_FORWARD();
                        changeTime = false;
                    }
                    else
                        changeTime = false;
                }
                if (HAS_RESPRAY_HAPPENED() && IS_SCREEN_FADING_IN())
                {
                    GET_TIME_OF_DAY(out hour, out int minute);
                    newDay = GET_CURRENT_DAY_OF_WEEK();

                    if ((hour < 3))
                        SET_TIME_ONE_DAY_FORWARD();
                }
            }
        }
    }
}

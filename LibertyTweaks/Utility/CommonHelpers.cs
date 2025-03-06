using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static IVSDKDotNet.Native.Natives;

namespace LibertyTweaks
{
    internal static class CommonHelpers
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        public static bool ShouldExecute(int delayInMilliseconds)
        {
            if (!stopwatch.IsRunning)
                stopwatch.Start();

            if (stopwatch.ElapsedMilliseconds >= delayInMilliseconds)
            {
                stopwatch.Restart();
                return true;
            }

            return false;
        }
        public static void HandleScreenFade(uint duration, bool playerControl, Action onFadeComplete)
        {
            DO_SCREEN_FADE_OUT(duration);
            if (!playerControl)
                SET_PLAYER_CONTROL(Main.PlayerIndex, false);

            if (IS_SCREEN_FADING())
            {
                Main.TheDelayedCaller.Add(TimeSpan.FromSeconds(duration / 1000.0), "Main", () =>
                {
                    DO_SCREEN_FADE_IN(duration);
                    if (!playerControl)
                        SET_PLAYER_CONTROL(Main.PlayerIndex, true);

                    onFadeComplete?.Invoke();
                });
            }
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Lerp(float a, float b, float t)
        {
            t = Clamp(t, 0f, 1f);
            return a + (b - a) * t;
        }
        public static Vector3 LerpVector(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t),
                Lerp(a.Z, b.Z, t)
            );
        }
        public static float[] ParseFloatArray(string input)
        {
            return input.Split(',').Select(float.Parse).ToArray();
        }

    }
}

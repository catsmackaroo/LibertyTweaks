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
        private static bool hasSaved = false;
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

        public static float SmoothStep(float start, float end, float amount)
        {
            amount = Clamp(amount, 0, 1);
            amount = amount * amount * (3 - 2 * amount);
            return start + (end - start) * amount;
        }

        public static Vector3 SmoothStepVector3(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                SmoothStep(a.X, b.X, t),
                SmoothStep(a.Y, b.Y, t),
                SmoothStep(a.Z, b.Z, t)
            );
        }
        public static float[] ParseFloatArray(string input)
        {
            return input.Split(',').Select(float.Parse).ToArray();
        }
        public static Vector3 ParseVector3(string value)
        {
            var parts = value.Split(',');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                return new Vector3(x, y, z);
            }
            return Vector3.Zero;
        }

        public static bool HasGameSaved()
        {
            var loaded = IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("SG_LOAD_SUC");
            if (GET_IS_DISPLAYINGSAVEMESSAGE() && !loaded)
            {
                if (!hasSaved)
                {
                    hasSaved = true;
                    return true;
                }
            }
            else
            {
                hasSaved = false;
            }

            return false;
        }

    }
}

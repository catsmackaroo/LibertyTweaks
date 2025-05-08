using CCL.GTAIV;
using System.Drawing;
using System.Numerics;

namespace LibertyTweaks
{
    internal static class MiscHelpers
    {
        // credits: ItsClonkAndre & catsmackaroo
        // white by default

        public static void FlashRadar()
        {
            int startTransparency = 255;
            int endTransparency = 0;
            float duration = Main.GenerateRandomNumberFloat(0.25f, 0.55f);
            float interval = 10;
            float steps = duration * 1000 / interval;
            float stepAmount = 1.0f / steps;
            float elapsed = 0.0f;

            float blipSize = 150f;
            Vector2 location = Main.PlayerPed.Matrix.Pos.ToVector2();
            SizeF size = new SizeF(blipSize - location.X, blipSize - location.Y);
            Vector2 pos = new Vector2(location.X - (size.Width / 2f), location.Y + (size.Height / 2f));
            NativeBlip b = NativeBlip.AddBlipGangTerritory(pos, size, Color.White);
            b.SetColorRGB(Color.FromArgb(startTransparency, startTransparency, startTransparency));

            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += (sender, e) =>
            {
                elapsed += stepAmount;
                int transparency = (int)CommonHelpers.SmoothStep(startTransparency, endTransparency, elapsed);
                b.SetColorRGB(Color.FromArgb(transparency, transparency, transparency));

                if (elapsed >= 1.0f)
                {
                    timer.Stop();
                    timer.Dispose();
                    b.SetColorRGB(Color.FromArgb(transparency, transparency, transparency));
                    b.Delete();
                    b = null;
                }
            };
            timer.Start();
        }
    }
}

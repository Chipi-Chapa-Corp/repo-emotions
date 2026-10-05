using System;

namespace RepoEmoteWheel;

// Small point-filtered textures suit the game's low-resolution HUD. No bundled game assets.
internal static class WheelArt
{
    internal struct EyePose
    {
        public float UpperAngle, UpperClosed, LowerAngle, LowerClosed, Pupil, OffsetX, OffsetY;
    }

    internal static byte[] Eyes(EyePose left, EyePose right, int width = 160, int height = 80)
    {
        var pixels = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            // Unity textures are bottom-up. Two slightly tall, close-set semibot eyes.
            double cx = x < width / 2 ? width * .29 : width * .71;
            var eye = x < width / 2 ? left : right;
            double px = (x + .5 - cx) / (width * .19);
            double py = (y + .5 - height * .5) / (height * .39);
            double radius = px * px + py * py;
            if (radius > 1.13) continue;
            int p = (y * width + x) * 4;
            if (radius > 1) { Put(pixels, p, 12, 12, 10, 240); continue; }
            // Lid rotations are taken directly from PlayerExpression, in degrees.
            double upper = Math.Cos(eye.UpperClosed * Math.PI / 180);
            double lower = -Math.Cos(eye.LowerClosed * Math.PI / 180);
            double top = py - Math.Tan(eye.UpperAngle * Math.PI / 180) * px;
            double bottom = py - Math.Tan(eye.LowerAngle * Math.PI / 180) * px;
            bool open = top <= upper && bottom >= lower;
            if (!open)
            {
                double shade = .8 + .2 * (py + 1) / 2;
                Put(pixels, p, (byte)(64 * shade), (byte)(62 * shade), (byte)(53 * shade), 245);
                // A readable crease when the actual expression closes both eyelids.
                if (upper < 0 && lower > 0 && Math.Abs(py) < .04 && Math.Abs(px) < .88)
                    Put(pixels, p, 184, 180, 157, 255);
                continue;
            }
            double lighting = Math.Max(.68, 1 - .14 * (px * px + (py - .35) * (py - .35)));
            Put(pixels, p, (byte)(243 * lighting), (byte)(240 * lighting), (byte)(217 * lighting), 255);
            double pupilX = px - Math.Sin(eye.OffsetY * Math.PI / 180) * .7;
            double pupilY = py + Math.Sin(eye.OffsetX * Math.PI / 180) * .7;
            double pupilRadius = Math.Max(.075, Math.Min(.62, eye.Pupil * .30));
            if (pupilX * pupilX + pupilY * pupilY < pupilRadius * pupilRadius)
                Put(pixels, p, 13, 14, 12, 255);
        }
        return pixels;
    }

    internal static byte[] Ring(bool highlight, int size = 384)
    {
        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            double dx = (x + .5 - size / 2d) / (size / 2d);
            double dy = (y + .5 - size / 2d) / (size / 2d);
            double r = Math.Sqrt(dx * dx + dy * dy);
            double angle = Math.Atan2(dx, dy);
            double segment = (angle + Math.PI * 2 + Math.PI / 6) % (Math.PI / 3);
            if (r < .31 || r > .99 || segment < .018 || segment > Math.PI / 3 - .018) continue;
            if (highlight && Math.Abs(angle) > Math.PI / 6 - .018) continue;
            int p = (y * size + x) * 4;
            if (highlight)
            {
                // Warm, subdued selection, with a thin ochre outer edge.
                if (r > .967) Put(pixels, p, 214, 178, 77, 240);
                else Put(pixels, p, 150, 128, 67, 55);
            }
            else
            {
                if (r > .98 || r < .322) Put(pixels, p, 160, 155, 133, 60);
                else Put(pixels, p, 13, 14, 12, 175);
            }
        }
        return pixels;
    }

    internal static byte[] Pointer(int size = 20)
    {
        var pixels = new byte[size * size * 4];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Upper-left tip, echoing the game's triangular pointer.
            int topY = size - 1 - y;
            if (x < 2 || topY < 2 || x > topY * .72 + 2 || topY > size - 3) continue;
            Put(pixels, (y * size + x) * 4, 232, 188, 66, 255);
        }
        return pixels;
    }

    private static void Put(byte[] p, int i, byte r, byte g, byte b, byte a)
    { p[i] = r; p[i + 1] = g; p[i + 2] = b; p[i + 3] = a; }
}

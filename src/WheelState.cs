using System;

namespace RepoEmoteWheel;

// Unity-independent behavior, also used by the executable tests.
public sealed class WheelState
{
    public const int Count = 6;
    public bool IsOpen { get; private set; }
    public int Hovered { get; private set; } = -1;
    public int Active { get; private set; } = -1;
    public double EndsAt { get; private set; }
    private bool wasHeld;

    // Coordinates use screen-space: positive y points down; sector zero is at the top.
    public static int Sector(double x, double y, double deadZone)
    {
        if (x * x + y * y < deadZone * deadZone) return -1;
        double angle = Math.Atan2(x, -y) + Math.PI / Count;
        if (angle < 0) angle += 2 * Math.PI;
        return (int)(angle / (2 * Math.PI / Count)) % Count;
    }

    public void Tick(double now, bool allowed, bool held, bool cancel, double x, double y,
        double deadZone, double duration)
    {
        if (Active >= 0 && now >= EndsAt) Active = -1;
        if (!allowed || cancel)
        {
            IsOpen = false;
            Hovered = -1;
            Active = -1;
            wasHeld = held;
            return;
        }
        if (held && !wasHeld)
        {
            IsOpen = true;
            Active = -1;
        }
        if (IsOpen)
        {
            Hovered = Sector(x, y, deadZone);
            if (!held)
            {
                Active = Hovered;
                EndsAt = now + duration;
                IsOpen = false;
            }
        }
        wasHeld = held;
    }
}

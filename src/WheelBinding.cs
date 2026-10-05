using System;
using System.Linq;
using UnityEngine;

namespace RepoEmoteWheel;

internal static class WheelBinding
{
    // REPOConfig renders an acceptable-value list as its native dropdown.
    internal static readonly string[] Buttons = Enum.GetNames(typeof(KeyCode))
        .Where(name => name != "None" && name != "Escape" &&
            !name.StartsWith("Joystick", StringComparison.Ordinal)).ToArray();
}

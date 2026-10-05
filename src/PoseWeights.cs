using System;

namespace RepoEmoteWheel;

internal static class PoseWeights
{
    // Native expression weights rise AND decay every frame. They are relative
    // weights, not percentages: a fully held expression settles near 50, not 100.
    // Include the neutral expression, just as PlayerExpression.BlendEyeSettings does.
    internal static float Normalize(float[] weights)
    {
        float sum = 0;
        for (int i = 0; i < weights.Length; i++) sum += weights[i] = Math.Max(0, weights[i]);
        if (sum < .0001f) return 0;
        for (int i = 0; i < weights.Length; i++) weights[i] /= sum;
        return 1f - weights[0];
    }

    // Solve |sideways + forward * scale| = targetLength. Only stretch the
    // arm's length; preserve its width and the hand's sideways socket offset.
    internal static float ReachScale(float sidewaysSquared, float forwardSquared, float dot, float targetLength)
    {
        if (forwardSquared < .000001f) return 1f;
        double discriminant = dot * dot + forwardSquared * (targetLength * targetLength - sidewaysSquared);
        return Math.Max(.01f, (float)((-dot + Math.Sqrt(Math.Max(0, discriminant))) / forwardSquared));
    }
}

using UnityEngine;
using HarmonyLib;

namespace RepoEmoteWheel;

// Restore before the game's Update/Animator pass, then layer poses on that frame's
// natural arm movement. No animator, physics, grabbing, or network state is replaced.
[DefaultExecutionOrder(-10000)]
public sealed class ExpressionPose : MonoBehaviour
{
    private PlayerExpression expression;
    private PlayerAvatarVisuals visuals;
    private Transform left, right;
    private Quaternion leftRotation, rightRotation;
    private Vector3 leftPosition, rightPosition;
    private Vector3 leftScale, rightScale;
    private Vector3 leftPalm, rightPalm;
    private readonly float[] weights = new float[WheelState.Count + 1];
    private bool applied;
    private PlayerAvatarRightArm rightControl;
    private static readonly AccessTools.FieldRef<PlayerAvatar, bool> Tumbling =
        AccessTools.FieldRefAccess<PlayerAvatar, bool>("isTumbling");
    private static readonly AccessTools.FieldRef<PhysGrabber, bool> Grabbing =
        AccessTools.FieldRefAccess<PhysGrabber, bool>("physGrabBeamActive");
    private static readonly AccessTools.FieldRef<MapToolController, bool> MapActive =
        AccessTools.FieldRefAccess<MapToolController, bool>("Active");

    private void Awake()
    {
        expression = GetComponent<PlayerExpression>();
        visuals = GetComponent<PlayerAvatarVisuals>();
        var l = GetComponent<PlayerAvatarLeftArm>();
        var r = rightControl = GetComponent<PlayerAvatarRightArm>();
        left = l ? l.leftArmTransform : null;
        right = r ? r.rightArmTransform : null;
        if (!expression || !visuals || !left || !right) enabled = false;
        else
        {
            leftPalm = PalmOffset(left, "Flashlight Target Client", .471f);
            rightPalm = PalmOffset(right, "Grabber Target", .5133f);
        }
    }

    private void Update() => Restore();

    private void LateUpdate()
    {
        if (!Plugin.Instance || !LevelGenerator.Instance || !LevelGenerator.Instance.Generated ||
            SemiFunc.MenuLevel()) return;
        var avatar = expression.playerAvatar;
        if (!avatar || !avatar.isActiveAndEnabled || Tumbling(avatar) ||
            visuals.currentPose == PlayerAvatarVisuals.Pose.Crawl ||
            visuals.currentPose == PlayerAvatarVisuals.Pose.Tumble) return;
        // Leave the tool arms to gameplay when carrying or consulting the map.
        if (avatar.physGrabber && Grabbing(avatar.physGrabber)) return;
        if (rightControl && rightControl.mapToolController && MapActive(rightControl.mapToolController)) return;

        for (int i = 0; i < weights.Length; i++)
            weights[i] = i < expression.expressions.Count ? expression.expressions[i].weight : 0;
        float total = PoseWeights.Normalize(weights);
        if (total < .001f) return;

        leftRotation = left.localRotation;
        rightRotation = right.localRotation;
        leftPosition = left.localPosition;
        rightPosition = right.localPosition;
        leftScale = left.localScale;
        rightScale = right.localScale;
        applied = true;
        ApplyArm(left, leftPalm, -1);
        ApplyArm(right, rightPalm, 1);
    }

    private static Vector3 PalmOffset(Transform arm, string marker, float fallbackLength)
    {
        // These native tool sockets mark the ends of the rigid arms. Using the
        // actual endpoint also accounts for the left socket's small lateral offset.
        foreach (var child in arm.GetComponentsInChildren<Transform>(true))
            if (child.name == marker) return arm.InverseTransformPoint(child.position);
        return Vector3.forward * fallbackLength;
    }

    private void ApplyArm(Transform arm, Vector3 palm, int side)
    {
        Quaternion original = arm.rotation;
        Quaternion result = original;
        float accumulated = weights[0];
        float reach = weights[0];
        Vector3 naturalScale = arm.localScale;
        // Move the shoulder before solving the hand, rather than shifting the
        // already-posed arm away from its target afterward.
        arm.position += visuals.transform.TransformVector(Vector3.forward * (.08f * weights[2]));
        Vector3 sideways = arm.TransformVector(new Vector3(palm.x, palm.y, 0));
        Vector3 forward = arm.TransformVector(new Vector3(0, 0, palm.z));
        for (int i = 1; i <= WheelState.Count && i < expression.expressions.Count; i++)
        {
            float weight = weights[i];
            if (weight < .0001f) continue;
            Vector3 direction = Direction(i, arm, side);
            float scale = i >= 5 ? PoseWeights.ReachScale(sideways.sqrMagnitude, forward.sqrMagnitude,
                Vector3.Dot(sideways, forward), direction.magnitude) : 1f;
            Vector3 axis = i >= 5 ? sideways + forward * scale : arm.forward;
            Quaternion target = direction.sqrMagnitude > .0001f
                ? Quaternion.FromToRotation(axis, direction) * original
                : original;
            accumulated += weight;
            result = Quaternion.Slerp(result, target, weight / accumulated);
            // Head/mouth are contact poses. Rotation alone cannot put a fixed-
            // length arm's palm on a target closer than its normal reach.
            reach += weight * scale;
        }
        arm.rotation = result;
        arm.localScale = new Vector3(naturalScale.x, naturalScale.y, naturalScale.z * reach);
    }

    private Vector3 Direction(int index, Transform arm, int side)
    {
        Transform frame = visuals.transform;
        switch (index)
        {
            case 1: // Angry: lift 80 degrees from hanging, sweep 10 degrees back.
                float up = 80f * Mathf.Deg2Rad, back = 10f * Mathf.Deg2Rad;
                return frame.TransformDirection(new Vector3(side * Mathf.Sin(up) * Mathf.Cos(back),
                    -Mathf.Cos(up), -Mathf.Sin(up) * Mathf.Sin(back)));
            case 2: // Sad: slack arms and shoulders slightly forward.
                return frame.TransformDirection(Vector3.down);
            case 3: // Narrow, suspicious eyes: left hand points forward.
                return side < 0 ? frame.forward : Vector3.zero;
            case 4: // Closed eyes: hands resting behind the back.
                return frame.TransformDirection(new Vector3(-side * .25f, -.35f, -1f));
            case 5: // Wide shocked eyes: hands at the sides of the head.
                return HeadTarget(side * .30f, .02f, 0, true) - arm.position;
            case 6: // Half-open shocked eyes: hands close together at the mouth.
                return HeadTarget(side * .10f, -.01f, .30f, false) - arm.position;
            default:
                return Vector3.zero;
        }
    }

    private Vector3 HeadTarget(float x, float y, float z, bool top)
    {
        Transform anchor = top ? visuals.attachPointTopHeadMiddle : visuals.attachPointJawBottom;
        if (!anchor) anchor = visuals.headSideTransform;
        // Follow the animated head/jaw, including the expression's head tilt.
        Vector3 offset = (anchor ? anchor : visuals.transform).TransformVector(new Vector3(x, y, z));
        return (anchor ? anchor.position : visuals.transform.position) + offset;
    }

    private void Restore()
    {
        if (!applied) return;
        if (left) { left.localRotation = leftRotation; left.localPosition = leftPosition; left.localScale = leftScale; }
        if (right) { right.localRotation = rightRotation; right.localPosition = rightPosition; right.localScale = rightScale; }
        applied = false;
    }

    private void OnDisable() => Restore();
    private void OnDestroy() => Restore();
}

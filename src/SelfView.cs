using HarmonyLib;
using UnityEngine;

namespace RepoEmoteWheel;

// Restore before gameplay reads the camera; apply after its normal animation.
// Only the rendered camera is moved. The aim/network camera ancestors stay put.
[DefaultExecutionOrder(-11000)]
public sealed class SelfView : MonoBehaviour
{
    private readonly HoldState hold = new HoldState();
    private Transform cameraTransform;
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private bool applied;
    private PlayerAvatar avatar;
    private static readonly AccessTools.FieldRef<CameraPosition, bool> PositionOverridden =
        AccessTools.FieldRefAccess<CameraPosition, bool>("overridePositionActive");

    internal bool Active => hold.Active;
    private static bool NativeCameraOverride => CameraPosition.instance && PositionOverridden(CameraPosition.instance);

    private void Update()
    {
        RestoreCamera();
        var plugin = Plugin.Instance;
        bool wasActive = Active;
        hold.Tick(plugin && plugin.Allowed() && !NativeCameraOverride &&
            PlayerAvatar.instance.playerAvatarVisuals && CameraUtils.Instance && CameraUtils.Instance.MainCamera,
            plugin && plugin.SelfViewHeld);
        if (!Active)
        {
            ReleaseAvatar();
            if (wasActive) plugin?.ReportSelfView(false);
            return;
        }

        if (avatar != PlayerAvatar.instance)
        {
            ReleaseAvatar();
            avatar = PlayerAvatar.instance;
        }
        // Renew before PlayerAvatarVisuals.Update applies local visibility.
        avatar.playerAvatarVisuals.ShowSelfOverride(Mathf.Max(.1f, Time.deltaTime * 2f));
        avatar.physGrabber?.SetThirdPerson(true);
        avatar.flashlightController?.SetThirdPerson(true);
        if (!wasActive) plugin.ReportSelfView(true);
    }

    private void LateUpdate()
    {
        var plugin = Plugin.Instance;
        if (!Active) return;
        if (!plugin || !plugin.Allowed() || NativeCameraOverride || !avatar ||
            !avatar.playerAvatarVisuals || !CameraUtils.Instance || !CameraUtils.Instance.MainCamera)
        {
            Cancel();
            return;
        }

        var visuals = avatar.playerAvatarVisuals;
        Vector3 forward = Vector3.ProjectOnPlane(visuals.transform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < .01f) forward = avatar.transform.forward;
        Vector3 target = visuals.headLookAtTransform.position - Vector3.up * .35f;
        Vector3 offset = forward * 3f + Vector3.up * .15f;
        float distance = offset.magnitude;
        Vector3 direction = offset / distance;
        // Stop before walls and props; the player's own body must not block the view.
        int mask = SemiFunc.LayerMaskGetVisionObstruct() & ~LayerMask.GetMask("Player");
        if (Physics.SphereCast(target, .18f, direction, out var hit, distance, mask, QueryTriggerInteraction.Ignore))
            distance = Mathf.Max(.05f, hit.distance - .05f);

        cameraTransform = CameraUtils.Instance.MainCamera.transform;
        savedPosition = cameraTransform.localPosition;
        savedRotation = cameraTransform.localRotation;
        applied = true;
        Vector3 position = target + direction * distance;
        cameraTransform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position, Vector3.up));
    }

    private void RestoreCamera()
    {
        if (!applied) return;
        if (cameraTransform)
        {
            cameraTransform.localPosition = savedPosition;
            cameraTransform.localRotation = savedRotation;
        }
        applied = false;
    }

    private void ReleaseAvatar()
    {
        if (!avatar) return;
        // A native camera mode taking over owns these flags now.
        if (!NativeCameraOverride)
        {
            if (avatar.playerAvatarVisuals) avatar.playerAvatarVisuals.ShowSelfOverride(0f);
            avatar.physGrabber?.SetThirdPerson(false);
            avatar.flashlightController?.SetThirdPerson(false);
        }
        avatar = null;
    }

    internal void Cancel()
    {
        bool wasActive = Active;
        hold.Cancel(Plugin.Instance && Plugin.Instance.SelfViewHeld);
        RestoreCamera();
        ReleaseAvatar();
        if (wasActive && Plugin.Instance) Plugin.Instance.ReportSelfView(false);
    }

    private void OnApplicationFocus(bool focused) { if (!focused) Cancel(); }
    private void OnDisable() => Cancel();
    private void OnDestroy() => Cancel();
}

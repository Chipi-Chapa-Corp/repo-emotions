using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RepoEmoteWheel;

[BepInPlugin(Id, "Refined Emotions", "1.3.6")]
public sealed class Plugin : BaseUnityPlugin
{
    // Keep the original plugin ID so existing bindings/configuration survive the rename.
    public const string Id = "local.repo.mmbemotewheel";
    internal static Plugin Instance;
    internal readonly WheelState State = new WheelState();
    internal const float Duration = 5f;
    private ConfigEntry<string> button;
    private KeyCode buttonCode = KeyCode.Mouse2;
    private ConfigEntry<string> selfViewButton;
    private KeyCode selfViewCode = KeyCode.V;
    private SelfView selfView;
    private Harmony harmony;
    private Vector2 pointer;
    private bool heldLast;
    private bool updateReported;
    private string lastBlockReason;
    private float nextDiagnostic;
    private int closedFrame = -1;
    private WheelView view;
    private static readonly AccessTools.FieldRef<MenuManager, MenuPage> MenuPage =
        AccessTools.FieldRefAccess<MenuManager, MenuPage>("currentMenuPage");
    private static readonly AccessTools.FieldRef<PlayerAvatar, bool> Disabled =
        AccessTools.FieldRefAccess<PlayerAvatar, bool>("isDisabled");
    private static readonly AccessTools.FieldRef<PlayerExpression, bool> LocalExpression =
        AccessTools.FieldRefAccess<PlayerExpression, bool>("isLocal");

    private void Awake()
    {
        Instance = this;
        button = Config.Bind("Emote wheel", "Button", "Mouse2",
            new ConfigDescription("Hold to open the wheel, release to emote. Mouse2 is middle mouse.",
                new AcceptableValueList<string>(WheelBinding.Buttons)));
        ReadButton();
        button.SettingChanged += ButtonChanged;
        selfViewButton = Config.Bind("Emote wheel", "Self-view", "V",
            new ConfigDescription("Hold to look at yourself from the front. Release to return to first person.",
                new AcceptableValueList<string>(WheelBinding.Buttons)));
        ReadSelfViewButton();
        selfViewButton.SettingChanged += SelfViewButtonChanged;
        selfView = gameObject.AddComponent<SelfView>();
        harmony = new Harmony(Id);
        harmony.PatchAll(typeof(Plugin));
        Logger.LogInfo("Refined Emotions ready: hold " + buttonCode + ", point, release. Duration: " + Duration + "s.");
    }

    private void ReadButton()
    {
        if (!Enum.TryParse(button.Value, out buttonCode) || buttonCode == KeyCode.None)
            buttonCode = KeyCode.Mouse2;
    }

    private void ReadSelfViewButton()
    {
        if (!Enum.TryParse(selfViewButton.Value, out selfViewCode) || selfViewCode == KeyCode.None)
            selfViewCode = KeyCode.V;
    }

    private void SelfViewButtonChanged(object sender, EventArgs args)
    {
        ReadSelfViewButton();
        selfView?.Cancel();
    }

    internal bool SelfViewHeld => Input.GetKey(selfViewCode);
    internal void ReportSelfView(bool active) => Logger.LogInfo("Self-view " + (active ? "started" : "ended") + ".");

    private void ButtonChanged(object sender, EventArgs args)
    {
        ReadButton();
        heldLast = Held;
        State.Tick(Time.unscaledTime, false, heldLast, false, 0, 0, 55, Duration);
        closedFrame = Time.frameCount;
    }

    [HarmonyPatch(typeof(PlayerExpression), "Start"), HarmonyPostfix]
    private static void AddPoses(PlayerExpression __instance)
    {
        if (!__instance.GetComponent<ExpressionPose>())
            __instance.gameObject.AddComponent<ExpressionPose>();
    }

    internal bool Allowed() => BlockReason() == null;

    private string BlockReason()
    {
        if (!Application.isFocused) return "window not focused";
        if (!PlayerAvatar.instance) return "no local avatar";
        if (!PlayerAvatar.instance.playerExpression) return "no expression component";
        if (Disabled(PlayerAvatar.instance)) return "avatar disabled/dead";
        if (!LevelGenerator.Instance || !LevelGenerator.Instance.Generated) return "level loading";
        if (!GameDirector.instance) return "no game director";
        if (GameDirector.instance.DisableInput) return "game input disabled";
        if (!MenuManager.instance) return "no menu manager";
        if (MenuPage(MenuManager.instance)) return "menu open: " + MenuPage(MenuManager.instance).name;
        if (SemiFunc.MenuLevel()) return "menu level";
        if (ChatManager.instance && !ChatManager.instance.StateIsInactive()) return "chat active";
        return null;
    }

    private float Scale => Mathf.Min(Screen.width / 1000f, Screen.height / 800f);
    private bool Held => Input.GetKey(buttonCode);
    internal bool BlockLook => State.IsOpen || closedFrame == Time.frameCount ||
        (selfView && selfView.Active) || (Held && Allowed());

    private void Update()
    {
        if (!updateReported)
        {
            updateReported = true;
            Logger.LogInfo("Wheel Update running; mouse=" + (Mouse.current?.displayName ?? "none"));
            foreach (var method in harmony.GetPatchedMethods()) Logger.LogInfo("Patched: " + method.DeclaringType.Name + "." + method.Name);
        }
        if (Time.unscaledTime >= nextDiagnostic)
        {
            nextDiagnostic = Time.unscaledTime + 1f;
            string reason = BlockReason() ?? "ready";
            if (reason != lastBlockReason)
            {
                Logger.LogInfo("Wheel state: " + reason);
                lastBlockReason = reason;
            }
        }
        bool held = Held;
        if (held != heldLast) Logger.LogInfo(buttonCode + " " + (held ? "down" : "up") + "; " + (BlockReason() ?? "ready"));
        bool opening = held && !heldLast;
        if (opening) pointer = Vector2.zero;
        else if (State.IsOpen && Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            pointer += new Vector2(delta.x, -delta.y) / Mathf.Max(Scale, 0.1f);
            pointer = Vector2.ClampMagnitude(pointer, 172f);
        }
        bool cancel = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool wasOpen = State.IsOpen;
        int previous = State.Active;
        State.Tick(Time.unscaledTime, Allowed(), held, cancel, pointer.x, pointer.y, 55, Duration);
        heldLast = held;
        if (wasOpen && !State.IsOpen) closedFrame = Time.frameCount;
        if (previous != State.Active)
        {
            if (State.Active >= 0)
            {
                Logger.LogInfo($"Expression {State.Active + 1} ({Name(State.Active)}) started; {Duration:0.0}s.");
                if (PlayerExpressionsUI.instance) PlayerExpressionsUI.instance.ShrinkReset();
            }
            else Logger.LogInfo("Wheel expression ended.");
        }
    }

    private void LateUpdate()
    {
        if (view == null && State.IsOpen) view = new WheelView();
        view?.Render(State, pointer, Scale, Allowed());
    }

    private string Name(int index)
    {
        var expression = PlayerAvatar.instance ? PlayerAvatar.instance.playerExpression : null;
        if (expression && expression.expressions.Count > index + 1)
            return expression.expressions[index + 1].expressionName;
        return "Expression " + (index + 1);
    }

    [HarmonyPatch(typeof(InputManager), nameof(InputManager.GetMouseX)), HarmonyPrefix]
    private static bool MouseX(ref float __result) => FilterMouse(ref __result);
    [HarmonyPatch(typeof(InputManager), nameof(InputManager.GetMouseY)), HarmonyPrefix]
    private static bool MouseY(ref float __result) => FilterMouse(ref __result);
    private static bool FilterMouse(ref float result)
    {
        if (!Instance || !Instance.BlockLook) return true;
        result = 0;
        return false;
    }

    // Feed the game's own override path, which handles animation, the HUD avatar and RPCs.
    // This works regardless of the user's hold/toggle setting for the number keys.
    [HarmonyPatch(typeof(PlayerExpression), "Update"), HarmonyPrefix]
    private static void ApplyExpression(PlayerExpression __instance)
    {
        if (!Instance || Instance.State.Active < 0 || !Instance.Allowed() ||
            Time.unscaledTime >= Instance.State.EndsAt || !LocalExpression(__instance)) return;
        int index = Instance.State.Active + 1;
        if (__instance.expressions.Count > index) __instance.OverrideExpressionSet(index, 100f);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) State.Tick(Time.unscaledTime, false, Held, false, 0, 0, 55, Duration);
    }

    private void OnDestroy()
    {
        if (button != null) button.SettingChanged -= ButtonChanged;
        if (selfViewButton != null) selfViewButton.SettingChanged -= SelfViewButtonChanged;
        if (selfView) { selfView.Cancel(); Destroy(selfView); }
        foreach (var pose in FindObjectsOfType<ExpressionPose>()) Destroy(pose);
        harmony?.UnpatchSelf();
        view?.Dispose();
        if (Instance == this) Instance = null;
    }

}

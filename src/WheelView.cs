using System;
using UnityEngine;
using UnityEngine.UI;

namespace RepoEmoteWheel;

// Native uGUI canvas; the input/selection state is independent of the presentation.
internal sealed class WheelView : IDisposable
{
    private readonly GameObject root;
    private readonly Canvas canvas;
    private readonly CanvasGroup group, wheelGroup;
    private readonly RectTransform wheel;
    private readonly RawImage selection, cursor;
    private readonly RawImage[] eyes = new RawImage[WheelState.Count];
    private readonly float[] hover = new float[WheelState.Count];
    private readonly Texture2D[] portraits = new Texture2D[WheelState.Count];
    private readonly Texture2D background, highlight, pointerTexture;
    private readonly Image cancelA, cancelB;
    private PlayerExpression source;
    private float visible;
    private int lastSelected = -1;

    internal WheelView()
    {
        root = new GameObject("Refined Emotions UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
        UnityEngine.Object.DontDestroyOnLoad(root);
        canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        group = root.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        wheel = Rect("Wheel", root.transform, Vector2.zero, new Vector2(360, 360));
        wheelGroup = wheel.gameObject.AddComponent<CanvasGroup>();
        background = Texture(WheelArt.Ring(false), 384, 384, "Wheel background");
        highlight = Texture(WheelArt.Ring(true), 384, 384, "Wheel selection");
        pointerTexture = Texture(WheelArt.Pointer(), 20, 20, "Wheel pointer");
        Image("Background", wheel, Vector2.zero, new Vector2(360, 360), background);
        selection = Image("Selection", wheel, Vector2.zero, new Vector2(360, 360), highlight);
        for (int i = 0; i < eyes.Length; i++)
        {
            float a = i * Mathf.PI / 3;
            eyes[i] = Image("Expression " + (i + 1), wheel,
                new Vector2(Mathf.Sin(a) * 122, Mathf.Cos(a) * 122), new Vector2(94, 47), null);
        }
        // No text: a quiet X marks the center's cancel zone.
        cancelA = Line("Cancel A", wheel, Vector2.zero, new Vector2(12, 1.5f), 45);
        cancelB = Line("Cancel B", wheel, Vector2.zero, new Vector2(12, 1.5f), -45);
        cursor = Image("Pointer", wheel, Vector2.zero, new Vector2(15, 15), pointerTexture);
        cursor.rectTransform.pivot = new Vector2(.1f, .9f);
        group.alpha = 0;
    }

    internal void Render(WheelState state, Vector2 pointer, float scale, bool allowed)
    {
        canvas.scaleFactor = Mathf.Max(.1f, scale);
        float dt = Mathf.Min(Time.unscaledDeltaTime, .05f);
        bool open = state.IsOpen && allowed;
        visible = Mathf.MoveTowards(visible, open ? 1 : 0, dt * (open ? 9 : 14));
        if (open && PlayerAvatar.instance)
            PrepareEyes(PlayerAvatar.instance.playerExpression);
        // Keep the short close animation, then hide the entire canvas.
        wheel.gameObject.SetActive(visible > 0);
        group.alpha = 1;
        wheelGroup.alpha = visible;
        wheel.localScale = Vector3.one * Mathf.Lerp(.94f, 1f, 1f - (1f - visible) * (1f - visible));
        if (open) lastSelected = state.Hovered;
        selection.enabled = lastSelected >= 0;
        selection.rectTransform.localRotation = Quaternion.Euler(0, 0, -lastSelected * 60);
        for (int i = 0; i < eyes.Length; i++)
        {
            hover[i] = Mathf.Lerp(hover[i], open && state.Hovered == i ? 1 : 0, 1 - Mathf.Exp(-dt * 20));
            eyes[i].rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 1.13f, hover[i]);
            eyes[i].color = Color.Lerp(new Color(.83f, .83f, .79f, 1), Color.white, hover[i]);
        }
        Color cancelColor = new Color(.80f, .78f, .67f, state.Hovered < 0 ? .70f : .22f);
        cancelA.color = cancelB.color = cancelColor;
        cursor.rectTransform.anchoredPosition = new Vector2(pointer.x, -pointer.y);
        canvas.enabled = visible > 0;
    }

    private void PrepareEyes(PlayerExpression expression)
    {
        if (!expression || source == expression) return;
        source = expression;
        for (int i = 0; i < eyes.Length; i++)
        {
            if (portraits[i]) UnityEngine.Object.Destroy(portraits[i]);
            if (expression.expressions.Count <= i + 1) continue;
            var pose = expression.expressions[i + 1];
            portraits[i] = Texture(WheelArt.Eyes(Pose(pose.leftEye), Pose(pose.rightEye)), 160, 80, "Eyes: " + pose.expressionName);
            eyes[i].texture = portraits[i];
        }
    }

    private static WheelArt.EyePose Pose(EyeSettings eye) => new WheelArt.EyePose
    {
        UpperAngle = eye.upperLidAngle, UpperClosed = eye.upperLidClosedPercent,
        LowerAngle = eye.lowerLidAngle, LowerClosed = eye.lowerLidClosedPercent,
        Pupil = eye.pupilSize, OffsetX = eye.pupilOffsetRotationX, OffsetY = eye.pupilOffsetRotationY
    };

    private static Texture2D Texture(byte[] pixels, int width, int height, string name)
    {
        var t = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        t.LoadRawTextureData(pixels);
        t.Apply(false, true);
        return t;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var r = go.GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
        r.anchoredPosition = position;
        r.sizeDelta = size;
        return r;
    }

    private static RawImage Image(string name, Transform parent, Vector2 position, Vector2 size, Texture texture)
    {
        var r = Rect(name, parent, position, size);
        var image = r.gameObject.AddComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        return image;
    }

    private static Image Line(string name, Transform parent, Vector2 position, Vector2 size, float angle)
    {
        var r = Rect(name, parent, position, size);
        r.localRotation = Quaternion.Euler(0, 0, angle);
        var image = r.gameObject.AddComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(root);
        UnityEngine.Object.Destroy(background);
        UnityEngine.Object.Destroy(highlight);
        UnityEngine.Object.Destroy(pointerTexture);
        foreach (var portrait in portraits) if (portrait) UnityEngine.Object.Destroy(portrait);
    }
}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RepoEmoteWheel;

// Application..cctor can load BepInEx before Unity can schedule new behaviours.
// Subscribe during plugin Awake, but create our components only once a scene loads.
// Never put them on BepInEx_Manager or change the user's shared loader settings.
internal sealed class RuntimeHost : IDisposable
{
    private readonly Plugin owner;
    private GameObject root;
    private bool disposed;
    internal SelfView SelfView { get; private set; }

    internal RuntimeHost(Plugin owner)
    {
        this.owner = owner;
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (disposed || root || !owner) return;
        root = new GameObject("Refined Emotions Runtime");
        UnityEngine.Object.DontDestroyOnLoad(root);
        root.AddComponent<WheelRuntime>().Owner = owner;
        SelfView = root.AddComponent<SelfView>();
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        SceneManager.sceneLoaded -= SceneLoaded;
        if (SelfView) SelfView.Cancel();
        if (root)
        {
            // Destroy is deferred; stop callbacks immediately during plugin teardown.
            root.SetActive(false);
            UnityEngine.Object.Destroy(root);
        }
        root = null;
        SelfView = null;
    }
}

public sealed class WheelRuntime : MonoBehaviour
{
    internal Plugin Owner;

    private void Update() { if (Owner) Owner.Tick(); }
    private void LateUpdate() { if (Owner) Owner.Render(); }
    private void OnApplicationFocus(bool focused) { if (!focused && Owner) Owner.CancelWheel(); }
    private void OnDisable() { if (Owner) Owner.CancelWheel(); }
}

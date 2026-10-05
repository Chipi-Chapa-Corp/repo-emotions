using System;
using System.Collections.Generic;
using System.Reflection;
using RepoEmoteWheel;
using UnityEngine;
using UnityEngine.SceneManagement;

// Exercise production RuntimeHost against a small Unity lifecycle stand-in.
// This verifies deferred ownership/dispatch, not Unity engine integration.
internal static class RuntimeLifecycle
{
    internal static void Check(Action<bool, string> check)
    {
        var plugin = new Plugin();
        var host = new RuntimeHost(plugin);
        check(GameObject.Created.Count == 0 && host.SelfView == null,
            "early loader initialization creates no Unity runtime objects");
        SceneManager.Load();
        var root = GameObject.Created[0];
        check(GameObject.Created.Count == 1 && root.Persistent && root.activeSelf,
            "first loaded scene creates an independent persistent runtime");
        check(root.Components.Count == 2 && host.SelfView != null,
            "wheel and self-view are both attached after scene load");
        root.Frame();
        check(plugin.Ticks == 1 && plugin.Renders == 1,
            "runtime drives input and UI without plugin Unity callbacks");
        SceneManager.Load();
        root.Frame();
        check(GameObject.Created.Count == 1 && plugin.Ticks == 2 && plugin.Renders == 2,
            "scene changes do not duplicate input or rendering");
        root.Send("OnApplicationFocus", false);
        check(plugin.Cancels == 1, "runtime forwards focus loss cancellation");
        root.Send("OnApplicationFocus", true);
        check(plugin.Cancels == 1, "focus gain does not cancel again");
        var selfView = host.SelfView;
        host.Dispose();
        check(!root.activeSelf && root.Destroyed && selfView.Cancels == 1 && plugin.Cancels == 2,
            "teardown stops callbacks immediately and cancels wheel and camera");
        root.Frame();
        SceneManager.Load();
        host.Dispose();
        check(plugin.Ticks == 2 && GameObject.Created.Count == 1 && SceneManager.Subscribers == 0,
            "disposed runtime cannot tick or respawn and removes its scene subscription");
        using (var early = new RuntimeHost(new Plugin())) early.Dispose();
        SceneManager.Load();
        check(GameObject.Created.Count == 1 && SceneManager.Subscribers == 0,
            "teardown before first scene leaves no runtime or subscription");
        var absent = new Plugin();
        using (var orphan = new RuntimeHost(absent))
        {
            UnityEngine.Object.Destroy(absent);
            SceneManager.Load();
            check(GameObject.Created.Count == 1, "destroyed plugin cannot create an orphan runtime");
        }
    }
}

namespace RepoEmoteWheel
{
    public sealed class Plugin : MonoBehaviour
    {
        internal int Ticks, Renders, Cancels;
        internal void Tick() => Ticks++;
        internal void Render() => Renders++;
        internal void CancelWheel() => Cancels++;
    }

    public sealed class SelfView : MonoBehaviour
    {
        internal int Cancels;
        internal void Cancel() => Cancels++;
    }
}

namespace UnityEngine
{
    public class Object
    {
        internal bool Destroyed;
        public static implicit operator bool(Object value) => value != null && !value.Destroyed;
        public static void Destroy(Object value) => value.Destroyed = true;
        public static void DontDestroyOnLoad(GameObject value) => value.Persistent = true;
    }

    public class MonoBehaviour : Object { }

    public sealed class GameObject : Object
    {
        internal static readonly List<GameObject> Created = new List<GameObject>();
        internal readonly List<MonoBehaviour> Components = new List<MonoBehaviour>();
        internal bool Persistent;
        internal bool activeSelf = true;
        public GameObject(string name) => Created.Add(this);
        public T AddComponent<T>() where T : MonoBehaviour, new()
        {
            var component = new T();
            Components.Add(component);
            return component;
        }
        public void SetActive(bool active)
        {
            activeSelf = active;
            if (!active) Send("OnDisable");
        }
        internal void Frame()
        {
            if (!activeSelf || Destroyed) return;
            Send("Update");
            Send("LateUpdate");
        }
        internal void Send(string message, params object[] args)
        {
            foreach (var component in Components)
                component.GetType().GetMethod(message, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(component, args);
        }
    }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { }
    public enum LoadSceneMode { Single, Additive }
    public static class SceneManager
    {
        public static event Action<Scene, LoadSceneMode> sceneLoaded;
        internal static int Subscribers => sceneLoaded?.GetInvocationList().Length ?? 0;
        internal static void Load() => sceneLoaded?.Invoke(new Scene(), LoadSceneMode.Single);
    }
}

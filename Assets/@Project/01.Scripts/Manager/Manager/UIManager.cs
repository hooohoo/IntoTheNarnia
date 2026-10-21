using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UIManager : Singleton<UIManager>, IListener<GameStateEvent>
{
    private static readonly int PopupBaseOrder = 100;
    private static readonly int PopupOrderStep = 10;
    [Singleton(typeof(AddressableManager))] private AddressableManager resources;
    [Singleton(typeof(EventManager))] private EventManager events;
    [Singleton(typeof(GameStateManager))] private GameStateManager states;
    private readonly Dictionary<string, UI_Scene> sceneCache = new Dictionary<string, UI_Scene>();
    private readonly Dictionary<string, UI_Popup> popupCache = new Dictionary<string, UI_Popup>();
    private readonly List<UI_Popup> popupStack = new List<UI_Popup>();
    private readonly Dictionary<string, Queue<UI_Sub>> subPools = new Dictionary<string, Queue<UI_Sub>>();
    private readonly Dictionary<UI_Base, HashSet<UI_Sub>> ownerSubs = new Dictionary<UI_Base, HashSet<UI_Sub>>();
    private Transform sceneRoot;
    private Transform popupRoot;
    private Transform poolRoot;
    private object owner;
    private object screenOwner;
    private FieldScreen screen;
    public override void Init() { events.AddListener<GameStateEvent>(this); }
    public void SetSceneUIRoot(object sceneOwner, Transform root)
    {
        ClearUIs();
        owner = sceneOwner;
        sceneRoot = CreateRoot("SceneUI_Root", root);
        popupRoot = CreateRoot("PopupUI_Root", root);
        poolRoot = CreateRoot("SubUI_Pool", root);
    }
    private static Transform CreateRoot(string name, Transform parent)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);
        return root;
    }
    private T CreateUI<T>(string key, Transform parent) where T : UI_Base
    {
        if (parent == null) throw new InvalidOperationException("Scene UI root is not bound.");
        var instance = UnityEngine.Object.Instantiate(resources.Load<GameObject>(key), parent, false);
        instance.name = key;
        var ui = instance.GetComponent<T>();
        if (ui == null) { UnityEngine.Object.Destroy(instance); throw new MissingComponentException(key + " requires " + typeof(T).Name); }
        ui.SetUIKey(key);
        ui.Init();
        return ui;
    }
    public T ShowSceneUI<T>(string key) where T : UI_Scene
    {
        if (!sceneCache.TryGetValue(key, out var value))
        {
            value = CreateUI<T>(key, sceneRoot);
            value.SetSortOrder(sceneCache.Count);
            sceneCache.Add(key, value);
        }
        value.Open();
        return (T)value;
    }
    public void CloseSceneUI(string key)
    {
        if (!sceneCache.TryGetValue(key, out var ui)) return;
        CloseSubUIs(ui);
        ui.Close();
    }
    public T ShowPopupUI<T>(string key) where T : UI_Popup
    {
        if (!popupCache.TryGetValue(key, out var value))
        {
            value = CreateUI<T>(key, popupRoot);
            popupCache.Add(key, value);
        }
        if (!popupStack.Contains(value)) popupStack.Add(value);
        ReorderPopups();
        value.Open();
        return (T)value;
    }
    public void ClosePopupUI(string key)
    {
        if (!popupCache.TryGetValue(key, out var ui) || !popupStack.Remove(ui)) return;
        CloseSubUIs(ui);
        ui.Close();
        ReorderPopups();
    }
    public void CloseTopPopup()
    {
        if (popupStack.Count == 0) return;
        UI_Popup popup = popupStack[popupStack.Count - 1];
        if (popup.UIKey == AssetKeys.UI.Settings) HideSettings();
        else if (popup.UIKey == AssetKeys.UI.Dialogue) states.Pop(GameStateType.Dialogue);
        else ClosePopupUI(popup.UIKey);
    }
    private void ReorderPopups()
    {
        for (int i = 0; i < popupStack.Count; i++) popupStack[i].SetSortOrder(PopupBaseOrder + i * PopupOrderStep);
    }
    public T ShowSubUI<T>(string key, UI_Base parent, Transform container) where T : UI_Sub
    {
        if (parent == null || container == null) throw new ArgumentNullException(nameof(parent));
        UI_Sub ui = null;
        if (subPools.TryGetValue(key, out var queue))
            while (queue.Count > 0 && ui == null) ui = queue.Dequeue();
        if (ui == null) ui = CreateUI<T>(key, container);
        ui.transform.SetParent(container, false);
        ui.SetOwnerUI(parent);
        if (!ownerSubs.TryGetValue(parent, out var children))
        {
            children = new HashSet<UI_Sub>();
            ownerSubs.Add(parent, children);
        }
        children.Add(ui);
        ui.Open();
        ui.OnSpawn();
        return (T)ui;
    }
    public void CloseSubUI(UI_Sub ui)
    {
        if (ui == null || ui.OwnerUI == null || !ownerSubs.TryGetValue(ui.OwnerUI, out var children) || !children.Remove(ui)) return;
        ui.OnDespawn();
        ui.Close();
        ui.SetOwnerUI(null);
        ui.transform.SetParent(poolRoot, false);
        if (!subPools.TryGetValue(ui.UIKey, out var queue))
        {
            queue = new Queue<UI_Sub>();
            subPools.Add(ui.UIKey, queue);
        }
        queue.Enqueue(ui);
    }
    public void CloseSubUIs(UI_Base parent)
    {
        if (!ownerSubs.TryGetValue(parent, out var children)) return;
        var values = new List<UI_Sub>(children);
        for (int i = 0; i < values.Count; i++) CloseSubUI(values[i]);
        ownerSubs.Remove(parent);
    }
    public void Bind(object sceneOwner, FieldScreen value)
    {
        screen?.Release();
        screenOwner = sceneOwner;
        screen = value;
        screen.Show(states.Current);
    }
    public void Unbind(object sceneOwner)
    {
        if (!ReferenceEquals(screenOwner, sceneOwner)) return;
        screen?.Release();
        screen = null;
        screenOwner = null;
    }
    public void OnEvent(GameStateEvent eventType, object param = null)
    {
        var message = (StateChanged)param;
        screen?.Show(message.Current);
        if (!states.ContainsState(GameStateType.Settings)) ClosePopupUI(AssetKeys.UI.Settings);
        if (!states.ContainsState(GameStateType.Dialogue)) ClosePopupUI(AssetKeys.UI.Dialogue);
    }
    public void ShowSettings() { ShowPopupUI<UI_Settings>(AssetKeys.UI.Settings); states.Push(GameStateType.Settings); }
    public void HideSettings() { states.Pop(GameStateType.Settings); }
    public void ShowDialogue(string speaker, string[] lines) { ShowPopupUI<UI_Dialogue>(AssetKeys.UI.Dialogue).Show(speaker, lines); }
    public void ReleaseSceneUIs(object sceneOwner)
    {
        if (!ReferenceEquals(owner, sceneOwner)) return;
        Unbind(sceneOwner);
        ClearUIs();
        owner = null;
    }
    private void ClearUIs()
    {
        var parents = new List<UI_Base>(ownerSubs.Keys);
        for (int i = 0; i < parents.Count; i++) CloseSubUIs(parents[i]);
        foreach (var queue in subPools.Values)
            while (queue.Count > 0) { var sub = queue.Dequeue(); if (sub != null) sub.Release(); }
        foreach (var ui in sceneCache.Values) if (ui != null) { ui.Close(); ui.Release(); }
        foreach (var ui in popupCache.Values) if (ui != null) { ui.Close(); ui.Release(); }
        sceneCache.Clear();
        popupCache.Clear();
        popupStack.Clear();
        subPools.Clear();
        ownerSubs.Clear();
        if (sceneRoot != null) UnityEngine.Object.Destroy(sceneRoot.gameObject);
        if (popupRoot != null) UnityEngine.Object.Destroy(popupRoot.gameObject);
        if (poolRoot != null) UnityEngine.Object.Destroy(poolRoot.gameObject);
        sceneRoot = null;
        popupRoot = null;
        poolRoot = null;
    }
    public override void Release()
    {
        events.RemoveListener<GameStateEvent>(this);
        screen = null;
        screenOwner = null;
        ClearUIs();
        owner = null;
    }
}

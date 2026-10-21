using System;
using System.Collections.Generic;

public sealed class EventManager : Singleton<EventManager>
{
    private static readonly string LogTag = "[EventManager]";
    private readonly Dictionary<Type, List<object>> listeners = new Dictionary<Type, List<object>>();
    public void AddListener<TEnum>(IListener<TEnum> listener) where TEnum : Enum
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        if (!listeners.TryGetValue(typeof(TEnum), out var entries))
        {
            entries = new List<object>();
            listeners.Add(typeof(TEnum), entries);
        }
        if (!entries.Contains(listener)) entries.Add(listener);
    }
    public void RemoveListener<TEnum>(IListener<TEnum> listener) where TEnum : Enum
    {
        if (listeners.TryGetValue(typeof(TEnum), out var entries)) entries.Remove(listener);
    }
    public void PostNotification<TEnum>(TEnum eventType, object param = null) where TEnum : Enum
    {
        if (!listeners.TryGetValue(typeof(TEnum), out var entries)) return;
        var snapshot = entries.ToArray();
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            object listener = snapshot[i];
            if (!entries.Contains(listener)) continue;
            if (listener is UnityEngine.Object owner && owner == null) { entries.Remove(listener); continue; }
            if (listener is UnityEngine.Behaviour behaviour && !behaviour.isActiveAndEnabled) continue;
            try { ((IListener<TEnum>)listener).OnEvent(eventType, param); }
            catch (Exception exception) { LogUtil.LogError(LogTag + " " + eventType + ": " + exception); }
        }
    }
    public override void Release() { listeners.Clear(); }
}

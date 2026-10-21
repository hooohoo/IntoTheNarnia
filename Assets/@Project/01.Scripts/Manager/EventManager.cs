using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EventManager : Singleton<EventManager>
{
    private readonly Dictionary<Type, List<object>> listeners = new Dictionary<Type, List<object>>();
    public void Subscribe<T>(IEventListener<T> listener)
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        if (!listeners.TryGetValue(typeof(T), out var entries))
        {
            entries = new List<object>();
            listeners.Add(typeof(T), entries);
        }
        if (!entries.Contains(listener)) entries.Add(listener);
    }
    public void Unsubscribe<T>(IEventListener<T> listener)
    {
        if (listeners.TryGetValue(typeof(T), out var entries)) entries.Remove(listener);
    }
    public void Publish<T>(T message)
    {
        if (!listeners.TryGetValue(typeof(T), out var entries)) return;
        // A distinct snapshot also makes nested Publish safe.
        var snapshot = entries.ToArray();
        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            var listener = snapshot[i];
            if (!entries.Contains(listener)) continue;
            if (listener is UnityEngine.Object owner && owner == null) { entries.Remove(listener); continue; }
            try { ((IEventListener<T>)listener).OnEvent(message); }
            catch (Exception exception) { Debug.LogException(exception); }
        }
    }
    public override void Release() { listeners.Clear(); }
}

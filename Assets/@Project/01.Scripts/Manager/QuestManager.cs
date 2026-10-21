using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class QuestManager : Singleton<QuestManager>, IEventListener<DoorChanged>
{
    [Singleton(typeof(EventManager))] private EventManager events;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    private readonly Dictionary<string, QuestProgress> progress = new Dictionary<string, QuestProgress>();
    private QuestProgress current;
    private string sceneName;
    private object owner;
    public int Level => current == null ? 0 : current.Level;
    public override void Init() { events.Subscribe<DoorChanged>(this); }
    public void Bind(object sceneOwner, string scene)
    {
        owner = sceneOwner;
        sceneName = scene;
        if (!progress.TryGetValue(scene, out current)) { current = new QuestProgress(); progress.Add(scene, current); }
        ApplyUnlocks();
    }
    public void Unbind(object sceneOwner)
    {
        if (!ReferenceEquals(owner, sceneOwner)) return;
        owner = null;
        sceneName = null;
        current = null;
    }
    public void Begin()
    {
        if (current == null || current.Level != 0) return;
        current.Begin();
        ApplyUnlocks();
        events.Publish(new QuestChanged(sceneName, Level));
    }
    public void OnEvent(DoorChanged message)
    {
        if (!message.IsOpen || message.Door == null) return;
        OpenArea(message.Door.name);
        if (current == null || !current.OpenDoor(message.Door.name)) return;
        ApplyUnlocks();
        events.Publish(new QuestChanged(sceneName, Level));
    }
    private void ApplyUnlocks()
    {
        if (objects.SiblingsDoor != null) objects.SiblingsDoor.IsUnlocked = Level >= 1;
        if (objects.WardrobeDoor != null) objects.WardrobeDoor.IsUnlocked = Level >= 2;
    }
    public void OpenArea(string name)
    {
        int area = NavMesh.GetAreaFromName(name);
        if (area < 0 || objects.PlayerAgent == null) return;
        objects.PlayerAgent.areaMask |= 1 << area;
    }
    public override void Release()
    {
        events.Unsubscribe<DoorChanged>(this);
        progress.Clear();
        owner = null;
        current = null;
    }
}

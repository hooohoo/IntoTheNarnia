using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class ObjectManager : Singleton<ObjectManager>
{
    [Singleton(typeof(AddressableManager))] private AddressableManager resources;
    private readonly Dictionary<string, Queue<OBJ_Base>> pools = new Dictionary<string, Queue<OBJ_Base>>();
    private readonly HashSet<OBJ_Base> active = new HashSet<OBJ_Base>();
    private object owner;
    private Transform sceneRoot;
    public PlayerController Player { get; private set; }
    public NavMeshAgent PlayerAgent { get; private set; }
    public DoorController InteractionDoor { get; private set; }
    public DoorController SiblingsDoor { get; set; }
    public DoorController WardrobeDoor { get; set; }
    public void SetSceneRoot(object sceneOwner, Transform root)
    {
        ClearPool();
        owner = sceneOwner;
        sceneRoot = root;
    }
    public T Spawn<T>(string key, Vector3 position, Quaternion rotation) where T : OBJ_Base
    {
        if (sceneRoot == null) throw new InvalidOperationException("Scene object root is not bound.");
        OBJ_Base instance = null;
        if (pools.TryGetValue(key, out var queue))
            while (queue.Count > 0 && instance == null) instance = queue.Dequeue();
        if (instance == null)
        {
            GameObject prefab = resources.Load<GameObject>(key);
            var gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation, sceneRoot);
            instance = gameObject.GetComponent<T>();
            if (instance == null) { UnityEngine.Object.Destroy(gameObject); throw new MissingComponentException(key + " requires " + typeof(T).Name); }
            instance.SetObjectKey(key);
        }
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.gameObject.SetActive(true);
        active.Add(instance);
        instance.Init();
        instance.OnSpawn();
        return (T)instance;
    }
    public void Despawn(OBJ_Base instance)
    {
        if (instance == null || !active.Remove(instance)) return;
        instance.OnDespawn();
        instance.gameObject.SetActive(false);
        if (!pools.TryGetValue(instance.ObjectKey, out var queue))
        {
            queue = new Queue<OBJ_Base>();
            pools.Add(instance.ObjectKey, queue);
        }
        queue.Enqueue(instance);
        if (Player == instance) SetPlayer(null);
    }
    public void SetPlayer(PlayerController player)
    {
        Player = player;
        PlayerAgent = player == null ? null : player.GetComponent<NavMeshAgent>();
    }
    public void SetInteraction(DoorController door) { InteractionDoor = door; }
    public void ClearInteraction(DoorController door) { if (InteractionDoor == door) InteractionDoor = null; }
    public void ClearScene()
    {
        SetPlayer(null);
        InteractionDoor = null;
        SiblingsDoor = null;
        WardrobeDoor = null;
    }
    public void ReleaseScene(object sceneOwner)
    {
        if (!ReferenceEquals(owner, sceneOwner)) return;
        ClearPool();
        sceneRoot = null;
        owner = null;
    }
    private void ClearPool()
    {
        foreach (var instance in active)
            if (instance != null) { instance.OnDespawn(); instance.gameObject.SetActive(false); UnityEngine.Object.Destroy(instance.gameObject); }
        active.Clear();
        foreach (var queue in pools.Values)
            while (queue.Count > 0) { var instance = queue.Dequeue(); if (instance != null) UnityEngine.Object.Destroy(instance.gameObject); }
        pools.Clear();
        ClearScene();
    }
    public override void Release() { ClearPool(); sceneRoot = null; owner = null; }
}

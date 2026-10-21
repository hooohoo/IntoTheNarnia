public abstract class OBJ_Base : RuntimeBehaviour
{
    public string ObjectKey { get; private set; }
    public void SetObjectKey(string value) { ObjectKey = value; }
    public virtual void Init() { EnsureInitialized(); }
    public virtual void OnSpawn() { }
    public virtual void OnDespawn() { }
}

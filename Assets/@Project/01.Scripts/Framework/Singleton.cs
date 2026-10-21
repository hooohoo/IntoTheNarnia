public abstract class Singleton<T> : IManager where T : class, IManager
{
    // Infrastructure/serialized compatibility adapters only. Gameplay uses injection.
    public static T Instance => FacadeManager.Instance.Get<T>();
    public virtual void Init() { }
    public virtual void Release() { }
}

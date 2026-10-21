using System;
using System.Collections.Generic;

public sealed class ServiceRegistry
{
    private readonly Dictionary<Type, IManager> services = new Dictionary<Type, IManager>();
    private readonly List<IManager> initialized = new List<IManager>();
    public bool IsReady { get; private set; }

    public void Initialize(IReadOnlyList<IManager> managers)
    {
        if (services.Count != 0) throw new InvalidOperationException("Registry is already initialized.");
        var order = DependencyResolver.Sort(managers);
        foreach (var manager in managers) services.Add(manager.GetType(), manager);
        try
        {
            foreach (var manager in order) Inject(manager);
            foreach (var manager in order)
            {
                // A partially initialized manager must also receive Release.
                initialized.Add(manager);
                manager.Init();
            }
            IsReady = true;
        }
        catch (Exception initializationFailure)
        {
            try { Release(); }
            catch (Exception cleanupFailure) { throw new AggregateException("Initialization and rollback failed.", initializationFailure, cleanupFailure); }
            throw;
        }
    }

    public T Get<T>() where T : class, IManager => (T)Get(typeof(T));
    public IManager Get(Type type)
    {
        if (!services.TryGetValue(type, out var service)) throw new InvalidOperationException("Unregistered service: " + type.Name);
        return service;
    }
    public void Inject(object target)
    {
        foreach (var field in DependencyResolver.GetInjectedFields(target.GetType()))
        {
            var attribute = (SingletonAttribute)Attribute.GetCustomAttribute(field, typeof(SingletonAttribute));
            if (!field.FieldType.IsAssignableFrom(attribute.ServiceType)) throw new InvalidOperationException("Invalid injection field: " + field.Name);
            field.SetValue(target, Get(attribute.ServiceType));
        }
    }
    public void Release()
    {
        IsReady = false;
        List<Exception> failures = null;
        for (int i = initialized.Count - 1; i >= 0; i--)
        {
            try { initialized[i].Release(); }
            catch (Exception exception)
            {
                if (failures == null) failures = new List<Exception>();
                failures.Add(exception);
            }
        }
        initialized.Clear();
        services.Clear();
        if (failures != null) throw new AggregateException("Manager cleanup failed.", failures);
    }
}

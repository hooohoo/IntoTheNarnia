using System;
using System.Collections.Generic;
using System.Reflection;

public static class DependencyResolver
{
    public static List<FieldInfo> GetInjectedFields(Type type)
    {
        var result = new List<FieldInfo>();
        while (type != null && type != typeof(object))
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (field.IsDefined(typeof(SingletonAttribute), false)) result.Add(field);
            type = type.BaseType;
        }
        return result;
    }

    public static List<IManager> Sort(IReadOnlyList<IManager> managers)
    {
        var registered = new Dictionary<Type, IManager>();
        var dependencies = new Dictionary<Type, HashSet<Type>>();
        foreach (var manager in managers)
        {
            if (manager == null) throw new ArgumentException("Null manager registration.");
            var type = manager.GetType();
            if (registered.ContainsKey(type)) throw new InvalidOperationException("Duplicate manager: " + type.Name);
            registered.Add(type, manager);
        }
        foreach (var manager in managers)
        {
            var required = new HashSet<Type>();
            foreach (var field in GetInjectedFields(manager.GetType()))
            {
                var dependency = field.GetCustomAttribute<SingletonAttribute>().ServiceType;
                if (!registered.ContainsKey(dependency)) throw new InvalidOperationException("Unregistered dependency: " + dependency.Name);
                if (!field.FieldType.IsAssignableFrom(dependency)) throw new InvalidOperationException("Invalid injection field: " + field.Name);
                required.Add(dependency);
            }
            dependencies.Add(manager.GetType(), required);
        }
        var ordered = new List<IManager>();
        var ready = new Queue<Type>();
        foreach (var manager in managers)
            if (dependencies[manager.GetType()].Count == 0) ready.Enqueue(manager.GetType());
        while (ready.Count > 0)
        {
            var type = ready.Dequeue();
            ordered.Add(registered[type]);
            foreach (var manager in managers)
            {
                var next = manager.GetType();
                if (dependencies[next].Remove(type) && dependencies[next].Count == 0) ready.Enqueue(next);
            }
        }
        if (ordered.Count != managers.Count) throw new InvalidOperationException("Cyclic manager dependency.");
        return ordered;
    }
}

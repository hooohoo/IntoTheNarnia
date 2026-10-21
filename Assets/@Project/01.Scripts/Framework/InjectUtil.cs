using System;
using System.Reflection;
using UnityEngine;

public static class InjectUtil
{
    public static void BindComponents(MonoBehaviour target)
    {
        for (var type = target.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var attribute = field.GetCustomAttribute<FindComponentsAttribute>();
                if (attribute == null) continue;
                if (!typeof(Component).IsAssignableFrom(field.FieldType)) throw new InvalidOperationException("FindComponents requires Component: " + field.Name);
                var root = string.IsNullOrEmpty(attribute.Path) ? target.transform : target.transform.Find(attribute.Path);
                var component = root == null ? null : root.GetComponent(field.FieldType);
                if (component == null) throw new MissingComponentException(target.name + ": " + field.Name);
                field.SetValue(target, component);
            }
        }
    }
}

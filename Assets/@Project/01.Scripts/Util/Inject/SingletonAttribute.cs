using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class SingletonAttribute : Attribute
{
    public Type ServiceType { get; }
    public SingletonAttribute(Type serviceType) { ServiceType = serviceType; }
}

using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class FindComponentsAttribute : Attribute
{
    public string Path { get; }
    public FindComponentsAttribute(string path = "") { Path = path; }
}

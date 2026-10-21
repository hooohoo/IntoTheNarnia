using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBinding
{
    public static GameObject NamedChild(GameObject root, string name)
    {
        if (root == null) throw new MissingReferenceException("Missing scene binding root for " + name);
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child.gameObject;
        throw new MissingReferenceException(root.name + ": missing child " + name);
    }
    public static List<T> InScene<T>(Scene scene) where T : Component
    {
        var result = new List<T>();
        foreach (var root in scene.GetRootGameObjects()) result.AddRange(root.GetComponentsInChildren<T>(true));
        return result;
    }
    public static T FirstInScene<T>(Scene scene) where T : Component
    {
        var components = InScene<T>(scene);
        return components.Count == 0 ? null : components[0];
    }
    public static Camera MainCamera(Scene scene)
    {
        foreach (var camera in InScene<Camera>(scene)) if (camera.CompareTag("MainCamera")) return camera;
        return FirstInScene<Camera>(scene);
    }
}

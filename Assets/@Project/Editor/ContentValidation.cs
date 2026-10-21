using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class ContentValidation
{
    [MenuItem("Narnia/Validate Build Content")]
    public static void Run()
    {
        var previous = EditorSceneManager.GetSceneManagerSetup();
        var failures = new List<string>();
        int callbacks = 0;
        int renderers = 0;
        try
        {
            foreach (var entry in EditorBuildSettings.scenes)
            {
                if (!entry.enabled) continue;
                Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                if (SceneBinding.FirstInScene<SceneRoot>(scene) == null) failures.Add(entry.path + ": SceneRoot missing");
                foreach (var root in scene.GetRootGameObjects())
                {
                    ValidateObject(root, entry.path, failures, ref callbacks);
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        renderers++;
                        foreach (var material in renderer.sharedMaterials)
                            if (material != null && material.shader != null && material.shader.name == "Standard")
                                failures.Add(entry.path + ": Built-in Standard on " + renderer.name + ": " + AssetDatabase.GetAssetPath(material));
                    }
                    foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                        if (component != null && component.GetType().Assembly.GetName().Name == "IntoTheNarnia.Samples") failures.Add(entry.path + ": runtime sample component " + component.GetType().Name);
                }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/@Project/02.Prefabs/UI", "Assets/@Project/02.Prefabs/Character" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ValidateObject(AssetDatabase.LoadAssetAtPath<GameObject>(path), path, failures, ref callbacks);
            }
        }
        finally
        {
            bool restore = previous.Length > 0;
            for (int i = 0; i < previous.Length; i++) if (string.IsNullOrEmpty(previous[i].path) || !File.Exists(previous[i].path)) restore = false;
            if (restore) EditorSceneManager.RestoreSceneManagerSetup(previous);
            else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
        if (!(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)) failures.Add("URP is not configured");
        string report = "Renderers: " + renderers + "\nPersistent calls: " + callbacks + "\nFailures: " + failures.Count + "\n" + string.Join("\n", failures);
        Directory.CreateDirectory("Docs/Validation/ChalkParity");
        File.WriteAllText("Docs/Validation/ChalkParity/Content.txt", report);
        Debug.Log(report);
        if (failures.Count > 0) throw new InvalidOperationException(report);
    }
    private static void ValidateObject(GameObject root, string path, List<string> failures, ref int callbacks)
    {
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0) failures.Add(path + ": missing script " + transform.name);
            foreach (var component in transform.GetComponents<MonoBehaviour>())
            {
                if (component == null) continue;
                using (var serialized = new SerializedObject(component))
                {
                    var property = serialized.GetIterator();
                    while (property.Next(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                            failures.Add(path + ": missing reference " + component.name + "." + property.propertyPath);
                        if (!property.propertyPath.EndsWith("m_PersistentCalls.m_Calls") || !property.isArray) continue;
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            var call = property.GetArrayElementAtIndex(i);
                            string name = call.FindPropertyRelative("m_MethodName").stringValue;
                            if (string.IsNullOrEmpty(name)) continue;
                            callbacks++;
                            var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
                            int mode = call.FindPropertyRelative("m_Mode").enumValueIndex;
                            Type[] arguments = Type.EmptyTypes;
                            if (mode == 2) arguments = new[] { typeof(UnityEngine.Object) };
                            if (mode == 3) arguments = new[] { typeof(int) };
                            if (mode == 4) arguments = new[] { typeof(float) };
                            if (mode == 5) arguments = new[] { typeof(string) };
                            if (mode == 6) arguments = new[] { typeof(bool) };
                            if (target == null || UnityEventBase.GetValidMethodInfo(target, name, arguments) == null)
                                failures.Add(path + ": invalid callback " + component.name + "." + name);
                        }
                    }
                }
            }
        }
    }
}

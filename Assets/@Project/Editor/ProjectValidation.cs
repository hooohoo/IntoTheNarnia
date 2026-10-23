using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectValidation
{
    [MenuItem("Narnia/Validate Project Structure")]
    public static void Run()
    {
        var failures = new List<string>();
        int scenes = 0;
        int calls = 0;
        var previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
        foreach (var sceneGuid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/@Project/00.Scenes" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(sceneGuid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            scenes++;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                    if (missing > 0) failures.Add(path + ": Missing Script on " + transform.name);
                    foreach (var component in transform.GetComponents<MonoBehaviour>())
                    {
                        if (component == null) continue;
                        using (var serialized = new SerializedObject(component))
                        {
                            var property = serialized.GetIterator();
                            while (property.Next(true))
                            {
                                if (!property.propertyPath.EndsWith("m_PersistentCalls.m_Calls") || !property.isArray) continue;
                                for (int i = 0; i < property.arraySize; i++)
                                {
                                    var call = property.GetArrayElementAtIndex(i);
                                    var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
                                    var method = call.FindPropertyRelative("m_MethodName").stringValue;
                                    if (string.IsNullOrEmpty(method)) continue;
                                    calls++;
                                    bool found = false;
                                    if (target != null)
                                        foreach (var info in target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                            if (info.Name == method) { found = true; break; }
                                    if (!found) failures.Add(path + ": broken UnityEvent " + method + " target=" + (target == null ? "null" : target.GetType().FullName + ":" + target.name) + " owner=" + component.name);
                                }
                            }
                        }
                    }
                }
            }
            if (scene.name == "Professor_House") ProfessorHouseValidation.Check(scene, failures);
        }
        }
        finally
        {
            bool restore = previous.Length > 0;
            for (int i = 0; i < previous.Length; i++)
                if (string.IsNullOrEmpty(previous[i].path) || !File.Exists(previous[i].path)) restore = false;
            if (restore) EditorSceneManager.RestoreSceneManagerSetup(previous);
            else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
        foreach (var scene in EditorBuildSettings.scenes)
            if (!File.Exists(scene.path)) failures.Add("Missing build scene: " + scene.path);
        string report = "Scenes: " + scenes + "\nPersistent calls: " + calls + "\nFailures: " + failures.Count + "\n" + string.Join("\n", failures);
        Debug.Log(report);
        string output = Environment.GetEnvironmentVariable("NARNIA_VALIDATION_REPORT");
        if (!string.IsNullOrEmpty(output)) File.WriteAllText(output, report);
        if (failures.Count != 0) throw new InvalidOperationException(report);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressableValidation
{
    [MenuItem("Narnia/Validate Addressable Contracts")]
    public static void Run()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) throw new InvalidOperationException("Addressables settings are missing.");
        var failures = new List<string>();
        var addresses = new HashSet<string>();
        var labels = new HashSet<string>();
        int count = 0;
        foreach (var group in settings.groups)
        {
            if (group == null || group.GetSchema<BundledAssetGroupSchema>() == null) continue;
            foreach (var entry in group.entries)
            {
                count++;
                string path = entry.AssetPath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) failures.Add("Missing asset " + entry.guid);
                if (path.Contains("/Resources/")) failures.Add("Addressable in Resources: " + path);
                if (entry.address != Path.GetFileNameWithoutExtension(path)) failures.Add("Address must equal filename: " + path);
                if (!addresses.Add(entry.address)) failures.Add("Duplicate address: " + entry.address);
                foreach (string label in entry.labels) labels.Add(label);
                if (group.Name != "Dependencies" && !entry.labels.Contains(group.Name)) failures.Add("Missing feature label: " + path);
                if (path.EndsWith(".prefab"))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (entry.address.StartsWith("UI_") && prefab.GetComponent<UI_Base>() == null) failures.Add("UI prefab lacks UI_Base: " + path);
                }
            }
        }
        foreach (string label in new[] { AddressableLabels.Title, AddressableLabels.Loading, AddressableLabels.ProfessorHouse, AddressableLabels.Story })
            if (!labels.Contains(label)) failures.Add("Required feature label missing: " + label);
        string result = "Entries: " + count + "\nLabels: " + string.Join(", ", labels) + "\nFailures: " + failures.Count + "\n" + string.Join("\n", failures);
        Directory.CreateDirectory("Docs/Validation/ChalkParity");
        File.WriteAllText("Docs/Validation/ChalkParity/Addressables.txt", result);
        Debug.Log(result);
        if (failures.Count != 0) throw new InvalidOperationException(result);
    }
}

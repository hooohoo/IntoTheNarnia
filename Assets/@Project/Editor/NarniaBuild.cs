using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class NarniaBuild
{
    [MenuItem("Narnia/Build/Addressable Content")]
    public static void BuildContent()
    {
        AddressableValidation.Run();
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        if (!string.IsNullOrEmpty(result.Error)) throw new BuildFailedException(result.Error);
        Write("ContentBuild.txt", "Target: " + EditorUserBuildSettings.activeBuildTarget + "\nDuration: " + result.Duration + "\nOutput: " + result.OutputPath + "\nResult: Succeeded\n");
    }
    [MenuItem("Narnia/Build/Analyze Shared Dependencies")]
    public static void AnalyzeDependencies()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var rule = new CheckBundleDupeDependencies();
        var results = rule.RefreshAnalysis(settings);
        var lines = new List<string>();
        bool duplicates = false;
        for (int i = 0; i < results.Count; i++)
        {
            lines.Add(results[i].severity + ": " + results[i].resultName);
            if (results[i].severity == MessageType.Warning) duplicates = true;
        }
        if (duplicates)
        {
            rule.FixIssues(settings);
            var group = settings.FindGroup("Duplicate Asset Isolation");
            if (group != null)
            {
                group.Name = "Dependencies";
                foreach (var entry in group.entries) entry.address = Path.GetFileNameWithoutExtension(entry.AssetPath);
            }
            AssetDatabase.SaveAssets();
            rule.ClearAnalysis();
            results = rule.RefreshAnalysis(settings);
            lines.Add("After dependency isolation:");
            for (int i = 0; i < results.Count; i++)
            {
                lines.Add(results[i].severity + ": " + results[i].resultName);
                if (results[i].severity == MessageType.Warning || results[i].severity == MessageType.Error)
                    throw new BuildFailedException("Addressable duplicate dependency remains: " + results[i].resultName);
            }
        }
        Write("Dependencies.txt", string.Join("\n", lines));
    }
    [MenuItem("Narnia/Build/macOS Player")]
    public static void BuildMac()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneOSX)
            throw new BuildFailedException("Select macOS before building so Addressables and player targets match.");
        ProjectValidation.Run();
        ContentValidation.Run();
        AddressableValidation.Run();
        var paths = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes) if (scene.enabled) paths.Add(scene.path);
        string output = Environment.GetEnvironmentVariable("NARNIA_BUILD_PATH");
        if (string.IsNullOrEmpty(output)) output = "Builds/macOS/IntoTheNarnia.app";
        // BuildWithPlayer is serialized in Addressables settings; this produces the matching local catalog/bundles.
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = paths.ToArray(), locationPathName = output, target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        });
        Write("PlayerBuild.txt", "Result: " + report.summary.result + "\nTotal size: " + report.summary.totalSize + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nDuration: " + report.summary.totalTime + "\n");
        var messages = new List<string>();
        foreach (var step in report.steps)
            foreach (var message in step.messages)
                if (message.type == LogType.Warning || message.type == LogType.Error || message.type == LogType.Exception)
                    messages.Add(message.type + ": " + message.content);
        Write("PlayerBuildMessages.txt", string.Join("\n", messages));
        if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Player build failed: " + report.summary.result);
    }
    public static void UsePackedPlayMode()
    {
        AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex = 2;
        EditorUtility.SetDirty(AddressableAssetSettingsDefaultObject.Settings);
        AssetDatabase.SaveAssets();
    }
    public static void UseAssetDatabasePlayMode()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string previous = settings.ActivePlayModeDataBuilder.GetType().FullName;
        settings.ActivePlayModeDataBuilderIndex = 0;
        Write("PlayModeConfiguration.txt", "Verified using: " + previous + "\nLocal editor default: " + settings.ActivePlayModeDataBuilder.GetType().FullName + "\nPlay mode selection is local Library/AddressablesConfig.dat; it is not committed.\n");
        AssetDatabase.SaveAssets();
    }
    private static void Write(string name, string value)
    {
        string output = Environment.GetEnvironmentVariable("NARNIA_BUILD_REPORT_DIR");
        if (string.IsNullOrEmpty(output)) output = "Docs/Validation/ChalkParity";
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, name), value);
        Debug.Log(value);
    }
}

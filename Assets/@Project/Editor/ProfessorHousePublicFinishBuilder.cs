using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public static class ProfessorHousePublicFinishBuilder
{
    private static readonly string ModelFolder = "Assets/@Project/10.Models/ProfessorHousePublicFinish";
    private static readonly string MaterialFolder = "Assets/@Project/04.Materials/ProfessorHousePublicFinish";
    private static readonly string ProfilePath = "Assets/@Project/Settings/Lighting/ProfessorHouseHallVolume.asset";

    public static void AddFurnishings(GameObject environment)
    {
        ProfessorHouseHallFinishBuilder.AddLayer(environment, ModelFolder, MaterialFolder, "PublicFinish");
        SetSmoothness("PublicRose", .08f);
        SetSmoothness("PublicLinen", .08f);
        SetSmoothness("PublicBookGreen", .1f);
        SetSmoothness("PublicChina", .42f);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/MAT_PublicFire.mat");
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", new Color(2.3f, .8f, .12f));
        EditorUtility.SetDirty(material);
    }

    private static void SetSmoothness(string name, float value)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/MAT_" + name + ".mat");
        material.SetFloat("_Smoothness", value);
        EditorUtility.SetDirty(material);
    }

    public static void ConfigureLighting()
    {
        var root = new GameObject("ProfessorHouse_PublicLighting");
        AddRoom(root.transform, "Reception", new Vector3(5,2,5), new Vector3(9.7f,3.9f,9.7f));
        AddRoom(root.transform, "Foyer", new Vector3(17,2,5), new Vector3(13.7f,3.9f,9.7f));
        AddRoom(root.transform, "Dining", new Vector3(29,2,5), new Vector3(9.7f,3.9f,9.7f));
        AddRoom(root.transform, "PublicLibrary", new Vector3(5,2,28), new Vector3(9.7f,3.9f,11.7f));
        AddLight(root.transform, "ReceptionChandelier", new Vector3(5.2f,2.62f,3.2f), 4.1f, 8, new Color(1,.89f,.73f));
        AddLight(root.transform, "ReceptionFire", new Vector3(8.48f,.85f,2.8f), 1.2f, 3, new Color(1,.58f,.22f));
        AddLight(root.transform, "ReceptionWindow", new Vector3(1.1f,2.2f,5), 2.2f, 6, new Color(1,.95f,.83f));
        AddLight(root.transform, "FoyerChandelier", new Vector3(17,2.62f,4.8f), 4.8f, 10, new Color(1,.9f,.76f));
        AddLight(root.transform, "DiningChandelier", new Vector3(29.5f,2.62f,4.4f), 4.2f, 8, new Color(1,.89f,.73f));
        AddLight(root.transform, "LibraryChandelier", new Vector3(5.2f,2.62f,27.2f), 4.1f, 9, new Color(1,.9f,.76f));
        AddLight(root.transform, "LibraryWindow", new Vector3(3.5f,2.1f,32.2f), 2.5f, 6, new Color(1,.96f,.85f));
    }

    private static void AddRoom(Transform parent, string name, Vector3 center, Vector3 size)
    {
        var obj = new GameObject(name + "_Volume");
        obj.transform.SetParent(parent, false);
        obj.transform.position = center;
        var volume = obj.AddComponent<Volume>();
        volume.isGlobal = false;
        volume.priority = 20;
        volume.blendDistance = .3f;
        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        var bounds = obj.AddComponent<BoxCollider>();
        bounds.isTrigger = true;
        bounds.size = size;
    }

    private static void AddLight(Transform parent, string name, Vector3 position, float intensity, float range, Color color)
    {
        var light = new GameObject(name).AddComponent<Light>();
        light.transform.SetParent(parent, false);
        light.transform.position = position;
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    public static void ValidatePublicAccess()
    {
        var names = new[] {"Reception seating", "Reception fireplace", "Dining west aisle", "Dining east aisle", "Dining north aisle", "Library reading table", "Library window nook", "Library north books", "Library collection gallery door", "Foyer arrival bench", "Foyer console"};
        var starts = new[] {new Vector3(5,0,8.5f), new Vector3(5,0,8.5f), new Vector3(25,0,6), new Vector3(25,0,6), new Vector3(25,0,6), new Vector3(5,0,23), new Vector3(5,0,23), new Vector3(5,0,23), new Vector3(5,0,23), new Vector3(17,0,1), new Vector3(17,0,1)};
        var ends = new[] {new Vector3(4.1f,0,3.2f), new Vector3(7.7f,0,2.8f), new Vector3(26.7f,0,3.8f), new Vector3(32.2f,0,4.4f), new Vector3(29.5f,0,8.8f), new Vector3(3.25f,0,27.2f), new Vector3(3.7f,0,32.25f), new Vector3(6.55f,0,32.5f), new Vector3(11,0,28), new Vector3(11.8f,0,3), new Vector3(21.25f,0,8.2f)};
        var lines = new List<string>();
        var failures = new List<string>();
        int samples = 0;
        Physics.SyncTransforms();
        for (int i = 0; i < names.Length; i++)
        {
            int before = failures.Count;
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(starts[i], out var start, .3f, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(ends[i], out var end, .3f, NavMesh.AllAreas) ||
                !NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path) || path.status != NavMeshPathStatus.PathComplete)
                failures.Add(names[i] + " unreachable");
            else
                for (int j = 1; j < path.corners.Length; j++)
                    CheckSegment(names[i], path.corners[j-1], path.corners[j], ref samples, failures);
            lines.Add((before == failures.Count ? "PASS " : "FAIL ") + names[i]);
        }
        // These story approaches must remain direct, not silently rerouted around furniture.
        var story = new[] {new Vector3(.35f,.04f,7.5f),new Vector3(5,.04f,7.5f),new Vector3(5,.04f,9.5f),new Vector3(13,.04f,6),new Vector3(5.45f,.04f,6),new Vector3(5.45f,.04f,9.3f)};
        int storyBefore = failures.Count;
        CheckSegment("Children garden approach",story[0],story[1],ref samples,failures);
        CheckSegment("Children gallery approach",story[1],story[2],ref samples,failures);
        CheckSegment("Macready reception approach",story[3],story[4],ref samples,failures);
        CheckSegment("Macready gallery approach",story[4],story[5],ref samples,failures);
        lines.Add((storyBefore == failures.Count ? "PASS " : "FAIL ") + "four direct story approach segments");
        lines.Add("Capsule samples: " + samples + "; failures: " + failures.Count);
        lines.AddRange(failures);
        File.WriteAllLines(ProfessorHouseConnectedBuilder.ReportFolder + "/PublicAccess.txt",lines);
        if (failures.Count > 0) throw new InvalidOperationException("Public room access: " + failures[0]);
    }

    private static void CheckSegment(string name, Vector3 start, Vector3 end, ref int samples, List<string> failures)
    {
        int steps = Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(start,end)/.2f));
        for (int i = 0; i <= steps; i++)
        {
            var foot = Vector3.Lerp(start,end,(float)i/steps);
            samples++;
            var overlaps = Physics.OverlapCapsule(foot+Vector3.up*.285f,foot+Vector3.up*.98f,.22f,~0,QueryTriggerInteraction.Ignore);
            for (int j = 0; j < overlaps.Length; j++)
                failures.Add(name + " capsule obstruction at " + foot.ToString("F3") + ": " + overlaps[j].name);
        }
    }
}

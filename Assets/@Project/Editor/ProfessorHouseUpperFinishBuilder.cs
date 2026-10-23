using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public static class ProfessorHouseUpperFinishBuilder
{
    private static readonly string ModelFolder = "Assets/@Project/10.Models/ProfessorHouseUpperFinish";
    private static readonly string MaterialFolder = "Assets/@Project/04.Materials/ProfessorHouseUpperFinish";
    private static readonly string ProfilePath = "Assets/@Project/Settings/Lighting/ProfessorHouseHallVolume.asset";

    public static void AddFurnishings(GameObject environment)
    {
        ProfessorHouseHallFinishBuilder.AddLayer(environment, ModelFolder, MaterialFolder, "UpperFinish");
    }

    public static void ValidateRoomAccess()
    {
        var names = new[] {"Study to reading chair", "Study behind desk", "Library reading area", "Professor bedside", "Girls left bed", "Girls right bed", "Boys left bed", "Boys right bed"};
        var starts = new[] {new Vector3(7,4.32f,18),new Vector3(7,4.32f,18),new Vector3(7,4.32f,25),new Vector3(9,4.32f,9),new Vector3(34.25f,4.32f,29),new Vector3(34.25f,4.32f,29),new Vector3(42.75f,4.32f,29),new Vector3(42.75f,4.32f,29)};
        var ends = new[] {new Vector3(2.3f,4.32f,13.3f),new Vector3(4.8f,4.32f,19.3f),new Vector3(4.7f,4.32f,25.6f),new Vector3(6.5f,4.32f,7.3f),new Vector3(31.875f,4.32f,30.25f),new Vector3(36.575f,4.32f,30.25f),new Vector3(40.375f,4.32f,30.25f),new Vector3(45.075f,4.32f,30.25f)};
        var report = new List<string>();
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
            {
                for (int j = 1; j < path.corners.Length; j++)
                {
                    int steps = Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(path.corners[j-1],path.corners[j])/.2f));
                    for (int n = 0; n <= steps; n++)
                    {
                        var foot = Vector3.Lerp(path.corners[j-1],path.corners[j],(float)n/steps);
                        samples++;
                        if (Physics.CheckCapsule(foot+Vector3.up*.285f,foot+Vector3.up*.98f,.22f,~0,QueryTriggerInteraction.Ignore))
                            failures.Add(names[i] + " capsule obstruction at " + foot.ToString("F3"));
                    }
                }
            }
            report.Add((before == failures.Count ? "PASS " : "FAIL ") + names[i]);
        }
        report.Add("Capsule samples: " + samples + "; failures: " + failures.Count);
        report.AddRange(failures);
        File.WriteAllLines(ProfessorHouseConnectedBuilder.ReportFolder + "/RoomAccess.txt",report);
        if (failures.Count > 0) throw new InvalidOperationException("Upper room access: " + failures[0]);
    }

    public static void ConfigureLighting()
    {
        var root = new GameObject("ProfessorHouse_UpperLighting");
        AddRoom(root.transform, "Study", new Vector3(4,6.5f,16), new Vector3(7.8f,4.5f,11.8f), 7);
        AddRoom(root.transform, "Library", new Vector3(4,6.5f,25), new Vector3(7.8f,4.5f,5.8f), 6);
        AddRoom(root.transform, "ProfessorBedroom", new Vector3(5,6.5f,5), new Vector3(9.8f,4.5f,9.8f), 7);
        AddRoom(root.transform, "GirlsBedroom", new Vector3(34.25f,6.5f,31), new Vector3(8.3f,4.5f,5.8f), 6);
        AddRoom(root.transform, "BoysBedroom", new Vector3(42.75f,6.5f,31), new Vector3(8.3f,4.5f,5.8f), 6);
    }

    private static void AddRoom(Transform parent, string name, Vector3 center, Vector3 size, float intensity)
    {
        var obj = new GameObject(name + "_WarmBounce");
        obj.transform.SetParent(parent, false);
        obj.transform.position = center;
        var light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1,.86f,.66f);
        light.intensity = intensity;
        light.range = Mathf.Min(8, Mathf.Max(size.x,size.z)*.8f);
        light.shadows = LightShadows.None;
        var volume = obj.AddComponent<Volume>();
        volume.isGlobal = false;
        volume.priority = 20;
        volume.blendDistance = .3f;
        volume.sharedProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        var bounds = obj.AddComponent<BoxCollider>();
        bounds.isTrigger = true;
        bounds.size = size;
    }
}

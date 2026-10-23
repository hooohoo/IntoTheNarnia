using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public static class ProfessorHouseGalleryFinishBuilder
{
    private static readonly string ModelFolder = "Assets/@Project/10.Models/ProfessorHouseGalleryFinish";
    private static readonly string MaterialFolder = "Assets/@Project/04.Materials/ProfessorHouseGalleryFinish";
    private static readonly string ProfilePath = "Assets/@Project/Settings/Lighting/ProfessorHouseHallVolume.asset";

    public static void AddFurnishings(GameObject environment)
    {
        ProfessorHouseHallFinishBuilder.AddLayer(environment, ModelFolder, MaterialFolder, "GalleryFinish");
        SetSurface("GallerySteel", .55f, .38f);
        SetSurface("GallerySteelEdge", .6f, .44f);
        SetSurface("GalleryBronze", .62f, .36f);
        var glass = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/MAT_GalleryGlass.mat");
        var color = glass.GetColor("_BaseColor");
        color.a = .065f;
        glass.SetColor("_BaseColor", color);
        glass.SetFloat("_Surface", 1);
        glass.SetFloat("_SrcBlend", (int)BlendMode.SrcAlpha);
        glass.SetFloat("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        glass.SetFloat("_ZWrite", 0);
        glass.SetFloat("_Cull", (int)CullMode.Off);
        glass.SetFloat("_Smoothness", .62f);
        glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        glass.SetOverrideTag("RenderType", "Transparent");
        glass.renderQueue = (int)RenderQueue.Transparent;
        glass.SetShaderPassEnabled("ShadowCaster", false);
        EditorUtility.SetDirty(glass);
        var root = environment.transform.Find("OBJ_ProfessorHouse_GalleryFinish");
        var renderers = root.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i].name == "V_GalleryFinish_CaseGlass") renderers[i].shadowCastingMode = ShadowCastingMode.Off;
        PrefabUtility.ApplyPrefabInstance(root.gameObject, InteractionMode.AutomatedAction);
    }

    private static void SetSurface(string name, float metallic, float smoothness)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/MAT_" + name + ".mat");
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
    }

    public static void ConfigureLighting()
    {
        var root = new GameObject("ProfessorHouse_GalleryLighting");
        AddRoom(root.transform, "ArtifactGallery", new Vector3(5,2.85f,16), new Vector3(9.7f,4.0f,11.7f), 8, 9);
        AddLight(root.transform, "GalleryWindowBounce", new Vector3(1.2f,2.2f,17.5f), new Color(1,.94f,.79f), 3, 5);
        for (int floor = 0; floor < 2; floor++)
        {
            float y = (floor == 0 ? 0 : 4.32f) + 2.8f;
            string prefix = floor == 0 ? "Ground" : "Upper";
            AddRoom(root.transform, prefix + "StairCorridor", new Vector3(27,y,18), new Vector3(5.7f,4,15.7f), 5, 9);
            AddRoom(root.transform, prefix + "FrontGallery", new Vector3(42,y,8), new Vector3(15.7f,4,3.7f), 5, 9);
            AddRoom(root.transform, prefix + "EastCorridor", new Vector3(47,y,18), new Vector3(5.7f,4,15.7f), 5, 9);
        }
        AddRoom(root.transform, "FamilyCorridor", new Vector3(22,7.12f,8), new Vector3(23.7f,4.5f,3.7f), 5, 10);
        AddRoom(root.transform, "ChildrenCorridor", new Vector3(40,7.12f,27), new Vector3(19.7f,4.5f,1.7f), 4, 10);
    }

    private static void AddLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
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

    private static void AddRoom(Transform parent, string name, Vector3 center, Vector3 size, float intensity, float range)
    {
        AddLight(parent, name + "_WarmBounce", center - Vector3.up * .6f, new Color(1,.87f,.7f), intensity * .65f, range);
        var obj = new GameObject(name + "_Volume");
        obj.transform.SetParent(parent, false);
        // Volume is centered within the floor-to-ceiling space, independently of the light.
        obj.transform.position = new Vector3(center.x, center.y - .7f, center.z);
        var volume = obj.AddComponent<Volume>();
        volume.isGlobal = false;
        volume.priority = 20;
        volume.blendDistance = .3f;
        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        var bounds = obj.AddComponent<BoxCollider>();
        bounds.isTrigger = true;
        bounds.size = size;
    }

    public static void ValidateGalleryAccess()
    {
        var names = new[] {"Armor viewing point", "North collections", "East display", "Window cabinet"};
        var ends = new[] {new Vector3(2.5f,0,17), new Vector3(6.8f,0,20), new Vector3(8.15f,0,19), new Vector3(1.5f,0,14)};
        var lines = new List<string>();
        var failures = new List<string>();
        int samples = 0;
        Physics.SyncTransforms();
        for (int i = 0; i < ends.Length; i++)
        {
            int before = failures.Count;
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(new Vector3(5,0,12), out var start, .3f, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(ends[i], out var end, .3f, NavMesh.AllAreas) ||
                !NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path) || path.status != NavMeshPathStatus.PathComplete)
                failures.Add(names[i] + " unreachable");
            else
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
            lines.Add((before == failures.Count ? "PASS " : "FAIL ") + names[i]);
        }
        // The first solid obstacle after the broken pane must still be the armor.
        if (!Physics.Raycast(new Vector3(.23f,2.04f,18.12f),Vector3.right,out var hit,3,~0,QueryTriggerInteraction.Ignore) ||
            !hit.collider.name.StartsWith("Collision_Armor body_",StringComparison.Ordinal))
            failures.Add("Accident axis blocked before armor");
        else lines.Add("PASS accident axis reaches armor collider at " + hit.point.ToString("F3"));
        lines.Add("Capsule samples: " + samples + "; failures: " + failures.Count);
        lines.AddRange(failures);
        File.WriteAllLines(ProfessorHouseConnectedBuilder.ReportFolder + "/GalleryAccess.txt",lines);
        if (failures.Count > 0) throw new InvalidOperationException("Gallery access: " + failures[0]);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ProfessorHouseValidation
{
    [MenuItem("Narnia/Professor House/Validate v2 Hall")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ProfessorHouseBuilder.ScenePath, OpenSceneMode.Single);
        var failures = new List<string>();
        Check(scene, failures);
        ProfessorHouseBuilder.ValidateNavigation();
        string report = "Professor_House v2 failures: " + failures.Count + "\n" + string.Join("\n", failures);
        Directory.CreateDirectory("Docs/Validation/ProfessorHouseV2");
        File.WriteAllText("Docs/Validation/ProfessorHouseV2/SceneValidation.txt", report);
        var dependencies = AssetDatabase.GetDependencies(ProfessorHouseBuilder.ScenePath, true);
        Array.Sort(dependencies, StringComparer.Ordinal);
        File.WriteAllLines("Docs/Validation/ProfessorHouseV2/SceneDependencies.txt", dependencies);
        int renderers = 0;
        long triangles = 0;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Environment_ProfessorHouse_v2")
            {
                renderers = root.GetComponentsInChildren<MeshRenderer>().Length;
                foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
                    for (int i = 0; i < filter.sharedMesh.subMeshCount; i++)
                        triangles += filter.sharedMesh.GetIndexCount(i) / 3;
            }
        File.WriteAllText("Docs/Validation/ProfessorHouseV2/Geometry.txt", "Environment renderers: " + renderers + "\nEnvironment triangles (all submeshes): " + triangles + "\nLightmaps: " + LightmapSettings.lightmaps.Length + "\n");
        if (failures.Count != 0)
            throw new InvalidOperationException(report);
        Debug.Log(report);
    }

    public static void Check(Scene scene, List<string> failures)
    {
        GameObject environment = null;
        int doors = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Environment_ProfessorHouse_v2")
                environment = root;
            foreach (var door in root.GetComponentsInChildren<DoorController>())
            {
                doors++;
                if (door.transform.Find("Leaf") == null)
                    failures.Add("Door leaf missing: " + door.name);
                var animator = door.GetComponent<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null)
                    failures.Add("Door animator missing: " + door.name);
            }
        }
        if (environment == null)
        {
            failures.Add("Professor House v2 environment missing.");
            return;
        }
        if (doors != 2)
            failures.Add("Expected the two quest doors.");
        var renderers = environment.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length > 64)
            failures.Add("Hall exceeded the 64 renderer authoring budget.");
        foreach (var renderer in renderers)
        {
            var filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                failures.Add("Missing mesh: " + renderer.name);
                continue;
            }
            if (!filter.sharedMesh.HasVertexAttribute(VertexAttribute.TexCoord1))
                failures.Add("Missing lightmap UVs: " + renderer.name);
            foreach (var material in renderer.sharedMaterials)
                if (material == null || material.shader == null || material.shader.name != "Universal Render Pipeline/Lit")
                    failures.Add("Invalid URP material: " + renderer.name);
            int index = renderer.lightmapIndex;
            if (renderer.name == "Sconces")
            {
                if (renderer.receiveGI != ReceiveGI.LightProbes)
                    failures.Add("Small candle fittings must use light probes.");
            }
            else if (index < 0 || index >= LightmapSettings.lightmaps.Length)
                failures.Add("Missing baked lightmap: " + renderer.name);
        }
        if (Lightmapping.lightingDataAsset == null || LightmapSettings.lightmaps.Length == 0)
            failures.Add("Lighting data missing.");
        foreach (var lightmap in LightmapSettings.lightmaps)
            if (lightmap.lightmapColor == null)
                failures.Add("Broken lightmap color reference.");
        var surface = environment.GetComponent<NavMeshSurface>();
        if (surface == null || surface.navMeshData == null)
            failures.Add("Navigation asset missing.");
        // Reject mirrored imports: visual geometry and collision coordinates must agree.
        var architecture = environment.transform.Find("Architecture");
        var stairs = architecture == null ? null : architecture.Find("Stair_Body");
        if (stairs == null || stairs.GetComponent<Renderer>().bounds.center.x < 1)
            failures.Add("Stair geometry is not aligned with the navigation ramp.");
        var ceiling = architecture == null ? null : architecture.Find("Ceiling");
        var floor = architecture == null ? null : architecture.Find("Ground_Flagstones_0_0");
        if (ceiling == null || ceiling.GetComponent<Renderer>().bounds.center.y < 6)
            failures.Add("Ceiling is not above the gallery.");
        if (floor == null || Mathf.Abs(floor.GetComponent<Renderer>().bounds.max.y - .025f) > .05f)
            failures.Add("Floor vertical axis or import scale changed.");
        foreach (var transform in environment.GetComponentsInChildren<Transform>())
            if (transform.lossyScale.x <= 0 || transform.lossyScale.y <= 0 || transform.lossyScale.z <= 0)
                failures.Add("Mirrored scale: " + transform.name);
    }
}

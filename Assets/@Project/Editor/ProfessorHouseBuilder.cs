using System;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Reproducible authoring tool. Runtime ownership remains with FieldManager / SceneRoot.
public static class ProfessorHouseBuilder
{
    public static readonly string ScenePath = "Assets/@Project/00.Scenes/Professor_House.unity";
    public static readonly string ModelFolder = "Assets/@Project/10.Models/ProfessorHouse";
    public static readonly string MaterialFolder = "Assets/@Project/04.Materials/ProfessorHouse";
    public static readonly string PrefabFolder = "Assets/@Project/02.Prefabs/Environment_Prefab/ProfessorHouse";
    public static readonly string DataFolder = "Assets/@Project/00.Scenes/Professor_House/V2";
    private static readonly string AnimationFolder = "Assets/@Project/05.Animations/ProfessorHouse";
    private static readonly string PreviewFolder = "Docs/Validation/ProfessorHouseV2";
    private static readonly Vector3 Spawn = new Vector3(-.9f, .15f, -2.9f);
    private static readonly string[] PaletteNames = { "Walnut", "WalnutEdge", "WalnutPanel", "Plaster", "Limestone", "StoneLight", "StoneWarm", "Mortar", "RugRed", "RugBorder", "RugInset", "Olive", "Brass", "Ivory", "Ink", "Skin", "BookBlue", "BookRed", "Leaves", "Glow", "Sky", "DistantGreen" };
    private static readonly string[] PaletteColors = { "35231B", "463024", "402C21", "D5C19D", "BAAB90", "CCBDA3", "B7A187", "A99B84", "984E3C", "C29456", "B06A4D", "68705A", "AF8951", "E2D8BD", "303C3D", "C69270", "526D70", "793B34", "56684B", "FFD389", "B5CFCD", "80947A" };
    private static readonly float[] Smoothness = { .28f, .32f, .24f, .08f, .12f, .14f, .13f, .08f, .08f, .10f, .08f, .13f, .42f, .18f, .10f, .16f, .12f, .12f, .10f, 0, 0, 0 };

    [MenuItem("Narnia/Professor House/Rebuild v2 Hall")]
    public static void Build()
    {
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory(PrefabFolder);
        Directory.CreateDirectory(DataFolder);
        Directory.CreateDirectory(AnimationFolder);
        AssetDatabase.Refresh();
        CreateMaterialsAndImport();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var map = InScene<ProfessorHouseMapManager>(scene);
        var field = InScene<FieldManager>(scene);
        if (map == null || field == null)
            throw new InvalidOperationException("Existing scene contracts are missing.");
        // Scene GUID, root services, camera bindings and portal components are retained.
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Environment" || root.name == "Environment_ProfessorHouse_v2" || root.name == "Lighting" || root.name == "HouseInteractions" || root.name == "HousePreviewCameras" || root.name == "Lucy")
                UnityEngine.Object.DestroyImmediate(root);
        UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
        StaticOcclusionCulling.Clear();
        Lightmapping.Clear();
        LightmapSettings.lightmaps = Array.Empty<LightmapData>();
        var environment = new GameObject("Environment_ProfessorHouse_v2");
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/FBX_ProfessorHouse_Hall_v2.fbx"));
        visual.name = "Architecture";
        visual.transform.SetParent(environment.transform, false);
        foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>())
        {
            GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            renderer.receiveGI = ReceiveGI.Lightmaps;
            if (renderer.name == "Sconces")
            {
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic);
                renderer.receiveGI = ReceiveGI.LightProbes;
            }
            if (renderer.name == "Garden_View")
                renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
        CreateCollisions(environment.transform);
        var surface = environment.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.overrideVoxelSize = true;
        surface.voxelSize = .07f;
        surface.BuildNavMesh();
        string navPath = DataFolder + "/Hall_NavMesh.asset";
        SaveNavMesh(surface, navPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(environment, PrefabFolder + "/OBJ_ProfessorHouse_Hall_v2.prefab", InteractionMode.AutomatedAction);
        var interactions = new GameObject("HouseInteractions");
        var controller = CreateDoorAnimator();
        CreateDoor(interactions.transform, "TheSiblingsRoomDoor", -3.3f, controller);
        CreateDoor(interactions.transform, "TheWardrobeRoomDoor", 0, controller);
        using (var serialized = new SerializedObject(map))
        {
            SetPortal((NextMapController)serialized.FindProperty("siblingsPortal").objectReferenceValue, -3.3f);
            SetPortal((NextMapController)serialized.FindProperty("wardrobePortal").objectReferenceValue, 0);
        }
        field.transform.position = Spawn;
        using (var serialized = new SerializedObject(field))
        {
            var ground = (GameObject)serialized.FindProperty("miniMapGround").objectReferenceValue;
            if (ground != null)
            {
                ground.name = "MiniMapBounds";
                ground.transform.position = new Vector3(0, -4, 1);
                ground.transform.localScale = new Vector3(1.2f, 1, 1.6f);
                foreach (var renderer in ground.GetComponents<Renderer>())
                    UnityEngine.Object.DestroyImmediate(renderer);
                foreach (var filter in ground.GetComponents<MeshFilter>())
                    UnityEngine.Object.DestroyImmediate(filter);
                foreach (var collider in ground.GetComponents<Collider>())
                    UnityEngine.Object.DestroyImmediate(collider);
                var bounds = ground.AddComponent<BoxCollider>();
                bounds.size = new Vector3(10, .1f, 10);
                bounds.isTrigger = true;
            }
        }
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Test_QuestStartBox")
            {
                root.transform.position = Spawn + new Vector3(0, .2f, 1);
                foreach (var renderer in root.GetComponents<Renderer>())
                    renderer.enabled = false;
            }
        ConfigureCamera(scene);
        CreateLighting();
        CreatePreviewCameras();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        ValidateNavigation();
        if (!Application.isBatchMode)
            FrameHall();
        Debug.Log("HOUSE_V2_BUILD_COMPLETE");
    }

    private static void CreateMaterialsAndImport()
    {
        string whitePath = MaterialFolder + "/TEX_House_BaseWhite.asset";
        var white = AssetDatabase.LoadAssetAtPath<Texture2D>(whitePath);
        if (white == null)
        {
            white = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            white.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            white.Apply();
            AssetDatabase.CreateAsset(white, whitePath);
        }
        white.name = "TEX_House_BaseWhite";
        var materials = new Dictionary<string, Material>();
        for (int i = 0; i < PaletteNames.Length; i++)
        {
            string path = MaterialFolder + "/MAT_House_" + PaletteNames[i] + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.name = Path.GetFileNameWithoutExtension(path);
            ColorUtility.TryParseHtmlString("#" + PaletteColors[i], out var color);
            material.SetColor("_BaseColor", color);
            material.SetTexture("_BaseMap", white);
            material.SetFloat("_Smoothness", Smoothness[i]);
            material.SetFloat("_Metallic", PaletteNames[i] == "Brass" ? .55f : 0);
            if (PaletteNames[i] == "Glow")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            materials.Add(PaletteNames[i], material);
            EditorUtility.SetDirty(material);
        }
        foreach (string name in new[] { "FBX_ProfessorHouse_Hall_v2", "FBX_ProfessorHouse_Door_v2" })
        {
            string path = ModelFolder + "/" + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
                throw new FileNotFoundException("Run the Blender generator first.", path);
            importer.importCameras = false;
            importer.importLights = false;
            importer.importAnimation = false;
            importer.generateSecondaryUV = true;
            importer.secondaryUVMarginMethod = ModelImporterSecondaryUVMarginMethod.Calculate;
            importer.secondaryUVMinLightmapResolution = 20;
            importer.secondaryUVMinObjectScale = 1;
            importer.secondaryUVHardAngle = 88;
            importer.isReadable = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            foreach (var pair in materials)
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key), pair.Value);
            importer.SaveAndReimport();
        }
    }

    private static BoxCollider Box(Transform parent, string name, Vector3 center, Vector3 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = center;
        var collider = obj.AddComponent<BoxCollider>();
        collider.size = size;
        return collider;
    }
    private static void CreateCollisions(Transform parent)
    {
        var root = new GameObject("Collisions").transform;
        root.SetParent(parent, false);
        Box(root, "Ground", new Vector3(0, -.15f, 0), new Vector3(12, .30f, 14));
        Box(root, "GalleryBack", new Vector3(0, 3.45f, 5.55f), new Vector3(12, .40f, 2.9f));
        Box(root, "GalleryLeft", new Vector3(-4.75f, 3.45f, -1.45f), new Vector3(2.5f, .40f, 11.1f));
        Box(root, "LeftWall", new Vector3(-6.1f, 3.8f, 0), new Vector3(.3f, 7.6f, 14.4f));
        Box(root, "RightWall", new Vector3(6.1f, 3.8f, 0), new Vector3(.3f, 7.6f, 14.4f));
        Box(root, "FrontWall", new Vector3(0, 3.8f, -7.1f), new Vector3(12.4f, 7.6f, .3f));
        Box(root, "BackLowerWall", new Vector3(0, 1.8f, 7.1f), new Vector3(12.4f, 3.6f, .3f));
        foreach (var span in new[] { new Vector2(-5.15f, 1.7f), new Vector2(-1.65f, 1.5f), new Vector2(3.45f, 5.1f) })
            Box(root, "BackUpperWall", new Vector3(span.x, 5.65f, 7.1f), new Vector3(span.y, 4.1f, .3f));
        Box(root, "Ceiling", new Vector3(0, 7.75f, 0), new Vector3(12.4f, .2f, 14.4f));
        // Continuous slope avoids small navigation islands on individual stair treads.
        var ramp = new Mesh { name = "StairCollision" };
        ramp.vertices = new[] { new Vector3(1.55f, 0, -2.55f), new Vector3(4.25f, 0, -2.55f), new Vector3(1.55f, 3.65f, 4.1f), new Vector3(4.25f, 3.65f, 4.1f), new Vector3(1.55f, 0, 4.1f), new Vector3(4.25f, 0, 4.1f) };
        ramp.triangles = new[] { 0, 2, 1, 1, 2, 3, 0, 4, 2, 1, 3, 5, 2, 4, 3, 3, 4, 5, 0, 1, 4, 1, 5, 4 };
        ramp.RecalculateNormals();
        ramp.RecalculateBounds();
        string rampPath = DataFolder + "/StairCollision.asset";
        var saved = AssetDatabase.LoadAssetAtPath<Mesh>(rampPath);
        if (saved == null)
        {
            AssetDatabase.CreateAsset(ramp, rampPath);
            saved = ramp;
        }
        else
        {
            EditorUtility.CopySerialized(ramp, saved);
            UnityEngine.Object.DestroyImmediate(ramp);
        }
        var slope = new GameObject("StairRamp");
        slope.transform.SetParent(root, false);
        slope.AddComponent<MeshCollider>().sharedMesh = saved;
        // Invisible guards follow the handrail, rather than enclosing the whole staircase.
        foreach (float x in new[] { 1.51f, 4.29f })
        {
            var guard = Box(root, "StairGuard", new Vector3(x, 2.30f, .78f), new Vector3(.25f, 1.1f, 7.65f));
            guard.transform.localRotation = Quaternion.Euler(-28.76f, 0, 0);
        }
        Box(root, "LeftGalleryGuard", new Vector3(-3.55f, 4.25f, -1.3f), new Vector3(.24f, 1.2f, 10.95f));
        Box(root, "BackGalleryGuard", new Vector3(-1.02f, 4.25f, 4.06f), new Vector3(5.07f, 1.2f, .24f));
        Box(root, "BackRightGuard", new Vector3(5.0f, 4.25f, 4.06f), new Vector3(1.45f, 1.2f, .24f));
        foreach (float x in new[] { -3.3f, 0 })
        {
            Box(root, "VestibuleFloor", new Vector3(x, 3.50f, 8), new Vector3(1.82f, .30f, 2.1f));
            foreach (float side in new[] { -1f, 1f })
                Box(root, "VestibuleSide", new Vector3(x + side * 1.04f, 5.1f, 8), new Vector3(.26f, 3, 2.2f));
            Box(root, "VestibuleEnd", new Vector3(x, 5.1f, 9.05f), new Vector3(2.3f, 3, .22f));
            var gate = new GameObject(x < 0 ? "TheSiblingsRoomDoorArea" : "TheWardrobeRoomDoorArea");
            gate.transform.SetParent(root, false);
            gate.transform.localPosition = new Vector3(x, 3.9f, 7.07f);
            var modifier = gate.AddComponent<NavMeshModifierVolume>();
            modifier.center = Vector3.zero;
            modifier.size = new Vector3(2.1f, 1.3f, .72f);
            modifier.area = NavMesh.GetAreaFromName(x < 0 ? "TheSiblingsRoomDoor" : "TheWardrobeRoomDoor");
        }
        Box(root, "Console", new Vector3(-4.85f, .6f, -2.65f), new Vector3(1.35f, 1.2f, 1.9f));
        Box(root, "Chair", new Vector3(.25f, .8f, 5.82f), new Vector3(1.0f, 1.6f, 1.0f));
        Box(root, "Bust", new Vector3(-1.8f, 1.2f, 5.95f), new Vector3(.75f, 2.4f, .75f));
    }
    private static void SaveNavMesh(NavMeshSurface surface, string path)
    {
        var generated = surface.navMeshData;
        if (generated == null)
            throw new InvalidOperationException("Navigation generation failed.");
        generated.name = Path.GetFileNameWithoutExtension(path);
        var existing = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        surface.RemoveData();
        if (existing == null)
            AssetDatabase.CreateAsset(generated, path);
        else
        {
            EditorUtility.CopySerialized(generated, existing);
            surface.navMeshData = existing;
            UnityEngine.Object.DestroyImmediate(generated);
        }
        surface.AddData();
    }
    private static RuntimeAnimatorController CreateDoorAnimator()
    {
        string controllerPath = AnimationFolder + "/HouseDoor.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (existing != null)
            return existing;
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("State", AnimatorControllerParameterType.Int);
        var machine = controller.layers[0].stateMachine;
        AnimatorState closed = null, open = null;
        foreach (bool isOpen in new[] { false, true })
        {
            string name = isOpen ? "Open" : "Closed";
            var clip = new AnimationClip { name = "HouseDoor@" + name, frameRate = 30 };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Leaf", typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0, 1, isOpen ? -105 : 0));
            AssetDatabase.CreateAsset(clip, AnimationFolder + "/" + clip.name + ".anim");
            var state = machine.AddState(name);
            state.motion = clip;
            if (isOpen)
                open = state;
            else
                closed = state;
        }
        machine.defaultState = closed;
        var opening = closed.AddTransition(open);
        opening.hasExitTime = false;
        opening.duration = .32f;
        opening.AddCondition(AnimatorConditionMode.Equals, 1, "State");
        var closing = open.AddTransition(closed);
        closing.hasExitTime = false;
        closing.duration = .32f;
        closing.AddCondition(AnimatorConditionMode.Equals, 0, "State");
        return controller;
    }
    private static void CreateDoor(Transform parent, string name, float x, RuntimeAnimatorController controller)
    {
        var door = new GameObject(name);
        door.transform.SetParent(parent, false);
        door.transform.localPosition = new Vector3(x - .86f, 3.65f, 6.91f);
        var leaf = new GameObject("Leaf");
        leaf.transform.SetParent(door.transform, false);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/FBX_ProfessorHouse_Door_v2.fbx"));
        model.transform.SetParent(leaf.transform, false);
        var solid = leaf.AddComponent<BoxCollider>();
        solid.center = new Vector3(.86f, 1.27f, 0);
        solid.size = new Vector3(1.72f, 2.54f, .16f);
        var trigger = door.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(.86f, 1.30f, -.30f);
        trigger.size = new Vector3(2.45f, 2.8f, 2.0f);
        var body = door.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        var animator = door.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        var interaction = door.AddComponent<DoorController>();
        using (var serialized = new SerializedObject(interaction))
        {
            serialized.FindProperty("animator").objectReferenceValue = animator;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        PrefabUtility.SaveAsPrefabAssetAndConnect(door, PrefabFolder + "/OBJ_" + name + ".prefab", InteractionMode.AutomatedAction);
    }
    private static void SetPortal(NextMapController portal, float x)
    {
        if (portal == null)
            throw new InvalidOperationException("Missing map portal.");
        portal.transform.position = new Vector3(x, 4.95f, 8.3f);
        portal.transform.rotation = Quaternion.identity;
        portal.transform.localScale = Vector3.one;
        var collider = portal.GetComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = new Vector3(1.65f, 2.6f, .38f);
        collider.isTrigger = true;
    }
    private static void ConfigureCamera(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                if (camera.CompareTag("MainCamera"))
                {
                    camera.transform.position = Spawn + new Vector3(0, 2.8f, -3.2f);
                    camera.transform.LookAt(Spawn + Vector3.up);
                    camera.nearClipPlane = .1f;
                }
        var rig = InScene<CinemachineFreeLook>(scene);
        if (rig == null)
            return;
        rig.m_Lens.FieldOfView = 62;
        rig.m_Orbits = new[] { new CinemachineFreeLook.Orbit(4.5f, 3.2f), new CinemachineFreeLook.Orbit(2.6f, 3.3f), new CinemachineFreeLook.Orbit(.7f, 2.1f) };
        rig.m_YAxis.Value = .55f;
        rig.m_XAxis.Value = 0;
        for (int i = 0; i < 3; i++)
        {
            var composer = rig.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
                composer.m_TrackedObjectOffset = new Vector3(0, 1.25f, 0);
        }
    }
    private static Light Light(Transform parent, string name, LightType type, Vector3 position, Color color, float intensity, float range)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        var light = obj.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
        light.shadowBias = .025f;
        light.shadowNormalBias = .2f;
        return light;
    }
    private static void CreateLighting()
    {
        var root = new GameObject("Lighting").transform;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.62f, .67f, .68f);
        RenderSettings.ambientEquatorColor = new Color(.44f, .39f, .32f);
        RenderSettings.ambientGroundColor = new Color(.29f, .27f, .23f);
        RenderSettings.ambientIntensity = 1;
        RenderSettings.fog = false;
        var sun = Light(root, "Afternoon window light", LightType.Directional, Vector3.zero, new Color(1, .88f, .68f), 1.7f, 50);
        sun.transform.rotation = Quaternion.LookRotation(new Vector3(-1, -.72f, .28f));
        sun.lightmapBakeType = LightmapBakeType.Mixed;
        sun.bounceIntensity = 1.3f;
        RenderSettings.sun = sun;
        foreach (float z in new[] { -4.3f, -.6f, 3.1f })
        {
            var window = Light(root, "Window bounce", LightType.Area, new Vector3(5.70f, 4.2f, z), new Color(.88f, .94f, 1), 6f, 14);
            window.transform.rotation = Quaternion.LookRotation(Vector3.left);
            window.areaSize = new Vector2(2.1f, 4.1f);
            window.lightmapBakeType = LightmapBakeType.Baked;
        }
        var ceilingFill = Light(root, "Soft ceiling bounce", LightType.Area, new Vector3(0, 7.1f, 0), new Color(1, .92f, .80f), 4.5f, 18);
        ceilingFill.transform.rotation = Quaternion.LookRotation(Vector3.down);
        ceilingFill.areaSize = new Vector2(10, 12);
        ceilingFill.lightmapBakeType = LightmapBakeType.Baked;
        var galleryFill = Light(root, "Under gallery bounce", LightType.Area, new Vector3(0, 3.1f, 5.5f), new Color(1, .91f, .79f), 3.5f, 12);
        galleryFill.transform.rotation = Quaternion.LookRotation(Vector3.down);
        galleryFill.areaSize = new Vector2(10, 2.2f);
        galleryFill.lightmapBakeType = LightmapBakeType.Baked;
        foreach (var p in new[] { new Vector3(-4.8f, 2.6f, 6.1f), new Vector3(1.25f, 2.6f, 6.1f), new Vector3(-1.65f, 5.8f, 6.1f) })
        {
            var candle = Light(root, "Warm sconce bounce", LightType.Point, p, new Color(1, .63f, .30f), 1.3f, 4.5f);
            candle.lightmapBakeType = LightmapBakeType.Baked;
        }
        var probes = new GameObject("Character light probes");
        probes.transform.SetParent(root, false);
        var positions = new List<Vector3>();
        foreach (float y in new[] { .7f, 2.1f, 4.3f, 5.7f })
            for (float x = -5; x <= 5; x += 2.5f)
                for (float z = -5.5f; z <= 6; z += 2.75f)
                    positions.Add(new Vector3(x, y, z));
        probes.AddComponent<LightProbeGroup>().probePositions = positions.ToArray();
        var reflection = new GameObject("Hall reflection");
        reflection.transform.SetParent(root, false);
        reflection.transform.position = new Vector3(0, 3, 0);
        var probe = reflection.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Baked;
        probe.size = new Vector3(12, 7.5f, 14);
        probe.center = new Vector3(0, .7f, 0);
        probe.resolution = 128;
        probe.boxProjection = true;
        probe.intensity = .6f;
        string profilePath = DataFolder + "/Hall_Grade.asset";
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            AssetDatabase.AddObjectToAsset(tone, profile);
            var grade = profile.Add<ColorAdjustments>(true);
            grade.postExposure.Override(.3f);
            grade.contrast.Override(6);
            grade.saturation.Override(-4);
            AssetDatabase.AddObjectToAsset(grade, profile);
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(.12f);
            bloom.threshold.Override(1.1f);
            AssetDatabase.AddObjectToAsset(bloom, profile);
        }
        var volume = root.gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;
        string settingsPath = DataFolder + "/Hall_Lighting.lighting";
        var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(settingsPath);
        if (settings == null)
        {
            settings = new LightingSettings();
            AssetDatabase.CreateAsset(settings, settingsPath);
        }
        settings.bakedGI = true;
        settings.realtimeGI = false;
        settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
        LightmapSettings.lightmapsMode = LightmapsMode.CombinedDirectional;
        settings.lightmapResolution = 20;
        settings.lightmapMaxSize = 1024;
        settings.directSampleCount = 32;
        settings.indirectSampleCount = 128;
        settings.environmentSampleCount = 64;
        settings.maxBounces = 3;
        settings.ao = true;
        settings.aoMaxDistance = 1.2f;
        settings.aoExponentIndirect = 1.1f;
        settings.mixedBakeMode = MixedLightingMode.Shadowmask;
        Lightmapping.lightingSettings = settings;
    }
    private static void CreatePreviewCameras()
    {
        var root = new GameObject("HousePreviewCameras").transform;
        PreviewCamera(root, "Hall", new Vector3(-2.1f, 5.8f, -6.6f), new Vector3(.5f, 1.9f, 3.15f), 62);
        PreviewCamera(root, "Stair", new Vector3(-1.5f, 2.9f, -4.6f), new Vector3(1.2f, 2.7f, 3.8f), 68);
        PreviewCamera(root, "Gallery", new Vector3(-4.8f, 5.6f, 1.7f), new Vector3(.9f, 4.7f, 5.8f), 72);
    }
    private static void PreviewCamera(Transform parent, string name, Vector3 position, Vector3 target, float fov)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.position = position;
        obj.transform.LookAt(target);
        var camera = obj.AddComponent<Camera>();
        camera.enabled = false;
        camera.fieldOfView = fov;
        camera.nearClipPlane = .05f;
        camera.farClipPlane = 80;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.62f, .69f, .69f);
        camera.useOcclusionCulling = false;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
    }
    [MenuItem("Narnia/Professor House/Capture v2 Previews")]
    public static void CapturePreviews()
    {
        Directory.CreateDirectory(PreviewFolder);
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "HousePreviewCameras")
                foreach (var camera in root.GetComponentsInChildren<Camera>())
                {
                    var rt = new RenderTexture(1600, 1100, 24, RenderTextureFormat.ARGB32);
                    rt.antiAliasing = 4;
                    var request = new UniversalRenderPipeline.SingleCameraRequest { destination = rt };
                    RenderPipeline.SubmitRenderRequest(camera, request);
                    var previous = RenderTexture.active;
                    RenderTexture.active = rt;
                    var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                    texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    texture.Apply();
                    bool varied = false;
                    var first = texture.GetPixel(0, 0);
                    for (int y = 0; y < texture.height && !varied; y += 64)
                        for (int x = 0; x < texture.width; x += 64)
                            if (texture.GetPixel(x, y) != first)
                            {
                                varied = true;
                                break;
                            }
                    byte[] pixels = varied ? texture.EncodeToPNG() : null;
                    RenderTexture.active = previous;
                    camera.targetTexture = null;
                    rt.Release();
                    UnityEngine.Object.DestroyImmediate(rt);
                    UnityEngine.Object.DestroyImmediate(texture);
                    if (!varied)
                        throw new InvalidOperationException("Empty URP capture. Use a graphics-enabled editor without -batchmode.");
                    File.WriteAllBytes(PreviewFolder + "/" + camera.name + ".png", pixels);
                }
        Debug.Log("HOUSE_V2_PREVIEWS_COMPLETE");
    }
    // Without -quit: let URP initialise before submitting editor render requests.
    public static void PreviewBatch()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RunWhenReady(CapturePreviews);
    }
    [MenuItem("Narnia/Professor House/Frame Hall in Scene View")]
    public static void FrameHall()
    {
        if (SceneManager.GetActiveScene().path != ScenePath)
            return;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == "HousePreviewCameras")
            {
                var camera = root.transform.Find("Hall").GetComponent<Camera>();
                var view = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
                view.in2DMode = false;
                view.sceneLighting = true;
                view.LookAt(camera.transform.position + camera.transform.forward * 6, camera.transform.rotation, 3, false, true);
                view.Repaint();
            }
    }
    public static void InspectOrientation()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var scene = SceneManager.GetActiveScene();
        var lines = new List<string>();
        var view = SceneView.lastActiveSceneView;
        if (view != null)
            lines.Add("Previous Scene View pivot=" + view.pivot + " up=" + (view.rotation * Vector3.up) + " rotation=" + view.rotation.eulerAngles);
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
                if (renderer.name.StartsWith("Ground_Flagstones") || renderer.name == "Ceiling" || renderer.name == "Stair_Body")
                    lines.Add(renderer.name + ": center=" + renderer.bounds.center + " min=" + renderer.bounds.min + " max=" + renderer.bounds.max + " scale=" + renderer.transform.lossyScale);
            foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                lines.Add("Camera " + camera.name + ": enabled=" + camera.enabled + " active=" + camera.gameObject.activeInHierarchy + " position=" + camera.transform.position + " up=" + camera.transform.up + " rotation=" + camera.transform.eulerAngles);
        }
        File.WriteAllLines(PreviewFolder + "/Orientation.txt", lines);
        FrameHall();
        RunWhenReady(() => { CapturePreviews(); Debug.Log("HOUSE_ORIENTATION_CHECK_COMPLETE"); });
    }
    public static void BuildAndBakeBatch()
    {
        RunWhenReady(() => { Build(); Bake(); ProfessorHouseValidation.Run(); NarniaBuild.UsePackedPlayMode(); });
    }
    private static void RunWhenReady(Action work)
    {
        int frames = 0;
        EditorApplication.CallbackFunction capture = null;
        capture = () =>
        {
            if (++frames < 30)
                return;
            EditorApplication.update -= capture;
            try
            {
                work();
                EditorApplication.Exit(0);
            }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        };
        EditorApplication.update += capture;
    }
    public static void Bake()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!Lightmapping.Bake())
            throw new InvalidOperationException("Lightmap bake failed.");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        CapturePreviews();
        ValidateNavigation();
        Debug.Log("HOUSE_V2_BAKE_COMPLETE");
    }
    public static void ValidateNavigation()
    {
        var lines = new List<string>();
        CheckPath(Spawn, new Vector3(2.9f, 3.7f, 4.8f), 7, true, "Ground to gallery", lines);
        foreach (float x in new[] { -3.3f, 0 })
        {
            string name = x < 0 ? "TheSiblingsRoomDoor" : "TheWardrobeRoomDoor";
            var front = new Vector3(x, 3.7f, 6);
            var beyond = new Vector3(x, 3.7f, 8.3f);
            CheckPath(Spawn, front, 7, true, name + " approach", lines);
            CheckPath(front, beyond, 7, false, name + " locked gate", lines);
            CheckPath(front, beyond, 7 | (1 << NavMesh.GetAreaFromName(name)), true, name + " open gate", lines);
        }
        Directory.CreateDirectory(PreviewFolder);
        File.WriteAllText(PreviewFolder + "/Navigation.txt", string.Join("\n", lines) + "\n");
    }
    private static void CheckPath(Vector3 start, Vector3 end, int mask, bool expected, string label, List<string> lines)
    {
        if (!NavMesh.SamplePosition(start, out var a, 1, NavMesh.AllAreas) || !NavMesh.SamplePosition(end, out var b, 1, NavMesh.AllAreas))
            throw new InvalidOperationException(label + ": endpoint outside NavMesh");
        var path = new NavMeshPath();
        bool complete = NavMesh.CalculatePath(a.position, b.position, mask, path) && path.status == NavMeshPathStatus.PathComplete;
        if (complete != expected)
            throw new InvalidOperationException(label + ": unexpected path " + path.status);
        lines.Add("PASS " + label + " (" + path.status + ")");
    }
    private static T InScene<T>(Scene scene) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var value = root.GetComponentInChildren<T>(true);
            if (value != null)
                return value;
        }
        return null;
    }
}

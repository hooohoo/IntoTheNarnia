using System;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ProfessorHouseConnectedBuilder
{
    public static readonly string ModelFolder = "Assets/@Project/10.Models/ProfessorHouseConnected";
    public static readonly string MaterialFolder = "Assets/@Project/04.Materials/ProfessorHouseConnected";
    public static readonly string PrefabFolder = "Assets/@Project/02.Prefabs/Environment_Prefab/ProfessorHouseConnected";
    public static readonly string DataFolder = "Assets/@Project/00.Scenes/Professor_House_Connected";
    public static readonly string ReportFolder = "Docs/Validation/ProfessorHousePublicFinish";
    private static readonly string LogTag = "[ProfessorHouseConnectedBuilder]";

    [Serializable] public sealed class ModelManifest
    {
        [SerializeField] private string version;
        [SerializeField] private string sourceLayoutSha256;
        [SerializeField] private string[] chunks;
        [SerializeField] private MeshEntry[] meshes;
        [SerializeField] private PaletteEntry[] palette;
        [SerializeField] private ProfessorHouseWalkthroughRoot.TraversalRoute[] routes;
        [SerializeField] private DoorEntry[] doors;
        [SerializeField] private Vector3 spawn;
        [SerializeField] private int roomCount;
        [SerializeField] private int doorCount;

        public string Version => version;
        public string SourceLayoutSha256 => sourceLayoutSha256;
        public string[] Chunks => chunks;
        public MeshEntry[] Meshes => meshes;
        public PaletteEntry[] Palette => palette;
        public ProfessorHouseWalkthroughRoot.TraversalRoute[] Routes => routes;
        public DoorEntry[] Doors => doors;
        public Vector3 Spawn => spawn;
        public int RoomCount => roomCount;
        public int DoorCount => doorCount;
    }
    [Serializable] public sealed class MeshEntry
    {
        [SerializeField] private string name;
        [SerializeField] private string chunk;
        [SerializeField] private bool collision;
        [SerializeField] private bool glass;
        [SerializeField] private Vector3 boundsMin;
        [SerializeField] private Vector3 boundsMax;
        [SerializeField] private int vertices;
        [SerializeField] private int triangles;

        public string Name => name;
        public string Chunk => chunk;
        public bool Collision => collision;
        public bool Glass => glass;
        public Vector3 BoundsMin => boundsMin;
        public Vector3 BoundsMax => boundsMax;
        public int Vertices => vertices;
        public int Triangles => triangles;
    }
    [Serializable] public sealed class PaletteEntry
    {
        [SerializeField] private string name;
        [SerializeField] private string hex;

        public string Name => name;
        public string Hex => hex;
    }
    [Serializable] public sealed class DoorEntry
    {
        [SerializeField] private string floor;
        [SerializeField] private string a;
        [SerializeField] private string b;
        [SerializeField] private string axis;
        [SerializeField] private float[] point;
        [SerializeField] private string restriction;

        public string Floor => floor;
        public string A => a;
        public string B => b;
        public string Axis => axis;
        public float[] Point => point;
        public string Restriction => restriction;
    }

    [MenuItem("Narnia/Professor House/Connected Layout/Build Walkthrough")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before rebuilding the walkthrough.");
        var manifest = ReadManifest();
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory(PrefabFolder);
        Directory.CreateDirectory(DataFolder);
        Directory.CreateDirectory(ReportFolder);
        AssetDatabase.Refresh();
        ImportModels(manifest);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var environment = new GameObject("ProfessorHouse_Connected_Environment");
        var entries = new Dictionary<string, MeshEntry>();
        for (int i = 0; i < manifest.Meshes.Length; i++)
            entries.Add(manifest.Meshes[i].Name, manifest.Meshes[i]);
        var checks = new List<string>();
        for (int i = 0; i < manifest.Chunks.Length; i++)
        {
            string chunk = manifest.Chunks[i];
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/FBX_ProfessorHouse_" + chunk + ".fbx");
            if (model == null) throw new FileNotFoundException("Missing exported chunk " + chunk);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = "OBJ_ProfessorHouse_" + chunk;
            instance.transform.SetParent(environment.transform, false);
            var filters = instance.GetComponentsInChildren<MeshFilter>(true);
            for (int j = 0; j < filters.Length; j++)
            {
                var filter = filters[j];
                if (!entries.TryGetValue(filter.name, out var entry))
                    throw new InvalidOperationException("Unexpected imported mesh " + filter.name);
                var renderer = filter.GetComponent<MeshRenderer>();
                CheckBounds(filter, entry, checks);
                if (entry.Collision)
                {
                    if (renderer != null) UnityEngine.Object.DestroyImmediate(renderer);
                    filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                }
                else
                {
                    renderer.receiveGI = ReceiveGI.LightProbes;
                    renderer.shadowCastingMode = entry.Glass ? ShadowCastingMode.Off : ShadowCastingMode.On;
                    GameObjectUtility.SetStaticEditorFlags(filter.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic);
                }
            }
            PrefabUtility.SaveAsPrefabAssetAndConnect(instance, PrefabFolder + "/" + instance.name + ".prefab", InteractionMode.AutomatedAction);
        }
        File.WriteAllText(ReportFolder + "/ImportBounds.txt", string.Join("\n", checks) + "\n");
        ProfessorHouseHallFinishBuilder.AddFurnishings(environment);
        ProfessorHouseUpperFinishBuilder.AddFurnishings(environment);
        ProfessorHouseGalleryFinishBuilder.AddFurnishings(environment);
        ProfessorHousePublicFinishBuilder.AddFurnishings(environment);
        BuildNavigation(environment);
        CreateLighting();
        ProfessorHouseHallFinishBuilder.ConfigureLighting();
        ProfessorHouseUpperFinishBuilder.ConfigureLighting();
        ProfessorHouseGalleryFinishBuilder.ConfigureLighting();
        ProfessorHousePublicFinishBuilder.ConfigureLighting();
        var rootObject = new GameObject("ProfessorHouse_Walkthrough");
        rootObject.transform.position = manifest.Spawn;
        var camera = CreateCamera(manifest.Spawn, out var rig);
        ProfessorHouseHallFinishBuilder.ConfigureCamera(camera);
        var miniMap = new GameObject("MiniMapBounds");
        miniMap.transform.position = new Vector3(21, -3, 15);
        var miniMapCollider = miniMap.AddComponent<BoxCollider>();
        miniMapCollider.size = new Vector3(66, .1f, 40);
        miniMapCollider.isTrigger = true;
        rootObject.AddComponent<ProfessorHouseWalkthroughRoot>().Configure(camera, rig, miniMap, manifest.Routes);
        EditorSceneManager.SaveScene(scene, ProfessorHouseWalkthroughRoot.ScenePath);
        AssetDatabase.SaveAssets();
        ProfessorHouseConnectedValidation.Run();
        ProfessorHouseUpperFinishBuilder.ValidateRoomAccess();
        ProfessorHouseGalleryFinishBuilder.ValidateGalleryAccess();
        ProfessorHousePublicFinishBuilder.ValidatePublicAccess();
        FrameHall();
        LogUtil.Log(LogTag + " CONNECTED_WALKTHROUGH_BUILD_COMPLETE");
    }

    public static ModelManifest ReadManifest()
    {
        string path = ModelFolder + "/ProfessorHouse_Connected.json";
        if (!File.Exists(path)) throw new FileNotFoundException("Run Tools/Art/export_professor_house_connected.py first.", path);
        return JsonUtility.FromJson<ModelManifest>(File.ReadAllText(path));
    }

    private static void ImportModels(ModelManifest manifest)
    {
        var materials = new Dictionary<string, Material>();
        for (int i = 0; i < manifest.Palette.Length; i++)
        {
            var entry = manifest.Palette[i];
            string path = MaterialFolder + "/MAT_Connected_" + entry.Name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            string hex = entry.Hex;
            if (entry.Name == "professorFloor" || entry.Name == "childrenFloor" || entry.Name == "atticFloor") hex = "745039";
            if (entry.Name == "corridorFloor") hex = "6B4933";
            if (entry.Name == "serviceFloor") hex = "AAA08B";
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            bool glass = entry.Name == "glass";
            if (glass) color.a = .12f;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", entry.Name.StartsWith("wardrobe") ? .42f : entry.Name.StartsWith("wood") ? .26f : .10f);
            material.SetFloat("_Metallic", entry.Name == "brass" || entry.Name == "metal" ? .5f : 0);
            if (glass)
            {
                material.SetFloat("_Surface", 1);
                material.SetFloat("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)RenderQueue.Transparent;
                material.SetShaderPassEnabled("ShadowCaster", false);
            }
            EditorUtility.SetDirty(material);
            materials.Add(entry.Name, material);
        }
        for (int i = 0; i < manifest.Chunks.Length; i++)
        {
            string path = ModelFolder + "/FBX_ProfessorHouse_" + manifest.Chunks[i] + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new FileNotFoundException("Missing FBX", path);
            importer.importCameras = false;
            importer.importLights = false;
            importer.importAnimation = false;
            importer.isReadable = true;
            importer.generateSecondaryUV = false;
            if (manifest.Chunks[i] == "Hall")
            {
                importer.importNormals = ModelImporterNormals.Calculate;
                importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
                importer.normalSmoothingAngle = 70;
                importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
            }
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            foreach (var pair in materials)
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key), pair.Value);
            importer.SaveAndReimport();
        }
    }

    private static void CheckBounds(MeshFilter filter, MeshEntry entry, List<string> checks)
    {
        var b = filter.sharedMesh.bounds;
        var world = new Bounds(filter.transform.TransformPoint(b.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
        {
            var corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            world.Encapsulate(filter.transform.TransformPoint(corner));
        }
        float delta = Mathf.Max(Vector3.Distance(world.min, entry.BoundsMin), Vector3.Distance(world.max, entry.BoundsMax));
        if (delta > .03f)
            throw new InvalidOperationException(entry.Name + " import axis/scale mismatch: " + delta + "; actual=" + world + "; expected=" + entry.BoundsMin + " / " + entry.BoundsMax);
        checks.Add("PASS " + entry.Name + " bounds delta=" + delta.ToString("F5"));
    }

    private static void BuildNavigation(GameObject environment)
    {
        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(environment.transform, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        var settings = NavMesh.GetSettingsByID(0);
        settings.agentHeight = ProfessorHouseWalkthroughRoot.CharacterHeight;
        settings.agentRadius = ProfessorHouseWalkthroughRoot.CharacterRadius;
        settings.agentClimb = .22f;
        settings.agentSlope = 40;
        settings.overrideVoxelSize = true;
        settings.voxelSize = .055f;
        settings.minRegionArea = .5f;
        var data = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(new Vector3(20, 6, 16), new Vector3(72, 20, 48)), Vector3.zero, Quaternion.identity);
        if (data == null) throw new InvalidOperationException("Connected NavMesh bake returned no data.");
        string path = DataFolder + "/Connected_NavMesh.asset";
        var saved = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if (saved == null)
        {
            AssetDatabase.CreateAsset(data, path);
            saved = data;
        }
        else
        {
            EditorUtility.CopySerialized(data, saved);
            UnityEngine.Object.DestroyImmediate(data);
        }
        var surface = environment.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.overrideVoxelSize = true;
        surface.voxelSize = .055f;
        surface.navMeshData = saved;
        surface.AddData();
    }

    private static Camera CreateCamera(Vector3 spawn, out CinemachineFreeLook rig)
    {
        var cameraObject = new GameObject("CAM_Connected_Main");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CinemachineBrain>();
        camera.transform.position = spawn + new Vector3(0, 1.6f, -2.3f);
        camera.transform.LookAt(spawn + Vector3.up * .85f);
        camera.nearClipPlane = .06f;
        camera.farClipPlane = 100;
        camera.backgroundColor = new Color(.76f, .8f, .79f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
        rig = new GameObject("CAM_Connected_FreeLook").AddComponent<CinemachineFreeLook>();
        rig.m_Lens.FieldOfView = 62;
        rig.m_Lens.NearClipPlane = .06f;
        rig.m_Lens.FarClipPlane = 100;
        rig.m_Orbits = new[] {new CinemachineFreeLook.Orbit(2.3f, 2), new CinemachineFreeLook.Orbit(1.5f, 2.3f), new CinemachineFreeLook.Orbit(.5f, 1.4f)};
        rig.m_YAxis.Value = .5f;
        rig.m_XAxis.Value = 0;
        for (int i = 0; i < 3; i++)
        {
            var composer = rig.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
            if (composer != null) composer.m_TrackedObjectOffset = new Vector3(0, .8f, 0);
        }
        var collision = rig.gameObject.AddComponent<CinemachineCollider>();
        collision.m_CollideAgainst = ~0;
        collision.m_CameraRadius = .12f;
        collision.m_MinimumDistanceFromTarget = .15f;
        collision.m_DistanceLimit = 5;
        collision.m_Strategy = CinemachineCollider.ResolutionStrategy.PullCameraForward;
        collision.m_Damping = .15f;
        collision.m_DampingWhenOccluded = 0;
        return camera;
    }

    private static void CreateLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.63f, .68f, .72f);
        RenderSettings.ambientEquatorColor = new Color(.54f, .49f, .40f);
        RenderSettings.ambientGroundColor = new Color(.25f, .22f, .18f);
        var root = new GameObject("Connected_ReviewLighting");
        var sun = new GameObject("Daylight").AddComponent<Light>();
        sun.transform.SetParent(root.transform, false);
        sun.type = LightType.Directional;
        sun.color = new Color(1, .88f, .69f);
        sun.intensity = 1.1f;
        sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(45, -55, 0);
        RenderSettings.sun = sun;
        var positions = new[] {new Vector3(17,6,16),new Vector3(27,6,30),new Vector3(4,11,27),new Vector3(34,7,29)};
        for (int i = 0; i < positions.Length; i++)
        {
            var light = new GameObject("WarmFill_" + i).AddComponent<Light>();
            light.transform.SetParent(root.transform, false);
            light.transform.position = positions[i];
            light.type = LightType.Point;
            light.color = new Color(1, .80f, .58f);
            light.intensity = 1.6f;
            light.range = 18;
        }
    }

    [MenuItem("Narnia/Professor House/Connected Layout/Open Walkthrough")]
    public static void Open()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ProfessorHouseWalkthroughRoot.ScenePath);
        FrameHall();
    }

    private static void FrameHall()
    {
        if (SceneView.lastActiveSceneView == null) return;
        SceneView.lastActiveSceneView.LookAt(new Vector3(17, 3, 19), Quaternion.Euler(8, 0, 0), 12, false, true);
    }

    public static void BuildBatch()
    {
        try { Build(); EditorApplication.Exit(0); }
        catch (Exception exception) { LogUtil.LogException(exception); EditorApplication.Exit(1); }
    }
}

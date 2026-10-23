using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ProfessorHouseHallFinishBuilder
{
    public static readonly string ModelFolder = "Assets/@Project/10.Models/ProfessorHouseHallFinish";
    public static readonly string MaterialFolder = "Assets/@Project/04.Materials/ProfessorHouseHallFinish";
    public static readonly string TexturePath = "Assets/@Project/03.Sprites/ProfessorHouseHallFinish/TEX_Hall_Paintings.png";
    public static readonly string RendererPath = "Assets/@Project/Settings/Rendering/ProfessorHouseHallRenderer.asset";
    private static readonly string LogTag = "[ProfessorHouseHallFinishBuilder]";
    private static readonly string PrefabPath = "Assets/@Project/02.Prefabs/Environment_Prefab/ProfessorHouseConnected/OBJ_ProfessorHouse_HallFinish.prefab";
    private static readonly string ProfilePath = "Assets/@Project/Settings/Lighting/ProfessorHouseHallVolume.asset";

    [Serializable] private sealed class FinishManifest
    {
        [SerializeField] private ProfessorHouseConnectedBuilder.PaletteEntry[] palette;
        [SerializeField] private ProfessorHouseConnectedBuilder.MeshEntry[] meshes;
        [SerializeField] private BoxEntry[] colliders;
        public ProfessorHouseConnectedBuilder.PaletteEntry[] Palette => palette;
        public ProfessorHouseConnectedBuilder.MeshEntry[] Meshes => meshes;
        public BoxEntry[] Colliders => colliders;
    }

    [Serializable] private sealed class BoxEntry
    {
        [SerializeField] private string name;
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 size;
        public string Name => name;
        public Vector3 Center => center;
        public Vector3 Size => size;
    }

    [MenuItem("Narnia/Professor House/Connected Layout/Build Finished Hall")]
    public static void Build()
    {
        ProfessorHouseConnectedBuilder.Build();
    }

    public static void AddFurnishings(GameObject environment)
    {
        var manifest = JsonUtility.FromJson<FinishManifest>(File.ReadAllText(ModelFolder + "/HallFinish.json"));
        Directory.CreateDirectory(MaterialFolder);
        AssetDatabase.Refresh();
        var textureImporter = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        if (textureImporter == null) throw new FileNotFoundException("Missing painting atlas.", TexturePath);
        textureImporter.textureType = TextureImporterType.Default;
        textureImporter.sRGBTexture = true;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.mipmapEnabled = true;
        textureImporter.maxTextureSize = 2048;
        textureImporter.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        var materials = new Dictionary<string, Material>();
        for (int i = 0; i < manifest.Palette.Length; i++)
        {
            var entry = manifest.Palette[i];
            var material = MakeMaterial(entry.Name, entry.Hex, .25f);
            if (entry.Name == "HallGold") { material.SetFloat("_Metallic", .45f); material.SetFloat("_Smoothness", .42f); }
            if (entry.Name == "HallPainting") { material.SetTexture("_BaseMap", texture); material.SetFloat("_Smoothness", .08f); }
            if (entry.Name == "HallSage") material.SetFloat("_Smoothness", .10f);
            if (entry.Name == "HallFlame")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(4.5f, 2.3f, .65f));
            }
            EditorUtility.SetDirty(material);
            materials.Add(entry.Name, material);
        }
        string modelPath = ModelFolder + "/FBX_ProfessorHouse_HallFinish.fbx";
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
        if (importer == null) throw new FileNotFoundException("Run the hall finish Blender generator first.", modelPath);
        importer.importCameras = false;
        importer.importLights = false;
        importer.importAnimation = false;
        importer.isReadable = false;
        importer.generateSecondaryUV = false;
        importer.importNormals = ModelImporterNormals.Import;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        foreach (var entry in materials)
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), entry.Key), entry.Value);
        importer.SaveAndReimport();
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        instance.name = "OBJ_ProfessorHouse_HallFinish";
        instance.transform.SetParent(environment.transform, false);
        var renderers = instance.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length != manifest.Meshes.Length) throw new InvalidOperationException("Hall finish mesh count mismatch.");
        var report = new List<string>();
        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            ProfessorHouseConnectedBuilder.MeshEntry expected = null;
            for (int j = 0; j < manifest.Meshes.Length; j++)
                if (manifest.Meshes[j].Name == renderer.name) { expected = manifest.Meshes[j]; break; }
            if (expected == null || Vector3.Distance(renderer.bounds.min, expected.BoundsMin) > .03f || Vector3.Distance(renderer.bounds.max, expected.BoundsMax) > .03f)
                throw new InvalidOperationException("Hall finish import axis/scale mismatch: " + renderer.name);
            renderer.receiveGI = ReceiveGI.LightProbes;
            GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic);
            report.Add("PASS " + renderer.name + " import bounds; triangles=" + expected.Triangles);
        }
        for (int i = 0; i < manifest.Colliders.Length; i++)
        {
            var entry = manifest.Colliders[i];
            var obj = new GameObject("Collision_" + entry.Name + "_" + i);
            obj.transform.SetParent(instance.transform, false);
            obj.transform.position = entry.Center;
            obj.AddComponent<BoxCollider>().size = entry.Size;
        }
        PrefabUtility.SaveAsPrefabAssetAndConnect(instance, PrefabPath, InteractionMode.AutomatedAction);
        ApplyHallPalette(environment.transform.Find("OBJ_ProfessorHouse_Hall"));
        report.Add("Furniture and garden trunk box colliders: " + manifest.Colliders.Length);
        report.Add("Painting atlas uses four UV quarters without modifying source pixels.");
        Directory.CreateDirectory(ProfessorHouseConnectedBuilder.ReportFolder);
        File.WriteAllLines(ProfessorHouseConnectedBuilder.ReportFolder + "/HallFinishImport.txt", report);
        LogUtil.Log(LogTag + " HALL_FINISH_IMPORTED");
    }

    private static Material MakeMaterial(string name, string hex, float smoothness)
    {
        string path = MaterialFolder + "/MAT_" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        if (!ColorUtility.TryParseHtmlString("#" + hex, out var color)) throw new InvalidOperationException("Invalid material color " + name);
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ApplyHallPalette(Transform hall)
    {
        if (hall == null) throw new InvalidOperationException("Missing base hall chunk.");
        var colors = new Dictionary<string,string>
        {
            {"wood","563324"}, {"woodLight","795038"}, {"woodEdge","69412B"},
            {"plaster","E0CDAA"}, {"stone","DFD4BC"}, {"stoneAlt","C9BFA8"},
            {"floor","684533"}, {"rug","803A2E"}, {"rugGold","BF9254"}, {"brass","B98B46"}, {"flame","FFE2A4"}
        };
        var replacements = new Dictionary<string,Material>();
        foreach (var pair in colors)
            replacements.Add("MAT_Connected_" + pair.Key, MakeMaterial("HallBase_" + pair.Key, pair.Value, pair.Key.StartsWith("wood") ? .33f : pair.Key.StartsWith("stone") ? .24f : .12f));
        var flame = replacements["MAT_Connected_flame"];
        flame.EnableKeyword("_EMISSION");
        flame.SetColor("_EmissionColor", new Color(3, 1.7f, .6f));
        EditorUtility.SetDirty(flame);
        var renderers = hall.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            var slots = renderers[i].sharedMaterials;
            for (int j = 0; j < slots.Length; j++)
                if (slots[j] != null && replacements.TryGetValue(slots[j].name, out var material)) slots[j] = material;
            renderers[i].sharedMaterials = slots;
        }
        PrefabUtility.ApplyPrefabInstance(hall.gameObject, InteractionMode.AutomatedAction);
    }

    public static void ConfigureLighting()
    {
        var oldFill = GameObject.Find("Connected_ReviewLighting/WarmFill_0");
        if (oldFill != null) UnityEngine.Object.DestroyImmediate(oldFill);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.53f, .52f, .49f);
        var ambient = new SphericalHarmonicsL2();
        ambient.AddAmbientLight(new Color(.36f, .35f, .32f));
        RenderSettings.ambientProbe = ambient;
        RenderSettings.sun.transform.rotation = Quaternion.Euler(32, 180, 0);
        RenderSettings.sun.color = new Color(1, .9f, .75f);
        RenderSettings.sun.intensity = 1.35f;
        var root = new GameObject("ProfessorHouse_HallLighting");
        AddLight(root.transform, "HallDiffuseBounce", new Vector3(17,5.8f,15.7f), new Color(1,.89f,.73f), 12, 10);
        AddLight(root.transform, "WestCandleBounce", new Vector3(11.1f,2.65f,17.5f), new Color(1,.78f,.55f), 1.2f, 4);
        AddLight(root.transform, "EastCandleBounce", new Vector3(22.9f,2.7f,18), new Color(1,.78f,.55f), 1.2f, 4);
        Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath));
        AssetDatabase.Refresh();
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null) { profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, ProfilePath); }
        if (!profile.TryGet(out Tonemapping tone)) { tone = profile.Add<Tonemapping>(); AssetDatabase.AddObjectToAsset(tone, profile); }
        // The shared Chalk-compatible pipeline uses LDR; do not apply HDR tone mapping.
        tone.mode.Override(TonemappingMode.None);
        if (!profile.TryGet(out ColorAdjustments color)) { color = profile.Add<ColorAdjustments>(); AssetDatabase.AddObjectToAsset(color, profile); }
        color.postExposure.Override(.15f);
        color.contrast.Override(7);
        color.saturation.Override(-3);
        if (!profile.TryGet(out Bloom bloom)) { bloom = profile.Add<Bloom>(); AssetDatabase.AddObjectToAsset(bloom, profile); }
        bloom.active = false;
        bloom.intensity.Override(0);
        bloom.threshold.Override(1.2f);
        var volume = root.AddComponent<Volume>();
        volume.isGlobal = false;
        volume.sharedProfile = profile;
        volume.blendDistance = 1.5f;
        volume.priority = 20;
        var bounds = root.AddComponent<BoxCollider>();
        bounds.isTrigger = true;
        bounds.center = new Vector3(17,4.5f,16);
        bounds.size = new Vector3(14,9,12);
        EditorUtility.SetDirty(profile);
        EnsureRenderer();
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

    private static int EnsureRenderer()
    {
        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (renderer == null)
        {
            renderer = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/@Project/Settings/Rendering/NarniaRenderer.asset"));
            renderer.name = "ProfessorHouseHallRenderer";
            AssetDatabase.CreateAsset(renderer, RendererPath);
            var data = new SerializedObject(renderer);
            data.FindProperty("postProcessData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            data.ApplyModifiedPropertiesWithoutUndo();
            var type = typeof(UniversalRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion", true);
            var feature = (ScriptableRendererFeature)ScriptableObject.CreateInstance(type);
            feature.name = "Hall Ambient Occlusion";
            AssetDatabase.AddObjectToAsset(feature, renderer);
            var settings = new SerializedObject(feature);
            settings.FindProperty("m_Shader").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.render-pipelines.universal/Shaders/Utils/ScreenSpaceAmbientOcclusion.shader");
            settings.FindProperty("m_Settings.AOMethod").enumValueIndex = 1;
            settings.FindProperty("m_Settings.Downsample").boolValue = true;
            settings.FindProperty("m_Settings.Intensity").floatValue = 1.1f;
            settings.FindProperty("m_Settings.Radius").floatValue = .2f;
            settings.FindProperty("m_Settings.DirectLightingStrength").floatValue = .2f;
            settings.ApplyModifiedPropertiesWithoutUndo();
            renderer.rendererFeatures.Add(feature);
            feature.Create();
            EditorUtility.SetDirty(renderer);
        }
        var pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
        var serialized = new SerializedObject(pipeline);
        var list = serialized.FindProperty("m_RendererDataList");
        for (int i = 0; i < list.arraySize; i++)
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == renderer) return i;
        int index = list.arraySize;
        list.InsertArrayElementAtIndex(index);
        list.GetArrayElementAtIndex(index).objectReferenceValue = renderer;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return index;
    }

    public static void ConfigureCamera(Camera camera)
    {
        var data = camera.GetUniversalAdditionalCameraData();
        data.SetRenderer(EnsureRenderer());
        data.renderPostProcessing = true;
        camera.allowHDR = true;
    }
}

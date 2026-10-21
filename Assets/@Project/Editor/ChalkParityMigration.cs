using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cinemachine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Events;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// One-time, explicit migration. Never runs automatically on import or during a player build.
public static class ChalkParityMigration
{
    private const string UiPath = "Assets/@Project/02.Prefabs/UI";
    private const string ScenePath = "Assets/@Project/00.Scenes/";
    [MenuItem("Narnia/Migration/Apply Chalk Parity")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (File.Exists(UiPath + "/UI_Field.prefab")) throw new InvalidOperationException("UI migration was already applied. Use the recorded source revision for a new migration.");
        var previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            Directory.CreateDirectory(UiPath);
            AssetDatabase.Refresh();
            ConvertTitle();
            ConvertLoading();
            ConvertHouse();
            AddRoomRoots();
            SetupAddressables();
            SetupRendering();
            AssetDatabase.SaveAssets();
        }
        finally
        {
            bool restorable = previous.Length > 0;
            for (int i = 0; i < previous.Length; i++)
                if (string.IsNullOrEmpty(previous[i].path) || !File.Exists(previous[i].path)) restorable = false;
            if (restorable) EditorSceneManager.RestoreSceneManagerSetup(previous);
            else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
    }
    private static void SetReference(Object target, string field, Object value)
    {
        using (var serialized = new SerializedObject(target))
        {
            var property = serialized.FindProperty(field);
            if (property == null) throw new InvalidOperationException(target.GetType().Name + "." + field);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
    private static T GetReference<T>(Object target, string field) where T : Object
    {
        using (var serialized = new SerializedObject(target)) return (T)serialized.FindProperty(field).objectReferenceValue;
    }
    private static void ConvertTitle()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath + "Title.unity", OpenSceneMode.Single);
        Title old = SceneBinding.FirstInScene<Title>(scene);
        Canvas canvas = old.GetComponentInParent<Canvas>();
        if (canvas == null) canvas = old.GetComponent<Canvas>();
        GameObject copy = Object.Instantiate(canvas.gameObject);
        Object.DestroyImmediate(copy.GetComponent<Title>());
        var view = copy.AddComponent<UI_Title>();
        foreach (var button in copy.GetComponentsInChildren<Button>(true))
        {
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                string method = button.onClick.GetPersistentMethodName(i);
                UnityEventTools.RemovePersistentListener(button.onClick, i);
                if (method == nameof(Title.StartButton)) UnityEventTools.AddPersistentListener(button.onClick, view.StartButton);
                else if (method == nameof(Title.ContinueButton)) UnityEventTools.AddPersistentListener(button.onClick, view.ContinueButton);
                else if (method == nameof(Title.SettingButton)) UnityEventTools.AddPersistentListener(button.onClick, view.SettingButton);
                else throw new InvalidOperationException("Unknown title callback: " + method);
            }
        }
        Save(copy, AssetKeys.UI.Title);
        Object.DestroyImmediate(canvas.gameObject);
        new GameObject("@Title").AddComponent<Title>();
        EditorSceneManager.SaveScene(scene);
    }
    private static void ConvertLoading()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath + "LoadingScene.unity", OpenSceneMode.Single);
        LoadingController old = SceneBinding.FirstInScene<LoadingController>(scene);
        Canvas canvas = old.GetComponent<Canvas>();
        GameObject copy = Object.Instantiate(canvas.gameObject);
        var component = copy.GetComponent<LoadingController>();
        Image background = GetReference<Image>(component, "backgroundImage");
        Image progress = GetReference<Image>(component, "progressImage");
        Object.DestroyImmediate(component);
        var view = copy.AddComponent<UI_Loading>();
        SetReference(view, "background", background);
        SetReference(view, "progress", progress);
        Save(copy, AssetKeys.UI.Loading);
        Object.DestroyImmediate(canvas.gameObject);
        new GameObject("@Loading").AddComponent<LoadingController>();
        EditorSceneManager.SaveScene(scene);
    }
    private static void ConvertHouse()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath + "Professor_House.unity", OpenSceneMode.Single);
        var field = SceneBinding.FirstInScene<FieldManager>(scene);
        GameObject old = GetReference<GameObject>(field, "uiRoot");
        var miniMap = old.GetComponentInChildren<MiniMapController>(true);
        SetReference(field, "miniMapGround", GetReference<GameObject>(miniMap, "plane"));
        // Separate overlays retain their existing visual hierarchy, anchors and serialized UI references.
        GameObject settingsCopy = Object.Instantiate(old);
        GameObject window = SceneBinding.NamedChild(settingsCopy, nameof(Define.UIName.SettingWindow));
        window.transform.SetParent(settingsCopy.transform, true);
        RemoveOtherChildren(settingsCopy, window);
        var settings = settingsCopy.AddComponent<UI_Settings>();
        foreach (var button in window.GetComponentsInChildren<Button>(true))
        {
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                string method = button.onClick.GetPersistentMethodName(i);
                if (method == nameof(ButtonManager.SettingWindowOff))
                {
                    UnityEventTools.RemovePersistentListener(button.onClick, i);
                    UnityEventTools.AddPersistentListener(button.onClick, settings.SettingWindowOff);
                }
            }
        }
        window.SetActive(true);
        Save(settingsCopy, AssetKeys.UI.Settings);
        GameObject dialogueCopy = Object.Instantiate(old);
        GameObject message = SceneBinding.NamedChild(dialogueCopy, nameof(Define.UIName.MessageBox));
        message.transform.SetParent(dialogueCopy.transform, true);
        RemoveOtherChildren(dialogueCopy, message);
        var dialogue = dialogueCopy.AddComponent<UI_Dialogue>();
        SetReference(dialogue, "view", message.GetComponent<MessageBoxController>());
        message.SetActive(true);
        Save(dialogueCopy, AssetKeys.UI.Dialogue);
        GameObject hudCopy = Object.Instantiate(old);
        Object.DestroyImmediate(SceneBinding.NamedChild(hudCopy, nameof(Define.UIName.SettingWindow)));
        Object.DestroyImmediate(SceneBinding.NamedChild(hudCopy, nameof(Define.UIName.MessageBox)));
        var hud = hudCopy.AddComponent<UI_Field>();
        SetReference(hud, "joystick", SceneBinding.NamedChild(hudCopy, nameof(Define.UIName.Joystick)));
        SetReference(hud, "action", SceneBinding.NamedChild(hudCopy, nameof(Define.UIName.ActionButton)));
        SetReference(hud, "settings", SceneBinding.NamedChild(hudCopy, nameof(Define.UIName.SettingButton)));
        SetReference(hud, "miniMap", SceneBinding.NamedChild(hudCopy, nameof(Define.UIName.MiniMap)));
        Save(hudCopy, AssetKeys.UI.Field);
        Object.DestroyImmediate(old);
        SetReference(field, "uiRoot", null);
        // Production cameras must not retain a prototype follower aimed at the inactive legacy player.
        foreach (var controller in SceneBinding.InScene<CameraController>(scene)) Object.DestroyImmediate(controller);
        Camera camera = SceneBinding.MainCamera(scene);
        if (camera.GetComponent<CinemachineBrain>() == null) camera.gameObject.AddComponent<CinemachineBrain>();
        var cameraRoot = GetReference<GameObject>(field, "cameraRoot");
        cameraRoot.SetActive(true);
        foreach (var rig in cameraRoot.GetComponentsInChildren<CinemachineFreeLook>(true)) rig.gameObject.SetActive(true);
        EditorSceneManager.SaveScene(scene);
    }
    private static void RemoveOtherChildren(GameObject root, GameObject keep)
    {
        for (int i = root.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = root.transform.GetChild(i).gameObject;
            if (child != keep) Object.DestroyImmediate(child);
        }
    }
    private static void Save(GameObject copy, string name)
    {
        copy.name = name;
        copy.SetActive(true);
        PrefabUtility.SaveAsPrefabAsset(copy, UiPath + "/" + name + ".prefab");
        Object.DestroyImmediate(copy);
    }
    private static void AddRoomRoots()
    {
        foreach (string name in new[] { "TheSiblingsRoom", "TheWardrobeRoom" })
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath + name + ".unity", OpenSceneMode.Single);
            if (SceneBinding.FirstInScene<SceneRoot>(scene) == null) new GameObject("@" + name).AddComponent<RoomScene>();
            EditorSceneManager.SaveScene(scene);
        }
    }
    [MenuItem("Narnia/Resources/Configure Addressables")]
    public static void SetupAddressables()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AddressableAssetSettings.Create("Assets/AddressableAssetsData", "AddressableAssetSettings", true, true);
            AddressableAssetSettingsDefaultObject.Settings = settings;
        }
        settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
        Add(settings, AddressableLabels.Title, UiPath + "/UI_Title.prefab");
        Add(settings, AddressableLabels.Loading, UiPath + "/UI_Loading.prefab");
        foreach (string name in AssetKeys.LoadingBackgrounds)
            Add(settings, AddressableLabels.Loading, "Assets/@Project/03.Sprites/Loading/BackgroundImage/" + name + ".jpg");
        Add(settings, AddressableLabels.ProfessorHouse, "Assets/@Project/02.Prefabs/Character/Lucy.prefab");
        foreach (string name in new[] { AssetKeys.UI.Field, AssetKeys.UI.Settings, AssetKeys.UI.Dialogue })
            Add(settings, AddressableLabels.ProfessorHouse, UiPath + "/" + name + ".prefab");
        Add(settings, AddressableLabels.Story, "Assets/@Project/06.Data/Message/QuestData.json");
        settings.ActivePlayModeDataBuilderIndex = 0;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
    private static void Add(AddressableAssetSettings settings, string label, string path)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid)) throw new FileNotFoundException(path);
        var group = settings.FindGroup(label);
        if (group == null) group = settings.CreateGroup(label, false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        var schema = group.GetSchema<BundledAssetGroupSchema>();
        schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
        schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
        schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = Path.GetFileNameWithoutExtension(path);
        settings.AddLabel(label);
        entry.SetLabel(label, true);
        EditorUtility.SetDirty(group);
    }
    public static void ConfigureUIScalingAndBuild()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { UiPath }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var scaler = root.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        NarniaBuild.BuildMac();
    }
    public static void FinishRenderingAndBuild()
    {
        var pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
        string path = "Assets/@Project/04.Materials/MAT_Default.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(pipeline.defaultMaterial);
            AssetDatabase.CreateAsset(material, path);
        }
        foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/@Project/00.Scenes" }))
        {
            Scene scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid), OpenSceneMode.Single);
            bool changed = false;
            foreach (var renderer in SceneBinding.InScene<Renderer>(scene))
            {
                Material[] materials = renderer.sharedMaterials;
                bool replace = false;
                for (int n = 0; n < materials.Length; n++)
                {
                    if (materials[n] != null && materials[n].shader != null && materials[n].shader.name == "Standard" && AssetDatabase.GetAssetPath(materials[n]) == "Resources/unity_builtin_extra")
                    {
                        materials[n] = material;
                        replace = true;
                    }
                }
                if (replace) { renderer.sharedMaterials = materials; changed = true; }
            }
            if (changed) EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        NarniaBuild.BuildMac();
    }
    [MenuItem("Narnia/Migration/Convert Rendering to URP")]
    public static void SetupRendering()
    {
        string directory = "Assets/@Project/Settings/Rendering";
        Directory.CreateDirectory(directory);
        AssetDatabase.Refresh();
        string path = directory + "/NarniaURP.asset";
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
        if (pipeline == null)
        {
            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, directory + "/NarniaRenderer.asset");
            ResourceReloader.ReloadAllNullIn(renderer, UniversalRenderPipelineAsset.packagePath);
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.useSRPBatcher = true;
            pipeline.supportsHDR = false;
            pipeline.msaaSampleCount = 4;
            pipeline.shadowDistance = 40;
            AssetDatabase.CreateAsset(pipeline, path);
        }
        GraphicsSettings.defaultRenderPipeline = pipeline;
        int previous = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipeline;
        }
        QualitySettings.SetQualityLevel(previous, false);
        PlayerSettings.colorSpace = ColorSpace.Linear;
        // Use Unity's pinned URP 14 conversion table (includes legacy/particle/terrain mappings).
        Type converter = typeof(StandardUpgrader).Assembly.GetType("UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader", true);
        MethodInfo method = converter.GetMethod("GetUpgraders", BindingFlags.NonPublic | BindingFlags.Static);
        object[] args = { new List<MaterialUpgrader>() };
        method.Invoke(null, args);
        var upgraders = (List<MaterialUpgrader>)args[0];
        int converted = 0;
        var skipped = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (!(asset is Material material) || !AssetDatabase.IsMainAsset(material) || material.shader == null) continue;
                string shader = material.shader.name;
                if (shader.StartsWith("Universal Render Pipeline/") || shader.StartsWith("TextMeshPro/") || shader.StartsWith("UI/") || shader.StartsWith("Skybox/") || shader.StartsWith("Hidden/")) continue;
                string message = string.Empty;
                if (MaterialUpgrader.Upgrade(material, upgraders, MaterialUpgrader.UpgradeFlags.None, ref message)) { converted++; EditorUtility.SetDirty(material); }
                else skipped.Add(assetPath + " : " + shader);
            }
        }
        Directory.CreateDirectory("Docs/Validation/ChalkParity");
        File.WriteAllText("Docs/Validation/ChalkParity/Rendering.txt", "Converted materials: " + converted + "\nUnconverted shaders (inspect usage):\n" + string.Join("\n", skipped));
        AssetDatabase.SaveAssets();
    }
}

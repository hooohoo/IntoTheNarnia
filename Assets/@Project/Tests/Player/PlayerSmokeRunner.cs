using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Opt-in development-player smoke / preview entry. Inert without the corresponding launch environment.
public static class PlayerSmokeRunner
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Start()
    {
        string output = Environment.GetEnvironmentVariable("NARNIA_SMOKE_OUTPUT");
        if (!string.IsNullOrEmpty(output)) RunAsync(output).Forget();
        else if (Environment.GetEnvironmentVariable("NARNIA_PREVIEW_HOUSE") == "1") OpenHouseAsync().Forget();
    }
    private static async UniTask OpenHouseAsync()
    {
        await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
        var facade = FacadeManager.Instance;
        await facade.Get<GameSceneManager>().LoadSceneAsync(Define.SceneName.Professor_House);
        var dialogue = SceneBinding.FirstInScene<MessageBoxController>(SceneManager.GetActiveScene());
        for (int i = 0; i < 12 && facade.Get<GameStateManager>().Current == GameStateType.Dialogue; i++) dialogue.PlayNextLine();
        Debug.Log("HOUSE_PREVIEW_READY");
    }
    private static async UniTask RunAsync(string output)
    {
        try
        {
            Directory.CreateDirectory(output);
            await GameSceneManager.WaitForReadyAsync(SceneManager.GetActiveScene());
            await CaptureAsync(Path.Combine(output, "Title.png"));
            var facade = FacadeManager.Instance;
            await facade.Get<GameSceneManager>().LoadSceneAsync(Define.SceneName.Professor_House);
            if (facade.Get<ObjectManager>().Player == null) throw new InvalidOperationException("Player missing");
            await CaptureAsync(Path.Combine(output, "ProfessorHouse.png"));
            var dialogue = SceneBinding.FirstInScene<MessageBoxController>(SceneManager.GetActiveScene());
            for (int i = 0; i < 12 && facade.Get<GameStateManager>().Current == GameStateType.Dialogue; i++) dialogue.PlayNextLine();
            await CaptureAsync(Path.Combine(output, "Exploration.png"));
            if (Environment.GetEnvironmentVariable("NARNIA_HOUSE_CAPTURE") == "1")
                await CaptureHouseAsync(output, facade.Get<CameraManager>().Camera);
            facade.Get<UIManager>().ShowSettings();
            await CaptureAsync(Path.Combine(output, "Settings.png"));
            facade.Get<UIManager>().HideSettings();
            await facade.Get<GameSceneManager>().LoadSceneAsync(Define.SceneName.Title);
            if (facade.Get<AddressableManager>().OwnedBatchCount != 1) throw new InvalidOperationException("Scene resource ownership leaked");
            File.WriteAllText(Path.Combine(output, "Result.txt"), "PASS: packaged Title -> Professor_House -> Title; player present; only Title label retained.\n");
            Application.Quit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            File.WriteAllText(Path.Combine(output, "Result.txt"), "FAIL: " + exception);
            Application.Quit(1);
        }
    }
    private static async UniTask CaptureHouseAsync(string output, Camera mainCamera)
    {
        // Opt-in art captures from the same packaged scene, with its actual lightmaps and materials.
        var scene = SceneManager.GetActiveScene();
        var canvases = SceneBinding.InScene<Canvas>(scene);
        var enabledCanvases = new System.Collections.Generic.List<Canvas>();
        foreach (var canvas in canvases) if (canvas.enabled) { enabledCanvases.Add(canvas); canvas.enabled = false; }
        mainCamera.enabled = false;
        try
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "HousePreviewCameras")
                    foreach (var camera in root.GetComponentsInChildren<Camera>())
                    {
                        camera.enabled = true;
                        await CaptureAsync(Path.Combine(output, "Hall_" + camera.name + ".png"));
                        camera.enabled = false;
                    }
        }
        finally
        {
            mainCamera.enabled = true;
            foreach (var canvas in enabledCanvases) canvas.enabled = true;
        }
    }
    private static async UniTask CaptureAsync(string path)
    {
        while (!UnityEngine.Rendering.SplashScreen.isFinished) await UniTask.Yield();
        for (int i = 0; i < 5; i++) await UniTask.Yield();
        ScreenCapture.CaptureScreenshot(path);
        float start = Time.realtimeSinceStartup;
        while (!File.Exists(path))
        {
            if (Time.realtimeSinceStartup - start > 30) throw new TimeoutException("Screenshot: " + path);
            await UniTask.Yield();
        }
    }
}

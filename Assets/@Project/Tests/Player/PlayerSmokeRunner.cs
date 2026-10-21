using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Opt-in development-player smoke entry. Inert unless the launch environment supplies an output folder.
public static class PlayerSmokeRunner
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Start()
    {
        string output = Environment.GetEnvironmentVariable("NARNIA_SMOKE_OUTPUT");
        if (!string.IsNullOrEmpty(output)) RunAsync(output).Forget();
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

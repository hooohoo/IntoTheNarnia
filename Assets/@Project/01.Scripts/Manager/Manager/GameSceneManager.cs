using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSceneManager : Singleton<GameSceneManager>
{
    [Singleton(typeof(GameStateManager))] private GameStateManager states;
    private CancellationTokenSource lifetime;
    private AsyncOperation operation;
    public bool IsLoading { get; private set; }
    public float Progress { get; private set; }
    public override void Init() { lifetime = new CancellationTokenSource(); }
    public void LoadScene(Define.SceneName target) { LoadSceneAsync(target).Forget(); }
    public async UniTask LoadSceneAsync(Define.SceneName target)
    {
        if (IsLoading) return;
        string scene = target.ToString();
        if (target == Define.SceneName.LoadingScene || !Application.CanStreamedLevelBeLoaded(scene))
            throw new ArgumentException("Scene is not a playable build scene: " + scene);
        IsLoading = true;
        Progress = 0;
        states.RegisterSceneStates();
        states.Change(GameStateType.Loading);
        var token = lifetime.Token;
        try
        {
            operation = SceneManager.LoadSceneAsync(nameof(Define.SceneName.LoadingScene));
            while (!operation.isDone) await UniTask.Yield(PlayerLoopTiming.Update, token);
            await WaitForReadyAsync(SceneManager.GetActiveScene(), token);
            operation = SceneManager.LoadSceneAsync(scene);
            while (!operation.isDone)
            {
                Progress = operation.progress * 0.8f;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            await WaitForReadyAsync(SceneManager.GetActiveScene(), token);
            Progress = 1;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally { operation = null; IsLoading = false; }
    }
    public static async UniTask WaitForReadyAsync(Scene scene, CancellationToken token = default)
    {
        var root = SceneBinding.FirstInScene<SceneRoot>(scene);
        if (root == null) throw new InvalidOperationException("Scene has no composition root: " + scene.name);
        while (root != null && !root.IsReady)
        {
            if (root.Failure != null) throw new InvalidOperationException("Scene composition failed: " + scene.name, root.Failure);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        if (root == null) throw new OperationCanceledException("Scene exited while preparing.");
    }
    public override void Release()
    {
        lifetime?.Cancel();
        lifetime?.Dispose();
        lifetime = null;
    }
}

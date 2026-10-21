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
    public override void Init()
    {
        lifetime = new CancellationTokenSource();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void LoadScene(Define.SceneName target) { LoadSceneAsync(target).Forget(); }
    public async UniTask LoadSceneAsync(Define.SceneName target)
    {
        if (IsLoading) return;
        string scene = target.ToString();
        if (target == Define.SceneName.LoadingScene || !Application.CanStreamedLevelBeLoaded(scene))
            throw new ArgumentException("Scene is not a playable build scene: " + scene);
        IsLoading = true;
        Progress = 0;
        states.Change(GameStateType.Loading);
        var token = lifetime.Token;
        try
        {
            operation = SceneManager.LoadSceneAsync(Define.SceneName.LoadingScene.ToString());
            while (!operation.isDone) await UniTask.Yield(PlayerLoopTiming.Update, token);
            operation = SceneManager.LoadSceneAsync(scene);
            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f)
            {
                Progress = operation.progress;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            float elapsed = 0;
            while (elapsed < 1)
            {
                elapsed += Time.unscaledDeltaTime;
                Progress = Mathf.Lerp(0.9f, 1, elapsed);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            operation.allowSceneActivation = true;
            while (!operation.isDone) await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally
        {
            if (operation != null) operation.allowSceneActivation = true;
            operation = null;
            IsLoading = false;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == nameof(Define.SceneName.LoadingScene))
        {
            if (!IsLoading) SceneManager.LoadSceneAsync(nameof(Define.SceneName.Title));
            return;
        }
        if (scene.name == nameof(Define.SceneName.Title)) states.Change(GameStateType.Title);
        else if (states.Current == GameStateType.Loading || states.Current == GameStateType.Title) states.Change(GameStateType.Exploration);
    }
    public override void Release()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        lifetime?.Cancel();
        lifetime?.Dispose();
        lifetime = null;
        if (operation != null) operation.allowSceneActivation = true;
    }
}

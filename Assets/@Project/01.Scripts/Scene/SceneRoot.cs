using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SceneRoot : RuntimeBehaviour
{
    private static readonly string LogTag = "[SceneRoot]";
    [Singleton(typeof(AddressableManager))] protected AddressableManager resources;
    [Singleton(typeof(GameStateManager))] protected GameStateManager states;
    public bool IsReady { get; private set; }
    public Exception Failure { get; private set; }
    protected override void Initialize()
    {
        IsReady = false;
        Failure = null;
        states.RegisterSceneStates();
        states.Change(GameStateType.Loading);
        InitializeAsync(Lifetime).Forget();
    }
    private async UniTask InitializeAsync(CancellationToken token)
    {
        try
        {
            await BuildAsync(token);
            token.ThrowIfCancellationRequested();
            IsReady = true;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception exception)
        {
            Failure = exception;
            states.Change(GameStateType.Loading);
            LogUtil.LogError(LogTag + " " + gameObject.scene.name + ": " + exception);
            enabled = false;
        }
    }
    protected abstract UniTask BuildAsync(CancellationToken token);
    protected abstract void ReleaseScene();
    protected override void Release()
    {
        IsReady = false;
        // Instances/pools must be destroyed before the label handles are released.
        try { ReleaseScene(); }
        finally { resources.ReleaseOwner(this); }
    }
}

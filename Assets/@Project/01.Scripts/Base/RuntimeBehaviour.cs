using System;
using System.Threading;
using UnityEngine;

public abstract class RuntimeBehaviour : MonoBehaviour
{
    private bool initialized;
    private CancellationTokenSource lifetime;
    private FrameLoop loop;
    protected CancellationToken Lifetime => lifetime == null ? new CancellationToken(true) : lifetime.Token;

    protected virtual void OnEnable() { EnsureInitialized(); }
    protected virtual void OnDisable() { Shutdown(); }
    protected virtual void OnDestroy() { Shutdown(); }

    public void EnsureInitialized()
    {
        if (initialized) return;
        var facade = FacadeManager.Instance;
        if (facade == null) return;
        loop = facade.Loop;
        lifetime = new CancellationTokenSource();
        initialized = true;
        try
        {
            facade.Inject(this);
            InjectUtil.InjectComponents(this);
            Initialize();
            if (initialized && isActiveAndEnabled) loop.Register(this);
        }
        catch (Exception exception)
        {
            try { Shutdown(); }
            catch (Exception cleanupFailure) { exception = new AggregateException(exception, cleanupFailure); }
            LogUtil.LogException(exception, this);
            enabled = false;
        }
    }
    protected virtual void Initialize() { }
    protected virtual void Release() { }
    private void Shutdown()
    {
        if (!initialized) return;
        initialized = false;
        loop?.Unregister(this);
        lifetime?.Cancel();
        try { Release(); }
        finally
        {
            lifetime?.Dispose();
            lifetime = null;
            loop = null;
        }
    }
}

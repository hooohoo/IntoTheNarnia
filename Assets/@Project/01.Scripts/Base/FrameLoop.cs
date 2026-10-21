using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class FrameLoop
{
    private readonly List<IFrameTick> receivers = new List<IFrameTick>();
    private readonly List<ILateFrameTick> lateReceivers = new List<ILateFrameTick>();
    private readonly List<IFrameTick> snapshot = new List<IFrameTick>();
    private readonly List<ILateFrameTick> lateSnapshot = new List<ILateFrameTick>();
    private readonly HashSet<IFrameTick> active = new HashSet<IFrameTick>();
    private readonly HashSet<ILateFrameTick> lateActive = new HashSet<ILateFrameTick>();
    private CancellationTokenSource lifetime;
    private InputManager input;

    public void Initialize(InputManager inputManager)
    {
        input = inputManager;
        lifetime = new CancellationTokenSource();
        RunAsync(lifetime.Token).Forget();
        RunLateAsync(lifetime.Token).Forget();
    }
    public void Register(object receiver)
    {
        if (receiver is IFrameTick tick && active.Add(tick)) receivers.Add(tick);
        if (receiver is ILateFrameTick late && lateActive.Add(late)) lateReceivers.Add(late);
    }
    public void Unregister(object receiver)
    {
        if (receiver is IFrameTick tick) { receivers.Remove(tick); active.Remove(tick); }
        if (receiver is ILateFrameTick late) { lateReceivers.Remove(late); lateActive.Remove(late); }
    }
    private async UniTask RunAsync(CancellationToken token)
    {
        try
        {
            while (true)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                try { input.ReadFrame(); }
                catch (Exception exception) { LogUtil.LogException(exception); }
                snapshot.Clear();
                snapshot.AddRange(receivers);
                foreach (var receiver in snapshot)
                {
                    if (!active.Contains(receiver)) continue;
                    try { receiver.Tick(Time.deltaTime); }
                    catch (Exception exception) { LogUtil.LogException(exception); }
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }
    private async UniTask RunLateAsync(CancellationToken token)
    {
        try
        {
            while (true)
            {
                await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, token);
                lateSnapshot.Clear();
                lateSnapshot.AddRange(lateReceivers);
                foreach (var receiver in lateSnapshot)
                {
                    if (!lateActive.Contains(receiver)) continue;
                    try { receiver.LateTick(Time.deltaTime); }
                    catch (Exception exception) { LogUtil.LogException(exception); }
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }
    public void Release()
    {
        lifetime?.Cancel();
        lifetime?.Dispose();
        lifetime = null;
        receivers.Clear();
        lateReceivers.Clear();
        active.Clear();
        lateActive.Clear();
    }
}

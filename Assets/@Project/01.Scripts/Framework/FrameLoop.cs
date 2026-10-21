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
        if (receiver is IFrameTick tick && !receivers.Contains(tick)) receivers.Add(tick);
        if (receiver is ILateFrameTick late && !lateReceivers.Contains(late)) lateReceivers.Add(late);
    }
    public void Unregister(object receiver)
    {
        if (receiver is IFrameTick tick) receivers.Remove(tick);
        if (receiver is ILateFrameTick late) lateReceivers.Remove(late);
    }
    private async UniTask RunAsync(CancellationToken token)
    {
        try
        {
            while (true)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                input.ReadFrame();
                snapshot.Clear();
                snapshot.AddRange(receivers);
                foreach (var receiver in snapshot)
                {
                    if (!receivers.Contains(receiver)) continue;
                    try { receiver.Tick(Time.deltaTime); }
                    catch (Exception exception) { Debug.LogException(exception); }
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
                    if (!lateReceivers.Contains(receiver)) continue;
                    try { receiver.LateTick(Time.deltaTime); }
                    catch (Exception exception) { Debug.LogException(exception); }
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
    }
}

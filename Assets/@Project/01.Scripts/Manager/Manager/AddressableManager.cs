using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class AddressableManager : Singleton<AddressableManager>
{
    private static readonly string LogTag = "[AddressableManager]";
    private sealed class Batch
    {
        public string Key;
        public string Label;
        public AsyncOperationHandle Handle;
        public bool HasHandle;
        public bool Finished;
        public bool Retired;
        public readonly HashSet<object> Owners = new HashSet<object>();
        public readonly List<string> Keys = new List<string>();
        public readonly UniTaskCompletionSource<bool> Completion = new UniTaskCompletionSource<bool>();
    }
    private sealed class CachedAsset
    {
        public UnityEngine.Object Asset;
        public readonly HashSet<Batch> Batches = new HashSet<Batch>();
    }
    private readonly Dictionary<string, Batch> batches = new Dictionary<string, Batch>(StringComparer.Ordinal);
    private readonly Dictionary<string, CachedAsset> cache = new Dictionary<string, CachedAsset>(StringComparer.Ordinal);
    public int CachedAssetCount => cache.Count;
    public int OwnedBatchCount => batches.Count;

    // An owner acquires a label/type only once. Concurrent callers share the same load.
    public async UniTask<bool> PreloadAsync<T>(string label, object owner, CancellationToken token = default) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("A feature label is required.", nameof(label));
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        token.ThrowIfCancellationRequested();
        string key = label + ":" + typeof(T).FullName;
        if (!batches.TryGetValue(key, out var batch))
        {
            batch = new Batch { Key = key, Label = label };
            batches.Add(key, batch);
            batch.Owners.Add(owner);
            LoadBatchAsync<T>(batch).Forget();
        }
        else batch.Owners.Add(owner);
        bool loaded = await batch.Completion.Task.AttachExternalCancellation(token);
        return loaded && !batch.Retired && batch.Owners.Contains(owner);
    }
    private async UniTask LoadBatchAsync<T>(Batch batch) where T : UnityEngine.Object
    {
        bool loaded = false;
        try
        {
            var handle = Addressables.LoadAssetsAsync<T>(batch.Label, null, false);
            batch.Handle = handle;
            batch.HasHandle = true;
            while (!handle.IsDone) await UniTask.Yield();
            if (batch.Retired) return;
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null || handle.Result.Count == 0)
                throw new InvalidOperationException("Required label failed or is empty: " + batch.Label, handle.OperationException);
            var seen = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);
            for (int i = 0; i < handle.Result.Count; i++)
            {
                T asset = handle.Result[i];
                if (asset == null) throw new InvalidOperationException("Null asset in " + batch.Label);
                if (seen.TryGetValue(asset.name, out var other) && other != asset)
                    throw new InvalidOperationException("Duplicate address: " + asset.name);
                if (cache.TryGetValue(asset.name, out var existing) && existing.Asset != asset)
                    throw new InvalidOperationException("Concurrent address collision: " + asset.name);
                seen[asset.name] = asset;
            }
            foreach (var pair in seen)
            {
                if (!cache.TryGetValue(pair.Key, out var entry))
                {
                    entry = new CachedAsset { Asset = pair.Value };
                    cache.Add(pair.Key, entry);
                }
                entry.Batches.Add(batch);
                batch.Keys.Add(pair.Key);
            }
            loaded = true;
        }
        catch (Exception exception)
        {
            LogUtil.LogError(LogTag + " " + exception);
        }
        finally
        {
            batch.Finished = true;
            if (!loaded || batch.Retired)
            {
                if (batches.TryGetValue(batch.Key, out var current) && ReferenceEquals(current, batch)) batches.Remove(batch.Key);
                batch.Retired = true;
                ReleaseHandle(batch);
            }
            batch.Completion.TrySetResult(loaded && !batch.Retired);
        }
    }
    public T Load<T>(string address) where T : UnityEngine.Object
    {
        if (!cache.TryGetValue(address, out var entry) || !(entry.Asset is T result))
            throw new InvalidOperationException("Asset is not preloaded as " + typeof(T).Name + ": " + address);
        return result;
    }
    public void ReleaseLabel(string label, object owner)
    {
        var values = new List<Batch>(batches.Values);
        for (int i = 0; i < values.Count; i++)
        {
            Batch batch = values[i];
            if (batch.Label != label || !batch.Owners.Remove(owner) || batch.Owners.Count != 0) continue;
            Retire(batch);
        }
    }
    public void ReleaseOwner(object owner)
    {
        var values = new List<Batch>(batches.Values);
        for (int i = 0; i < values.Count; i++)
        {
            Batch batch = values[i];
            if (batch.Owners.Remove(owner) && batch.Owners.Count == 0) Retire(batch);
        }
    }
    private void Retire(Batch batch)
    {
        batch.Retired = true;
        batches.Remove(batch.Key);
        for (int i = 0; i < batch.Keys.Count; i++)
        {
            string key = batch.Keys[i];
            if (!cache.TryGetValue(key, out var entry)) continue;
            entry.Batches.Remove(batch);
            if (entry.Batches.Count == 0) cache.Remove(key);
        }
        batch.Keys.Clear();
        // A running Addressables operation cannot be aborted. Its completion owns the final release.
        if (batch.Finished) ReleaseHandle(batch);
    }
    private static void ReleaseHandle(Batch batch)
    {
        if (batch.HasHandle && batch.Handle.IsValid()) Addressables.Release(batch.Handle);
        batch.HasHandle = false;
    }
    public override void Release()
    {
        var values = new List<Batch>(batches.Values);
        for (int i = 0; i < values.Count; i++) Retire(values[i]);
        cache.Clear();
    }
}

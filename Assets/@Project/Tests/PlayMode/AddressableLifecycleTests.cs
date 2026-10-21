using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class AddressableLifecycleTests
{
    [UnityTest] public IEnumerator SharedLabelSurvivesUntilItsLastOwnerLeaves() { return SharedAsync().ToCoroutine(); }
    private static async UniTask SharedAsync()
    {
        var manager = new AddressableManager();
        object first = new object();
        object second = new object();
        try
        {
            UniTask<bool> a = manager.PreloadAsync<TextAsset>(AddressableLabels.Story, first);
            UniTask<bool> b = manager.PreloadAsync<TextAsset>(AddressableLabels.Story, second);
            Assert.IsTrue(await a);
            Assert.IsTrue(await b);
            Assert.AreEqual(1, manager.OwnedBatchCount);
            manager.ReleaseOwner(first);
            Assert.IsNotNull(manager.Load<TextAsset>(AssetKeys.QuestData));
            manager.ReleaseOwner(second);
            Assert.AreEqual(0, manager.CachedAssetCount);
            Assert.AreEqual(0, manager.OwnedBatchCount);
        }
        finally { manager.Release(); }
    }
    [UnityTest] public IEnumerator LeavingDuringPreloadDoesNotOverwriteReentry() { return ReentryAsync().ToCoroutine(); }
    private static async UniTask ReentryAsync()
    {
        var manager = new AddressableManager();
        object oldOwner = new object();
        object newOwner = new object();
        try
        {
            var oldLoad = manager.PreloadAsync<TextAsset>(AddressableLabels.Story, oldOwner);
            manager.ReleaseOwner(oldOwner);
            var newLoad = manager.PreloadAsync<TextAsset>(AddressableLabels.Story, newOwner);
            Assert.IsFalse(await oldLoad);
            Assert.IsTrue(await newLoad);
            Assert.AreEqual(1, manager.CachedAssetCount);
            Assert.IsNotNull(manager.Load<TextAsset>(AssetKeys.QuestData));
            manager.ReleaseOwner(newOwner);
            Assert.AreEqual(0, manager.OwnedBatchCount);
        }
        finally { manager.Release(); }
    }
    [UnityTest] public IEnumerator MissingLabelReleasesItsHandleAndAllowsRecovery() { return FailureAsync().ToCoroutine(); }
    private static async UniTask FailureAsync()
    {
        var manager = new AddressableManager();
        object owner = new object();
        bool previous = LogAssert.ignoreFailingMessages;
        try
        {
            // Addressables and the manager both report the intentionally missing key.
            LogAssert.ignoreFailingMessages = true;
            Assert.IsFalse(await manager.PreloadAsync<TextAsset>("MissingFeatureForFailureTest", owner));
            LogAssert.ignoreFailingMessages = previous;
            Assert.AreEqual(0, manager.OwnedBatchCount);
            Assert.AreEqual(0, manager.CachedAssetCount);
            Assert.IsTrue(await manager.PreloadAsync<TextAsset>(AddressableLabels.Story, owner));
            manager.ReleaseOwner(owner);
            Assert.AreEqual(0, manager.OwnedBatchCount);
        }
        finally { LogAssert.ignoreFailingMessages = previous; manager.Release(); }
    }

}

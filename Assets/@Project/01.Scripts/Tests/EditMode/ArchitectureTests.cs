using System;
using System.Collections.Generic;
using NUnit.Framework;

public sealed class ArchitectureTests
{
    private sealed class First : IManager
    {
        public static readonly List<string> Calls = new List<string>();
        public void Init() { Calls.Add("first:init"); }
        public void Release() { Calls.Add("first:release"); }
    }
    private sealed class Second : IManager
    {
        [Singleton(typeof(First))] private First first;
        public First Dependency => first;
        public void Init() { First.Calls.Add("second:init"); }
        public void Release() { First.Calls.Add("second:release"); }
    }
    private sealed class Failing : IManager
    {
        [Singleton(typeof(First))] private First first;
        public void Init() { if (first != null) throw new InvalidOperationException("Initialization failure"); }
        public void Release() { First.Calls.Add("failing:release"); }
    }
    private sealed class CycleA : IManager
    {
        [Singleton(typeof(CycleB))] private CycleB dependency;
        public void Init() { GC.KeepAlive(dependency); }
        public void Release() { }
    }
    private sealed class CycleB : IManager
    {
        [Singleton(typeof(CycleA))] private CycleA dependency;
        public void Init() { GC.KeepAlive(dependency); }
        public void Release() { }
    }
    private sealed class Listener : IEventListener<int>
    {
        public Action<int> Callback;
        public int Calls;
        public void OnEvent(int message) { Calls++; Callback?.Invoke(message); }
    }
    private EventManager events;
    private Listener removing;
    private Listener removed;
    private Listener added;
    [SetUp] public void Setup() { First.Calls.Clear(); }
    [Test] public void DependenciesInitializeFirstAndReleaseLast()
    {
        var registry = new ServiceRegistry();
        var first = new First();
        var second = new Second();
        registry.Initialize(new IManager[] { second, first });
        Assert.AreSame(first, second.Dependency);
        registry.Release();
        CollectionAssert.AreEqual(new[] { "first:init", "second:init", "second:release", "first:release" }, First.Calls);
        registry.Release();
        Assert.AreEqual(4, First.Calls.Count);
    }
    [Test] public void DuplicateRegistrationFailsBeforeInitialization()
    {
        Assert.Throws<InvalidOperationException>(Duplicate);
        Assert.IsEmpty(First.Calls);
    }
    private void Duplicate() { DependencyResolver.Sort(new IManager[] { new First(), new First() }); }
    [Test] public void MissingDependencyFails() { Assert.Throws<InvalidOperationException>(Missing); }
    private void Missing() { DependencyResolver.Sort(new IManager[] { new Second() }); }
    [Test] public void DependencyCycleFails() { Assert.Throws<InvalidOperationException>(Cycle); }
    private void Cycle() { DependencyResolver.Sort(new IManager[] { new CycleA(), new CycleB() }); }
    [Test] public void FailedInitializationReleasesPartiallyInitializedService()
    {
        Assert.Throws<InvalidOperationException>(FailInitialization);
        CollectionAssert.AreEqual(new[] { "first:init", "failing:release", "first:release" }, First.Calls);
    }
    private void FailInitialization() { new ServiceRegistry().Initialize(new IManager[] { new Failing(), new First() }); }
    [Test] public void ListenerMutationUsesSnapshotAndSkipsRemovedReceiver()
    {
        events = new EventManager();
        removed = new Listener();
        added = new Listener();
        removing = new Listener { Callback = ModifyListeners };
        events.Subscribe<int>(removed);
        events.Subscribe<int>(removing);
        events.Subscribe<int>(removing);
        events.Publish(1);
        Assert.AreEqual(0, removed.Calls);
        Assert.AreEqual(0, added.Calls);
        Assert.AreEqual(1, removing.Calls);
        events.Publish(2);
        Assert.AreEqual(1, added.Calls);
        events.Release();
        events.Publish(3);
        Assert.AreEqual(1, added.Calls);
    }
    private void ModifyListeners(int message) { events.Unsubscribe<int>(removed); events.Subscribe<int>(added); }
    [Test] public void NestedPublishUsesIndependentSnapshot()
    {
        events = new EventManager();
        var listener = new Listener { Callback = NestedPublish };
        events.Subscribe<int>(listener);
        events.Publish(1);
        Assert.AreEqual(2, listener.Calls);
    }
    private void NestedPublish(int value) { if (value == 1) events.Publish(2); }
    [Test] public void LegacyDialogueCanContainClosingBraceInQuotedText()
    {
        string first = "{\"_questLevel\":1,\"_sceneName\":\"Professor_House\",\"_characterName\":\"Lucy\",\"_line\":\"문자 } 와 대사\"}";
        var entries = new JsonDialogueProvider().Load(first + first);
        Assert.AreEqual(2, entries.Count);
        Assert.AreEqual("문자 } 와 대사", entries[0].Line);
    }
    [Test] public void DialogueArrayPreservesExternalKeys()
    {
        var entries = new JsonDialogueProvider().Load("[{\"_questLevel\":2,\"_sceneName\":\"Professor_House\",\"_characterName\":\"Lucy\",\"_line\":\"Hello\"}]");
        Assert.AreEqual(2, entries[0].QuestLevel);
        Assert.AreEqual("Lucy", entries[0].CharacterName);
    }
    [Test] public void EmptyDialogueIsValid() { Assert.AreEqual(0, new JsonDialogueProvider().Load(" ").Count); }
    [Test] public void InvalidDialogueReportsMissingFields() { Assert.Throws<FormatException>(InvalidDialogue); }
    private void InvalidDialogue() { new JsonDialogueProvider().Load("{\"_line\":\"missing scene\"}"); }
    [Test] public void QuestAdvancesOnlyForExpectedDoorAndOnce()
    {
        var quest = new QuestProgress();
        Assert.IsFalse(quest.OpenDoor("TheSiblingsRoomDoor"));
        quest.Begin();
        Assert.IsFalse(quest.OpenDoor("TheWardrobeRoomDoor"));
        Assert.IsTrue(quest.OpenDoor("TheSiblingsRoomDoor"));
        Assert.IsFalse(quest.OpenDoor("TheSiblingsRoomDoor"));
        Assert.IsTrue(quest.OpenDoor("TheWardrobeRoomDoor"));
        quest.Begin();
        Assert.AreEqual(3, quest.Level);
    }
    [Test] public void ReplacingDialogueDiscardsPreviousQueue()
    {
        var model = new DialogueModel();
        model.Reset(new[] { "old", "stale" });
        model.Reset(new[] { "new" });
        Assert.IsTrue(model.TryNext(out var line));
        Assert.AreEqual("new", line);
        Assert.IsFalse(model.TryNext(out line));
    }
}

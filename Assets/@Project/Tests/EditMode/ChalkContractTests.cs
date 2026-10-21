using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class ChalkContractTests
{
    private ServiceRegistry registry;
    private GameStateManager states;
    private EventManager events;
    private sealed class Reentrant : IListener<GameStateEvent>
    {
        public GameStateManager States;
        public readonly List<GameStateType> Observed = new List<GameStateType>();
        public void OnEvent(GameStateEvent eventType, object param = null)
        {
            var state = (StateChanged)param;
            Observed.Add(state.Current);
            if (state.Current == GameStateType.Dialogue) States.Push(GameStateType.Settings);
            Assert.AreEqual(state.Current, States.Current, "Reentrant changes must wait for notification completion.");
        }
    }
    private sealed class Broken : IManager
    {
        public void Init() { throw new InvalidOperationException("original initialization failure"); }
        public void Release() { throw new InvalidOperationException("cleanup failure"); }
    }
    [SetUp] public void Setup()
    {
        registry = new ServiceRegistry();
        registry.Initialize(new IManager[] { new InputManager(), new EventManager(), new GameStateManager() });
        states = registry.Get<GameStateManager>();
        events = registry.Get<EventManager>();
        states.RegisterSceneStates();
        states.Change(GameStateType.Exploration);
    }
    [TearDown] public void Cleanup() { registry.Release(); }
    [Test] public void ClosingCoveredOverlayDoesNotRestoreItLater()
    {
        states.Push(GameStateType.Dialogue);
        states.Push(GameStateType.Settings);
        states.Pop(GameStateType.Dialogue);
        Assert.AreEqual(GameStateType.Settings, states.Current);
        states.Pop(GameStateType.Settings);
        Assert.AreEqual(GameStateType.Exploration, states.Current);
        Assert.IsTrue(registry.Get<InputManager>().AllowsInteraction);
    }
    [Test] public void ReentrantStateChangePublishesInTransitionOrder()
    {
        var listener = new Reentrant { States = states };
        events.AddListener<GameStateEvent>(listener);
        states.Push(GameStateType.Dialogue);
        CollectionAssert.AreEqual(new[] { GameStateType.Dialogue, GameStateType.Settings }, listener.Observed);
        Assert.AreEqual(GameStateType.Settings, states.Current);
    }
    [Test] public void RollbackPreservesOriginalAndCleanupFailures()
    {
        AggregateException failure = Assert.Throws<AggregateException>(InitializeBroken);
        Assert.AreEqual(2, failure.InnerExceptions.Count);
        StringAssert.Contains("original", failure.InnerExceptions[0].Message);
        StringAssert.Contains("cleanup", failure.InnerExceptions[1].Message);
    }
    private static void InitializeBroken() { new ServiceRegistry().Initialize(new IManager[] { new Broken() }); }
    private sealed class MemoryDialogueProvider : IDialogueProvider
    {
        private readonly string line;
        public MemoryDialogueProvider(string line) { this.line = line; }
        public UniTask<IReadOnlyList<CharacterLineClass>> LoadAsync(object owner, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var entries = new JsonDialogueProvider().Load("[{\"_questLevel\":1,\"_sceneName\":\"Test\",\"_characterName\":\"Lucy\",\"_line\":\"" + line + "\"}]");
            return UniTask.FromResult(entries);
        }
    }
    [Test] public void ReplacingProviderInvalidatesCachedDialogue()
    {
        var data = new DataManager();
        data.SetDialogueProvider(new MemoryDialogueProvider("first"));
        data.LoadDialogueAsync(this, CancellationToken.None).GetAwaiter().GetResult();
        Assert.AreEqual("first", data.GetDialogue("Test")[0].Line);
        data.SetDialogueProvider(new MemoryDialogueProvider("second"));
        data.LoadDialogueAsync(this, CancellationToken.None).GetAwaiter().GetResult();
        Assert.AreEqual("second", data.GetDialogue("Test")[0].Line);
        data.Release();
    }

}

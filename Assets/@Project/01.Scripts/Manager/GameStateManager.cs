using System;
using System.Collections.Generic;

public sealed class GameStateManager : Singleton<GameStateManager>
{
    [Singleton(typeof(InputManager))] private InputManager input;
    [Singleton(typeof(EventManager))] private EventManager events;
    private readonly Dictionary<GameStateType, IGameState> states = new Dictionary<GameStateType, IGameState>();
    private readonly Stack<GameStateType> overlays = new Stack<GameStateType>();
    private IGameState current;
    public GameStateType Current => current == null ? GameStateType.Title : current.Type;
    public override void Init()
    {
        foreach (GameStateType type in Enum.GetValues(typeof(GameStateType))) states.Add(type, new GameState(type, input));
        Change(GameStateType.Title);
    }
    public void Change(GameStateType type) { overlays.Clear(); Transition(type); }
    public void Push(GameStateType type)
    {
        if (Current == type || overlays.Contains(type)) return;
        overlays.Push(Current);
        Transition(type);
    }
    public void Pop(GameStateType owner)
    {
        if (Current != owner || overlays.Count == 0) return;
        Transition(overlays.Pop());
    }
    private void Transition(GameStateType type)
    {
        var previous = Current;
        current?.Exit();
        current = states[type];
        current.Enter();
        events.Publish(new StateChanged(previous, type));
    }
    public override void Release()
    {
        current?.Exit();
        current = null;
        overlays.Clear();
        states.Clear();
    }
}

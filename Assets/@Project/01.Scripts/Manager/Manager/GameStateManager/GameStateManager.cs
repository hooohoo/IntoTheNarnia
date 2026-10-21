using System;
using System.Collections.Generic;

public sealed class GameStateManager : Singleton<GameStateManager>
{
    private enum CommandKind { Change, Push, Pop }
    private readonly struct Command
    {
        public readonly CommandKind Kind;
        public readonly GameStateType Type;
        public Command(CommandKind kind, GameStateType type) { Kind = kind; Type = type; }
    }
    [Singleton(typeof(InputManager))] private InputManager input;
    [Singleton(typeof(EventManager))] private EventManager events;
    private readonly Dictionary<GameStateType, IGameState> states = new Dictionary<GameStateType, IGameState>();
    private readonly List<GameStateType> stack = new List<GameStateType>();
    private readonly Queue<Command> pending = new Queue<Command>();
    private IGameState current;
    private bool transitioning;
    public GameStateType Current => current == null ? GameStateType.Title : current.Type;
    public void RegisterState(IGameState state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (states.ContainsKey(state.Type)) throw new InvalidOperationException("State already registered: " + state.Type);
        states.Add(state.Type, state);
    }
    public bool ContainsState(GameStateType type) { return stack.Contains(type); }
    public bool HasState(GameStateType type) { return states.ContainsKey(type); }
    public void RegisterSceneStates()
    {
        ClearAllStates();
        foreach (GameStateType type in Enum.GetValues(typeof(GameStateType))) RegisterState(new GameState(type, input));
    }
    public void Change(GameStateType type) { Submit(new Command(CommandKind.Change, type)); }
    public void ChangeState(GameStateType type) { Change(type); }
    public void Push(GameStateType type) { Submit(new Command(CommandKind.Push, type)); }
    public void Pop(GameStateType owner) { Submit(new Command(CommandKind.Pop, owner)); }
    private void Submit(Command command)
    {
        if (!states.ContainsKey(command.Type)) throw new InvalidOperationException("Scene did not register state: " + command.Type);
        pending.Enqueue(command);
        if (transitioning) return;
        transitioning = true;
        try
        {
            while (pending.Count > 0)
            {
                Command next = pending.Dequeue();
                switch (next.Kind)
                {
                    case CommandKind.Change:
                        stack.Clear();
                        stack.Add(next.Type);
                        break;
                    case CommandKind.Push:
                        if (stack.Contains(next.Type)) continue;
                        if (stack.Count == 0) stack.Add(Current);
                        stack.Add(next.Type);
                        break;
                    case CommandKind.Pop:
                        int index = stack.LastIndexOf(next.Type);
                        if (index <= 0) continue;
                        stack.RemoveAt(index);
                        break;
                }
                Transition(stack[stack.Count - 1]);
            }
        }
        finally { transitioning = false; pending.Clear(); }
    }
    private void Transition(GameStateType type)
    {
        if (current != null && current.Type == type) return;
        GameStateType previous = Current;
        current?.Exit();
        current = states[type];
        current.Enter();
        events.PostNotification(GameStateEvent.Changed, new StateChanged(previous, type));
    }
    public void ClearAllStates()
    {
        if (transitioning) throw new InvalidOperationException("Cannot replace scene states during a transition.");
        current?.Exit();
        current = null;
        states.Clear();
        stack.Clear();
        pending.Clear();
    }
    public override void Release() { ClearAllStates(); }
}

public readonly struct StateChanged
{
    public GameStateType Previous { get; }
    public GameStateType Current { get; }
    public StateChanged(GameStateType previous, GameStateType current) { Previous = previous; Current = current; }
}

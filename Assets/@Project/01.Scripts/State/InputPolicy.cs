public sealed class InputPolicy
{
    public bool AllowsMovement { get; }
    public bool AllowsCamera { get; }
    public bool AllowsInteraction { get; }
    public InputPolicy(bool movement, bool camera, bool interaction)
    {
        AllowsMovement = movement;
        AllowsCamera = camera;
        AllowsInteraction = interaction;
    }
}

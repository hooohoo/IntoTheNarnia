public readonly struct DoorChanged
{
    public DoorController Door { get; }
    public bool IsOpen { get; }
    public DoorChanged(DoorController door, bool isOpen) { Door = door; IsOpen = isOpen; }
}

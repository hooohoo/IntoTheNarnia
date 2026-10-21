public sealed class QuestProgress
{
    public int Level { get; private set; }
    public void Begin() { if (Level == 0) Level = 1; }
    public bool OpenDoor(string name)
    {
        if (Level == 1 && name == nameof(Define.ObjectName.TheSiblingsRoomDoor)) { Level = 2; return true; }
        if (Level == 2 && name == nameof(Define.ObjectName.TheWardrobeRoomDoor)) { Level = 3; return true; }
        return false;
    }
}

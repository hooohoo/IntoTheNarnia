public readonly struct QuestChanged
{
    public string SceneName { get; }
    public int Level { get; }
    public QuestChanged(string sceneName, int level) { SceneName = sceneName; Level = level; }
}

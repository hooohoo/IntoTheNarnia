public sealed class Main : RuntimeBehaviour
{
    [Singleton(typeof(DataManager))] private DataManager data;
    private PlayerStat player;
    protected override void Initialize() { LoadJson(); }
    public void LoadJson() { player = data.LoadPlayer(); }
    public void Save() { data.SavePlayer(player); }
}

using UnityEngine;
using UnityEngine.Serialization;

public sealed class ProfessorHouseMapManager : RuntimeBehaviour, IListener<DialogueEvent>
{
    [FormerlySerializedAs("_theSiblingsRoomPortal"), SerializeField] private NextMapController siblingsPortal;
    [FormerlySerializedAs("_theWardrobeRoomPortal"), SerializeField] private NextMapController wardrobePortal;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(QuestManager))] private QuestManager quests;
    [Singleton(typeof(DataManager))] private DataManager data;
    [Singleton(typeof(UIManager))] private UIManager ui;
    [Singleton(typeof(EventManager))] private EventManager events;
    private bool built;
    public void Build()
    {
        EnsureInitialized();
        if (built) return;
        built = true;
        foreach (var door in SceneBinding.InScene<DoorController>(gameObject.scene))
        {
            if (door.name == nameof(Define.ObjectName.TheSiblingsRoomDoor)) objects.SiblingsDoor = door;
            if (door.name == nameof(Define.ObjectName.TheWardrobeRoomDoor)) objects.WardrobeDoor = door;
        }
        if (siblingsPortal != null) siblingsPortal.LinkedScene = Define.SceneName.TheSiblingsRoom;
        if (wardrobePortal != null) wardrobePortal.LinkedScene = Define.SceneName.TheWardrobeRoom;
        quests.Bind(this, gameObject.scene.name);
        events.AddListener<DialogueEvent>(this);
        if (quests.Level != 0) return;
        var lines = data.GetDialogue(gameObject.scene.name);
        if (lines.Count > 0) ui.ShowDialogue(lines[0].CharacterName, lines[0].Line.Split(','));
        else quests.Begin();
    }
    public void OnEvent(DialogueEvent eventType, object param = null)
    {
        var message = (DialogueCompleted)param;
        if (message.SceneName == gameObject.scene.name) quests.Begin();
    }
    // Existing inspector/debug entry point.
    public void Quest_1() { quests.Begin(); }
    public void Clear()
    {
        if (!built) return;
        built = false;
        events.RemoveListener<DialogueEvent>(this);
        quests.Unbind(this);
    }
    protected override void Release() { Clear(); }
}

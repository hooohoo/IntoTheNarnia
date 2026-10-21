using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-400)]
public sealed class ProfessorHouseMapManager : RuntimeBehaviour, IEventListener<DialogueCompleted>
{
    [FormerlySerializedAs("_theSiblingsRoomPortal"), SerializeField] private NextMapController siblingsPortal;
    [FormerlySerializedAs("_theWardrobeRoomPortal"), SerializeField] private NextMapController wardrobePortal;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    [Singleton(typeof(QuestManager))] private QuestManager quests;
    [Singleton(typeof(DataManager))] private DataManager data;
    [Singleton(typeof(UIManager))] private UIManager ui;
    [Singleton(typeof(EventManager))] private EventManager events;
    protected override void Initialize()
    {
        foreach (var door in SceneBinding.InScene<DoorController>(gameObject.scene))
        {
            if (door.name == nameof(Define.ObjectName.TheSiblingsRoomDoor)) objects.SiblingsDoor = door;
            if (door.name == nameof(Define.ObjectName.TheWardrobeRoomDoor)) objects.WardrobeDoor = door;
        }
        if (siblingsPortal != null) siblingsPortal.LinkedScene = Define.SceneName.TheSiblingsRoom;
        if (wardrobePortal != null) wardrobePortal.LinkedScene = Define.SceneName.TheWardrobeRoom;
        quests.Bind(this, gameObject.scene.name);
        events.Subscribe<DialogueCompleted>(this);
        if (quests.Level != 0) return;
        var lines = data.GetDialogue(gameObject.scene.name);
        if (lines.Count > 0) ui.ShowDialogue(lines[0].CharacterName, lines[0].Line.Split(','));
        else quests.Begin();
    }
    public void OnEvent(DialogueCompleted message)
    {
        if (message.SceneName == gameObject.scene.name) quests.Begin();
    }
    // Existing inspector/debug entry point.
    public void Quest_1() { quests.Begin(); }
    protected override void Release()
    {
        events.Unsubscribe<DialogueCompleted>(this);
        quests.Unbind(this);
    }
}

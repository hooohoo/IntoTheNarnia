using UnityEngine;

public sealed class Test_QuestBoxController : RuntimeBehaviour
{
    [Singleton(typeof(QuestManager))] private QuestManager quests;
    public void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) quests.Begin(); }
}

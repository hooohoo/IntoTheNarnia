using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public sealed class FactoryManager : Singleton<FactoryManager>
{
    private static readonly float SpawnSearchRadius = 15;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    public PlayerController CreatePlayer(Vector3 start, string character, Scene scene)
    {
        if (NavMesh.SamplePosition(start, out var hit, SpawnSearchRadius, NavMesh.AllAreas)) start = hit.position;
        var player = objects.Spawn<PlayerController>(character, start, Quaternion.identity);
        player.CharacterName = character;
        objects.SetPlayer(player);
        return player;
    }
}

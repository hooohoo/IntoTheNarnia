using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class FactoryManager : Singleton<FactoryManager>
{
    [Singleton(typeof(ResourceManager))] private ResourceManager resources;
    [Singleton(typeof(ObjectManager))] private ObjectManager objects;
    public PlayerController CreatePlayer(Vector3 start, string character, Scene scene)
    {
        Vector3 origin = start + Vector3.up * 100;
        int mask = ~LayerMask.GetMask("Player");
        if (Physics.Raycast(origin, Vector3.down, out var hit, Mathf.Infinity, mask, QueryTriggerInteraction.Ignore)) start = hit.point;
        var instance = Object.Instantiate(resources.GetPlayer(character), start, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(instance, scene);
        var player = instance.GetComponent<PlayerController>();
        if (player == null) { Object.Destroy(instance); throw new MissingComponentException("Character requires PlayerController: " + character); }
        player.CharacterName = character;
        objects.SetPlayer(player);
        return player;
    }
}

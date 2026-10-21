using UnityEngine;
using UnityEngine.Serialization;

public sealed class NextMapController : RuntimeBehaviour
{
    [FormerlySerializedAs("_linkedScene"), SerializeField] private Define.SceneName linkedScene;
    [Singleton(typeof(GameSceneManager))] private GameSceneManager scenes;
    [Singleton(typeof(InputManager))] private InputManager input;
    public Define.SceneName LinkedScene { get => linkedScene; set => linkedScene = value; }
    private void OnTriggerEnter(Collider other)
    {
        if (input.AllowsInteraction && other.CompareTag("Player")) scenes.LoadScene(linkedScene);
    }
}

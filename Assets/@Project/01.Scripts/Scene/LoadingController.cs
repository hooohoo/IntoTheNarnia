using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class LoadingController : RuntimeBehaviour, IFrameTick
{
    private static readonly string[] Backgrounds = { "NarniaLandscape", "TheWhiteWitch", "TheWardrobe" };
    [FormerlySerializedAs("_BackgroundIMG"), SerializeField] private Image backgroundImage;
    [FormerlySerializedAs("_LoadingProgressIMG"), SerializeField] private Image progressImage;
    [Singleton(typeof(ResourceManager))] private ResourceManager resources;
    [Singleton(typeof(GameSceneManager))] private GameSceneManager scenes;
    protected override void Initialize()
    {
        if (backgroundImage != null) backgroundImage.sprite = resources.GetImage(Backgrounds[Random.Range(0, Backgrounds.Length)]);
    }
    public void Tick(float deltaTime) { if (progressImage != null) progressImage.fillAmount = scenes.Progress; }
}

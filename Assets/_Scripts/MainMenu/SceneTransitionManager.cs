using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Awaitable TransitionToScene(string sceneName)
    {
        fadeImage.raycastTarget = true;

        await Fade(1f);
        await SceneManager.LoadSceneAsync(sceneName);
        await Fade(0f);

        fadeImage.raycastTarget = false;
    }

    private async Awaitable Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration));
            await Awaitable.NextFrameAsync();
        }

        fadeImage.color = new Color(0f, 0f, 0f, targetAlpha);
    }
}
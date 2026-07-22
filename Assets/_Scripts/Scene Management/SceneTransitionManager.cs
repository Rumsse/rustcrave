using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image transitionImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float wipeDuration = 0.4f;

    private RectTransform transitionRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);

        if (Instance != null)
            return;

        Instance = this;
        DontDestroyOnLoad(gameObject);
        transitionRect = transitionImage.rectTransform;
    }

    #region Transitions

    public async Awaitable FadeToScene(string sceneName)
    {
        transitionRect.anchoredPosition = Vector2.zero;
        transitionImage.raycastTarget = true;

        await Fade(1f);
        await SceneManager.LoadSceneAsync(sceneName);

        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        await Fade(0f);

        transitionImage.raycastTarget = false;
    }

    public async Awaitable WipeToScene(string sceneName, bool reverse = false)
    {
        transitionImage.raycastTarget = true;

        float slideDistance = transitionRect.rect.width;
        float startX = reverse ? -slideDistance : slideDistance;

        await WipeAndFade(startX, 0f, 1f);
        await SceneManager.LoadSceneAsync(sceneName);

        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        float endX = reverse ? slideDistance : -slideDistance;
        await WipeAndFade(0f, endX, 0f);

        transitionImage.raycastTarget = false;
    }

    #endregion

    #region Core Logic

    private async Awaitable Fade(float targetAlpha)
    {
        float startAlpha = transitionImage.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;

            Color color = transitionImage.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            transitionImage.color = color;

            await Awaitable.NextFrameAsync();
        }

        Color finalColor = transitionImage.color;
        finalColor.a = targetAlpha;
        transitionImage.color = finalColor;
    }

    private async Awaitable WipeAndFade(float startX, float targetX, float targetAlpha)
    {
        float startAlpha = transitionImage.color.a;
        float time = 0f;

        while (time < wipeDuration)
        {
            time += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(time / wipeDuration);
            float alphaProgress = 1f - Mathf.Pow(1f - progress, 3f);

            transitionRect.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, progress), transitionRect.anchoredPosition.y);

            Color color = transitionImage.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, alphaProgress);
            transitionImage.color = color;

            await Awaitable.NextFrameAsync();
        }

        transitionRect.anchoredPosition = new Vector2(targetX, transitionRect.anchoredPosition.y);

        Color finalColor = transitionImage.color;
        finalColor.a = targetAlpha;
        transitionImage.color = finalColor;
    }

    #endregion
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image transitionImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float wipeDuration = 0.4f;

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

    #region Transitions

    public async Awaitable FadeToScene(string sceneName)
    {
        transitionImage.raycastTarget = true;
        transitionImage.fillAmount = 1f;

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
        transitionImage.color = new Color(0f, 0f, 0f, 0f);
        transitionImage.fillAmount = 0f;
        transitionImage.fillOrigin = reverse ? (int)Image.OriginHorizontal.Left : (int)Image.OriginHorizontal.Right;

        await WipeAndFade(1f, 1f);
        await SceneManager.LoadSceneAsync(sceneName);

        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();

        transitionImage.fillOrigin = reverse ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
        await WipeAndFade(0f, 0f);

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
            transitionImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration));
            await Awaitable.NextFrameAsync();
        }

        transitionImage.color = new Color(0f, 0f, 0f, targetAlpha);
    }

    private async Awaitable WipeAndFade(float targetFill, float targetAlpha)
    {
        float startFill = transitionImage.fillAmount;
        float startAlpha = transitionImage.color.a;
        float time = 0f;

        while (time < wipeDuration)
        {
            time += Time.unscaledDeltaTime;
            float progress = time / wipeDuration;

            transitionImage.fillAmount = Mathf.Lerp(startFill, targetFill, progress);
            transitionImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(startAlpha, targetAlpha, progress));

            await Awaitable.NextFrameAsync();
        }

        transitionImage.fillAmount = targetFill;
        transitionImage.color = new Color(0f, 0f, 0f, targetAlpha);
    }

    #endregion
}
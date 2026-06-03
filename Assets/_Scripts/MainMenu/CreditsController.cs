using UnityEngine;

public interface ICreditsController
{
    void Play();
    void Stop();
}

public class CreditsController : MonoBehaviour, ICreditsController
{
    [SerializeField] private RectTransform textRectTransform;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float stopYPosition = 1500f;
    [SerializeField] private float startYPosition = -1000f;

    private bool isScrolling;

    private void Awake() => ResetCredits();

    private void Update()
    {
        if (!isScrolling)
            return;

        MoveTextUp();
        CheckIfFinished();
    }

    public void Play()
    {
        ResetCredits();
        isScrolling = true;
    }

    public void Stop() => isScrolling = false;

    private void MoveTextUp()
    {
        Vector2 pos = textRectTransform.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;
        textRectTransform.anchoredPosition = pos;
    }

    private void CheckIfFinished()
    {
        if (textRectTransform.anchoredPosition.y < stopYPosition)
            return;

        Debug.Log("Credits sequence looped.");
        ResetCredits();
    }

    private void ResetCredits()
    {
        Vector2 pos = textRectTransform.anchoredPosition;
        pos.y = startYPosition;
        textRectTransform.anchoredPosition = pos;
    }
}
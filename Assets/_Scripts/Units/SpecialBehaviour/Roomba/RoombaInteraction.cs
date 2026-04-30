using System.Collections;
using TMPro;
using UnityEngine;
using FMODUnity;

public class RoombaInteraction : MonoBehaviour, IRoombaInteractable
{
    [SerializeField] private TMP_Text barkText;
    [SerializeField] private float barkDuration = 2f;
    [SerializeField] private string[] barks;
    [SerializeField] private EventReference[] barksSoundsArray;

    private Coroutine hideBarkCoroutine;
    private WaitForSeconds waitDuration;

    private void Awake() => waitDuration = new WaitForSeconds(barkDuration);

    private void Start() => barkText.gameObject.SetActive(false);

    public void Interact()
    {
        if (barkText == null)
        {
            Debug.LogWarning("Bark text reference is missing.");
            return;
        }

        if (barks.Length == 0)
        {
            ShowBark("Beep!");
            return;
        }

        int randomIndex = Random.Range(0, barks.Length);

        ShowBark(barks[randomIndex]);
        PlayBarkSound(randomIndex);
    }

    private void ShowBark(string message)
    {
        barkText.text = message;
        barkText.gameObject.SetActive(true);

        if (hideBarkCoroutine != null)
        {
            StopCoroutine(hideBarkCoroutine);
        }

        hideBarkCoroutine = StartCoroutine(HideBarkRoutine());
    }

    private void PlayBarkSound(int index)
    {
        if (barksSoundsArray != null && index < barksSoundsArray.Length && !barksSoundsArray[index].IsNull)
        {
            RuntimeManager.PlayOneShot(barksSoundsArray[index], transform.position);
        }
    }

    private IEnumerator HideBarkRoutine()
    {
        yield return waitDuration;
        barkText.gameObject.SetActive(false);
    }
}
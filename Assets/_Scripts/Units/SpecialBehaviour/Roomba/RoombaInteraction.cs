using System.Collections;
using TMPro;
using UnityEngine;

public class RoombaInteraction : MonoBehaviour, IRoombaInteractable
{
    [SerializeField] private TMP_Text barkText;
    [SerializeField] private float barkDuration = 2f;
    [SerializeField] private string[] barks;

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

        string message = barks.Length > 0 ? barks[Random.Range(0, barks.Length)] : "Beep!";
        barkText.text = message;
        barkText.gameObject.SetActive(true);

        if (hideBarkCoroutine != null)
            StopCoroutine(hideBarkCoroutine);

        hideBarkCoroutine = StartCoroutine(HideBarkRoutine());
    }

    private IEnumerator HideBarkRoutine()
    {
        yield return waitDuration;
        barkText.gameObject.SetActive(false);
    }
}
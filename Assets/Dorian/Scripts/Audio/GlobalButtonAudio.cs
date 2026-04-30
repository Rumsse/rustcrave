using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FMODUnity;

[RequireComponent(typeof(UIDocument))]
public class GlobalButtonAudio : MonoBehaviour
{
    [SerializeField] private EventReference hoverSound;
    [SerializeField] private EventReference clickSound;

    private UIDocument uiDocument;
    private List<Button> allButtons = new List<Button>();

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null || uiDocument.rootVisualElement == null)
            return;

        allButtons = uiDocument.rootVisualElement.Query<Button>().ToList();

        foreach (var button in allButtons)
        {
            button.RegisterCallback<PointerEnterEvent>(PlayHoverSound);
            button.clicked += PlayClickSound;
        }
    }

    private void OnDisable()
    {
        if (allButtons == null)
            return;

        foreach (var button in allButtons)
        {
            button.UnregisterCallback<PointerEnterEvent>(PlayHoverSound);
            button.clicked -= PlayClickSound;
        }

        allButtons.Clear();
    }

    private void PlayHoverSound(PointerEnterEvent evt)
    {
        if (!hoverSound.IsNull)
            AudioManager.PlayOneShot(hoverSound);
    }

    private void PlayClickSound()
    {
        if (!clickSound.IsNull)
            AudioManager.PlayOneShot(clickSound);
    }
}
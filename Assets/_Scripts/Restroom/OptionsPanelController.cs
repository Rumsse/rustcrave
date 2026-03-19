using UnityEngine;
using UnityEngine.UIElements;

public class OptionsPanelController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] string buttonName = "btn-options";

    Button openButton;

    void OnEnable()
    {
        if (uiDocument == null)
            return;

        var root = uiDocument.rootVisualElement;
        openButton = root.Q<Button>(buttonName);

        if (openButton == null)
            return;

        openButton.clicked += OpenOptions;
    }

    void OnDisable()
    {
        if (openButton == null)
            return;

        openButton.clicked -= OpenOptions;
    }

    void OpenOptions() => optionsPanel.SetActive(true);
}
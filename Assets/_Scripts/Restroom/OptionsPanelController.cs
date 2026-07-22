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

        openButton = uiDocument.rootVisualElement.Q<Button>(buttonName);

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

    void OpenOptions()
    {
        optionsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
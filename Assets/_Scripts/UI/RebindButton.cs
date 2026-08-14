using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindButton : MonoBehaviour
{
    [SerializeField] private InputActionReference targetAction;
    [SerializeField] private int bindingIndex;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private string waitingText = "Listening...";

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private Button button;

    private void Awake() => button = GetComponent<Button>();

    private void OnEnable()
    {
        button.onClick.AddListener(StartRebinding);
        UpdateUI();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(StartRebinding);
        rebindingOperation?.Dispose();
    }

    private void StartRebinding()
    {
        button.interactable = false;
        buttonText.text = waitingText;

        targetAction.action.Disable();

        rebindingOperation = targetAction.action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse/position")
            .WithControlsExcluding("Mouse/delta")
            .OnComplete(operation => FinishRebinding())
            .OnCancel(operation => FinishRebinding())
            .Start();
    }

    private void FinishRebinding()
    {
        rebindingOperation.Dispose();
        targetAction.action.Enable();
        button.interactable = true;

        UpdateUI();
        SettingsManager.Instance.SaveInputOverrides();
    }

    private void UpdateUI()
    {
        if (!targetAction)
            return;

        buttonText.text = InputControlPath.ToHumanReadableString(
            targetAction.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}
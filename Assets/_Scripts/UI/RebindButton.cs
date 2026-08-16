using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindButton : MonoBehaviour
{
    #region Configuration

    [SerializeField] private string actionName = "Gameplay/Move";
    [SerializeField] private int bindingIndex;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private string waitingText = "Waiting...";
    [SerializeField] private bool lockToKeyboard = true;

    #endregion

    #region State

    private InputAction targetAction;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private Button button;

    #endregion

    #region Unity Lifecycle

    private void Awake() => button = GetComponent<Button>();

    private void Start()
    {
        InitializeAction();
        UpdateUI();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(StartRebinding);

        if (targetAction != null)
            UpdateUI();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(StartRebinding);
        rebindingOperation?.Dispose();
    }

    #endregion

    #region Logic

    private void InitializeAction()
    {
        if (targetAction != null)
            return;

        if (SettingsManager.Instance != null && SettingsManager.Instance.InputActions != null)
            targetAction = SettingsManager.Instance.InputActions.FindAction(actionName);
        else
            Debug.LogWarning($"[RebindButton] InputActions asset missing in SettingsManager for {gameObject.name}");
    }

    private void StartRebinding()
    {
        InitializeAction();

        if (targetAction == null)
            return;

        button.interactable = false;
        buttonText.text = waitingText;

        targetAction.Disable();

        rebindingOperation = targetAction.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse/position")
            .WithControlsExcluding("Mouse/delta");

        if (lockToKeyboard)
        {
            rebindingOperation.WithControlsExcluding("<Mouse>/leftButton")
                              .WithControlsExcluding("<Mouse>/rightButton")
                              .WithControlsExcluding("<Pointer>/press");
        }

        rebindingOperation.OnComplete(operation => FinishRebinding())
                          .OnCancel(operation => FinishRebinding())
                          .Start();
    }

    private void FinishRebinding()
    {
        rebindingOperation.Dispose();
        targetAction.Enable();
        button.interactable = true;

        UpdateUI();

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SaveInputOverrides();
    }

    private void UpdateUI()
    {
        if (targetAction == null || buttonText == null)
            return;

        buttonText.text = InputControlPath.ToHumanReadableString(
            targetAction.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    #endregion
}
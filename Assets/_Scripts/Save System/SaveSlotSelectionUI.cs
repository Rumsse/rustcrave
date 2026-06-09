using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PanelMode { Save, Load }

[Serializable]
public struct SlotUI
{
    public Button slotButton;
    public TMP_Text saveNameText;
    public TMP_Text dateText;
    public TMP_Text emptyText;
}

public class SaveSlotSelectionUI : MonoBehaviour
{
    [SerializeField] MainMenuManager mainMenuManager;
    [SerializeField] GameObject savePanel;
    [SerializeField] TMP_Text titleText;

    [Header("Slots")]
    [SerializeField] SlotUI autosaveSlot;
    [SerializeField] List<SlotUI> manualSlots;

    [Header("Confirmation Panel")]
    [SerializeField] GameObject confirmationPanel;
    [SerializeField] TMP_Text confirmationMessage;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    PanelMode currentMode;
    int pendingSlotIndex = -1;

    #region Unity Lifecycle

    void OnEnable()
    {
        yesButton.onClick.AddListener(ExecutePendingAction);
        noButton.onClick.AddListener(CloseConfirmation);
    }

    void OnDisable()
    {
        yesButton.onClick.RemoveListener(ExecutePendingAction);
        noButton.onClick.RemoveListener(CloseConfirmation);
    }

    #endregion

    #region UI Triggers

    public void OpenForSave() => SetupPanel("Save Game", PanelMode.Save);

    public void OpenForLoad() => SetupPanel("Load Game", PanelMode.Load);

    public void ClosePanel() => savePanel.SetActive(false);

    public void CloseConfirmation() => confirmationPanel.SetActive(false);

    #endregion

    #region Logic

    void SetupPanel(string title, PanelMode mode)
    {
        currentMode = mode;
        titleText.text = title;

        savePanel.SetActive(true);
        confirmationPanel.SetActive(false);

        RefreshSlots();
    }

    void RefreshSlots()
    {
        bool hasAutosave = SaveManager.Instance.HasSaveFile(0);
        autosaveSlot.emptyText.gameObject.SetActive(!hasAutosave);
        autosaveSlot.saveNameText.gameObject.SetActive(hasAutosave);
        autosaveSlot.dateText.gameObject.SetActive(hasAutosave);

        if (hasAutosave)
        {
            autosaveSlot.saveNameText.text = "Autosave";
            autosaveSlot.dateText.text = SaveManager.Instance.GetSlotDate(0);
        }

        autosaveSlot.slotButton.interactable = currentMode == PanelMode.Load && hasAutosave;
        autosaveSlot.slotButton.onClick.RemoveAllListeners();
        autosaveSlot.slotButton.onClick.AddListener(() => OnSlotClicked(0));

        for (int i = 0; i < manualSlots.Count; i++)
        {
            int slotIndex = i + 1;
            bool hasSave = SaveManager.Instance.HasSaveFile(slotIndex);
            var slot = manualSlots[i];

            slot.emptyText.gameObject.SetActive(!hasSave);
            slot.saveNameText.gameObject.SetActive(hasSave);
            slot.dateText.gameObject.SetActive(hasSave);

            if (hasSave)
            {
                slot.saveNameText.text = $"Save {slotIndex}";
                slot.dateText.text = SaveManager.Instance.GetSlotDate(slotIndex);
            }

            slot.slotButton.interactable = currentMode != PanelMode.Load || hasSave;

            slot.slotButton.onClick.RemoveAllListeners();
            slot.slotButton.onClick.AddListener(() => OnSlotClicked(slotIndex));
        }
    }

    void OnSlotClicked(int slotIndex)
    {
        bool hasSave = SaveManager.Instance.HasSaveFile(slotIndex);

        if (currentMode == PanelMode.Load)
        {
            mainMenuManager.LoadMissionFromSlot(slotIndex);
            return;
        }

        if (!hasSave)
        {
            pendingSlotIndex = slotIndex;
            ExecutePendingAction();
            return;
        }

        pendingSlotIndex = slotIndex;
        confirmationMessage.text = $"Are you sure you want to overwrite Save {slotIndex}?\nAll previous progress will be lost.";
        confirmationPanel.SetActive(true);
    }

    void ExecutePendingAction()
    {
        confirmationPanel.SetActive(false);

        if (pendingSlotIndex == -1)
            return;

        SaveManager.Instance.SetCurrentSlot(pendingSlotIndex);
        SaveManager.Instance.SaveGame();
        RefreshSlots();

        pendingSlotIndex = -1;
    }

    #endregion
}
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GuideCategory
{
    Controls,
    Restroom,
    Boss,
    Items,
    Enemies,
    Mechanics
}

[System.Serializable]
public class GuidePage
{
    public string title;
    [TextArea(3, 10)]
    public string explanationText;
}

[System.Serializable]
public class GuideEntry
{
    public GuideCategory category;
    public List<GuidePage> pages;
}

public class GuidePanelsManager : MonoBehaviour
{
    [Header("Main Menu UI")]
    [SerializeField] private GameObject categoriesPanel;
    [SerializeField] private Button closeEntireGuideBtn;

    [Header("Paged Guide UI")]
    [SerializeField] private GameObject pagedGuidePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text explanationText;
    [SerializeField] private TMP_Text pageNumberText;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button backToCategoriesBtn;

    [Header("Data & Audio")]
    [SerializeField] private List<GuideEntry> guideEntries;
    [SerializeField] private EventReference interactionSound;

    private GuideEntry currentGuide;
    private int currentPageIndex;

    #region Unity Lifecycle

    private void OnEnable() => OpenMainMenu();

    private void Start()
    {
        if (previousBtn != null)
            previousBtn.onClick.AddListener(PrevPage);

        if (nextBtn != null)
            nextBtn.onClick.AddListener(NextPage);

        if (backToCategoriesBtn != null)
            backToCategoriesBtn.onClick.AddListener(OpenMainMenu);

        if (closeEntireGuideBtn != null)
            closeEntireGuideBtn.onClick.AddListener(CloseAll);
    }

    #endregion

    #region Flow Logic

    public void OpenMainMenu()
    {
        if (pagedGuidePanel != null)
            pagedGuidePanel.SetActive(false);

        if (categoriesPanel != null)
            categoriesPanel.SetActive(true);

        AudioManager.PlayOneShot(interactionSound);
    }

    public void OpenCategory(int categoryIndex)
    {
        GuideCategory category = (GuideCategory)categoryIndex;
        currentGuide = guideEntries.FirstOrDefault(g => g.category == category);

        if (currentGuide == null || currentGuide.pages == null || currentGuide.pages.Count == 0)
        {
            Debug.LogWarning($"GuideData not found or empty for category: {category}");
            return;
        }

        currentPageIndex = 0;

        if (categoriesPanel != null)
            categoriesPanel.SetActive(false);

        if (pagedGuidePanel != null)
            pagedGuidePanel.SetActive(true);

        AudioManager.PlayOneShot(interactionSound);
        UpdateUI();
    }

    public void CloseAll()
    {
        if (categoriesPanel != null)
            categoriesPanel.SetActive(false);

        if (pagedGuidePanel != null)
            pagedGuidePanel.SetActive(false);

        currentGuide = null;
        AudioManager.PlayOneShot(interactionSound);
        gameObject.SetActive(false);
    }

    #endregion

    #region UI Logic

    private void UpdateUI()
    {
        if (currentGuide == null)
            return;

        GuidePage currentPage = currentGuide.pages[currentPageIndex];

        if (titleText != null)
            titleText.text = currentPage.title;

        if (explanationText != null)
            explanationText.text = currentPage.explanationText;

        if (pageNumberText != null)
            pageNumberText.text = $"{currentPageIndex + 1} / {currentGuide.pages.Count}";

        if (previousBtn != null)
            previousBtn.gameObject.SetActive(currentPageIndex > 0);

        if (nextBtn != null)
            nextBtn.gameObject.SetActive(currentPageIndex < currentGuide.pages.Count - 1);
    }

    private void PrevPage()
    {
        if (currentPageIndex <= 0)
            return;

        currentPageIndex--;
        AudioManager.PlayOneShot(interactionSound);
        UpdateUI();
    }

    private void NextPage()
    {
        if (currentPageIndex >= currentGuide.pages.Count - 1)
            return;

        currentPageIndex++;
        AudioManager.PlayOneShot(interactionSound);
        UpdateUI();
    }

    #endregion
}
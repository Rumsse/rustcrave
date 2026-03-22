using UnityEngine;
using UnityEngine.SceneManagement;

public class GuidePanelsManager : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject restroomPanel;
    [SerializeField] private GameObject bossPanel;

    private void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (restroomPanel != null)
            restroomPanel.SetActive(false);
        if (bossPanel != null)
            bossPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    public void OpenRestroom()
    {
        if (restroomPanel != null)
            restroomPanel.SetActive(true);
    }

    public void CloseRestroom()
    {
        if (restroomPanel != null)
            restroomPanel.SetActive(false);
    }

    public void OpenBoss()
    {
        if (bossPanel != null)
            bossPanel.SetActive(true);
    }

    public void CloseBoss()
    {
        if (bossPanel != null)
            bossPanel.SetActive(false);
    }

}

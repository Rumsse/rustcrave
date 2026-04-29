using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class GuidePanelsManager : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject restroomPanel;
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private EventReference interactionSound;

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
            AudioManager.PlayOneShot(interactionSound);
    }

    public void CloseControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
            AudioManager.PlayOneShot(interactionSound);
    }

    public void OpenRestroom()
    {
        if (restroomPanel != null)
            restroomPanel.SetActive(true);
            AudioManager.PlayOneShot(interactionSound);
    }

    public void CloseRestroom()
    {
        if (restroomPanel != null)
            restroomPanel.SetActive(false);
            AudioManager.PlayOneShot(interactionSound);
    }

    public void OpenBoss()
    {
        if (bossPanel != null)
            bossPanel.SetActive(true);
            AudioManager.PlayOneShot(interactionSound);
    }

    public void CloseBoss()
    {
        if (bossPanel != null)
            bossPanel.SetActive(false);
            AudioManager.PlayOneShot(interactionSound);
    }

}

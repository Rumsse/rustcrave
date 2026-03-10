using UnityEngine;
using UnityEngine.SceneManagement;

public class TunnelEnd : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Collider triggerCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("TunnelEnd: Scene to load is not set!");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;
    }
#endif
}
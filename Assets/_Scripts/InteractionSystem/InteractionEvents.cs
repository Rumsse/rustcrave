using UnityEngine;
using UnityEngine.Events;

public class InteractionEvents : MonoBehaviour
{
    public UnityEvent onInteract;
    
    private IInteractable _interactable;

    private void Awake()
    {
        _interactable = GetComponent<IInteractable>();
    }

    private void OnEnable()
    {
        _interactable.onInteract += OnInteract;
    }

    private void OnDisable()
    {
        _interactable.onInteract -= OnInteract;
    }

    private void OnInteract()
    {
        onInteract.Invoke();
    }
}

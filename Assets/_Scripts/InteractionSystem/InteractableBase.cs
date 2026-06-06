using System;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private float _interactionTime = 3f;

    public event Action onInteract;

    public float InteractionTime => _interactionTime;

    public void Interact()
    {
        if (!CanInteract())
            return;
        
        onInteract?.Invoke();
        OnInteract();
    }

    public abstract void OnInteract();

    public virtual bool CanInteract() => true;
}

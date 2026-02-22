using System;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    public event Action onInteract;

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

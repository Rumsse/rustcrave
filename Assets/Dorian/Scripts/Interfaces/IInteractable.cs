using System;
using UnityEngine;

public interface IInteractable
{
    public event Action onInteract;
    
    public void Interact();
}

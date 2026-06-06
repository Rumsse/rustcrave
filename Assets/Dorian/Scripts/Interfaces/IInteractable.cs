using System;
using UnityEngine;

public interface IInteractable
{
    public event Action onInteract;

    float InteractionTime { get; }

    public void Interact();
}

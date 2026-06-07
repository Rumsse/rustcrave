using System;

public interface IInteractable
{
    public event Action onInteract;

    float InteractionTime { get; }

    public void Interact();
    void PlayEffect();
    void StopEffect();
}
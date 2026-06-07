using System;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private float _interactionTime = 1f;
    [SerializeField] private ParticleSystem _interactionEffect;

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

    public virtual void PlayEffect()
    {
        if (_interactionEffect != null)
        {
            Debug.Log("Starting interaction effect on: " + gameObject.name);
            _interactionEffect.Play(true);
        }
    }

    public virtual void StopEffect()
    {
        if (_interactionEffect != null)
            _interactionEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
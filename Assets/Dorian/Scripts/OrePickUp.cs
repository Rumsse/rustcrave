using System;
using UnityEngine;

public class OrePickUp : MonoBehaviour, IInteractable
{
    public ItemSO item;
    public int amount = 1;

    public event Action onInteract;

    public void Interact()
    {
        onInteract?.Invoke();
    }
}
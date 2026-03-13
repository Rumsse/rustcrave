using System;
using UnityEngine;

public class OrePickUp : MonoBehaviour, IInteractable
{
    public ItemSO item;
    public int oreValueAmount;

    public event Action onInteract;

    public void Interact()
    {
        onInteract?.Invoke();
    }
}
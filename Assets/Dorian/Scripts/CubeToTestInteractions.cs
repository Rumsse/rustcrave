using System;
using UnityEngine;

public class CubeToTestInteractions : MonoBehaviour, IInteractable
{
    public event Action onInteract;

    public void Interact()
    {
        Debug.Log("Interaction");
    }
}

using System;
using UnityEngine;

[Obsolete]
public class CubeToTestInteractions : MonoBehaviour, IInteractable
{
    public event Action onInteract;

    public void Interact()
    {
        Debug.Log("Interaction");
    }
}

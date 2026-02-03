using UnityEngine;

public class CubeToTestInteractions : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interaction");
    }
}

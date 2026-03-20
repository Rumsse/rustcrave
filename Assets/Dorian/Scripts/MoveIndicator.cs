using UnityEngine;

public class MoveIndicator : MonoBehaviour
{
    [SerializeField] private float lifetime;

    private void Awake()
    {
        Destroy(gameObject, lifetime);
    }
}

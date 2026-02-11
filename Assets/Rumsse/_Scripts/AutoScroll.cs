using UnityEngine;

public class AutoScroll : MonoBehaviour
{
    [SerializeField] private float speed = 2.0f;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
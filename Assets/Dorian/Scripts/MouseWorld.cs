using UnityEngine;

public class MouseWorld : MonoBehaviour
{

    private static MouseWorld Instance;

    [SerializeField] private LayerMask mouseWorldLayerMask;


    private void Awake()
    {
        Instance = this;
    }


    public static Vector3 GetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, Instance.mouseWorldLayerMask);
        return raycastHit.point;
    }
}

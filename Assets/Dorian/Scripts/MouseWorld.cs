using UnityEngine;

public class MouseWorld : MonoBehaviour
{

    private static MouseWorld Instance;

    [SerializeField] private LayerMask mouseWorldLayerMask;


    private void Awake()
    {
        Instance = this;
    }


    public static bool TryGetPosition(out Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Instance.mouseWorldLayerMask))
        {
            position = hit.point;
            return true;
        }

        position = default;
        return false;
    }

}

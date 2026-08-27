using UnityEngine;
using UnityEngine.EventSystems;

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
        position = default;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, Instance.mouseWorldLayerMask))
            return false;

        position = hit.point;
        return true;
    }

}

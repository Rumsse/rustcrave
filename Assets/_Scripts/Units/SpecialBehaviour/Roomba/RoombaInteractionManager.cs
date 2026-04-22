using UnityEngine;

public class RoombaInteractionManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem clickParticles;
    private Camera mainCamera;

    private void Awake() => mainCamera = Camera.main;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (!hit.collider.CompareTag("Roomba"))
            return;

        //PlayParticles(hit.point);

        if (hit.collider.TryGetComponent(out IRoombaInteractable interactable))
            interactable.Interact();
    }

    private void PlayParticles(Vector3 position)
    {
        if (clickParticles == null)
        {
            Debug.LogWarning("Particle system is missing.");
            return;
        }

        clickParticles.transform.position = position;
        clickParticles.Play();
    }
}
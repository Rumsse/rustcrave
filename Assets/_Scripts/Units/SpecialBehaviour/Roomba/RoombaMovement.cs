using FMODUnity;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class RoombaMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitAtPoint = 0f;
    [SerializeField] private EventReference moveSound; 
    

    private FMOD.Studio.EventInstance moveSoundInstance;
    private NavMeshAgent agent;
    private int currentIndex = 0;

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        currentIndex = 0;
        StartCoroutine(PatrolLoop());
        PlaySound();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        StopSound();
    }

    private void OnDestroy()
    {
        StopSound();
    }

    private void PlaySound()
    {
        if (!moveSound.IsNull)
        {
            moveSoundInstance = RuntimeManager.CreateInstance(moveSound);
            RuntimeManager.AttachInstanceToGameObject(moveSoundInstance, gameObject);
            moveSoundInstance.start();
        }
    }

    private void StopSound()
    {
        if (moveSoundInstance.isValid())
        {
            moveSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            moveSoundInstance.release();
            moveSoundInstance = default;
        }
    }


    System.Collections.IEnumerator PatrolLoop()
    {
        while (true)
        {
            agent.SetDestination(waypoints[currentIndex].position);

            yield return null;

            yield return new WaitUntil(() =>
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.1f);

            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }
}
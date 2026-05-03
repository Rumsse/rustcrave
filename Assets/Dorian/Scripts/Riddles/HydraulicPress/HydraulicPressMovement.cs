using UnityEngine;
using System.Collections;

public class HydraulicPressMovement : MonoBehaviour
{
    [SerializeField] private Transform pressTransform;
    [SerializeField] private Transform topPosition;
    [SerializeField] private Transform bottomPosition;
    [SerializeField] private float smashSpeed = 20f;
    [SerializeField] private float retractSpeed = 3f;
    [SerializeField] private float waitTimeAtTop = 4f;
    [SerializeField] private float waitTimeAtBottom = 0.5f;

    private void Start()
    {
        StartCoroutine(PressRoutine());
    }

    private IEnumerator PressRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTimeAtTop);
            yield return StartCoroutine(MovePress(bottomPosition.position, smashSpeed));

            yield return new WaitForSeconds(waitTimeAtBottom);
            yield return StartCoroutine(MovePress(topPosition.position, retractSpeed));
        }
    }

    private IEnumerator MovePress(Vector3 targetPosition, float speed)
    {
        while (pressTransform.position != targetPosition)
        {
            pressTransform.position = Vector3.MoveTowards(pressTransform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }
}
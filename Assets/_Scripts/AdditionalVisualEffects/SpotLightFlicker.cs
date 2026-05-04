using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SpotLightFlicker : MonoBehaviour
{
    [SerializeField] private float normalIntensity = 50f;
    [SerializeField] private float minGlitchIntensity = 0f;
    [SerializeField] private float maxGlitchIntensity = 15f;
    [SerializeField] private Vector2 timeBetweenGlitches = new Vector2(3f, 12f);
    [SerializeField] private Vector2 glitchDuration = new Vector2(0.1f, 0.4f);
    [SerializeField] private float flickerSpeed = 25f;

    private Light spotLight;
    private Coroutine flickerCoroutine;

    private void Awake() => spotLight = GetComponent<Light>();

    private void Start() => flickerCoroutine = StartCoroutine(FlickerRoutine());

    public void TurnOff()
    {
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);

        spotLight.enabled = false;
    }

    private IEnumerator FlickerRoutine()
    {
        var waitFlickerTick = new WaitForSeconds(1f / flickerSpeed);

        while (true)
        {
            spotLight.intensity = normalIntensity;
            yield return new WaitForSeconds(Random.Range(timeBetweenGlitches.x, timeBetweenGlitches.y));

            float glitchEndTime = Time.time + Random.Range(glitchDuration.x, glitchDuration.y);

            while (Time.time < glitchEndTime)
            {
                spotLight.intensity = Random.Range(minGlitchIntensity, maxGlitchIntensity);
                yield return waitFlickerTick;
            }
        }
    }
}
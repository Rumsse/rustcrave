using UnityEngine;

public class SteamPipeController : MonoBehaviour
{
    [SerializeField] private float activeDuration = 2f;
    [SerializeField] private float inactiveDuration = 3f;
    [SerializeField] private GameObject steamZone;
    [SerializeField] private ParticleSystem steamParticles;

    private float timer;
    private bool isSteamActive;

    private void Start()
    {
        SetSteamState(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float currentPhaseDuration = isSteamActive ? activeDuration : inactiveDuration;

        if (timer >= currentPhaseDuration)
        {
            timer = 0f;
            SetSteamState(!isSteamActive);
        }
    }

    private void SetSteamState(bool isActive)
    {
        isSteamActive = isActive;

        if (steamZone != null) steamZone.SetActive(isActive);

        if (steamParticles != null)
        {
            if (isActive) steamParticles.Play();
            else steamParticles.Stop();
        }
    }
}
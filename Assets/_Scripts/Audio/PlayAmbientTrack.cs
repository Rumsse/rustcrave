using System.Collections;
using FMODUnity;
using UnityEngine;

public class PlayAmbientTrack : MonoBehaviour
{
    [SerializeField] private bool _playOnStart;
    [SerializeField] private EventReference _eventReference;

    private IEnumerator Start()
    {
        if (_playOnStart)
        {
            yield return StartCoroutine(WaitAndPlay());
        }
    }

    public void PlayWhenReady()
    {
        StartCoroutine(WaitAndPlay());
    }

    private IEnumerator WaitAndPlay()
    {
        while (!RuntimeManager.IsInitialized || !RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        PlayAmbient();
    }

    public void PlayAmbient() => SoundtrackPlayer.Instance.PlayAmbientTrack(_eventReference);
}
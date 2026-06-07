using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundtrackPlayer : MonoBehaviour
{
    public static SoundtrackPlayer Instance { get; private set; }

    private EventInstance _currentTrack;
    private EventInstance _currentAmbientTrack;

    private EventReference _currentTrackRef;
    private EventReference _currentAmbientRef;

    public void StopTrack() => StopEvent(ref _currentTrack);
    public void StopAmbientTrack() => StopEvent(ref _currentAmbientTrack);

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayTrack(EventReference eventReference)
    {
        PlayEvent(ref _currentTrackRef, eventReference, ref _currentTrack);
    }

    public void PlayAmbientTrack(EventReference eventReference)
    {
        PlayEvent(ref _currentAmbientRef, eventReference, ref _currentAmbientTrack);
    }

    private void PlayEvent(ref EventReference currentRef, EventReference newRef, ref EventInstance instance)
    {
        if (currentRef.Guid == newRef.Guid && IsPlaying(instance))
        {
            return;
        }

        StopEvent(ref instance);

        currentRef = newRef;
        instance = RuntimeManager.CreateInstance(newRef);
        instance.start();
    }

    private void StopEvent(ref EventInstance instance)
    {
        if (instance.isValid())
        {
            instance.stop(STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }

    private bool IsPlaying(EventInstance instance)
    {
        if (!instance.isValid())
        {
            return false;
        }

        instance.getPlaybackState(out PLAYBACK_STATE state);
        return state != PLAYBACK_STATE.STOPPED;
    }


}
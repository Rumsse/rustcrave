using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class SoundtrackPlayer : MonoBehaviour
{
    public static SoundtrackPlayer Instance {get; private set;}
    
    public EventInstance CurrentTrack { get; private set; }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void PlayTrack(EventReference eventReference)
    {
        //if (CurrentTrack.isValid())
        //{
        //    CurrentTrack.stop(STOP_MODE.ALLOWFADEOUT);
        //    CurrentTrack.release();
        //}
        
        CurrentTrack = RuntimeManager.CreateInstance(eventReference);
        CurrentTrack.start();
    }
    
    public void StopTrack() => CurrentTrack.stop(STOP_MODE.ALLOWFADEOUT);
}
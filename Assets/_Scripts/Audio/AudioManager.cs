using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public static class AudioManager
{
    public static EventInstance CreateInstance(EventReference eventReference) {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }
        
    public static EventInstance CreateInstance(EventReference eventReference, GameObject obj, Rigidbody2D rb) {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        RuntimeManager.AttachInstanceToGameObject(eventInstance, obj, rb);
        return eventInstance;
    }

    public static void PlayOneShot(EventReference eventReference)
    {
        if (eventReference.IsNull)
            return;

        RuntimeManager.PlayOneShot(eventReference);
    }

    public static void PlayOneShotByPath(string soundPath)
    {
        RuntimeManager.PlayOneShot(soundPath);
    }
}

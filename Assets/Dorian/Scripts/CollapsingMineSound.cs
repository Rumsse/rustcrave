using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class CollapsingMineSound : MonoBehaviour
{
    [SerializeField] private EventReference collapseSound;
    [SerializeField] private float delayBeforeStart;

    private EventInstance soundInstance;

    private void Start()
    {
        if (!collapseSound.IsNull)
        {
            StartCoroutine(PlaySoundWithDelay());
        }
    }

    private IEnumerator PlaySoundWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        soundInstance = RuntimeManager.CreateInstance(collapseSound);
        RuntimeManager.AttachInstanceToGameObject(soundInstance, gameObject);
        soundInstance.start();
    }

    private void OnDestroy()
    {
        if (soundInstance.isValid())
        {
            soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            soundInstance.release();
        }
    }
}
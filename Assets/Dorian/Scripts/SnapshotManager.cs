using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class SnapshotManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private EventReference bossSnapshot;
    [SerializeField] private EventReference biomSnapshot;

    private FMOD.Studio.EventInstance currentSnapshotInstance;
    private FMOD.GUID activeSnapshotGuid;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopSnapshot(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = scene.name;

        if (name == "BossTest")
        {
            PlaySnapshot(bossSnapshot);
        }
        else if (name == "Tunnels Gameplay")
        {
            PlaySnapshot(biomSnapshot);
        }
        else if (name == "new Tunel Generation Rumsse")
        {
            PlaySnapshot(biomSnapshot);
        } 
        else
        {
            StopSnapshot(false);
            activeSnapshotGuid = new FMOD.GUID();
            Debug.Log(IsPlaying(currentSnapshotInstance));
        }
    }

    bool IsPlaying(FMOD.Studio.EventInstance instance)
    {
        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    private void PlaySnapshot(EventReference snapshotRef)
    {
        if (activeSnapshotGuid.Equals(snapshotRef.Guid))
        {
            return;
        }

        StopSnapshot(false);

        if (!snapshotRef.IsNull)
        {
            currentSnapshotInstance = RuntimeManager.CreateInstance(snapshotRef);
            currentSnapshotInstance.start();
            activeSnapshotGuid = snapshotRef.Guid;
        }
    }

    private void StopSnapshot(bool immediate)
    {
        if (currentSnapshotInstance.isValid())
        {
            FMOD.Studio.STOP_MODE stopMode = immediate
                ? FMOD.Studio.STOP_MODE.IMMEDIATE
                : FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

            currentSnapshotInstance.stop(stopMode);

            currentSnapshotInstance.release();

            currentSnapshotInstance = default;
            activeSnapshotGuid = new FMOD.GUID();
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct SceneSnapshotMapping
{
    public string sceneName;
    public EventReference snapshot;
}

public class SnapshotManager : MonoBehaviour
{
    [SerializeField] private List<SceneSnapshotMapping> sceneSnapshots;

    private FMOD.Studio.EventInstance currentSnapshotInstance;
    private FMOD.GUID activeSnapshotGuid;

    private void Start()
    {
        CheckAndPlaySnapshot(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopSnapshot(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndPlaySnapshot(scene);
    }

    private void CheckAndPlaySnapshot(Scene scene)
    {
        var mapping = sceneSnapshots.FirstOrDefault(x =>
            !string.IsNullOrEmpty(x.sceneName) &&
            string.Equals(x.sceneName.Trim(), scene.name.Trim(), System.StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(mapping.sceneName) && !mapping.snapshot.IsNull)
        {
            PlaySnapshot(mapping.snapshot);
        }
        else
        {
            StopSnapshot(false);
        }
    }

    private void PlaySnapshot(EventReference snapshotRef)
    {
        if (activeSnapshotGuid.Equals(snapshotRef.Guid)) return;

        StopSnapshot(false);

        currentSnapshotInstance = RuntimeManager.CreateInstance(snapshotRef);
        currentSnapshotInstance.start();
        activeSnapshotGuid = snapshotRef.Guid;
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
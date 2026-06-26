using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct SceneSnapshotMapping
{
    public string sceneName;
    public EventReference snapshot;
}

public class SnapshotManager : MonoBehaviour
{
    public static SnapshotManager Instance { get; private set; }

    [SerializeField] private List<SceneSnapshotMapping> sceneSnapshots;

    private FMOD.Studio.EventInstance _currentSnapshotInstance;
    private FMOD.GUID _activeSnapshotGuid;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        while (!RuntimeManager.IsInitialized || !RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        CheckAndPlaySnapshot(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        if (_activeSnapshotGuid.Equals(snapshotRef.Guid)) return;

        StopSnapshot(false);

        _currentSnapshotInstance = RuntimeManager.CreateInstance(snapshotRef);
        _currentSnapshotInstance.start();
        _activeSnapshotGuid = snapshotRef.Guid;
    }

    private void StopSnapshot(bool immediate)
    {
        if (_currentSnapshotInstance.isValid())
        {
            FMOD.Studio.STOP_MODE stopMode = immediate
                ? FMOD.Studio.STOP_MODE.IMMEDIATE
                : FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

            _currentSnapshotInstance.stop(stopMode);
            _currentSnapshotInstance.release();
            _currentSnapshotInstance = default;
            _activeSnapshotGuid = new FMOD.GUID();
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class SnapshotManager : MonoBehaviour
{
    [SerializeField]
    private bool snapshotActivated = false;


    private FMOD.Studio.EventInstance outsideSnapshotInstance;
    public EventReference bossSnapshot;
    public EventReference biomSnapshot;

    void FixedUpdate()
    {
        ToggleSnapshotLogic();
    }

    private void ToggleSnapshotLogic()
    {
        Scene scene = SceneManager.GetActiveScene();
        string sceneString = scene.ToString();
        if(sceneString == "BossTest" && !snapshotActivated)
        {
            ToggleSnapshot(true);
        }
        else if(sceneString != "BossTest" && snapshotActivated)
        {
            ToggleSnapshot(false);
        }
    }

    private void ToggleSnapshot(bool activate)
    {
        if (activate)
        {
            outsideSnapshotInstance = FMODUnity.RuntimeManager.CreateInstance(bossSnapshot);
            outsideSnapshotInstance.start();
        }
        else
        {
            if (outsideSnapshotInstance.isValid())
            {
                outsideSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                outsideSnapshotInstance.release();
            }
        }
        snapshotActivated = activate;
    }
}
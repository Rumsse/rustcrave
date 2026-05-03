using FMODUnity;
using UnityEngine;

public class PlayAmbientTrack : MonoBehaviour
{
    [SerializeField] private bool _playOnStart;
    [SerializeField] private EventReference _eventReference;

    private void Start()
    {
        if (_playOnStart) PlayAmbient();
    }

    public void PlayAmbient() => SoundtrackPlayer.Instance.PlayAmbientTrack(_eventReference);
}

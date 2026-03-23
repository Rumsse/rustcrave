using FMODUnity;
using UnityEngine;

public class PlayTrack : MonoBehaviour
{
    [SerializeField] private bool _playOnStart;
    [SerializeField] private EventReference _eventReference;

    private void Start()
    {
        if(_playOnStart) Play();   
    }
    
    public void Play() => SoundtrackPlayer.Instance.PlayTrack(_eventReference);
}

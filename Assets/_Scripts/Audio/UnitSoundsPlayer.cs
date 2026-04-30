using UnityEngine;

public class UnitSoundsPlayer : MonoBehaviour
{
    [SerializeField] private StatsManager _stats;
    
    public void PlayStepSound()
    {
        //AudioManager.PlayOneShot(_stats.Sounds.walkSound);
    }
    
    public void PlayOneShotByPath()
    {
        AudioManager.PlayOneShot(_stats.Sounds.mineSound);
    }
}

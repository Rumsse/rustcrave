using UnityEngine;

public class OreToTestMining : MonoBehaviour, IMineable
{
    public void Mine()
    {
        Debug.Log("Mining");
    }
}

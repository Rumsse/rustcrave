using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private SwarmState swarmState;

    private void Start() => swarmState.Initialize();

}
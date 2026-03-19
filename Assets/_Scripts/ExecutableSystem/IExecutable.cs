using UnityEngine;

public interface IExecutable
{
    public int PhaseIndex { get; }
    void Execute();
}

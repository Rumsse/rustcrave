using TunnelSystem;
using UnityEngine;

public class TunnelSegment : MonoBehaviour
{
    [Header("Socket Configuration")]
    [SerializeField] private Transform entrySocket;
    [SerializeField] private Transform exitSocket;
    [SerializeField] private Transform exitSocketLeft;
    [SerializeField] private Transform exitSocketRight;

    [Header("Segment Info")]
    [SerializeField] private TunnelType tunnelType;
    [SerializeField] private SocketType exitSocketType = SocketType.Single;
    [SerializeField] private bool leftPathBlocked;
    [SerializeField] private bool rightPathBlocked;



    public Transform EntrySocket => entrySocket;
    public Transform ExitSocket => exitSocket;
    public Transform ExitSocketLeft => exitSocketLeft;
    public Transform ExitSocketRight => exitSocketRight;
    public TunnelType TunnelType => tunnelType;
    public SocketType ExitSocketType => exitSocketType;
    public bool LeftPathBlocked => leftPathBlocked;
    public bool RightPathBlocked => rightPathBlocked;
    public bool HasAnyBlockedPath => leftPathBlocked || rightPathBlocked;


    public Transform GetExitSocket(bool useLeftPath)
    {
        if (exitSocketType == SocketType.Single)
            return exitSocket;

        if (exitSocketType == SocketType.DoubleBoth)
            return useLeftPath ? exitSocketLeft : exitSocketRight;

        return exitSocket;
    }




#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (entrySocket != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(entrySocket.position, 0.5f);
            Gizmos.DrawLine(entrySocket.position, entrySocket.position + entrySocket.forward * 2f);
        }

        if (exitSocket != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(exitSocket.position, 0.5f);
            Gizmos.DrawLine(exitSocket.position, exitSocket.position + exitSocket.forward * 2f);
        }

        if (exitSocketLeft != null)
        {
            Gizmos.color = leftPathBlocked ? Color.gray : Color.yellow;
            Gizmos.DrawWireSphere(exitSocketLeft.position, 0.5f);
            Gizmos.DrawLine(exitSocketLeft.position, exitSocketLeft.position + exitSocketLeft.forward * 2f);
        }

        if (exitSocketRight != null)
        {
            Gizmos.color = rightPathBlocked ? Color.gray : Color.cyan;
            Gizmos.DrawWireSphere(exitSocketRight.position, 0.5f);
            Gizmos.DrawLine(exitSocketRight.position, exitSocketRight.position + exitSocketRight.forward * 2f);
        }
    }
#endif
}
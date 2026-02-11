namespace TunnelSystem
{
    public enum TunnelType
    {
        Tunnel,
        DoubleTunnel,
        ConnectionStart,
        ConnectionEnd
    }

    public enum GeneratorState
    {
        GeneratingSingleTunnel,
        GeneratingDoubleTunnel
    }

    public enum SocketType
    {
        Single,
        DoubleLeft,
        DoubleRight,
        DoubleBoth
    }
}
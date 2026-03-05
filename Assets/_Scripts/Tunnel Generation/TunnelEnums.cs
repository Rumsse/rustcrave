namespace TunnelSystem
{
    public enum TunnelType
    {
        Tunnel,
        LinkStart,
        LinkEnd
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
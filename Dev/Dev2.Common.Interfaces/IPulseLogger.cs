namespace Dev2.Common.Interfaces
{
    public interface IPulseLogger
    {
        bool Start();
        int Interval { get; }
    }
}
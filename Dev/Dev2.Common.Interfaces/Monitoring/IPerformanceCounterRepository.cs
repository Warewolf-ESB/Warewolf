namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IPerformanceCounterRepository
    {
        IPerformanceCounterTo Counters { get; }
        void Save(IPerformanceCounterTo toSave);

        void ResetCounters();
    }
}
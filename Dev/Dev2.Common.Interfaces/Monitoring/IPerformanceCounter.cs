using System.Diagnostics;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IPerformanceCounter
    {
        void Increment();
        void IncrementBy(int ticks);
        void Decrement();
        string Category { get;}
        string Name { get; }
        CounterCreationData CreationData();
        bool IsActive { get; set; }
    }
}

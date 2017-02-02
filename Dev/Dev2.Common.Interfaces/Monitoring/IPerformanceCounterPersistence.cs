using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IPerformanceCounterPersistence
    {
        //void Save(IList<IPerformanceCounter> counters, string fileName);
        void Save(IList<IPerformanceCounter> counters);
        IList<IPerformanceCounter> LoadOrCreate();

        void Save(IList<IPerformanceCounter> _resourceCounters, string p);

        void Save(IList<IResourcePerformanceCounter> resourceCounters);
    }
}
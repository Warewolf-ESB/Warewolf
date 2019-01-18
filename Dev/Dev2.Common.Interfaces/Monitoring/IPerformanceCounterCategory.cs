using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces.Monitoring
{
    public interface IPerformanceCounterCategory
    {
        void Delete(string category);
        bool Exists(string category);
        IPerformanceCounterCategory New(string category);
        bool CounterExists(string name);
        void Create(string category, string categoryHelp, IEnumerable<IPerformanceCounter> counters);
    }
}

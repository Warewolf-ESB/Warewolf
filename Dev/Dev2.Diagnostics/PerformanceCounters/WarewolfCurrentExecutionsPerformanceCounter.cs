using Dev2.Common.Interfaces.Monitoring;
using System.Diagnostics;

namespace Dev2.Diagnostics.PerformanceCounters
{
   public class WarewolfCurrentExecutionsPerformanceCounter:IPerformanceCounter
    {
       
       private PerformanceCounter _counter;
       private bool _started;

       public WarewolfCurrentExecutionsPerformanceCounter()
       {
           _started = false;

       }

       public CounterCreationData CreationData()
       {
          
               CounterCreationData totalOps = new CounterCreationData
               {
                   CounterName = "Current connections",
                   CounterHelp = "Concurrent requests currently executing",
                   CounterType = PerformanceCounterType.NumberOfItems32
               };
               return totalOps;
       }

       public bool IsActive { get; set; }

       #region Implementation of IPerformanceCounter

       public void Increment()
       {
           Setup();
           if (IsActive)
           _counter.Increment();
       }

       private void Setup()
       {
           if (!_started)
           {
               _counter = new PerformanceCounter("Warewolf", "Current connections")
               {
                   MachineName = ".",
                   ReadOnly = false
               };
               _started = true;
           }
       }

       public void Decrement()
       {
           Setup();
           if(IsActive)
           _counter.Decrement();
       }

       public string Category
       {
           get
           {
               return "Warewolf";
           }
       }
       public string Name
       {
           get
           {
               return "Current connections";
           }
       }

       #endregion
    }
}

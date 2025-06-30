using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using Dev2.PerformanceCounters.Management;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common;
using System.Threading.Tasks;

namespace Warewolf.PerfMon
{
	public class PerfMonService : ServiceBase
	{
		private const string Category = "Warewolf";

		protected override void OnStart(string[] args)
		{
			Task.Run(() => InitializeAndRun());
		}

		public void InitializeAndRun()
		{
			try
			{
				while (true)
				{
					EnsureCategory();
					Thread.Sleep(TimeSpan.FromSeconds(5));
				}
			}
			catch (ThreadInterruptedException)
			{
				Dev2Logger.Info("Warewolf.PerfMon service was stopped.", "Warewolf Info");
			}
			catch (Exception ex)
			{
				Dev2Logger.Error($"Unhandled exception: {ex}", ex, "Warewolf Error");
			}
		}

		private void EnsureCategory()
		{
			var allCounters = PerformanceCounterPersistence.DefaultCounters;
			var register = new WarewolfPerformanceCounterRegister(allCounters, new List<IResourcePerformanceCounter>());
			register.RegisterCountersOnMachine(allCounters, Category);
			foreach(var counter in register.Counters)
			{
				counter.Setup();
			}
		}
	}
}

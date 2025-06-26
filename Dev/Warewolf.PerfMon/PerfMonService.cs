using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using Dev2.PerformanceCounters.Management;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common;

namespace Warewolf.PerfMon
{
	public class PerfMonService : ServiceBase
	{
		private const string Category = "Warewolf";

		protected override void OnStart(string[] args)
		{
			InitializeAndRun();
		}

		public void InitializeAndRun()
		{
			try
			{
				while (true)
				{
					EnsureCategory();
					Thread.Sleep(TimeSpan.FromSeconds(300));
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
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Dev2.PerformanceCounters.Management;
using Dev2.Common.Interfaces.Monitoring;

namespace Warewolf.PerfMon
{
	public class PerfMonService : ServiceBase
	{
		private const string Category = "Warewolf";
		private const string InstanceName = "All";
		private Thread _workerThread;
		private Dictionary<string, PerformanceCounter> _counters;

		protected override void OnStart(string[] args)
		{
			InitializeAndRun();
		}

		protected override void OnStop()
		{
			Shutdown();
		}

		public void InitializeAndRun()
		{
			try
			{
				EnsureCategory();

				// Keep counters alive
				while (true)
				{
					Thread.Sleep(TimeSpan.FromSeconds(30));
				}
			}
			catch (ThreadInterruptedException)
			{
				// Graceful exit
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry("WarewolfCounterService", $"Unhandled exception: {ex}", EventLogEntryType.Error);
			}
		}

		public void Shutdown()
		{
			foreach (var counter in _counters.Values)
			{
				counter.Dispose();
			}
			_workerThread?.Interrupt();
		}

		private void EnsureCategory()
		{
			var allCounters = PerformanceCounterPersistence.DefaultCounters;
			var register = new WarewolfPerformanceCounterRegister(allCounters, new List<IResourcePerformanceCounter>());
			register.RegisterCountersOnMachine(allCounters, Category);
		}
	}
}

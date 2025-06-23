using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

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
				InitializeCounters();

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
			if (!PerformanceCounterCategory.Exists(Category))
			{
				var counters = new CounterCreationDataCollection
				{
					new CounterCreationData("Concurrent requests currently executing", "Concurrent requests currently executing", PerformanceCounterType.NumberOfItems32),
					new CounterCreationData("Total Errors", "Total Errors", PerformanceCounterType.NumberOfItems32),
					new CounterCreationData("Request Per Second", "Request Per Second", PerformanceCounterType.RateOfCountsPerSecond32),
					new CounterCreationData("Average workflow execution time", "Average workflow execution time", PerformanceCounterType.AverageTimer32),
					new CounterCreationData("Average workflow execution time base", "Average workflow execution time base", PerformanceCounterType.AverageBase),
					new CounterCreationData("Count of Not Authorised errors", "Count of Not Authorised errors", PerformanceCounterType.NumberOfItems32),
					new CounterCreationData("Count of requests for workflows which don't exist", "Count of requests for workflows which don't exist", PerformanceCounterType.NumberOfItems32)
				};

				PerformanceCounterCategory.Create(
					Category,
					"Warewolf Performance Counters",
					PerformanceCounterCategoryType.MultiInstance,
					counters
				);
			}
		}

		private void InitializeCounters()
		{
			_counters = new Dictionary<string, PerformanceCounter>
			{
				["Concurrent requests currently executing"] = Create("Concurrent requests currently executing"),
				["Total Errors"] = Create("Total Errors"),
				["Request Per Second"] = Create("Request Per Second"),
				["Average workflow execution time"] = Create("Average workflow execution time"),
				["Average workflow execution time base"] = Create("Average workflow execution time base"),
				["Count of Not Authorised errors"] = Create("Count of Not Authorised errors"),
				["Count of requests for workflows which don't exist"] = Create("Count of requests for workflows which don't exist")
			};

			foreach (var pc in _counters.Values)
			{
				pc.RawValue = 0;
			}
		}

		private PerformanceCounter Create(string counterName)
		{
			return new PerformanceCounter(Category, counterName, InstanceName, false);
		}
	}
}

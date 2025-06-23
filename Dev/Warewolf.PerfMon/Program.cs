using System;
using System.ServiceProcess;

namespace Warewolf.PerfMon
{
	public class Program
	{
		public static void Main()
		{
			if (Environment.UserInteractive)
			{
				var service = new PerfMonService();
				Console.WriteLine("Running in console mode. Press Ctrl+C to exit...");
				service.InitializeAndRun();
			}
			else
			{
				ServiceBase.Run(new PerfMonService());
			}
		}
	}
}
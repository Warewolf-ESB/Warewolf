using System;
using System.ServiceProcess;
using Warewolf.PerfMon;

bool runConsole = false;
if (args != null && args.Length > 0)
{
	foreach (var arg in args)
	{
		if (string.Equals(arg, "-console", StringComparison.OrdinalIgnoreCase))
		{
			runConsole = true;
			break;
		}
	}
}

if (Environment.UserInteractive || runConsole)
{
	var service = new PerfMonService();
	Console.WriteLine("Running in console mode. Press Ctrl+C to exit...");
	service.InitializeAndRun();
}
else
{
	ServiceBase.Run(new PerfMonService());
}
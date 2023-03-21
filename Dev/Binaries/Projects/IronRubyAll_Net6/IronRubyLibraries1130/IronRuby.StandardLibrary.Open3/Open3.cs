using System.Diagnostics;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Open3
{
	[RubyModule("Open3")]
	public static class Open3
	{
		[RubyMethod("popen3", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static RubyArray OpenPipe(RubyContext context, object self, [NotNull][DefaultProtocol] MutableString command)
		{
			Process process = RubyProcess.CreateProcess(context, command, true, true, true);
			RubyArray rubyArray = new RubyArray();
			rubyArray.Add(new RubyIO(context, null, process.StandardInput, IOMode.WriteOnly));
			rubyArray.Add(new RubyIO(context, process.StandardOutput, null, IOMode.ReadOnly));
			rubyArray.Add(new RubyIO(context, process.StandardError, null, IOMode.ReadOnly));
			if (context.RubyOptions.Compatibility >= RubyCompatibility.Default)
			{
				rubyArray.Add(ThreadOps.RubyThreadInfo.FromThread(Thread.CurrentThread));
			}
			return rubyArray;
		}
	}
}

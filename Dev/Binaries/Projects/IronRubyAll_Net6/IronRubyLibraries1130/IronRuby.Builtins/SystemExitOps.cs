using System;
using System.Runtime.InteropServices;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("SystemExit", Extends = typeof(SystemExit))]
	public class SystemExitOps : Exception
	{
		[RubyMethod("status")]
		public static int GetStatus(SystemExit self)
		{
			return self.Status;
		}

		[RubyMethod("success?")]
		public static bool IsSuccessful(SystemExit self)
		{
			return self.Status == 0;
		}

		[RubyConstructor]
		public static SystemExit Factory(RubyClass self, object message)
		{
			return Factory(self, 0, message);
		}

		[RubyConstructor]
		public static SystemExit Factory(RubyClass self, [Optional] int status, object message)
		{
			SystemExit systemExit = new SystemExit(status, RubyExceptionData.GetClrMessage(self, message ?? "SystemExit"));
			RubyExceptionData.InitializeException(systemExit, message);
			return systemExit;
		}
	}
}

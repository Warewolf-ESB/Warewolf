using System;
using System.Runtime.InteropServices;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("SystemCallError", Extends = typeof(ExternalException), Inherits = typeof(SystemException))]
	public static class SystemCallErrorOps
	{
		[RubyMethod("errno")]
		public static int Errno(ExternalException self)
		{
			return self.ErrorCode;
		}

		[RubyConstructor]
		public static ExternalException Factory(RubyClass self, [DefaultProtocol] MutableString message)
		{
			ExternalException ex = new ExternalException(RubyExceptions.MakeMessage(ref message, "unknown error"));
			RubyExceptionData.InitializeException(ex, message);
			return ex;
		}

		[RubyConstructor]
		public static ExternalException Factory(RubyClass self, int errorCode)
		{
			switch (errorCode)
			{
			case 10048:
				return new Errno.AddressInUseError();
			case 10053:
				return new Errno.ConnectionAbortedError();
			case 10054:
				return new Errno.ConnectionResetError();
			case 10057:
				return new Errno.NotConnectedError();
			case 10061:
				return new Errno.ConnectionRefusedError();
			case 10064:
				return new Errno.HostDownError();
			default:
			{
				MutableString message = MutableString.CreateAscii("Unknown Error");
				ExternalException ex = new ExternalException(RubyExceptions.MakeMessage(ref message, "Unknown Error"), errorCode);
				RubyExceptionData.InitializeException(ex, message);
				return ex;
			}
			}
		}
	}
}

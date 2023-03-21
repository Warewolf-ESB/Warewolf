using System;
using System.Net.Sockets;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Sockets
{
	[RubyClass("SocketError", BuildConfig = "!SILVERLIGHT", Extends = typeof(SocketException), Inherits = typeof(SystemException))]
	[HideMethod("message")]
	public static class SocketErrorOps
	{
		[RubyConstructor]
		public static Exception Create(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new SocketException(0), message ?? MutableString.CreateAscii("SocketError"));
		}

		public static Exception Create(MutableString message)
		{
			return RubyExceptionData.InitializeException(new SocketException(0), message);
		}
	}
}

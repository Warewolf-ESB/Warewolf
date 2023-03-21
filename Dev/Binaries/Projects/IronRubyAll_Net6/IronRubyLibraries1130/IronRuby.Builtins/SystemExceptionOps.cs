using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("StandardError", Extends = typeof(SystemException), Inherits = typeof(Exception))]
	public static class SystemExceptionOps
	{
	}
}

using System;
using System.Security;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("SecurityError", Extends = typeof(SecurityException), Inherits = typeof(SystemException))]
	public static class SecurityErrorOps
	{
	}
}

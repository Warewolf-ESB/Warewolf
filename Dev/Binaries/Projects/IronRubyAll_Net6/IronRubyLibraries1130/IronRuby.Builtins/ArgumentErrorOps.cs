using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("ArgumentError", Extends = typeof(ArgumentException), Inherits = typeof(SystemException))]
	[HideMethod("message")]
	public static class ArgumentErrorOps
	{
	}
}

using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("RangeError", Extends = typeof(ArgumentOutOfRangeException), Inherits = typeof(SystemException))]
	[HideMethod("message")]
	public static class RangeErrorOps
	{
	}
}

using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("TypeError", Extends = typeof(InvalidOperationException), Inherits = typeof(SystemException))]
	public static class TypeErrorOps
	{
	}
}

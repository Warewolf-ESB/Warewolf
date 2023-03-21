using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("RuntimeError", Extends = typeof(RuntimeError), Inherits = typeof(SystemException))]
	public static class RuntimeErrorOps
	{
	}
}

using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("SystemStackError", Extends = typeof(SystemStackError), Inherits = typeof(SystemException))]
	public static class SystemStackErrorOps
	{
	}
}

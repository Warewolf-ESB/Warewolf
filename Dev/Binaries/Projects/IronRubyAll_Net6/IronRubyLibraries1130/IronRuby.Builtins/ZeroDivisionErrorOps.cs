using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("ZeroDivisionError", Extends = typeof(DivideByZeroException), Inherits = typeof(SystemException))]
	public static class ZeroDivisionErrorOps
	{
	}
}

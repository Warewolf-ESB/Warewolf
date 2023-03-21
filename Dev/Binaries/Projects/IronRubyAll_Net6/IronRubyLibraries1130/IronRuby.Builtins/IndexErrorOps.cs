using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("IndexError", Extends = typeof(IndexOutOfRangeException), Inherits = typeof(SystemException))]
	public static class IndexErrorOps
	{
	}
}

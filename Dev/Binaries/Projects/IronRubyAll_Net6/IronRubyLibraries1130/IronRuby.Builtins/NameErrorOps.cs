using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("NameError", Extends = typeof(MemberAccessException), Inherits = typeof(SystemException))]
	public static class NameErrorOps
	{
	}
}

using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("EncodingError", Extends = typeof(EncodingError), Inherits = typeof(SystemException))]
	public static class EncodingErrorOps
	{
	}
}

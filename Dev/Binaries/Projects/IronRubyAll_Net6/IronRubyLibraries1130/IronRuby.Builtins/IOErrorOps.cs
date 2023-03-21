using System;
using System.IO;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyException("IOError", Extends = typeof(IOException), Inherits = typeof(SystemException))]
	public static class IOErrorOps
	{
	}
}

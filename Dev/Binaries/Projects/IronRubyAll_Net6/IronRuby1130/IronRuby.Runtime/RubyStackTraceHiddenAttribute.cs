using System;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class RubyStackTraceHiddenAttribute : Attribute
	{
	}
}

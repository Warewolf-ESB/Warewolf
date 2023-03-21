using System;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RubySingletonAttribute : RubyModuleAttribute
	{
	}
}

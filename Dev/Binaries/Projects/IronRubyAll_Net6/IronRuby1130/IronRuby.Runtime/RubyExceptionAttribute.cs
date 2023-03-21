using System;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RubyExceptionAttribute : RubyClassAttribute
	{
		public RubyExceptionAttribute(string name)
			: base(name)
		{
		}
	}
}

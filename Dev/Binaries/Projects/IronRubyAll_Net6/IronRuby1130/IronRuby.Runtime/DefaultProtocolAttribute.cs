using System;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public class DefaultProtocolAttribute : Attribute
	{
	}
}

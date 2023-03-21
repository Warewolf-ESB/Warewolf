using System;
using System.Diagnostics;

namespace IronRuby.Compiler.Generation
{
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
	public sealed class ReflectionCachedAttribute : Attribute
	{
	}
}

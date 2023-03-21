using System;
using System.Diagnostics;

namespace IronRuby.Compiler.Generation
{
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class EmittedAttribute : Attribute
	{
		public bool UseReflection { get; set; }
	}
}

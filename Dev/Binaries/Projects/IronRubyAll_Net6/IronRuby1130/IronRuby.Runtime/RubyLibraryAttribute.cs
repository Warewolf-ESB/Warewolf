using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class RubyLibraryAttribute : Attribute
	{
		private Type _initializer;

		public Type Initializer
		{
			get
			{
				return _initializer;
			}
		}

		public RubyLibraryAttribute(Type initializer)
		{
			ContractUtils.RequiresNotNull(initializer, "initializer");
			_initializer = initializer;
		}
	}
}

using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public sealed class RubyMethodAttribute : RubyAttribute
	{
		internal const int CompatibilityEncodingShift = 16;

		private readonly string _name;

		private readonly RubyMethodAttributes _methodAttributes;

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public RubyMethodAttributes MethodAttributes
		{
			get
			{
				return _methodAttributes;
			}
		}

		public RubyMethodAttribute(string name)
			: this(name, RubyMethodAttributes.PublicInstance)
		{
		}

		public RubyMethodAttribute(string name, RubyMethodAttributes methodAttributes)
		{
			ContractUtils.RequiresNotNull(name, "name");
			_name = name;
			_methodAttributes = methodAttributes;
		}
	}
}

using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class RubyClassAttribute : RubyModuleAttribute
	{
		private Type _inherits;

		public Type Inherits
		{
			get
			{
				return _inherits;
			}
			set
			{
				ContractUtils.RequiresNotNull(value, "value");
				_inherits = value;
			}
		}

		public RubyClassAttribute()
		{
		}

		public RubyClassAttribute(string name)
			: base(name)
		{
		}
	}
}

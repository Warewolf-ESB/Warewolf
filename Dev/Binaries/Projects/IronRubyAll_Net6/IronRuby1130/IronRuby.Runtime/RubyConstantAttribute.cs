using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = true)]
	public sealed class RubyConstantAttribute : RubyAttribute
	{
		private readonly string _name;

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public RubyConstantAttribute()
		{
		}

		public RubyConstantAttribute(string name)
		{
			ContractUtils.RequiresNotNull(name, "name");
			_name = name;
		}
	}
}

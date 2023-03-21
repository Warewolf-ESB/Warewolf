using System;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class RubyModuleAttribute : RubyAttribute
	{
		private readonly string _name;

		private ModuleRestrictions? _restrictions;

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public Type Extends { get; set; }

		public Type DefineIn { get; set; }

		public ModuleRestrictions Restrictions
		{
			get
			{
				return _restrictions ?? ModuleRestrictions.None;
			}
			set
			{
				_restrictions = value;
			}
		}

		public ModuleRestrictions GetRestrictions(bool builtin)
		{
			return (ModuleRestrictions)(((int?)_restrictions) ?? ((builtin ? 7 : 0) | ((Extends == null) ? 8 : 0)));
		}

		public RubyModuleAttribute()
		{
		}

		public RubyModuleAttribute(string name)
		{
			ContractUtils.RequiresNotEmpty(name, "name");
			_name = name;
		}
	}
}

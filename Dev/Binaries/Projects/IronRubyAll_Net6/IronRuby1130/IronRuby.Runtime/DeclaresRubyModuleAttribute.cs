using System;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public class DeclaresRubyModuleAttribute : RubyAttribute
	{
		private readonly string _name;

		private readonly Type _moduleDefinitionType;

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public Type ModuleDefinitionType
		{
			get
			{
				return _moduleDefinitionType;
			}
		}

		public DeclaresRubyModuleAttribute(string name, Type moduleDefinitionType)
		{
			_name = name;
			_moduleDefinitionType = moduleDefinitionType;
		}
	}
}

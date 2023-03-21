using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public sealed class UndefineMethodAttribute : Attribute
	{
		private readonly string _name;

		private bool _isStatic;

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public bool IsStatic
		{
			get
			{
				return _isStatic;
			}
			set
			{
				_isStatic = value;
			}
		}

		public UndefineMethodAttribute(string name)
		{
			ContractUtils.RequiresNotNull(name, "name");
			_name = name;
		}
	}
}

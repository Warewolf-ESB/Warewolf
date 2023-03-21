using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public sealed class AliasMethodAttribute : Attribute
	{
		private readonly string _oldName;

		private readonly string _newName;

		private bool _isStatic;

		public string NewName
		{
			get
			{
				return _newName;
			}
		}

		public string OldName
		{
			get
			{
				return _oldName;
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

		public AliasMethodAttribute(string newName, string oldName)
		{
			ContractUtils.RequiresNotNull(newName, "newName");
			ContractUtils.RequiresNotNull(oldName, "oldName");
			_newName = newName;
			_oldName = oldName;
		}
	}
}

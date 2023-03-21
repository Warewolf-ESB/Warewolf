using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class IncludesAttribute : Attribute
	{
		private readonly Type[] _types;

		private bool _copy;

		public Type[] Types
		{
			get
			{
				return _types;
			}
		}

		public bool Copy
		{
			get
			{
				return _copy;
			}
			set
			{
				_copy = value;
			}
		}

		public IncludesAttribute()
		{
			_types = Type.EmptyTypes;
		}

		public IncludesAttribute(params Type[] types)
		{
			ContractUtils.RequiresNotNullItems(types, "types");
			_types = types;
		}
	}
}

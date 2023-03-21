using System;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public class ClrName : IEquatable<ClrName>
	{
		private readonly string _actual;

		private string _mangled;

		public string ActualName
		{
			get
			{
				return _actual;
			}
		}

		public string MangledName
		{
			get
			{
				if (_mangled == null)
				{
					_mangled = RubyUtils.TryMangleName(_actual) ?? _actual;
				}
				return _mangled;
			}
		}

		public bool HasMangledName
		{
			get
			{
				return !object.ReferenceEquals(_actual, MangledName);
			}
		}

		public ClrName(string actualName)
		{
			ContractUtils.RequiresNotNull(actualName, "actualName");
			_actual = actualName;
		}

		public bool Equals(ClrName other)
		{
			if (other != null)
			{
				return _actual == other._actual;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ClrName);
		}

		public override int GetHashCode()
		{
			return _actual.GetHashCode();
		}
	}
}

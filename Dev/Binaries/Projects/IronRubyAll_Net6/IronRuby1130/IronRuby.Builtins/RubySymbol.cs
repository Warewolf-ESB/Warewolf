using System;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[Serializable]
	public class RubySymbol : IComparable, IComparable<RubySymbol>, IEquatable<RubySymbol>, IEquatable<MutableString>, IRubyObjectState
	{
		internal static int MinId = 1;

		private readonly int _id;

		private readonly int _runtimeId;

		private readonly MutableString _string;

		public int RuntimeId
		{
			get
			{
				return _runtimeId;
			}
		}

		public int Id
		{
			get
			{
				return _id;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _string.Encoding;
			}
		}

		public MutableString String
		{
			get
			{
				return _string;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return _string.IsEmpty;
			}
		}

		public bool IsFrozen
		{
			get
			{
				return false;
			}
		}

		public bool IsTainted
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool IsUntrusted
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		internal RubySymbol(MutableString str, int id, int runtimeId)
		{
			_string = str;
			_id = id;
			_runtimeId = runtimeId;
		}

		public override int GetHashCode()
		{
			return _string.GetHashCode();
		}

		public bool Equals(RubySymbol other)
		{
			return object.ReferenceEquals(this, other);
		}

		public bool Equals(MutableString other)
		{
			if (other != null)
			{
				return _string.Equals(other);
			}
			return false;
		}

		public int CompareTo(RubySymbol other)
		{
			return _string.CompareTo(other._string);
		}

		public int CompareTo(MutableString other)
		{
			return _string.CompareTo(other);
		}

		public override bool Equals(object other)
		{
			RubySymbol rubySymbol = other as RubySymbol;
			if (rubySymbol != null)
			{
				return Equals(rubySymbol);
			}
			MutableString mutableString = other as MutableString;
			if (mutableString != null)
			{
				return Equals(mutableString);
			}
			return false;
		}

		int IComparable.CompareTo(object other)
		{
			RubySymbol rubySymbol = other as RubySymbol;
			if (rubySymbol != null)
			{
				return CompareTo(rubySymbol);
			}
			MutableString mutableString = other as MutableString;
			if (mutableString != null)
			{
				return CompareTo(mutableString);
			}
			return -1;
		}

		public override string ToString()
		{
			return _string.ToString();
		}

		public int GetByteCount()
		{
			return _string.GetByteCount();
		}

		public int GetCharCount()
		{
			return _string.GetCharCount();
		}

		public static explicit operator string(RubySymbol self)
		{
			return self._string.ToString();
		}

		public void Freeze()
		{
		}

		public bool EndsWith(char value)
		{
			return _string.GetLastChar() == value;
		}

		public MutableString GetSlice(int start)
		{
			return _string.GetSlice(start);
		}
	}
}

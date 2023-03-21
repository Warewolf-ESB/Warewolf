using System;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime
{
	public struct IntegerValue : IEquatable<IntegerValue>
	{
		private int _fixnum;

		private BigInteger _bignum;

		public int Fixnum
		{
			get
			{
				return _fixnum;
			}
		}

		public BigInteger Bignum
		{
			get
			{
				return _bignum;
			}
		}

		public bool IsFixnum
		{
			get
			{
				return object.ReferenceEquals(_bignum, null);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is IntegerValue)
			{
				return Equals((IntegerValue)obj);
			}
			if (obj is int)
			{
				return Equals(new IntegerValue((int)obj));
			}
			if (obj is BigInteger)
			{
				return Equals(new IntegerValue(obj as BigInteger));
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (IsFixnum)
			{
				return _fixnum.GetHashCode();
			}
			return _bignum.GetHashCode();
		}

		public IntegerValue(int value)
		{
			_fixnum = value;
			_bignum = null;
		}

		public IntegerValue(BigInteger value)
		{
			_fixnum = 0;
			_bignum = value;
		}

		public static implicit operator IntegerValue(int value)
		{
			return new IntegerValue(value);
		}

		public static implicit operator IntegerValue(BigInteger value)
		{
			return new IntegerValue(value);
		}

		public object ToObject()
		{
			return _bignum ?? ScriptingRuntimeHelpers.Int32ToObject(_fixnum);
		}

		public int ToInt32()
		{
			if (IsFixnum)
			{
				return _fixnum;
			}
			int ret;
			if (!_bignum.AsInt32(out ret))
			{
				throw RubyExceptions.CreateRangeError("Bignum too big to convert into 32-bit signed integer");
			}
			return ret;
		}

		public long ToInt64()
		{
			if (IsFixnum)
			{
				return _fixnum;
			}
			long ret;
			if (!_bignum.AsInt64(out ret))
			{
				throw RubyExceptions.CreateRangeError("Bignum too big to convert into 64-bit signed integer");
			}
			return ret;
		}

		[CLSCompliant(false)]
		public uint ToUInt32Unchecked()
		{
			if (IsFixnum)
			{
				return (uint)_fixnum;
			}
			uint ret;
			if (_bignum.AsUInt32(out ret))
			{
				return ret;
			}
			throw RubyExceptions.CreateRangeError("bignum too big to convert into 32-bit unsigned integer");
		}

		public bool Equals(IntegerValue other)
		{
			if (_fixnum != other.Fixnum)
			{
				return false;
			}
			if (object.ReferenceEquals(_bignum, null))
			{
				return object.ReferenceEquals(other._bignum, null);
			}
			return _bignum.Equals(other._bignum);
		}
	}
}

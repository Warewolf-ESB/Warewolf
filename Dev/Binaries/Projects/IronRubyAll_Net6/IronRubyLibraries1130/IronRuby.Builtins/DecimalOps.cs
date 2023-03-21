using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass(Extends = typeof(decimal), Inherits = typeof(Numeric), Restrictions = ModuleRestrictions.None)]
	public static class DecimalOps
	{
		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static decimal InducedFrom(RubyModule self, double value)
		{
			try
			{
				return (decimal)value;
			}
			catch (OverflowException)
			{
				throw RubyExceptions.CreateRangeError("number too big or to small to convert into System::Decimal");
			}
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static decimal InducedFrom(RubyModule self, decimal value)
		{
			return value;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static decimal InducedFrom(RubyModule self, int value)
		{
			return value;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static decimal InducedFrom(RubyModule self, [NotNull] BigInteger value)
		{
			try
			{
				return (decimal)value;
			}
			catch (OverflowException)
			{
				throw RubyExceptions.CreateRangeError("number too big to convert into System::Decimal");
			}
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static double InducedFrom(RubyModule self, object value)
		{
			throw RubyExceptions.CreateTypeError("failed to convert {0} into Decimal", self.Context.GetClassDisplayName(value));
		}

		[RubyMethod("==")]
		public static bool Equal(decimal self, double other)
		{
			return decimal.ToDouble(self) == other;
		}

		[RubyMethod("==")]
		public static bool Equal(BinaryOpStorage equals, decimal self, object other)
		{
			if (other is decimal)
			{
				return self == (decimal)other;
			}
			return Protocols.IsEqual(equals, other, self);
		}

		[RubyMethod("to_int")]
		[RubyMethod("to_i")]
		public static object ToInt(decimal self)
		{
			decimal x = ((!(self >= 0m)) ? decimal.Ceiling(self) : decimal.Floor(self));
			return Protocols.Normalize(x);
		}

		[RubyMethod("to_f")]
		public static double ToDouble(decimal self)
		{
			return decimal.ToDouble(self);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (Decimal)");
		}

		[RubyMethod("size")]
		public static int Size(object self)
		{
			return 16;
		}
	}
}

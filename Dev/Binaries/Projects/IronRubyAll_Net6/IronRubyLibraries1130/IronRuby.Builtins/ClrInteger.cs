using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule("Integer", DefineIn = typeof(IronRubyOps.Clr))]
	public static class ClrInteger
	{
		public static readonly object Zero = ScriptingRuntimeHelpers.Int32ToObject(0);

		public static readonly object One = ScriptingRuntimeHelpers.Int32ToObject(1);

		public static readonly object MinusOne = ScriptingRuntimeHelpers.Int32ToObject(-1);

		internal static object MinusMinValue()
		{
			return -(BigInteger)int.MinValue;
		}

		public static object Narrow(long value)
		{
			if (value < int.MinValue || value > int.MaxValue)
			{
				return (BigInteger)value;
			}
			return (int)value;
		}

		[RubyMethod("<<")]
		public static object LeftShift(int self, int shift)
		{
			if (self == 0)
			{
				return Zero;
			}
			if (shift == 0)
			{
				return self;
			}
			if (shift < 0)
			{
				if (shift == int.MinValue)
				{
					return 0;
				}
				return RightShift(self, -shift);
			}
			if (shift >= 31 || (self & ~((1 << 31 - shift) - 1)) != 0)
			{
				return BigInteger.LeftShift(self, shift);
			}
			return self << shift;
		}

		[RubyMethod("<<")]
		public static object LeftShift(RubyContext context, int self, [DefaultProtocol] IntegerValue other)
		{
			return ClrBigInteger.LeftShift(context, self, other);
		}

		[RubyMethod(">>")]
		public static object RightShift(int self, int shift)
		{
			if (shift < 0)
			{
				if (shift == int.MinValue)
				{
					throw RubyExceptions.CreateRangeError("bignum too big to convert into long");
				}
				return LeftShift(self, -shift);
			}
			if (shift == 0)
			{
				return self;
			}
			if (shift >= 32)
			{
				if (self >= 0)
				{
					return Zero;
				}
				return MinusOne;
			}
			return self >> shift;
		}

		[RubyMethod(">>")]
		public static object RightShift(RubyContext context, int self, [DefaultProtocol] IntegerValue other)
		{
			return ClrBigInteger.RightShift(context, self, other);
		}

		[RubyMethod("[]")]
		public static int Bit(int self, [DefaultProtocol] int index)
		{
			if (index < 0)
			{
				return 0;
			}
			if (index > 32)
			{
				if (self >= 0)
				{
					return 0;
				}
				return 1;
			}
			if ((self & (1 << index)) == 0)
			{
				return 0;
			}
			return 1;
		}

		[RubyMethod("[]")]
		public static int Bit(int self, [NotNull] BigInteger index)
		{
			if (index.IsNegative() || self >= 0)
			{
				return 0;
			}
			return 1;
		}

		[RubyMethod("^")]
		public static object BitwiseXor(int self, int other)
		{
			return self ^ other;
		}

		[RubyMethod("^")]
		public static object BitwiseXor(int self, [NotNull] BigInteger other)
		{
			return other ^ self;
		}

		[RubyMethod("^")]
		public static object BitwiseXor(RubyContext context, int self, [DefaultProtocol] IntegerValue other)
		{
			return ClrBigInteger.Xor(context, self, other);
		}

		[RubyMethod("&")]
		public static int BitwiseAnd(int self, int other)
		{
			return self & other;
		}

		[RubyMethod("&")]
		public static object BitwiseAnd(int self, [NotNull] BigInteger other)
		{
			BigInteger bigInteger = other & self;
			int ret;
			if (bigInteger.AsInt32(out ret))
			{
				return ret;
			}
			return bigInteger;
		}

		[RubyMethod("&")]
		public static object BitwiseAnd(RubyContext context, int self, [DefaultProtocol] IntegerValue other)
		{
			return ClrBigInteger.And(context, self, other);
		}

		[RubyMethod("|")]
		public static int BitwiseOr(int self, int other)
		{
			return self | other;
		}

		[RubyMethod("|")]
		public static object BitwiseOr(int self, [NotNull] BigInteger other)
		{
			BigInteger bigInteger = other | self;
			int ret;
			if (bigInteger.AsInt32(out ret))
			{
				return ret;
			}
			return bigInteger;
		}

		[RubyMethod("|")]
		public static object BitwiseOr(RubyContext context, int self, [DefaultProtocol] IntegerValue other)
		{
			return ClrBigInteger.BitwiseOr(context, self, other);
		}

		[RubyMethod("~")]
		public static int OnesComplement(int self)
		{
			return ~self;
		}

		[RubyMethod("*")]
		public static object Multiply(int self, int other)
		{
			return Narrow((long)self * (long)other);
		}

		[RubyMethod("*")]
		public static BigInteger Multiply(int self, [NotNull] BigInteger other)
		{
			return BigInteger.Multiply(self, other);
		}

		[RubyMethod("*")]
		public static double Multiply(int self, double other)
		{
			return (double)self * other;
		}

		[RubyMethod("*")]
		public static object Multiply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "*", self, other);
		}

		[RubyMethod("**")]
		public static object Power(int self, int other)
		{
			if (other >= 0)
			{
				BigInteger bigInteger = self;
				return Protocols.Normalize(bigInteger.Power(other));
			}
			if (self == 1)
			{
				return One;
			}
			return Math.Pow(self, other);
		}

		[RubyMethod("**")]
		public static double Power(int self, double other)
		{
			return Math.Pow(self, other);
		}

		[RubyMethod("**")]
		public static object Power(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, RubyContext context, int self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "**", self, other);
		}

		[RubyMethod("+")]
		public static object Add(int self, int other)
		{
			return Narrow((long)self + (long)other);
		}

		[RubyMethod("+")]
		public static object Add(int self, [NotNull] BigInteger other)
		{
			return self + other;
		}

		[RubyMethod("+")]
		public static double Add(int self, double other)
		{
			return (double)self + other;
		}

		[RubyMethod("+")]
		public static object Add(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "+", self, other);
		}

		[RubyMethod("-")]
		public static object Subtract(int self, int other)
		{
			return Narrow((long)self - (long)other);
		}

		[RubyMethod("-")]
		public static object Subtract(int self, BigInteger other)
		{
			return Protocols.Normalize(self - other);
		}

		[RubyMethod("-")]
		public static double Subtract(int self, double other)
		{
			return (double)self - other;
		}

		[RubyMethod("-")]
		public static object Subtract(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, RubyContext context, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", self, other);
		}

		[RubyMethod("-@")]
		public static object Minus(int self)
		{
			if (self == int.MinValue)
			{
				return MinusMinValue();
			}
			return -self;
		}

		[RubyMethod("div")]
		[RubyMethod("/")]
		public static object Divide(int self, int other)
		{
			if (self == int.MinValue && other == -1)
			{
				return MinusMinValue();
			}
			return MathUtils.FloorDivideUnchecked(self, other);
		}

		[RubyMethod("/")]
		public static object DivideOp(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "/", self, other);
		}

		[RubyMethod("div")]
		public static object Divide(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "div", self, other);
		}

		[RubyMethod("fdiv", Compatibility = RubyCompatibility.Default)]
		public static double FDiv(int self, [DefaultProtocol] int other)
		{
			return (double)self / (double)other;
		}

		[RubyMethod("%")]
		[RubyMethod("modulo")]
		public static int Modulo(int self, int other)
		{
			return MathUtils.FloorRemainder(self, other);
		}

		[RubyMethod("%")]
		public static object ModuloOp(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "%", self, other);
		}

		[RubyMethod("modulo")]
		public static object Modulo(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "modulo", self, other);
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(int self, int other)
		{
			return RubyOps.MakeArray2(Divide(self, other), Modulo(self, other));
		}

		[RubyMethod("divmod")]
		public static object DivMod(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, int self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "divmod", self, other);
		}

		[RubyMethod("abs")]
		public static object Abs(int self)
		{
			if (self < 0)
			{
				if (self == int.MinValue)
				{
					return MinusMinValue();
				}
				return -self;
			}
			return self;
		}

		[RubyMethod("quo")]
		public static double Quotient(int self, int other)
		{
			return (double)self / (double)other;
		}

		[RubyMethod("quo")]
		public static object Quotient(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, int self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "quo", self, other);
		}

		[RubyMethod("zero?")]
		public static bool IsZero(int self)
		{
			return self == 0;
		}

		[RubyMethod("<")]
		public static bool LessThan(int self, int other)
		{
			return self < other;
		}

		[RubyMethod("<")]
		public static bool LessThan(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, object self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<", self, other);
		}

		[RubyMethod("<=")]
		public static bool LessThanOrEqual(int self, int other)
		{
			return self <= other;
		}

		[RubyMethod("<=")]
		public static bool LessThanOrEqual(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, object self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<=", self, other);
		}

		[RubyMethod("<=>")]
		public static int Compare(int self, int other)
		{
			return self.CompareTo(other);
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, object self, object other)
		{
			return Protocols.CoerceAndCompare(coercionStorage, comparisonStorage, self, other);
		}

		[RubyMethod("==")]
		public static bool Equal(int self, int other)
		{
			return self == other;
		}

		[RubyMethod("==")]
		public static bool Equal(BinaryOpStorage equals, int self, object other)
		{
			return Protocols.IsEqual(equals, other, self);
		}

		[RubyMethod(">")]
		public static bool GreaterThan(int self, int other)
		{
			return self > other;
		}

		[RubyMethod(">")]
		public static bool GreaterThan(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, object self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">", self, other);
		}

		[RubyMethod(">=")]
		public static bool GreaterThanOrEqual(int self, int other)
		{
			return self >= other;
		}

		[RubyMethod(">=")]
		public static bool GreaterThanOrEqual(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, object self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">=", self, other);
		}

		[RubyMethod("to_f")]
		public static double ToFloat(int self)
		{
			return self;
		}

		[RubyMethod("to_s")]
		public static object ToString(object self)
		{
			return MutableString.CreateAscii(self.ToString());
		}

		[RubyMethod("to_s")]
		public static object ToString([NotNull] BigInteger self, int radix)
		{
			if (radix < 2 || radix > 36)
			{
				throw RubyExceptions.CreateArgumentError("illegal radix {0}", radix);
			}
			return MutableString.CreateAscii(self.ToString(radix).ToLowerInvariant());
		}
	}
}

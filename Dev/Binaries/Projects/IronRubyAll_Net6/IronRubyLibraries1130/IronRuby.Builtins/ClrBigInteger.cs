using System;
using Microsoft.Scripting.Math.Extensions;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule("BigInteger", DefineIn = typeof(IronRubyOps.Clr))]
	public sealed class ClrBigInteger
	{
		[RubyMethod("-@")]
		public static object Negate(BigInteger self)
		{
			return Protocols.Normalize(BigInteger.Negate(self));
		}

		[RubyMethod("abs")]
		public static object Abs(BigInteger self)
		{
			return Protocols.Normalize(self.Abs());
		}

		[RubyMethod("+")]
		public static object Add(BigInteger self, [NotNull] BigInteger other)
		{
			return Protocols.Normalize(self + other);
		}

		[RubyMethod("+")]
		public static object Add(BigInteger self, int other)
		{
			return Protocols.Normalize(self + other);
		}

		[RubyMethod("+")]
		public static object Add(BigInteger self, double other)
		{
			return self.ToFloat64() + other;
		}

		[RubyMethod("+")]
		public static object Add(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "+", self, other);
		}

		[RubyMethod("-")]
		public static object Subtract(BigInteger self, [NotNull] BigInteger other)
		{
			return Protocols.Normalize(self - other);
		}

		[RubyMethod("-")]
		public static object Subtract(BigInteger self, int other)
		{
			return Protocols.Normalize(self - other);
		}

		[RubyMethod("-")]
		public static object Subtract(BigInteger self, double other)
		{
			return self.ToFloat64() - other;
		}

		[RubyMethod("-")]
		public static object Subtract(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", self, other);
		}

		[RubyMethod("*")]
		public static object Multiply(BigInteger self, [NotNull] BigInteger other)
		{
			return Protocols.Normalize(self * other);
		}

		[RubyMethod("*")]
		public static object Multiply(BigInteger self, int other)
		{
			return Protocols.Normalize(self * other);
		}

		[RubyMethod("*")]
		public static object Multiply(BigInteger self, double other)
		{
			return self.ToFloat64() * other;
		}

		[RubyMethod("*")]
		public static object Multiply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "*", self, other);
		}

		[RubyMethod("div")]
		[RubyMethod("/")]
		public static object Divide(BigInteger self, [NotNull] BigInteger other)
		{
			return DivMod(self, other)[0];
		}

		[RubyMethod("/")]
		[RubyMethod("div")]
		public static object Divide(BigInteger self, int other)
		{
			return DivMod(self, other)[0];
		}

		[RubyMethod("/")]
		public static object DivideOp(BigInteger self, double other)
		{
			return self.ToFloat64() / other;
		}

		[RubyMethod("div")]
		public static object Divide(BigInteger self, double other)
		{
			return DivMod(self, other)[0];
		}

		[RubyMethod("/")]
		public static object Divide(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "/", self, other);
		}

		[RubyMethod("div")]
		public static object Div(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "div", self, other);
		}

		[RubyMethod("fdiv", Compatibility = RubyCompatibility.Default)]
		public static double FDiv(BigInteger self, [NotNull] BigInteger other)
		{
			return (double)self / (double)other;
		}

		[RubyMethod("quo")]
		public static object Quotient(BigInteger self, [NotNull] BigInteger other)
		{
			return Quotient(self, other.ToFloat64());
		}

		[RubyMethod("quo")]
		public static object Quotient(BigInteger self, int other)
		{
			return Quotient(self, (double)other);
		}

		[RubyMethod("quo")]
		public static object Quotient(BigInteger self, double other)
		{
			return self.ToFloat64() / other;
		}

		[RubyMethod("quo")]
		public static object Quotient(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "quo", self, other);
		}

		[RubyMethod("**")]
		public static object Power(RubyContext context, BigInteger self, [NotNull] BigInteger exponent)
		{
			context.ReportWarning("in a**b, b may be too big");
			double num = Math.Pow(self.ToFloat64(), exponent.ToFloat64());
			return num;
		}

		[RubyMethod("**")]
		public static object Power(BigInteger self, int exponent)
		{
			if (exponent < 0)
			{
				return Power(self, (double)exponent);
			}
			return Protocols.Normalize(self.Power(exponent));
		}

		[RubyMethod("**")]
		public static object Power(BigInteger self, double exponent)
		{
			return Math.Pow(self.ToFloat64(), exponent);
		}

		[RubyMethod("**")]
		public static object Power(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object exponent)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "**", self, exponent);
		}

		[RubyMethod("modulo")]
		[RubyMethod("%")]
		public static object Modulo(BigInteger self, [NotNull] BigInteger other)
		{
			RubyArray rubyArray = DivMod(self, other);
			return rubyArray[1];
		}

		[RubyMethod("%")]
		[RubyMethod("modulo")]
		public static object Modulo(BigInteger self, int other)
		{
			RubyArray rubyArray = DivMod(self, other);
			return rubyArray[1];
		}

		[RubyMethod("%")]
		[RubyMethod("modulo")]
		public static object Modulo(BigInteger self, double other)
		{
			if (other == 0.0)
			{
				return double.NaN;
			}
			RubyArray rubyArray = DivMod(self, other);
			return rubyArray[1];
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
		public static RubyArray DivMod(BigInteger self, [NotNull] BigInteger other)
		{
			BigInteger remainder;
			BigInteger x = BigInteger.DivRem(self, other, out remainder);
			if (self.Sign != other.Sign && !remainder.IsZero())
			{
				x -= (BigInteger)1;
				remainder += other;
			}
			return RubyOps.MakeArray2(Protocols.Normalize(x), Protocols.Normalize(remainder));
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(BigInteger self, int other)
		{
			return DivMod(self, (BigInteger)other);
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(BigInteger self, double other)
		{
			if (other == 0.0)
			{
				throw new FloatDomainError("NaN");
			}
			double num = self.ToFloat64();
			BigInteger x = Microsoft.Scripting.Math.BigInteger.Create(num / other);
			double num2 = num % other;
			return RubyOps.MakeArray2(Protocols.Normalize(x), num2);
		}

		[RubyMethod("divmod")]
		public static object DivMod(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "divmod", self, other);
		}

		[RubyMethod("remainder")]
		public static object Remainder(BigInteger self, [NotNull] BigInteger other)
		{
			BigInteger remainder;
			BigInteger.DivRem(self, other, out remainder);
			return Protocols.Normalize(remainder);
		}

		[RubyMethod("remainder")]
		public static object Remainder(BigInteger self, int other)
		{
			BigInteger remainder;
			BigInteger.DivRem(self, other, out remainder);
			return Protocols.Normalize(remainder);
		}

		[RubyMethod("remainder")]
		public static double Remainder(BigInteger self, double other)
		{
			return self.ToFloat64() % other;
		}

		[RubyMethod("remainder")]
		public static object Remainder(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, object self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "remainder", self, other);
		}

		[RubyMethod("<=>")]
		public static int Compare(BigInteger self, [NotNull] BigInteger other)
		{
			return BigInteger.Compare(self, other);
		}

		[RubyMethod("<=>")]
		public static int Compare(BigInteger self, int other)
		{
			return BigInteger.Compare(self, other);
		}

		[RubyMethod("<=>")]
		public static object Compare(RubyContext context, BigInteger self, double other)
		{
			return ClrFloat.Compare(ToFloat(context, self), other);
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, BigInteger self, object other)
		{
			return Protocols.CoerceAndCompare(coercionStorage, comparisonStorage, self, other);
		}

		[RubyMethod("==")]
		public static bool Equal(BigInteger self, [NotNull] BigInteger other)
		{
			return self == other;
		}

		[RubyMethod("==")]
		public static bool Equal(BigInteger self, int other)
		{
			return self == other;
		}

		[RubyMethod("==")]
		public static bool Equal(RubyContext context, BigInteger self, double other)
		{
			if (!double.IsNaN(other))
			{
				return Protocols.ConvertToDouble(context, self) == other;
			}
			return false;
		}

		[RubyMethod("==")]
		public static bool Equal(BinaryOpStorage equals, BigInteger self, object other)
		{
			return Protocols.IsEqual(equals, other, self);
		}

		[RubyMethod("eql?")]
		public static bool Eql(BigInteger self, [NotNull] BigInteger other)
		{
			return self == other;
		}

		[RubyMethod("eql?")]
		public static bool Eql(BigInteger self, int other)
		{
			return false;
		}

		[RubyMethod("eql?")]
		public static bool Eql(BigInteger self, object other)
		{
			return false;
		}

		[RubyMethod("<<")]
		public static object LeftShift(BigInteger self, int other)
		{
			BigInteger result = self << other;
			result = ShiftOverflowCheck(self, result);
			return Protocols.Normalize(result);
		}

		[RubyMethod("<<")]
		public static object LeftShift(BigInteger self, [NotNull] BigInteger other)
		{
			throw RubyExceptions.CreateRangeError("bignum too big to convert into long");
		}

		[RubyMethod("<<")]
		public static object LeftShift(RubyContext context, BigInteger self, [DefaultProtocol] IntegerValue other)
		{
			if (!other.IsFixnum)
			{
				return LeftShift(self, other.Bignum);
			}
			return LeftShift(self, other.Fixnum);
		}

		[RubyMethod(">>")]
		public static object RightShift(BigInteger self, int other)
		{
			BigInteger result = self >> other;
			result = ShiftOverflowCheck(self, result);
			return Protocols.Normalize(result);
		}

		[RubyMethod(">>")]
		public static object RightShift(BigInteger self, [NotNull] BigInteger other)
		{
			if (self.IsNegative())
			{
				return -1;
			}
			return 0;
		}

		[RubyMethod(">>")]
		public static object RightShift(RubyContext context, BigInteger self, [DefaultProtocol] IntegerValue other)
		{
			if (!other.IsFixnum)
			{
				return RightShift(self, other.Bignum);
			}
			return RightShift(self, other.Fixnum);
		}

		[RubyMethod("|")]
		public static object BitwiseOr(BigInteger self, int other)
		{
			return Protocols.Normalize(self | other);
		}

		[RubyMethod("|")]
		public static object BitwiseOr(BigInteger self, [NotNull] BigInteger other)
		{
			return Protocols.Normalize(self | other);
		}

		[RubyMethod("|")]
		public static object BitwiseOr(RubyContext context, BigInteger self, [DefaultProtocol] IntegerValue other)
		{
			if (!other.IsFixnum)
			{
				return BitwiseOr(self, other.Bignum);
			}
			return BitwiseOr(self, other.Fixnum);
		}

		[RubyMethod("&")]
		public static object And(BigInteger self, int other)
		{
			return Protocols.Normalize(self & other);
		}

		[RubyMethod("&")]
		public static object And(BigInteger self, [NotNull] BigInteger other)
		{
			return Protocols.Normalize(self & other);
		}

		[RubyMethod("&")]
		public static object And(RubyContext context, BigInteger self, [DefaultProtocol] IntegerValue other)
		{
			if (!other.IsFixnum)
			{
				return And(self, other.Bignum);
			}
			return And(self, other.Fixnum);
		}

		[RubyMethod("^")]
		public static object Xor(BigInteger self, int other)
		{
			return Protocols.Normalize(self ^ other);
		}

		[RubyMethod("^")]
		public static object Xor(BigInteger self, [NotNull] BigInteger other)
		{
			return Protocols.Normalize(self ^ other);
		}

		[RubyMethod("^")]
		public static object Xor(RubyContext context, BigInteger self, [DefaultProtocol] IntegerValue other)
		{
			if (!other.IsFixnum)
			{
				return Xor(self, other.Bignum);
			}
			return Xor(self, other.Fixnum);
		}

		[RubyMethod("~")]
		public static object Invert(BigInteger self)
		{
			return Protocols.Normalize(~self);
		}

		[RubyMethod("[]")]
		public static int Bit(BigInteger self, [DefaultProtocol] int index)
		{
			if (index < 0)
			{
				return 0;
			}
			int num = index / 8;
			int num2 = index % 8;
			byte[] array = self.ToByteArray();
			if (num >= array.Length)
			{
				if (self.Sign <= 0)
				{
					return 1;
				}
				return 0;
			}
			if ((array[num] & (1 << num2)) == 0)
			{
				return 0;
			}
			return 1;
		}

		[RubyMethod("[]")]
		public static int Bit(BigInteger self, [NotNull] BigInteger index)
		{
			if (index.IsNegative() || self.IsPositive())
			{
				return 0;
			}
			return 1;
		}

		[RubyMethod("to_f")]
		public static double ToFloat(RubyContext context, BigInteger self)
		{
			return Protocols.ConvertToDouble(context, self);
		}

		[RubyMethod("to_s")]
		public static MutableString ToString(BigInteger self)
		{
			return MutableString.CreateAscii(self.ToString());
		}

		[RubyMethod("to_s")]
		public static MutableString ToString(BigInteger self, int radix)
		{
			if (radix < 2 || radix > 36)
			{
				throw RubyExceptions.CreateArgumentError("illegal radix {0}", radix);
			}
			return MutableString.CreateAscii(self.ToString(radix).ToLowerInvariant());
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(BigInteger self, [NotNull] BigInteger other)
		{
			return RubyOps.MakeArray2(other, self);
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(RubyContext context, BigInteger self, object other)
		{
			throw RubyExceptions.CreateTypeError("can't coerce {0} to Bignum", context.GetClassDisplayName(other));
		}

		[RubyMethod("hash")]
		public static int Hash(BigInteger self)
		{
			return self.GetHashCode();
		}

		private static BigInteger ShiftOverflowCheck(BigInteger self, BigInteger result)
		{
			if (self.IsNegative() && result.IsZero())
			{
				return -1;
			}
			return result;
		}
	}
}

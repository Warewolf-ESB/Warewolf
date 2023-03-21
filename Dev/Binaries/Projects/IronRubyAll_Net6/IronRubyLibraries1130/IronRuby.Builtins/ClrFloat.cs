using System;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("Float", DefineIn = typeof(IronRubyOps.Clr))]
	public static class ClrFloat
	{
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static double InducedFrom(RubyModule self, double value)
		{
			return value;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static object InducedFrom(UnaryOpStorage tofStorage, RubyModule self, int value)
		{
			CallSite<Func<CallSite, object, object>> callSite = tofStorage.GetCallSite("to_f");
			return callSite.Target(callSite, ScriptingRuntimeHelpers.Int32ToObject(value));
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static object InducedFrom(UnaryOpStorage tofStorage, RubyModule self, [NotNull] BigInteger value)
		{
			CallSite<Func<CallSite, object, object>> callSite = tofStorage.GetCallSite("to_f");
			return callSite.Target(callSite, value);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static double InducedFrom(RubyModule self, object value)
		{
			throw RubyExceptions.CreateTypeError("failed to convert {0} into Float", self.Context.GetClassDisplayName(value));
		}

		[RubyMethod("*")]
		public static double Multiply(double self, int other)
		{
			return self * (double)other;
		}

		[RubyMethod("*")]
		public static double Multiply(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return self * Protocols.ConvertToDouble(context, other);
		}

		[RubyMethod("*")]
		public static double Multiply(double self, double other)
		{
			return self * other;
		}

		[RubyMethod("*")]
		public static object Multiply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "*", self, other);
		}

		[RubyMethod("+")]
		public static double Add(double self, int other)
		{
			return self + (double)other;
		}

		[RubyMethod("+")]
		public static double Add(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return self + Protocols.ConvertToDouble(context, other);
		}

		[RubyMethod("+")]
		public static double Add(double self, double other)
		{
			return self + other;
		}

		[RubyMethod("+")]
		public static object Add(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "+", self, other);
		}

		[RubyMethod("-")]
		public static double Subtract(double self, int other)
		{
			return self - (double)other;
		}

		[RubyMethod("-")]
		public static double Subtract(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return self - Protocols.ConvertToDouble(context, other);
		}

		[RubyMethod("-")]
		public static double Subtract(double self, double other)
		{
			return self - other;
		}

		[RubyMethod("-")]
		public static object Subtract(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", self, other);
		}

		[RubyMethod("/")]
		public static double Divide(double self, int other)
		{
			return self / (double)other;
		}

		[RubyMethod("/")]
		public static double Divide(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return self / Protocols.ConvertToDouble(context, other);
		}

		[RubyMethod("/")]
		public static double Divide(double self, double other)
		{
			return self / other;
		}

		[RubyMethod("/")]
		public static object Divide(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "/", self, other);
		}

		[RubyMethod("modulo")]
		[RubyMethod("%")]
		public static double Modulo(double self, int other)
		{
			return (double)InternalDivMod(self, other)[1];
		}

		[RubyMethod("%")]
		[RubyMethod("modulo")]
		public static double Modulo(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return (double)InternalDivMod(self, Protocols.ConvertToDouble(context, other))[1];
		}

		[RubyMethod("modulo")]
		[RubyMethod("%")]
		public static double Modulo(double self, double other)
		{
			return (double)InternalDivMod(self, other)[1];
		}

		[RubyMethod("%")]
		public static object ModuloOp(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "%", self, other);
		}

		[RubyMethod("modulo")]
		public static object Modulo(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "modulo", self, other);
		}

		[RubyMethod("**")]
		public static double Power(double self, int other)
		{
			return Math.Pow(self, other);
		}

		[RubyMethod("**")]
		public static double Power(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return Math.Pow(self, Protocols.ConvertToDouble(context, other));
		}

		[RubyMethod("**")]
		public static double Power(double self, double other)
		{
			return Math.Pow(self, other);
		}

		[RubyMethod("**")]
		public static object Power(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "**", self, other);
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(double self, int other)
		{
			return DivMod(self, (double)other);
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return DivMod(self, Protocols.ConvertToDouble(context, other));
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(double self, double other)
		{
			RubyArray rubyArray = InternalDivMod(self, other);
			if (rubyArray[0] is double || double.IsNaN((double)rubyArray[1]))
			{
				throw CreateFloatDomainError("NaN");
			}
			return rubyArray;
		}

		[RubyMethod("divmod")]
		public static object DivMod(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, double self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "divmod", self, other);
		}

		[RubyMethod("abs")]
		public static double Abs(double self)
		{
			return Math.Abs(self);
		}

		[RubyMethod("ceil")]
		public static object Ceil(double self)
		{
			double value = Math.Ceiling(self);
			return CastToInteger(value);
		}

		[RubyMethod("floor")]
		public static object Floor(double self)
		{
			double value = Math.Floor(self);
			return CastToInteger(value);
		}

		[RubyMethod("to_int")]
		[RubyMethod("to_i")]
		[RubyMethod("truncate")]
		public static object ToInt(double self)
		{
			if (self >= 0.0)
			{
				return Floor(self);
			}
			return Ceil(self);
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(double self, [DefaultProtocol] double other)
		{
			return RubyOps.MakeArray2(other, self);
		}

		[RubyMethod("to_f")]
		public static double ToFloat(double self)
		{
			return self;
		}

		[RubyMethod("round")]
		public static object Round(double self)
		{
			if (self > 0.0)
			{
				return Floor(self + 0.5);
			}
			if (self < 0.0)
			{
				return Ceil(self - 0.5);
			}
			return 0;
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(RubyContext context, double self)
		{
			StringFormatter stringFormatter = new StringFormatter(context, "%.15g", RubyEncoding.Binary, new object[1] { self });
			stringFormatter.TrailingZeroAfterWholeFloat = true;
			return stringFormatter.Format();
		}

		[RubyMethod("hash")]
		public static int Hash(double self)
		{
			return self.GetHashCode();
		}

		[RubyMethod("==")]
		public static bool Equal(double self, double other)
		{
			return self == other;
		}

		[RubyMethod("==")]
		public static bool Equal(BinaryOpStorage equals, double self, object other)
		{
			return Protocols.IsEqual(equals, other, self);
		}

		[RubyMethod("<=>")]
		public static object Compare(double self, double other)
		{
			if (double.IsNaN(self) || double.IsNaN(other))
			{
				return null;
			}
			return self.CompareTo(other);
		}

		[RubyMethod("<=>")]
		public static object Compare(double self, int other)
		{
			if (double.IsNaN(self))
			{
				return null;
			}
			return self.CompareTo(other);
		}

		[RubyMethod("<=>")]
		public static object Compare(RubyContext context, double self, [NotNull] BigInteger other)
		{
			if (double.IsNaN(self))
			{
				return null;
			}
			return self.CompareTo(Protocols.ConvertToDouble(context, other));
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, double self, object other)
		{
			return Protocols.CoerceAndCompare(coercionStorage, comparisonStorage, self, other);
		}

		[RubyMethod("<")]
		public static bool LessThan(double self, double other)
		{
			if (double.IsNaN(self) || double.IsNaN(other))
			{
				return false;
			}
			return self < other;
		}

		[RubyMethod("<")]
		public static bool LessThan(double self, int other)
		{
			return LessThan(self, (double)other);
		}

		[RubyMethod("<")]
		public static bool LessThan(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return LessThan(self, Protocols.ConvertToDouble(context, other));
		}

		[RubyMethod("<")]
		public static bool LessThan(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, double self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<", self, other);
		}

		[RubyMethod("<=")]
		public static bool LessThanOrEqual(double self, double other)
		{
			if (double.IsNaN(self) || double.IsNaN(other))
			{
				return false;
			}
			return self <= other;
		}

		[RubyMethod("<=")]
		public static bool LessThanOrEqual(double self, int other)
		{
			return LessThanOrEqual(self, (double)other);
		}

		[RubyMethod("<=")]
		public static bool LessThanOrEqual(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return LessThanOrEqual(self, Protocols.ConvertToDouble(context, other));
		}

		[RubyMethod("<=")]
		public static bool LessThanOrEqual(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, double self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<=", self, other);
		}

		[RubyMethod(">")]
		public static bool GreaterThan(double self, double other)
		{
			if (double.IsNaN(self) || double.IsNaN(other))
			{
				return false;
			}
			return self > other;
		}

		[RubyMethod(">")]
		public static bool GreaterThan(double self, int other)
		{
			return GreaterThan(self, (double)other);
		}

		[RubyMethod(">")]
		public static bool GreaterThan(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return GreaterThan(self, Protocols.ConvertToDouble(context, other));
		}

		[RubyMethod(">")]
		public static bool GreaterThan(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, double self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">", self, other);
		}

		[RubyMethod(">=")]
		public static bool GreaterThanOrEqual(double self, double other)
		{
			if (double.IsNaN(self) || double.IsNaN(other))
			{
				return false;
			}
			return self >= other;
		}

		[RubyMethod(">=")]
		public static bool GreaterThanOrEqual(double self, int other)
		{
			return GreaterThanOrEqual(self, (double)other);
		}

		[RubyMethod(">=")]
		public static bool GreaterThanOrEqual(RubyContext context, double self, [NotNull] BigInteger other)
		{
			return GreaterThanOrEqual(self, Protocols.ConvertToDouble(context, other));
		}

		[RubyMethod(">=")]
		public static bool GreaterThanOrEqual(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, double self, object other)
		{
			return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">=", self, other);
		}

		[RubyMethod("finite?")]
		public static bool IsFinite(double self)
		{
			return !double.IsInfinity(self);
		}

		[RubyMethod("infinite?")]
		public static object IsInfinite(double self)
		{
			if (double.IsInfinity(self))
			{
				return double.IsPositiveInfinity(self) ? 1 : (-1);
			}
			return null;
		}

		[RubyMethod("nan?")]
		public static bool IsNan(double self)
		{
			return double.IsNaN(self);
		}

		[RubyMethod("zero?")]
		public static bool IsZero(double self)
		{
			return self.Equals(0.0);
		}

		private static RubyArray InternalDivMod(double self, double other)
		{
			double num = Math.Floor(self / other);
			double num2 = self - num * other;
			if (other * num2 < 0.0)
			{
				num2 += other;
				num -= 1.0;
			}
			object item = num;
			if (!double.IsInfinity(num) && !double.IsNaN(num))
			{
				item = ToInt(num);
			}
			return RubyOps.MakeArray2(item, num2);
		}

		private static object CastToInteger(double value)
		{
			try
			{
				if (double.IsPositiveInfinity(value))
				{
					throw new FloatDomainError("Infinity");
				}
				if (double.IsNegativeInfinity(value))
				{
					throw new FloatDomainError("-Infinity");
				}
				if (double.IsNaN(value))
				{
					throw new FloatDomainError("NaN");
				}
				return Convert.ToInt32(value);
			}
			catch (OverflowException)
			{
				return BigInteger.Create(value);
			}
		}

		public static Exception CreateFloatDomainError(string message)
		{
			return new FloatDomainError("NaN");
		}

		public static Exception CreateFloatDomainError(string message, Exception inner)
		{
			return new FloatDomainError("NaN", inner);
		}
	}
}

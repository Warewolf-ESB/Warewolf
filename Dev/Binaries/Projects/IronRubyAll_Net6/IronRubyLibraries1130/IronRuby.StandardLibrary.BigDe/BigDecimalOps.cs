using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.BigDecimal
{
	[RubyClass("BigDecimal", Inherits = typeof(Numeric), Extends = typeof(BigDecimal))]
	public sealed class BigDecimalOps
	{
		[RubyConstant]
		public const uint BASE = 1000000000u;

		[RubyConstant]
		public const int EXCEPTION_ALL = 255;

		[RubyConstant]
		public const int EXCEPTION_INFINITY = 1;

		[RubyConstant]
		public const int EXCEPTION_NaN = 2;

		[RubyConstant]
		public const int EXCEPTION_OVERFLOW = 1;

		[RubyConstant]
		public const int EXCEPTION_UNDERFLOW = 4;

		[RubyConstant]
		public const int EXCEPTION_ZERODIVIDE = 1;

		[RubyConstant]
		public const int ROUND_CEILING = 5;

		[RubyConstant]
		public const int ROUND_DOWN = 2;

		[RubyConstant]
		public const int ROUND_FLOOR = 6;

		[RubyConstant]
		public const int ROUND_HALF_DOWN = 4;

		[RubyConstant]
		public const int ROUND_HALF_EVEN = 7;

		[RubyConstant]
		public const int ROUND_HALF_UP = 3;

		[RubyConstant]
		public const int ROUND_UP = 1;

		[RubyConstant]
		public const int ROUND_MODE = 256;

		[RubyConstant]
		public const int SIGN_NEGATIVE_FINITE = -2;

		[RubyConstant]
		public const int SIGN_NEGATIVE_INFINITE = -3;

		[RubyConstant]
		public const int SIGN_NEGATIVE_ZERO = -1;

		[RubyConstant]
		public const int SIGN_NaN = 0;

		[RubyConstant]
		public const int SIGN_POSITIVE_FINITE = 2;

		[RubyConstant]
		public const int SIGN_POSITIVE_INFINITE = 3;

		[RubyConstant]
		public const int SIGN_POSITIVE_ZERO = 1;

		internal static readonly object BigDecimalOpsClassKey = new object();

		internal static BigDecimal.Config GetConfig(RubyContext context)
		{
			ContractUtils.RequiresNotNull(context, "context");
			return (BigDecimal.Config)context.GetOrCreateLibraryData(BigDecimalOpsClassKey, () => new BigDecimal.Config());
		}

		[RubyConstructor]
		public static BigDecimal CreateBigDecimal(RubyContext context, RubyClass self, [DefaultProtocol] MutableString value, [Optional] int n)
		{
			return BigDecimal.Create(GetConfig(context), value.ConvertToString(), n);
		}

		[RubyMethod("_load", RubyMethodAttributes.PublicSingleton)]
		public static BigDecimal Load(RubyContext context, RubyClass self, [DefaultProtocol] MutableString str)
		{
			try
			{
				MutableString[] array = str.Split(new char[1] { ':' }, 2, StringSplitOptions.None);
				int num = 0;
				int maxPrecision = 1;
				string value = "";
				if (array[0] != null || array[0].IsEmpty)
				{
					num = int.Parse(array[0].ToString(), CultureInfo.InvariantCulture);
				}
				if (num != 0)
				{
					maxPrecision = num / 9 + ((num % 9 != 0) ? 1 : 0);
				}
				if (array.Length == 2 && array[1] != null)
				{
					value = array[1].ToString();
				}
				return BigDecimal.Create(GetConfig(context), value, maxPrecision);
			}
			catch
			{
				throw RubyExceptions.CreateTypeError("load failed: invalid character in the marshaled string.");
			}
		}

		[RubyMethod("double_fig", RubyMethodAttributes.PublicSingleton)]
		public static int DoubleFig(RubyClass self)
		{
			return 16;
		}

		[RubyMethod("mode", RubyMethodAttributes.PublicSingleton)]
		public static int Mode(RubyContext context, RubyClass self, int mode)
		{
			if (mode == 256)
			{
				return (int)GetConfig(context).RoundingMode;
			}
			return (int)GetConfig(context).OverflowMode & mode;
		}

		[RubyMethod("mode", RubyMethodAttributes.PublicSingleton)]
		public static int Mode(RubyContext context, RubyClass self, int mode, object value)
		{
			if (value == null)
			{
				return Mode(context, self, mode);
			}
			if (mode == 256)
			{
				if (value is int)
				{
					GetConfig(context).RoundingMode = (BigDecimal.RoundingModes)value;
					return (int)value;
				}
				throw RubyExceptions.CreateUnexpectedTypeError(context, value, "Fixnum");
			}
			if (value is bool)
			{
				BigDecimal.Config config = GetConfig(context);
				if (Enum.IsDefined(typeof(BigDecimal.OverflowExceptionModes), mode))
				{
					if ((bool)value)
					{
						config.OverflowMode |= (BigDecimal.OverflowExceptionModes)mode;
					}
					else
					{
						config.OverflowMode &= (BigDecimal.OverflowExceptionModes)(0xFF ^ mode);
					}
				}
				return (int)config.OverflowMode;
			}
			throw RubyExceptions.CreateTypeError("second argument must be true or false");
		}

		[RubyMethod("limit", RubyMethodAttributes.PublicSingleton)]
		public static int Limit(RubyContext context, RubyClass self, int n)
		{
			if (n < 0)
			{
				throw RubyExceptions.CreateArgumentError("argument must be positive");
			}
			BigDecimal.Config config = GetConfig(context);
			int limit = config.Limit;
			config.Limit = n;
			return limit;
		}

		[RubyMethod("limit", RubyMethodAttributes.PublicSingleton)]
		public static int Limit(RubyContext context, RubyClass self, [Optional] object n)
		{
			if (!(n is Missing))
			{
				throw RubyExceptions.CreateUnexpectedTypeError(context, n, "Fixnum");
			}
			return GetConfig(context).Limit;
		}

		[RubyMethod("ver", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Version(RubyClass self)
		{
			return MutableString.CreateAscii("1.0.1");
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static BigDecimal InducedFrom(RubyClass self, [NotNull] BigDecimal value)
		{
			return value;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static BigDecimal InducedFrom(RubyContext context, RubyClass self, int value)
		{
			return BigDecimal.Create(GetConfig(context), value.ToString(CultureInfo.InvariantCulture));
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static BigDecimal InducedFrom(RubyContext context, RubyClass self, [NotNull] BigInteger value)
		{
			return BigDecimal.Create(GetConfig(context), value.ToString(CultureInfo.InvariantCulture));
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static BigDecimal InducedFrom(RubyClass self, object value)
		{
			throw RubyExceptions.CreateTypeConversionError(self.Context.GetClassDisplayName(value), self.Name);
		}

		[RubyMethod("sign")]
		public static int Sign(BigDecimal self)
		{
			return self.GetSignCode();
		}

		[RubyMethod("exponent")]
		public static int Exponent(BigDecimal self)
		{
			return self.Exponent;
		}

		[RubyMethod("precs")]
		public static RubyArray Precision(BigDecimal self)
		{
			return RubyOps.MakeArray2(self.Precision * 9, self.MaxPrecision * 9);
		}

		[RubyMethod("split")]
		public static RubyArray Split(BigDecimal self)
		{
			return RubyOps.MakeArray4(self.Sign, MutableString.CreateAscii(self.GetFractionString()), 10, self.Exponent);
		}

		[RubyMethod("fix")]
		public static BigDecimal Fix(RubyContext context, BigDecimal self)
		{
			return BigDecimal.IntegerPart(GetConfig(context), self);
		}

		[RubyMethod("frac")]
		public static BigDecimal Fraction(RubyContext context, BigDecimal self)
		{
			return BigDecimal.FractionalPart(GetConfig(context), self);
		}

		[RubyMethod("to_s")]
		public static MutableString ToString(BigDecimal self)
		{
			return MutableString.CreateAscii(self.ToString());
		}

		[RubyMethod("to_s")]
		public static MutableString ToString(BigDecimal self, [DefaultProtocol] int separateAt)
		{
			return MutableString.CreateAscii(self.ToString(separateAt));
		}

		[RubyMethod("to_s")]
		public static MutableString ToString(BigDecimal self, [DefaultProtocol][NotNull] MutableString format)
		{
			string text = "";
			int num = 0;
			Match match = Regex.Match(format.ConvertToString(), "^(?<posSign>[+ ])?(?<separateAt>\\d+)?(?<floatFormat>[fF])?", RegexOptions.ExplicitCapture);
			Group group = match.Groups["posSign"];
			Group group2 = match.Groups["separateAt"];
			Group group3 = match.Groups["floatFormat"];
			if (group.Success)
			{
				text = match.Groups["posSign"].Value;
			}
			if (group2.Success)
			{
				num = int.Parse(match.Groups["separateAt"].Value, CultureInfo.InvariantCulture);
			}
			bool success = group3.Success;
			return MutableString.CreateAscii(self.ToString(num, text, success));
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, BigDecimal self)
		{
			MutableString mutableString = MutableString.CreateMutable(context.GetIdentifierEncoding());
			mutableString.AppendFormat("#<{0}:", context.GetClassOf(self).Name);
			RubyUtils.AppendFormatHexObjectId(mutableString, RubyUtils.GetObjectId(context, self));
			mutableString.AppendFormat(",'{0}',{1}({2})>", self.ToString(10), self.PrecisionDigits, self.MaxPrecisionDigits);
			return mutableString;
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(BigDecimal self, BigDecimal other)
		{
			return RubyOps.MakeArray2(other, self);
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(RubyContext context, BigDecimal self, double other)
		{
			return RubyOps.MakeArray2(other, ToFloat(context, self));
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(RubyContext context, BigDecimal self, int other)
		{
			return RubyOps.MakeArray2(BigDecimal.Create(GetConfig(context), other.ToString(CultureInfo.InvariantCulture)), self);
		}

		[RubyMethod("coerce")]
		public static RubyArray Coerce(RubyContext context, BigDecimal self, BigInteger other)
		{
			return RubyOps.MakeArray2(BigDecimal.Create(GetConfig(context), other.ToString(CultureInfo.InvariantCulture)), self);
		}

		[RubyMethod("_dump")]
		public static MutableString Dump(BigDecimal self, [Optional] object limit)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.MaxPrecisionDigits.ToString(CultureInfo.InvariantCulture)).Append(':')
				.Append(self.ToString());
		}

		[RubyMethod("to_f")]
		public static double ToFloat(RubyContext context, BigDecimal self)
		{
			return BigDecimal.ToFloat(GetConfig(context), self);
		}

		[RubyMethod("to_i")]
		[RubyMethod("to_int")]
		public static object ToI(RubyContext context, BigDecimal self)
		{
			return BigDecimal.ToInteger(GetConfig(context), self);
		}

		[RubyMethod("hash")]
		public static int Hash(BigDecimal self)
		{
			return self.GetHashCode();
		}

		[RubyMethod("add")]
		[RubyMethod("+")]
		public static BigDecimal Add(RubyContext context, BigDecimal self, [NotNull] BigDecimal other)
		{
			return BigDecimal.Add(GetConfig(context), self, other);
		}

		[RubyMethod("+")]
		[RubyMethod("add")]
		public static BigDecimal Add(RubyContext context, BigDecimal self, int other)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Add(config, self, BigDecimal.Create(config, other));
		}

		[RubyMethod("+")]
		[RubyMethod("add")]
		public static BigDecimal Add(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Add(config, self, BigDecimal.Create(config, other));
		}

		[RubyMethod("+")]
		[RubyMethod("add")]
		public static object Add(BinaryOpStorage coercionStorage, BinaryOpStorage opStorage, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, opStorage, "+", self, other);
		}

		[RubyMethod("add")]
		public static BigDecimal Add(RubyContext context, BigDecimal self, [NotNull] BigDecimal other, int n)
		{
			return BigDecimal.Add(GetConfig(context), self, other, n);
		}

		[RubyMethod("add")]
		public static BigDecimal Add(RubyContext context, BigDecimal self, int other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Add(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("add")]
		public static BigDecimal Add(RubyContext context, BigDecimal self, [NotNull] BigInteger other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Add(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("add")]
		public static BigDecimal Add(RubyContext context, BigDecimal self, double other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Add(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("add")]
		public static object Add(BinaryOpStorage coercionStorage, BinaryOpStorage opStorage, BigDecimal self, object other, [DefaultProtocol] int n)
		{
			return Protocols.CoerceAndApply(coercionStorage, opStorage, "+", self, other);
		}

		[RubyMethod("-")]
		[RubyMethod("sub")]
		public static BigDecimal Subtract(RubyContext context, BigDecimal self, BigDecimal other)
		{
			return BigDecimal.Subtract(GetConfig(context), self, other);
		}

		[RubyMethod("sub")]
		[RubyMethod("-")]
		public static BigDecimal Subtract(RubyContext context, BigDecimal self, int other)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Subtract(config, self, BigDecimal.Create(config, other));
		}

		[RubyMethod("sub")]
		[RubyMethod("-")]
		public static BigDecimal Subtract(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Subtract(config, self, BigDecimal.Create(config, other));
		}

		[RubyMethod("sub")]
		[RubyMethod("-")]
		public static object Subtract(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", self, other);
		}

		[RubyMethod("sub")]
		public static BigDecimal Subtract(RubyContext context, BigDecimal self, BigDecimal other, int n)
		{
			return BigDecimal.Subtract(GetConfig(context), self, other, n);
		}

		[RubyMethod("sub")]
		public static BigDecimal Subtract(RubyContext context, BigDecimal self, int other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Subtract(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("sub")]
		public static BigDecimal Subtract(RubyContext context, BigDecimal self, [NotNull] BigInteger other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Subtract(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("sub")]
		public static BigDecimal Subtract(RubyContext context, BigDecimal self, double other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Subtract(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("sub")]
		public static object Subtract(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, BigDecimal self, object other, [DefaultProtocol] int n)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "sub", self, other);
		}

		[RubyMethod("*")]
		public static BigDecimal Multiply(RubyContext context, BigDecimal self, BigDecimal other)
		{
			return BigDecimal.Multiply(GetConfig(context), self, other);
		}

		[RubyMethod("*")]
		public static BigDecimal Multiply(RubyContext context, BigDecimal self, int other)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Multiply(config, self, BigDecimal.Create(config, other));
		}

		[RubyMethod("*")]
		public static BigDecimal Multiply(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Multiply(config, self, BigDecimal.Create(config, other));
		}

		[RubyMethod("*")]
		public static object Multiply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "*", self, other);
		}

		[RubyMethod("mult")]
		public static BigDecimal Multiply(RubyContext context, BigDecimal self, BigDecimal other, int n)
		{
			return BigDecimal.Multiply(GetConfig(context), self, other, n);
		}

		[RubyMethod("mult")]
		public static BigDecimal Multiply(RubyContext context, BigDecimal self, int other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Multiply(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("mult")]
		public static BigDecimal Multiply(RubyContext context, BigDecimal self, [NotNull] BigInteger other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Multiply(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("mult")]
		public static BigDecimal Multiply(RubyContext context, BigDecimal self, double other, int n)
		{
			BigDecimal.Config config = GetConfig(context);
			return BigDecimal.Multiply(config, self, BigDecimal.Create(config, other), n);
		}

		[RubyMethod("mult")]
		public static object Multiply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, BigDecimal self, object other, [DefaultProtocol] int n)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "mult", self, other);
		}

		[RubyMethod("/")]
		[RubyMethod("quo")]
		public static BigDecimal Divide(RubyContext context, BigDecimal self, BigDecimal other)
		{
			BigDecimal remainder;
			return BigDecimal.Divide(GetConfig(context), self, other, 0, out remainder);
		}

		[RubyMethod("/")]
		public static object Divide(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "/", self, other);
		}

		[RubyMethod("quo")]
		public static object Quotient(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "quo", self, other);
		}

		[RubyMethod("div")]
		public static BigDecimal Div(RubyContext context, BigDecimal self, BigDecimal other)
		{
			if (BigDecimal.IsFinite(other))
			{
				BigDecimal.Config config = GetConfig(context);
				BigDecimal remainder;
				BigDecimal x = BigDecimal.Divide(config, self, other, 0, out remainder);
				if (BigDecimal.IsFinite(x))
				{
					return BigDecimal.IntegerPart(config, x);
				}
			}
			return BigDecimal.NaN;
		}

		[RubyMethod("div")]
		public static BigDecimal Div(RubyContext context, BigDecimal self, BigDecimal other, int n)
		{
			if (n < 0)
			{
				throw RubyExceptions.CreateArgumentError("argument must be positive");
			}
			BigDecimal remainder;
			return BigDecimal.Divide(GetConfig(context), self, other, n, out remainder);
		}

		[RubyMethod("modulo")]
		[RubyMethod("%")]
		public static BigDecimal Modulo(RubyContext context, BigDecimal self, [NotNull] BigDecimal other)
		{
			BigDecimal div;
			BigDecimal mod;
			BigDecimal.DivMod(GetConfig(context), self, other, out div, out mod);
			return mod;
		}

		[RubyMethod("modulo")]
		[RubyMethod("%")]
		public static BigDecimal Modulo(RubyContext context, BigDecimal self, int other)
		{
			return Modulo(context, self, BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("%")]
		[RubyMethod("modulo")]
		public static BigDecimal Modulo(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return Modulo(context, self, BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("modulo")]
		public static object Modulo(BinaryOpStorage moduloStorage, RubyContext context, BigDecimal self, double other)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = moduloStorage.GetCallSite("modulo");
			return callSite.Target(callSite, BigDecimal.ToFloat(GetConfig(context), self), other);
		}

		[RubyMethod("%")]
		public static object ModuloOp(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, RubyContext context, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "%", self, other);
		}

		[RubyMethod("modulo")]
		public static object Modulo(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, RubyContext context, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "modulo", self, other);
		}

		[RubyMethod("power")]
		[RubyMethod("**")]
		public static BigDecimal Power(RubyContext context, BigDecimal self, int other)
		{
			return BigDecimal.Power(GetConfig(context), self, other);
		}

		[RubyMethod("+@")]
		public static BigDecimal Identity(BigDecimal self)
		{
			return self;
		}

		[RubyMethod("-@")]
		public static BigDecimal Negate(RubyContext context, BigDecimal self)
		{
			return BigDecimal.Negate(GetConfig(context), self);
		}

		[RubyMethod("abs")]
		public static BigDecimal Abs(RubyContext context, BigDecimal self)
		{
			return BigDecimal.Abs(GetConfig(context), self);
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(RubyContext context, BigDecimal self, [NotNull] BigDecimal other)
		{
			BigDecimal div;
			BigDecimal mod;
			BigDecimal.DivMod(GetConfig(context), self, other, out div, out mod);
			return RubyOps.MakeArray2(div, mod);
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(RubyContext context, BigDecimal self, int other)
		{
			return DivMod(context, self, BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("divmod")]
		public static RubyArray DivMod(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return DivMod(context, self, BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("divmod")]
		public static object DivMod(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, RubyContext context, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "divmod", self, other);
		}

		[RubyMethod("remainder")]
		public static BigDecimal Remainder(RubyContext context, BigDecimal self, [NotNull] BigDecimal other)
		{
			BigDecimal bigDecimal = Modulo(context, self, other);
			if (self.Sign == other.Sign)
			{
				return bigDecimal;
			}
			return BigDecimal.Subtract(GetConfig(context), bigDecimal, other);
		}

		[RubyMethod("remainder")]
		public static object Remainder(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpSite, BigDecimal self, object other)
		{
			return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "remainder", self, other);
		}

		[RubyMethod("sqrt")]
		public static BigDecimal SquareRoot(RubyContext context, BigDecimal self, int n)
		{
			return BigDecimal.SquareRoot(GetConfig(context), self, n);
		}

		[RubyMethod("sqrt")]
		public static object SquareRoot(RubyContext context, BigDecimal self, object n)
		{
			throw RubyExceptions.CreateUnexpectedTypeError(context, n, "Fixnum");
		}

		[RubyMethod("<=>")]
		public static object Compare(BigDecimal self, [NotNull] BigDecimal other)
		{
			return self.CompareBigDecimal(other);
		}

		[RubyMethod("<=>")]
		public static object Compare(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("<=>")]
		public static object Compare(RubyContext context, BigDecimal self, int other)
		{
			return self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("<=>")]
		public static object Compare(RubyContext context, BigDecimal self, double other)
		{
			return self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, BigDecimal self, object other)
		{
			return Protocols.CoerceAndCompare(coercionStorage, comparisonStorage, self, other);
		}

		private static object GreaterThenResult(int? comparisonResult)
		{
			if (!comparisonResult.HasValue)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(comparisonResult.Value > 0);
		}

		[RubyMethod(">")]
		public static object GreaterThan(BigDecimal self, [NotNull] BigDecimal other)
		{
			return GreaterThenResult(self.CompareBigDecimal(other));
		}

		[RubyMethod(">")]
		public static object GreaterThan(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return GreaterThenResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod(">")]
		public static object GreaterThan(RubyContext context, BigDecimal self, int other)
		{
			return GreaterThenResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod(">")]
		public static object GreaterThan(RubyContext context, BigDecimal self, double other)
		{
			return GreaterThenResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod(">")]
		public static object GreaterThan(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, BigDecimal self, object other)
		{
			return Protocols.TryCoerceAndApply(coercionStorage, comparisonStorage, ">", self, other);
		}

		private static object GreaterThanOrEqualResult(int? comparisonResult)
		{
			if (!comparisonResult.HasValue)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(comparisonResult.Value >= 0);
		}

		[RubyMethod(">=")]
		public static object GreaterThanOrEqual(BigDecimal self, [NotNull] BigDecimal other)
		{
			return GreaterThanOrEqualResult(self.CompareBigDecimal(other));
		}

		[RubyMethod(">=")]
		public static object GreaterThanOrEqual(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return GreaterThanOrEqualResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod(">=")]
		public static object GreaterThanOrEqual(RubyContext context, BigDecimal self, int other)
		{
			return GreaterThanOrEqualResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod(">=")]
		public static object GreaterThanOrEqual(RubyContext context, BigDecimal self, double other)
		{
			return GreaterThanOrEqualResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod(">=")]
		public static object GreaterThanOrEqual(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, BigDecimal self, object other)
		{
			return Protocols.TryCoerceAndApply(coercionStorage, comparisonStorage, ">=", self, other);
		}

		private static object LessThenResult(int? comparisonResult)
		{
			if (!comparisonResult.HasValue)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(comparisonResult.Value < 0);
		}

		[RubyMethod("<")]
		public static object LessThan(BigDecimal self, [NotNull] BigDecimal other)
		{
			return LessThenResult(self.CompareBigDecimal(other));
		}

		[RubyMethod("<")]
		public static object LessThan(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return LessThenResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod("<")]
		public static object LessThan(RubyContext context, BigDecimal self, int other)
		{
			return LessThenResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod("<")]
		public static object LessThan(RubyContext context, BigDecimal self, double other)
		{
			return LessThenResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod("<")]
		public static object LessThan(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, BigDecimal self, object other)
		{
			return Protocols.TryCoerceAndApply(coercionStorage, comparisonStorage, "<", self, other);
		}

		private static object LessThanOrEqualResult(int? comparisonResult)
		{
			if (!comparisonResult.HasValue)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(comparisonResult.Value <= 0);
		}

		[RubyMethod("<=")]
		public static object LessThanOrEqual(BigDecimal self, [NotNull] BigDecimal other)
		{
			return LessThanOrEqualResult(self.CompareBigDecimal(other));
		}

		[RubyMethod("<=")]
		public static object LessThanOrEqual(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return LessThanOrEqualResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod("<=")]
		public static object LessThanOrEqual(RubyContext context, BigDecimal self, int other)
		{
			return LessThanOrEqualResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod("<=")]
		public static object LessThanOrEqual(RubyContext context, BigDecimal self, double other)
		{
			return LessThanOrEqualResult(self.CompareBigDecimal(BigDecimal.Create(GetConfig(context), other)));
		}

		[RubyMethod("<=")]
		public static object LessThanOrEqual(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, BigDecimal self, object other)
		{
			return Protocols.TryCoerceAndApply(coercionStorage, comparisonStorage, "<=", self, other);
		}

		[RubyMethod("eql?")]
		[RubyMethod("==")]
		[RubyMethod("===")]
		public static object Equal(BigDecimal self, [NotNull] BigDecimal other)
		{
			if (BigDecimal.IsNaN(self) || BigDecimal.IsNaN(other))
			{
				return null;
			}
			return self.Equals(other);
		}

		[RubyMethod("===")]
		[RubyMethod("==")]
		[RubyMethod("eql?")]
		public static bool Equal(RubyContext context, BigDecimal self, int other)
		{
			return self.Equals(BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("==")]
		[RubyMethod("eql?")]
		[RubyMethod("===")]
		public static bool Equal(RubyContext context, BigDecimal self, [NotNull] BigInteger other)
		{
			return self.Equals(BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("==")]
		[RubyMethod("eql?")]
		[RubyMethod("===")]
		public static bool Equal(RubyContext context, BigDecimal self, double other)
		{
			return self.Equals(BigDecimal.Create(GetConfig(context), other));
		}

		[RubyMethod("===")]
		[RubyMethod("eql?")]
		[RubyMethod("==")]
		public static object Equal(BinaryOpStorage equals, BigDecimal self, object other)
		{
			if (other == null)
			{
				return null;
			}
			return Protocols.IsEqual(equals, other, self);
		}

		[RubyMethod("ceil")]
		public static BigDecimal Ceil(RubyContext context, BigDecimal self, [Optional] int n)
		{
			return BigDecimal.LimitPrecision(GetConfig(context), self, n, BigDecimal.RoundingModes.Ceiling);
		}

		[RubyMethod("floor")]
		public static BigDecimal Floor(RubyContext context, BigDecimal self, [Optional] int n)
		{
			return BigDecimal.LimitPrecision(GetConfig(context), self, n, BigDecimal.RoundingModes.Floor);
		}

		[RubyMethod("round")]
		public static BigDecimal Round(RubyContext context, BigDecimal self, [Optional] int n)
		{
			return BigDecimal.LimitPrecision(GetConfig(context), self, n, GetConfig(context).RoundingMode);
		}

		[RubyMethod("round")]
		public static BigDecimal Round(RubyContext context, BigDecimal self, int n, int mode)
		{
			return BigDecimal.LimitPrecision(GetConfig(context), self, n, (BigDecimal.RoundingModes)mode);
		}

		[RubyMethod("truncate")]
		public static BigDecimal Truncate(RubyContext context, BigDecimal self, [Optional] int n)
		{
			return BigDecimal.LimitPrecision(GetConfig(context), self, n, BigDecimal.RoundingModes.Down);
		}

		[RubyMethod("finite?")]
		public static bool IsFinite(BigDecimal self)
		{
			return BigDecimal.IsFinite(self);
		}

		[RubyMethod("infinite?")]
		public static object IsInfinite(BigDecimal self)
		{
			if (BigDecimal.IsInfinite(self))
			{
				return self.Sign;
			}
			return null;
		}

		[RubyMethod("nan?")]
		public static bool IsNaN(BigDecimal self)
		{
			return BigDecimal.IsNaN(self);
		}

		[RubyMethod("nonzero?")]
		public static BigDecimal IsNonZero(BigDecimal self)
		{
			if (!BigDecimal.IsZero(self))
			{
				return self;
			}
			return null;
		}

		[RubyMethod("zero?")]
		public static bool IsZero(BigDecimal self)
		{
			return BigDecimal.IsZero(self);
		}
	}
}

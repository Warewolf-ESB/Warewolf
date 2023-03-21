using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using Microsoft.Scripting.Math;

namespace IronRuby.StandardLibrary.BigDecimal
{
	public class BigDecimal : IComparable<BigDecimal>, IEquatable<BigDecimal>
	{
		public class Config
		{
			public int Limit { get; set; }

			public RoundingModes RoundingMode { get; set; }

			public OverflowExceptionModes OverflowMode { get; set; }

			public Config()
			{
				Limit = 0;
				RoundingMode = RoundingModes.HalfUp;
				OverflowMode = OverflowExceptionModes.None;
			}
		}

		public enum RoundingModes
		{
			None,
			Up,
			Down,
			HalfUp,
			HalfDown,
			Ceiling,
			Floor,
			HalfEven
		}

		[Flags]
		public enum OverflowExceptionModes
		{
			None = 0,
			NaN = 2,
			Infinity = 1,
			Underflow = 4,
			Overflow = 1,
			ZeroDivide = 1,
			All = 0xFF
		}

		public enum NumberTypes
		{
			NaN,
			Finite,
			Infinite
		}

		private enum BasicOperations
		{
			Add,
			Subtract,
			Multiply,
			Divide
		}

		private const string NaNString = "NaN";

		private const string InfinityString = "Infinity";

		private const string NegativeInfinityString = "-Infinity";

		private const string ZeroString = "0.0";

		private const string NegativeZeroString = "-0.0";

		public const uint BASE = 1000000000u;

		public const int BASE_FIG = 9;

		public const int DOUBLE_FIG = 16;

		private readonly NumberTypes _type;

		private readonly int _sign;

		private readonly Fraction _fraction;

		private readonly int _exponent;

		private readonly int _maxPrecision;

		public static readonly BigDecimal One;

		public static readonly BigDecimal Half;

		public static readonly BigDecimal NaN;

		public static readonly BigDecimal PositiveInfinity;

		public static readonly BigDecimal NegativeInfinity;

		public static readonly BigDecimal PositiveZero;

		public static readonly BigDecimal NegativeZero;

		private static readonly int?[,,,,,] SC;

		private static readonly BigDecimal[,,,,,,] SR;

		public NumberTypes NumberType
		{
			get
			{
				return _type;
			}
		}

		public int Sign
		{
			get
			{
				return _sign;
			}
		}

		public int Exponent
		{
			get
			{
				return _exponent;
			}
		}

		public int Precision
		{
			get
			{
				return _fraction.Precision;
			}
		}

		public int MaxPrecision
		{
			get
			{
				return _maxPrecision;
			}
		}

		public int Digits
		{
			get
			{
				return _fraction.DigitCount;
			}
		}

		public int MaxPrecisionDigits
		{
			get
			{
				return _maxPrecision * 9;
			}
		}

		public int PrecisionDigits
		{
			get
			{
				return _fraction.Precision * 9;
			}
		}

		static BigDecimal()
		{
			One = new BigDecimal(1, Fraction.One, 1);
			Half = new BigDecimal(1, Fraction.Five, 0);
			NaN = new BigDecimal(NumberTypes.NaN, 0);
			PositiveInfinity = new BigDecimal(NumberTypes.Infinite, 1);
			NegativeInfinity = new BigDecimal(NumberTypes.Infinite, -1);
			PositiveZero = new BigDecimal(1, Fraction.Zero, 1);
			NegativeZero = new BigDecimal(-1, Fraction.Zero, 1);
			SC = new int?[2, 2, 3, 2, 2, 3];
			SR = new BigDecimal[2, 2, 3, 4, 2, 2, 3];
			CreateSC();
			CreateSR();
		}

		public BigDecimal()
			: this(0, NaN)
		{
		}

		private BigDecimal(int sign, Fraction fraction, int exponent)
			: this(sign, fraction, exponent, fraction.Precision)
		{
		}

		private BigDecimal(int sign, Fraction fraction, int exponent, int maxPrecision)
		{
			_type = NumberTypes.Finite;
			_sign = sign;
			_fraction = fraction;
			_exponent = exponent;
			_maxPrecision = ((maxPrecision == 0) ? fraction.Precision : maxPrecision);
		}

		private BigDecimal(int sign, BigDecimal copyFrom)
		{
			_type = copyFrom._type;
			_sign = sign;
			_fraction = copyFrom._fraction;
			_exponent = copyFrom._exponent;
			_maxPrecision = copyFrom._maxPrecision;
		}

		private BigDecimal(int sign, BigDecimal copyFrom, int maxPrecision)
		{
			_type = copyFrom._type;
			_sign = sign;
			_fraction = new Fraction(copyFrom._fraction, maxPrecision);
			_exponent = copyFrom._exponent;
			_maxPrecision = maxPrecision;
		}

		private BigDecimal(NumberTypes type, int sign)
			: this(type, sign, 1)
		{
		}

		private BigDecimal(NumberTypes type, int sign, int maxPrecision)
		{
			_type = type;
			_sign = sign;
			_fraction = Fraction.Zero;
			_exponent = 0;
			_maxPrecision = maxPrecision;
		}

		public static BigDecimal Create(Config config, object value)
		{
			return Create(config, value.ToString(), 0);
		}

		public static BigDecimal Create(Config config, double value)
		{
			return Create(config, value.ToString(CultureInfo.InvariantCulture), 0);
		}

		public static BigDecimal Create(Config config, string value)
		{
			return Create(config, value, 0);
		}

		public static BigDecimal Create(Config config, string value, int maxPrecision)
		{
			value = value.Trim();
			BigDecimal bigDecimal = CheckSpecialCases(value);
			if (bigDecimal == null)
			{
				Match match = Regex.Match(value, "^(?<sign>[-+]?)(?<integer>[\\d_]*)\\.?(?<fraction>[\\d_]*)([eEdD](?<exponent>[-+]?[\\d_]+))?", RegexOptions.ExplicitCapture);
				int sign = ((!(match.Groups["sign"].Value == "-")) ? 1 : (-1));
				string text = (match.Groups["integer"].Value ?? "0").TrimStart('0').Replace("_", "");
				string text2 = (match.Groups["fraction"].Value ?? "").TrimEnd('0').Replace("_", "");
				string text3 = text + text2;
				string text4 = text3.TrimStart('0');
				string digits = text4.Trim('0');
				Fraction fraction = Fraction.Create(digits);
				string text5 = match.Groups["exponent"].Value.Replace("_", "");
				int num = 0;
				if (!fraction.IsZero)
				{
					if (!string.IsNullOrEmpty(text5))
					{
						try
						{
							num = int.Parse(text5, CultureInfo.InvariantCulture);
						}
						catch (OverflowException)
						{
							num = ((!text5.StartsWith("-", StringComparison.Ordinal)) ? 1 : (-1));
							return ExponentOverflow(config, sign, num);
						}
					}
					num += text.Length;
					num -= text3.Length - text4.Length;
				}
				bigDecimal = new BigDecimal(sign, fraction, num, maxPrecision);
			}
			return CheckOverflowExceptions(config, bigDecimal);
		}

		private static BigDecimal CheckSpecialCases(string value)
		{
			BigDecimal result = null;
			if (value == null)
			{
				result = PositiveZero;
			}
			else
			{
				value = value.Trim();
				if (value == "NaN")
				{
					result = NaN;
				}
				else if (value.Contains("Infinity"))
				{
					switch (value)
					{
					case "+Infinity":
					case "Infinity":
						result = PositiveInfinity;
						break;
					case "-Infinity":
						result = NegativeInfinity;
						break;
					default:
						result = PositiveZero;
						break;
					}
				}
			}
			return result;
		}

		public static bool IsNaN(BigDecimal x)
		{
			return x._type == NumberTypes.NaN;
		}

		public static bool IsInfinite(BigDecimal x)
		{
			return x._type == NumberTypes.Infinite;
		}

		public static bool IsFinite(BigDecimal x)
		{
			return x._type == NumberTypes.Finite;
		}

		public static bool IsZero(BigDecimal x)
		{
			if (IsFinite(x))
			{
				return x._fraction.IsZero;
			}
			return false;
		}

		public static bool IsNonZeroFinite(BigDecimal x)
		{
			if (IsFinite(x))
			{
				return !x._fraction.IsZero;
			}
			return false;
		}

		public static bool IsPositive(BigDecimal x)
		{
			return x._sign == 1;
		}

		public static bool IsNegative(BigDecimal x)
		{
			return x._sign == -1;
		}

		public static bool IsPositiveInfinite(BigDecimal x)
		{
			if (IsInfinite(x))
			{
				return IsPositive(x);
			}
			return false;
		}

		public static bool IsNegativeInfinite(BigDecimal x)
		{
			if (IsInfinite(x))
			{
				return IsNegative(x);
			}
			return false;
		}

		public static bool IsPositiveZero(BigDecimal x)
		{
			if (IsZero(x))
			{
				return IsPositive(x);
			}
			return false;
		}

		public static bool IsNegativeZero(BigDecimal x)
		{
			if (IsZero(x))
			{
				return IsNegative(x);
			}
			return false;
		}

		public static bool IsOne(BigDecimal x)
		{
			if (x._fraction.IsOne)
			{
				return x._exponent == 1;
			}
			return false;
		}

		public static bool IsPositiveOne(BigDecimal x)
		{
			if (IsOne(x))
			{
				return IsPositive(x);
			}
			return false;
		}

		public static bool IsNegativeOne(BigDecimal x)
		{
			if (IsOne(x))
			{
				return IsPositive(x);
			}
			return false;
		}

		public static double ToFloat(Config config, BigDecimal x)
		{
			try
			{
				if (IsNegativeZero(x))
				{
					return -0.0;
				}
				return double.Parse(x.ToString(0, "", true), CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				return double.PositiveInfinity;
			}
		}

		public static object ToInteger(Config config, BigDecimal x)
		{
			if (IsFinite(x))
			{
				BigDecimal bigDecimal = IntegerPart(config, x);
				BigInteger bigInteger = BigInteger.Create(0);
				string text = bigDecimal._fraction.ToString();
				string text2 = text;
				foreach (char c in text2)
				{
					bigInteger *= (BigInteger)10;
					bigInteger += (BigInteger)(c - 48);
				}
				if (IsNegative(x))
				{
					bigInteger = BigInteger.Negate(bigInteger);
				}
				return ClrBigInteger.Multiply(bigInteger, BigInteger.Create(10).Power(bigDecimal.Exponent - text.Length));
			}
			return null;
		}

		public static BigDecimal Abs(Config config, BigDecimal x)
		{
			if (IsNegative(x))
			{
				return Negate(config, x);
			}
			return x;
		}

		public static BigDecimal Negate(Config config, BigDecimal x)
		{
			if (IsFinite(x))
			{
				return new BigDecimal(-x._sign, x._fraction, x._exponent);
			}
			return new BigDecimal(x.NumberType, -x.Sign);
		}

		public static BigDecimal Add(Config config, BigDecimal x, BigDecimal y)
		{
			return Add(config, x, y, 0);
		}

		public static BigDecimal Add(Config config, BigDecimal x, BigDecimal y, int limit)
		{
			return InternalAdd(config, x, y, limit);
		}

		public static BigDecimal Subtract(Config config, BigDecimal x, BigDecimal y)
		{
			return Subtract(config, x, y, 0);
		}

		public static BigDecimal Subtract(Config config, BigDecimal x, BigDecimal y, int limit)
		{
			return InternalAdd(config, x, Negate(config, y), limit);
		}

		public static BigDecimal Multiply(Config config, BigDecimal x, BigDecimal y)
		{
			return Multiply(config, x, y, 0);
		}

		public static BigDecimal Multiply(Config config, BigDecimal x, BigDecimal y, int limit)
		{
			BigDecimal bigDecimal = CheckSpecialResult(config, x, y, BasicOperations.Multiply);
			if (bigDecimal != null)
			{
				return bigDecimal;
			}
			if (limit == 0)
			{
				limit = config.Limit;
			}
			if (limit == 0)
			{
				limit = x.Digits + y.Digits;
			}
			int sign = y._sign * x._sign;
			if (IsOne(x))
			{
				return LimitPrecision(config, new BigDecimal(sign, y._fraction, y._exponent), limit, config.RoundingMode);
			}
			if (IsOne(y))
			{
				return LimitPrecision(config, new BigDecimal(sign, x._fraction, x._exponent), limit, config.RoundingMode);
			}
			Fraction fraction;
			int exponent;
			try
			{
				int offset;
				fraction = Fraction.Multiply(x._fraction, y._fraction, out offset);
				int offset2;
				fraction = Fraction.LimitPrecision(sign, fraction, limit, config.RoundingMode, out offset2);
				exponent = checked(x._exponent + y._exponent + offset + offset2);
			}
			catch (OverflowException)
			{
				return ExponentOverflow(config, sign, x._exponent);
			}
			return new BigDecimal(sign, fraction, exponent, limit);
		}

		public static BigDecimal Divide(Config config, BigDecimal x, BigDecimal y, int limit, out BigDecimal remainder)
		{
			BigDecimal bigDecimal = CheckSpecialResult(config, x, y, BasicOperations.Divide);
			if (bigDecimal != null)
			{
				remainder = PositiveZero;
				return bigDecimal;
			}
			if (limit == 0)
			{
				limit = config.Limit;
			}
			int sign = x._sign * y._sign;
			if (IsOne(y))
			{
				remainder = PositiveZero;
				return new BigDecimal(sign, x._fraction, x._exponent);
			}
			int minPrecision = (int)Math.Ceiling((double)limit / 9.0) + 1;
			Fraction r;
			int cOffset;
			int rOffset;
			Fraction fraction = Fraction.Divide(x._fraction, y._fraction, minPrecision, out r, out cOffset, out rOffset);
			int offset;
			if (limit == 0)
			{
				offset = 0;
			}
			else
			{
				fraction = Fraction.LimitPrecision(sign, fraction, limit, config.RoundingMode, out offset);
			}
			checked
			{
				int exponent;
				int exponent2;
				try
				{
					exponent = x._exponent - y._exponent + cOffset + offset;
					exponent2 = x._exponent - y._exponent + rOffset;
				}
				catch (OverflowException)
				{
					remainder = PositiveZero;
					return ExponentOverflow(config, sign, x._exponent);
				}
				remainder = new BigDecimal(sign, r, exponent2, r.MaxPrecision);
				return new BigDecimal(sign, fraction, exponent, fraction.MaxPrecision);
			}
		}

		public static void DivMod(Config config, BigDecimal x, BigDecimal y, out BigDecimal div, out BigDecimal mod)
		{
			div = NaN;
			mod = NaN;
			if (IsFinite(x) && IsFinite(y))
			{
				BigDecimal remainder;
				BigDecimal x2 = Divide(config, x, y, 0, out remainder);
				if (IsFinite(x2))
				{
					div = LimitPrecision(config, x2, 0, RoundingModes.Floor);
					mod = Subtract(config, x, Multiply(config, div, y));
				}
			}
		}

		public static BigDecimal Power(Config config, BigDecimal x, int power)
		{
			if (!IsFinite(x))
			{
				return CheckOverflowExceptions(config, NaN);
			}
			if (power == 0)
			{
				return One;
			}
			int sign = ((power % 2 == 0) ? 1 : x.Sign);
			if (IsOne(x))
			{
				return new BigDecimal(sign, One);
			}
			if (IsZero(x))
			{
				if (power < 0)
				{
					return CheckOverflowExceptions(config, new BigDecimal(sign, PositiveInfinity));
				}
				return new BigDecimal(sign, PositiveZero);
			}
			BigDecimal bigDecimal = x;
			int num = Math.Abs(power) - 1;
			int num2 = x.Precision * (num + 2);
			while (num > 0)
			{
				BigDecimal bigDecimal2 = new BigDecimal(x.Sign, x, num2);
				int i = 2;
				int num3 = 1;
				for (; i <= num; i += i)
				{
					bigDecimal2 = Multiply(config, bigDecimal2, bigDecimal2);
					num3 = i;
				}
				num -= num3;
				bigDecimal = Multiply(config, bigDecimal, bigDecimal2);
			}
			if (power < 0)
			{
				BigDecimal remainder;
				bigDecimal = Divide(config, One, bigDecimal, num2 * 10, out remainder);
			}
			return bigDecimal;
		}

		public static BigDecimal SquareRoot(Config config, BigDecimal x, int limit)
		{
			if (limit < 0)
			{
				throw new ArgumentException("argument must be positive");
			}
			if (IsZero(x) || IsPositiveOne(x))
			{
				return x;
			}
			if (x.Sign < 0)
			{
				throw new FloatDomainError("SQRT(negative value)");
			}
			if (IsNaN(x))
			{
				throw new FloatDomainError("SQRT(NaN)");
			}
			if (!IsFinite(x))
			{
				return CheckOverflowExceptions(config, x);
			}
			if (limit == 0)
			{
				limit = config.Limit;
			}
			int exponent = x.Exponent;
			exponent = ((exponent <= 0) ? (exponent - 1) : (exponent + 1));
			exponent -= Math.Max(x.PrecisionDigits, limit + 16);
			BigDecimal x2 = new BigDecimal(x._sign, x._fraction, 0, x._maxPrecision);
			int num = x._exponent / 2;
			if (x._exponent - num * 2 != 0)
			{
				num = (x._exponent + 1) / 2;
				x2 = Multiply(config, x2, Create(config, 0.1));
			}
			double d = ToFloat(config, x2);
			BigDecimal bigDecimal = Create(config, Math.Sqrt(d));
			bigDecimal = new BigDecimal(bigDecimal._sign, bigDecimal._fraction, bigDecimal._exponent + num);
			BigDecimal remainder = PositiveZero;
			BigDecimal positiveZero = PositiveZero;
			int num2 = Math.Max(limit, 100);
			for (int i = 0; i < num2; i++)
			{
				positiveZero = Divide(config, x, bigDecimal, limit, out remainder);
				remainder = Subtract(config, positiveZero, bigDecimal, limit);
				positiveZero = Multiply(config, Half, remainder, limit);
				if (IsZero(positiveZero))
				{
					break;
				}
				bigDecimal = Add(config, bigDecimal, positiveZero);
				if (positiveZero.Exponent <= exponent)
				{
					break;
				}
			}
			return bigDecimal;
		}

		public static BigDecimal FractionalPart(Config config, BigDecimal x)
		{
			if (IsFinite(x))
			{
				if (x.Exponent > 0)
				{
					if (x.Exponent < x.Digits)
					{
						return new BigDecimal(x.Sign, Fraction.Create(x._fraction.ToString().Substring(x.Exponent)), 0);
					}
					if (x.Sign <= 0)
					{
						return NegativeZero;
					}
					return PositiveZero;
				}
				return x;
			}
			return x;
		}

		public static BigDecimal IntegerPart(Config config, BigDecimal x)
		{
			if (IsFinite(x))
			{
				if (x.Exponent > 0)
				{
					if (x.Exponent < x._fraction.DigitCount)
					{
						return new BigDecimal(x.Sign, Fraction.Create(x._fraction.ToString().Substring(0, x.Exponent)), x.Exponent);
					}
					return x;
				}
				if (x.Sign <= 0)
				{
					return NegativeZero;
				}
				return PositiveZero;
			}
			return x;
		}

		public static BigDecimal LimitPrecision(Config config, BigDecimal x, int limit, RoundingModes mode)
		{
			checked
			{
				try
				{
					if (IsFinite(x))
					{
						int offset;
						Fraction fraction = Fraction.LimitPrecision(x._sign, x._fraction, limit + x.Exponent, mode, out offset);
						return new BigDecimal(x._sign, fraction, x._exponent + offset);
					}
					return x;
				}
				catch (OverflowException)
				{
					return ExponentOverflow(config, x._sign, x._exponent);
				}
			}
		}

		private static BigDecimal InternalAdd(Config config, BigDecimal x, BigDecimal y, int limit)
		{
			if (limit < 0)
			{
				throw new ArgumentException("limit must be positive");
			}
			if (limit == 0)
			{
				limit = config.Limit;
			}
			BigDecimal bigDecimal = CheckSpecialResult(config, x, y, BasicOperations.Add);
			if (bigDecimal != null)
			{
				return bigDecimal;
			}
			if (IsZero(x))
			{
				if (limit == 0)
				{
					return y;
				}
				return LimitPrecision(config, y, limit, config.RoundingMode);
			}
			if (IsZero(y))
			{
				if (limit == 0)
				{
					return x;
				}
				return LimitPrecision(config, x, limit, config.RoundingMode);
			}
			int num = Math.Max(x._exponent, y._exponent);
			int sign = x._sign;
			checked
			{
				Fraction fraction;
				try
				{
					int exponentDiff = x._exponent - y._exponent;
					int exponentOffset;
					if (x._sign == y._sign)
					{
						fraction = Fraction.Add(x._fraction, y._fraction, exponentDiff, out exponentOffset);
						num += exponentOffset;
						sign = x._sign;
					}
					else
					{
						fraction = Fraction.Subtract(x._fraction, y._fraction, exponentDiff, out exponentOffset, out sign);
						num += exponentOffset;
						if (sign == 0)
						{
							return PositiveZero;
						}
						sign = x._sign * sign;
					}
					if (limit == 0)
					{
						limit = fraction.DigitCount;
					}
					fraction = Fraction.LimitPrecision(sign, fraction, limit, config.RoundingMode, out exponentOffset);
					num += exponentOffset;
				}
				catch (OverflowException)
				{
					return ExponentOverflow(config, sign, x._exponent);
				}
				return new BigDecimal(sign, fraction, num);
			}
		}

		private static BigDecimal ExponentOverflow(Config config, int sign, int exponent)
		{
			if (exponent > 0)
			{
				if ((config.OverflowMode & OverflowExceptionModes.Infinity) == OverflowExceptionModes.Infinity)
				{
					throw new FloatDomainError("Exponent overflow");
				}
			}
			else if ((config.OverflowMode & OverflowExceptionModes.Underflow) == OverflowExceptionModes.Underflow)
			{
				throw new FloatDomainError("Exponent underflow");
			}
			return PositiveZero;
		}

		private static int? CheckSpecialComparison(BigDecimal x, BigDecimal y)
		{
			int num = ((x._sign != -1) ? 1 : 0);
			int num2 = ((y._sign != -1) ? 1 : 0);
			int num3 = ((!IsZero(x)) ? 1 : 0);
			int num4 = ((!IsZero(y)) ? 1 : 0);
			int type = (int)x._type;
			int type2 = (int)y._type;
			return SC[num, num3, type, num2, num4, type2];
		}

		private static void CreateSC()
		{
			SC[0, 0, 0, 0, 0, 0] = 0;
			SC[0, 0, 0, 0, 0, 1] = 0;
			SC[0, 0, 0, 0, 0, 2] = 0;
			SC[0, 0, 0, 0, 1, 0] = null;
			SC[0, 0, 0, 0, 1, 1] = 1;
			SC[0, 0, 0, 0, 1, 2] = 1;
			SC[0, 0, 0, 1, 0, 0] = 0;
			SC[0, 0, 0, 1, 0, 1] = 0;
			SC[0, 0, 0, 1, 0, 2] = 0;
			SC[0, 0, 0, 1, 1, 0] = null;
			SC[0, 0, 0, 1, 1, 1] = -1;
			SC[0, 0, 0, 1, 1, 2] = -1;
			SC[0, 0, 1, 0, 0, 0] = 0;
			SC[0, 0, 1, 0, 0, 1] = 0;
			SC[0, 0, 1, 0, 0, 2] = 0;
			SC[0, 0, 1, 0, 1, 0] = null;
			SC[0, 0, 1, 0, 1, 1] = 1;
			SC[0, 0, 1, 0, 1, 2] = 1;
			SC[0, 0, 1, 1, 0, 0] = 0;
			SC[0, 0, 1, 1, 0, 1] = 0;
			SC[0, 0, 1, 1, 0, 2] = 0;
			SC[0, 0, 1, 1, 1, 0] = null;
			SC[0, 0, 1, 1, 1, 1] = -1;
			SC[0, 0, 1, 1, 1, 2] = -1;
			SC[0, 0, 2, 0, 0, 0] = 0;
			SC[0, 0, 2, 0, 0, 1] = 0;
			SC[0, 0, 2, 0, 0, 2] = 0;
			SC[0, 0, 2, 0, 1, 0] = null;
			SC[0, 0, 2, 0, 1, 1] = 1;
			SC[0, 0, 2, 0, 1, 2] = 1;
			SC[0, 0, 2, 1, 0, 0] = 0;
			SC[0, 0, 2, 1, 0, 1] = 0;
			SC[0, 0, 2, 1, 0, 2] = 0;
			SC[0, 0, 2, 1, 1, 0] = null;
			SC[0, 0, 2, 1, 1, 1] = -1;
			SC[0, 0, 2, 1, 1, 2] = -1;
			SC[0, 1, 0, 0, 0, 0] = null;
			SC[0, 1, 0, 0, 0, 1] = null;
			SC[0, 1, 0, 0, 0, 2] = null;
			SC[0, 1, 0, 0, 1, 0] = null;
			SC[0, 1, 0, 0, 1, 1] = null;
			SC[0, 1, 0, 0, 1, 2] = null;
			SC[0, 1, 0, 1, 0, 0] = null;
			SC[0, 1, 0, 1, 0, 1] = null;
			SC[0, 1, 0, 1, 0, 2] = null;
			SC[0, 1, 0, 1, 1, 0] = null;
			SC[0, 1, 0, 1, 1, 1] = null;
			SC[0, 1, 0, 1, 1, 2] = null;
			SC[0, 1, 1, 0, 0, 0] = -1;
			SC[0, 1, 1, 0, 0, 1] = -1;
			SC[0, 1, 1, 0, 0, 2] = -1;
			SC[0, 1, 1, 0, 1, 0] = null;
			SC[0, 1, 1, 0, 1, 1] = 1;
			SC[0, 1, 1, 0, 1, 2] = 1;
			SC[0, 1, 1, 1, 0, 0] = -1;
			SC[0, 1, 1, 1, 0, 1] = -1;
			SC[0, 1, 1, 1, 0, 2] = -1;
			SC[0, 1, 1, 1, 1, 0] = null;
			SC[0, 1, 1, 1, 1, 1] = -1;
			SC[0, 1, 1, 1, 1, 2] = -1;
			SC[0, 1, 2, 0, 0, 0] = -1;
			SC[0, 1, 2, 0, 0, 1] = -1;
			SC[0, 1, 2, 0, 0, 2] = -1;
			SC[0, 1, 2, 0, 1, 0] = null;
			SC[0, 1, 2, 0, 1, 1] = -1;
			SC[0, 1, 2, 0, 1, 2] = 0;
			SC[0, 1, 2, 1, 0, 0] = -1;
			SC[0, 1, 2, 1, 0, 1] = -1;
			SC[0, 1, 2, 1, 0, 2] = -1;
			SC[0, 1, 2, 1, 1, 0] = null;
			SC[0, 1, 2, 1, 1, 1] = -1;
			SC[0, 1, 2, 1, 1, 2] = -1;
			SC[1, 0, 0, 0, 0, 0] = 0;
			SC[1, 0, 0, 0, 0, 1] = 0;
			SC[1, 0, 0, 0, 0, 2] = 0;
			SC[1, 0, 0, 0, 1, 0] = null;
			SC[1, 0, 0, 0, 1, 1] = 1;
			SC[1, 0, 0, 0, 1, 2] = 1;
			SC[1, 0, 0, 1, 0, 0] = 0;
			SC[1, 0, 0, 1, 0, 1] = 0;
			SC[1, 0, 0, 1, 0, 2] = 0;
			SC[1, 0, 0, 1, 1, 0] = null;
			SC[1, 0, 0, 1, 1, 1] = -1;
			SC[1, 0, 0, 1, 1, 2] = -1;
			SC[1, 0, 1, 0, 0, 0] = 0;
			SC[1, 0, 1, 0, 0, 1] = 0;
			SC[1, 0, 1, 0, 0, 2] = 0;
			SC[1, 0, 1, 0, 1, 0] = null;
			SC[1, 0, 1, 0, 1, 1] = 1;
			SC[1, 0, 1, 0, 1, 2] = 1;
			SC[1, 0, 1, 1, 0, 0] = 0;
			SC[1, 0, 1, 1, 0, 1] = 0;
			SC[1, 0, 1, 1, 0, 2] = 0;
			SC[1, 0, 1, 1, 1, 0] = null;
			SC[1, 0, 1, 1, 1, 1] = -1;
			SC[1, 0, 1, 1, 1, 2] = -1;
			SC[1, 0, 2, 0, 0, 0] = 0;
			SC[1, 0, 2, 0, 0, 1] = 0;
			SC[1, 0, 2, 0, 0, 2] = 0;
			SC[1, 0, 2, 0, 1, 0] = null;
			SC[1, 0, 2, 0, 1, 1] = 1;
			SC[1, 0, 2, 0, 1, 2] = 1;
			SC[1, 0, 2, 1, 0, 0] = 0;
			SC[1, 0, 2, 1, 0, 1] = 0;
			SC[1, 0, 2, 1, 0, 2] = 0;
			SC[1, 0, 2, 1, 1, 0] = null;
			SC[1, 0, 2, 1, 1, 1] = -1;
			SC[1, 0, 2, 1, 1, 2] = -1;
			SC[1, 1, 0, 0, 0, 0] = null;
			SC[1, 1, 0, 0, 0, 1] = null;
			SC[1, 1, 0, 0, 0, 2] = null;
			SC[1, 1, 0, 0, 1, 0] = null;
			SC[1, 1, 0, 0, 1, 1] = null;
			SC[1, 1, 0, 0, 1, 2] = null;
			SC[1, 1, 0, 1, 0, 0] = null;
			SC[1, 1, 0, 1, 0, 1] = null;
			SC[1, 1, 0, 1, 0, 2] = null;
			SC[1, 1, 0, 1, 1, 0] = null;
			SC[1, 1, 0, 1, 1, 1] = null;
			SC[1, 1, 0, 1, 1, 2] = null;
			SC[1, 1, 1, 0, 0, 0] = 1;
			SC[1, 1, 1, 0, 0, 1] = 1;
			SC[1, 1, 1, 0, 0, 2] = 1;
			SC[1, 1, 1, 0, 1, 0] = null;
			SC[1, 1, 1, 0, 1, 1] = 1;
			SC[1, 1, 1, 0, 1, 2] = 1;
			SC[1, 1, 1, 1, 0, 0] = 1;
			SC[1, 1, 1, 1, 0, 1] = 1;
			SC[1, 1, 1, 1, 0, 2] = 1;
			SC[1, 1, 1, 1, 1, 0] = null;
			SC[1, 1, 1, 1, 1, 1] = -1;
			SC[1, 1, 1, 1, 1, 2] = -1;
			SC[1, 1, 2, 0, 0, 0] = 1;
			SC[1, 1, 2, 0, 0, 1] = 1;
			SC[1, 1, 2, 0, 0, 2] = 1;
			SC[1, 1, 2, 0, 1, 0] = null;
			SC[1, 1, 2, 0, 1, 1] = 1;
			SC[1, 1, 2, 0, 1, 2] = 1;
			SC[1, 1, 2, 1, 0, 0] = 1;
			SC[1, 1, 2, 1, 0, 1] = 1;
			SC[1, 1, 2, 1, 0, 2] = 1;
			SC[1, 1, 2, 1, 1, 0] = null;
			SC[1, 1, 2, 1, 1, 1] = 1;
			SC[1, 1, 2, 1, 1, 2] = 0;
		}

		private static BigDecimal CheckSpecialResult(Config config, BigDecimal x, BigDecimal y, BasicOperations op)
		{
			int num = ((x._sign != -1) ? 1 : 0);
			int num2 = ((y._sign != -1) ? 1 : 0);
			int num3 = ((!IsZero(x)) ? 1 : 0);
			int num4 = ((!IsZero(y)) ? 1 : 0);
			int type = (int)x._type;
			int type2 = (int)y._type;
			BigDecimal bigDecimal = SR[num, num3, type, (int)op, num2, num4, type2];
			if (bigDecimal != null)
			{
				return CheckOverflowExceptions(config, bigDecimal);
			}
			return null;
		}

		private static BigDecimal CheckOverflowExceptions(Config config, BigDecimal result)
		{
			if (IsNaN(result) && (config.OverflowMode & OverflowExceptionModes.NaN) == OverflowExceptionModes.NaN)
			{
				throw new FloatDomainError("Computation results to 'NaN'");
			}
			if (IsInfinite(result) && (config.OverflowMode & OverflowExceptionModes.Infinity) == OverflowExceptionModes.Infinity)
			{
				throw new FloatDomainError("Computation results to 'Infinity'");
			}
			return result;
		}

		private static void CreateSR()
		{
			SR[0, 0, 0, 0, 0, 0, 0] = NegativeZero;
			SR[0, 0, 0, 0, 0, 0, 1] = NegativeZero;
			SR[0, 0, 0, 0, 0, 0, 2] = NegativeZero;
			SR[0, 0, 0, 0, 0, 1, 0] = NaN;
			SR[0, 0, 0, 0, 0, 1, 1] = null;
			SR[0, 0, 0, 0, 0, 1, 2] = NegativeInfinity;
			SR[0, 0, 0, 0, 1, 0, 0] = PositiveZero;
			SR[0, 0, 0, 0, 1, 0, 1] = PositiveZero;
			SR[0, 0, 0, 0, 1, 0, 2] = PositiveZero;
			SR[0, 0, 0, 0, 1, 1, 0] = NaN;
			SR[0, 0, 0, 0, 1, 1, 1] = null;
			SR[0, 0, 0, 0, 1, 1, 2] = PositiveInfinity;
			SR[0, 0, 0, 1, 0, 0, 0] = PositiveZero;
			SR[0, 0, 0, 1, 0, 0, 1] = PositiveZero;
			SR[0, 0, 0, 1, 0, 0, 2] = PositiveZero;
			SR[0, 0, 0, 1, 0, 1, 0] = NaN;
			SR[0, 0, 0, 1, 0, 1, 1] = null;
			SR[0, 0, 0, 1, 0, 1, 2] = PositiveInfinity;
			SR[0, 0, 0, 1, 1, 0, 0] = NegativeZero;
			SR[0, 0, 0, 1, 1, 0, 1] = NegativeZero;
			SR[0, 0, 0, 1, 1, 0, 2] = NegativeZero;
			SR[0, 0, 0, 1, 1, 1, 0] = NaN;
			SR[0, 0, 0, 1, 1, 1, 1] = null;
			SR[0, 0, 0, 1, 1, 1, 2] = NegativeInfinity;
			SR[0, 0, 0, 2, 0, 0, 0] = PositiveZero;
			SR[0, 0, 0, 2, 0, 0, 1] = PositiveZero;
			SR[0, 0, 0, 2, 0, 0, 2] = PositiveZero;
			SR[0, 0, 0, 2, 0, 1, 0] = NaN;
			SR[0, 0, 0, 2, 0, 1, 1] = PositiveZero;
			SR[0, 0, 0, 2, 0, 1, 2] = NaN;
			SR[0, 0, 0, 2, 1, 0, 0] = NegativeZero;
			SR[0, 0, 0, 2, 1, 0, 1] = NegativeZero;
			SR[0, 0, 0, 2, 1, 0, 2] = NegativeZero;
			SR[0, 0, 0, 2, 1, 1, 0] = NaN;
			SR[0, 0, 0, 2, 1, 1, 1] = NegativeZero;
			SR[0, 0, 0, 2, 1, 1, 2] = NaN;
			SR[0, 0, 0, 3, 0, 0, 0] = NaN;
			SR[0, 0, 0, 3, 0, 0, 1] = NaN;
			SR[0, 0, 0, 3, 0, 0, 2] = NaN;
			SR[0, 0, 0, 3, 0, 1, 0] = NaN;
			SR[0, 0, 0, 3, 0, 1, 1] = PositiveZero;
			SR[0, 0, 0, 3, 0, 1, 2] = PositiveZero;
			SR[0, 0, 0, 3, 1, 0, 0] = NaN;
			SR[0, 0, 0, 3, 1, 0, 1] = NaN;
			SR[0, 0, 0, 3, 1, 0, 2] = NaN;
			SR[0, 0, 0, 3, 1, 1, 0] = NaN;
			SR[0, 0, 0, 3, 1, 1, 1] = NegativeZero;
			SR[0, 0, 0, 3, 1, 1, 2] = NegativeZero;
			SR[0, 0, 1, 0, 0, 0, 0] = NegativeZero;
			SR[0, 0, 1, 0, 0, 0, 1] = NegativeZero;
			SR[0, 0, 1, 0, 0, 0, 2] = NegativeZero;
			SR[0, 0, 1, 0, 0, 1, 0] = NaN;
			SR[0, 0, 1, 0, 0, 1, 1] = null;
			SR[0, 0, 1, 0, 0, 1, 2] = NegativeInfinity;
			SR[0, 0, 1, 0, 1, 0, 0] = PositiveZero;
			SR[0, 0, 1, 0, 1, 0, 1] = PositiveZero;
			SR[0, 0, 1, 0, 1, 0, 2] = PositiveZero;
			SR[0, 0, 1, 0, 1, 1, 0] = NaN;
			SR[0, 0, 1, 0, 1, 1, 1] = null;
			SR[0, 0, 1, 0, 1, 1, 2] = PositiveInfinity;
			SR[0, 0, 1, 1, 0, 0, 0] = PositiveZero;
			SR[0, 0, 1, 1, 0, 0, 1] = PositiveZero;
			SR[0, 0, 1, 1, 0, 0, 2] = PositiveZero;
			SR[0, 0, 1, 1, 0, 1, 0] = NaN;
			SR[0, 0, 1, 1, 0, 1, 1] = null;
			SR[0, 0, 1, 1, 0, 1, 2] = PositiveInfinity;
			SR[0, 0, 1, 1, 1, 0, 0] = NegativeZero;
			SR[0, 0, 1, 1, 1, 0, 1] = NegativeZero;
			SR[0, 0, 1, 1, 1, 0, 2] = NegativeZero;
			SR[0, 0, 1, 1, 1, 1, 0] = NaN;
			SR[0, 0, 1, 1, 1, 1, 1] = null;
			SR[0, 0, 1, 1, 1, 1, 2] = NegativeInfinity;
			SR[0, 0, 1, 2, 0, 0, 0] = PositiveZero;
			SR[0, 0, 1, 2, 0, 0, 1] = PositiveZero;
			SR[0, 0, 1, 2, 0, 0, 2] = PositiveZero;
			SR[0, 0, 1, 2, 0, 1, 0] = NaN;
			SR[0, 0, 1, 2, 0, 1, 1] = PositiveZero;
			SR[0, 0, 1, 2, 0, 1, 2] = NaN;
			SR[0, 0, 1, 2, 1, 0, 0] = NegativeZero;
			SR[0, 0, 1, 2, 1, 0, 1] = NegativeZero;
			SR[0, 0, 1, 2, 1, 0, 2] = NegativeZero;
			SR[0, 0, 1, 2, 1, 1, 0] = NaN;
			SR[0, 0, 1, 2, 1, 1, 1] = NegativeZero;
			SR[0, 0, 1, 2, 1, 1, 2] = NaN;
			SR[0, 0, 1, 3, 0, 0, 0] = NaN;
			SR[0, 0, 1, 3, 0, 0, 1] = NaN;
			SR[0, 0, 1, 3, 0, 0, 2] = NaN;
			SR[0, 0, 1, 3, 0, 1, 0] = NaN;
			SR[0, 0, 1, 3, 0, 1, 1] = PositiveZero;
			SR[0, 0, 1, 3, 0, 1, 2] = PositiveZero;
			SR[0, 0, 1, 3, 1, 0, 0] = NaN;
			SR[0, 0, 1, 3, 1, 0, 1] = NaN;
			SR[0, 0, 1, 3, 1, 0, 2] = NaN;
			SR[0, 0, 1, 3, 1, 1, 0] = NaN;
			SR[0, 0, 1, 3, 1, 1, 1] = NegativeZero;
			SR[0, 0, 1, 3, 1, 1, 2] = NegativeZero;
			SR[0, 0, 2, 0, 0, 0, 0] = NegativeZero;
			SR[0, 0, 2, 0, 0, 0, 1] = NegativeZero;
			SR[0, 0, 2, 0, 0, 0, 2] = NegativeZero;
			SR[0, 0, 2, 0, 0, 1, 0] = NaN;
			SR[0, 0, 2, 0, 0, 1, 1] = null;
			SR[0, 0, 2, 0, 0, 1, 2] = NegativeInfinity;
			SR[0, 0, 2, 0, 1, 0, 0] = PositiveZero;
			SR[0, 0, 2, 0, 1, 0, 1] = PositiveZero;
			SR[0, 0, 2, 0, 1, 0, 2] = PositiveZero;
			SR[0, 0, 2, 0, 1, 1, 0] = NaN;
			SR[0, 0, 2, 0, 1, 1, 1] = null;
			SR[0, 0, 2, 0, 1, 1, 2] = PositiveInfinity;
			SR[0, 0, 2, 1, 0, 0, 0] = PositiveZero;
			SR[0, 0, 2, 1, 0, 0, 1] = PositiveZero;
			SR[0, 0, 2, 1, 0, 0, 2] = PositiveZero;
			SR[0, 0, 2, 1, 0, 1, 0] = NaN;
			SR[0, 0, 2, 1, 0, 1, 1] = null;
			SR[0, 0, 2, 1, 0, 1, 2] = PositiveInfinity;
			SR[0, 0, 2, 1, 1, 0, 0] = NegativeZero;
			SR[0, 0, 2, 1, 1, 0, 1] = NegativeZero;
			SR[0, 0, 2, 1, 1, 0, 2] = NegativeZero;
			SR[0, 0, 2, 1, 1, 1, 0] = NaN;
			SR[0, 0, 2, 1, 1, 1, 1] = null;
			SR[0, 0, 2, 1, 1, 1, 2] = NegativeInfinity;
			SR[0, 0, 2, 2, 0, 0, 0] = PositiveZero;
			SR[0, 0, 2, 2, 0, 0, 1] = PositiveZero;
			SR[0, 0, 2, 2, 0, 0, 2] = PositiveZero;
			SR[0, 0, 2, 2, 0, 1, 0] = NaN;
			SR[0, 0, 2, 2, 0, 1, 1] = PositiveZero;
			SR[0, 0, 2, 2, 0, 1, 2] = NaN;
			SR[0, 0, 2, 2, 1, 0, 0] = NegativeZero;
			SR[0, 0, 2, 2, 1, 0, 1] = NegativeZero;
			SR[0, 0, 2, 2, 1, 0, 2] = NegativeZero;
			SR[0, 0, 2, 2, 1, 1, 0] = NaN;
			SR[0, 0, 2, 2, 1, 1, 1] = NegativeZero;
			SR[0, 0, 2, 2, 1, 1, 2] = NaN;
			SR[0, 0, 2, 3, 0, 0, 0] = NaN;
			SR[0, 0, 2, 3, 0, 0, 1] = NaN;
			SR[0, 0, 2, 3, 0, 0, 2] = NaN;
			SR[0, 0, 2, 3, 0, 1, 0] = NaN;
			SR[0, 0, 2, 3, 0, 1, 1] = PositiveZero;
			SR[0, 0, 2, 3, 0, 1, 2] = PositiveZero;
			SR[0, 0, 2, 3, 1, 0, 0] = NaN;
			SR[0, 0, 2, 3, 1, 0, 1] = NaN;
			SR[0, 0, 2, 3, 1, 0, 2] = NaN;
			SR[0, 0, 2, 3, 1, 1, 0] = NaN;
			SR[0, 0, 2, 3, 1, 1, 1] = NegativeZero;
			SR[0, 0, 2, 3, 1, 1, 2] = NegativeZero;
			SR[0, 1, 0, 0, 0, 0, 0] = NaN;
			SR[0, 1, 0, 0, 0, 0, 1] = NaN;
			SR[0, 1, 0, 0, 0, 0, 2] = NaN;
			SR[0, 1, 0, 0, 0, 1, 0] = NaN;
			SR[0, 1, 0, 0, 0, 1, 1] = NaN;
			SR[0, 1, 0, 0, 0, 1, 2] = NaN;
			SR[0, 1, 0, 0, 1, 0, 0] = NaN;
			SR[0, 1, 0, 0, 1, 0, 1] = NaN;
			SR[0, 1, 0, 0, 1, 0, 2] = NaN;
			SR[0, 1, 0, 0, 1, 1, 0] = NaN;
			SR[0, 1, 0, 0, 1, 1, 1] = NaN;
			SR[0, 1, 0, 0, 1, 1, 2] = NaN;
			SR[0, 1, 0, 1, 0, 0, 0] = NaN;
			SR[0, 1, 0, 1, 0, 0, 1] = NaN;
			SR[0, 1, 0, 1, 0, 0, 2] = NaN;
			SR[0, 1, 0, 1, 0, 1, 0] = NaN;
			SR[0, 1, 0, 1, 0, 1, 1] = NaN;
			SR[0, 1, 0, 1, 0, 1, 2] = NaN;
			SR[0, 1, 0, 1, 1, 0, 0] = NaN;
			SR[0, 1, 0, 1, 1, 0, 1] = NaN;
			SR[0, 1, 0, 1, 1, 0, 2] = NaN;
			SR[0, 1, 0, 1, 1, 1, 0] = NaN;
			SR[0, 1, 0, 1, 1, 1, 1] = NaN;
			SR[0, 1, 0, 1, 1, 1, 2] = NaN;
			SR[0, 1, 0, 2, 0, 0, 0] = NaN;
			SR[0, 1, 0, 2, 0, 0, 1] = NaN;
			SR[0, 1, 0, 2, 0, 0, 2] = NaN;
			SR[0, 1, 0, 2, 0, 1, 0] = NaN;
			SR[0, 1, 0, 2, 0, 1, 1] = NaN;
			SR[0, 1, 0, 2, 0, 1, 2] = NaN;
			SR[0, 1, 0, 2, 1, 0, 0] = NaN;
			SR[0, 1, 0, 2, 1, 0, 1] = NaN;
			SR[0, 1, 0, 2, 1, 0, 2] = NaN;
			SR[0, 1, 0, 2, 1, 1, 0] = NaN;
			SR[0, 1, 0, 2, 1, 1, 1] = NaN;
			SR[0, 1, 0, 2, 1, 1, 2] = NaN;
			SR[0, 1, 0, 3, 0, 0, 0] = NaN;
			SR[0, 1, 0, 3, 0, 0, 1] = NaN;
			SR[0, 1, 0, 3, 0, 0, 2] = NaN;
			SR[0, 1, 0, 3, 0, 1, 0] = NaN;
			SR[0, 1, 0, 3, 0, 1, 1] = NaN;
			SR[0, 1, 0, 3, 0, 1, 2] = NaN;
			SR[0, 1, 0, 3, 1, 0, 0] = NaN;
			SR[0, 1, 0, 3, 1, 0, 1] = NaN;
			SR[0, 1, 0, 3, 1, 0, 2] = NaN;
			SR[0, 1, 0, 3, 1, 1, 0] = NaN;
			SR[0, 1, 0, 3, 1, 1, 1] = NaN;
			SR[0, 1, 0, 3, 1, 1, 2] = NaN;
			SR[0, 1, 1, 0, 0, 0, 0] = null;
			SR[0, 1, 1, 0, 0, 0, 1] = null;
			SR[0, 1, 1, 0, 0, 0, 2] = null;
			SR[0, 1, 1, 0, 0, 1, 0] = NaN;
			SR[0, 1, 1, 0, 0, 1, 1] = null;
			SR[0, 1, 1, 0, 0, 1, 2] = NegativeInfinity;
			SR[0, 1, 1, 0, 1, 0, 0] = null;
			SR[0, 1, 1, 0, 1, 0, 1] = null;
			SR[0, 1, 1, 0, 1, 0, 2] = null;
			SR[0, 1, 1, 0, 1, 1, 0] = NaN;
			SR[0, 1, 1, 0, 1, 1, 1] = null;
			SR[0, 1, 1, 0, 1, 1, 2] = PositiveInfinity;
			SR[0, 1, 1, 1, 0, 0, 0] = null;
			SR[0, 1, 1, 1, 0, 0, 1] = null;
			SR[0, 1, 1, 1, 0, 0, 2] = null;
			SR[0, 1, 1, 1, 0, 1, 0] = NaN;
			SR[0, 1, 1, 1, 0, 1, 1] = null;
			SR[0, 1, 1, 1, 0, 1, 2] = PositiveInfinity;
			SR[0, 1, 1, 1, 1, 0, 0] = null;
			SR[0, 1, 1, 1, 1, 0, 1] = null;
			SR[0, 1, 1, 1, 1, 0, 2] = null;
			SR[0, 1, 1, 1, 1, 1, 0] = NaN;
			SR[0, 1, 1, 1, 1, 1, 1] = null;
			SR[0, 1, 1, 1, 1, 1, 2] = NegativeInfinity;
			SR[0, 1, 1, 2, 0, 0, 0] = PositiveZero;
			SR[0, 1, 1, 2, 0, 0, 1] = PositiveZero;
			SR[0, 1, 1, 2, 0, 0, 2] = PositiveZero;
			SR[0, 1, 1, 2, 0, 1, 0] = NaN;
			SR[0, 1, 1, 2, 0, 1, 1] = null;
			SR[0, 1, 1, 2, 0, 1, 2] = PositiveInfinity;
			SR[0, 1, 1, 2, 1, 0, 0] = NegativeZero;
			SR[0, 1, 1, 2, 1, 0, 1] = NegativeZero;
			SR[0, 1, 1, 2, 1, 0, 2] = NegativeZero;
			SR[0, 1, 1, 2, 1, 1, 0] = NaN;
			SR[0, 1, 1, 2, 1, 1, 1] = null;
			SR[0, 1, 1, 2, 1, 1, 2] = NegativeInfinity;
			SR[0, 1, 1, 3, 0, 0, 0] = PositiveInfinity;
			SR[0, 1, 1, 3, 0, 0, 1] = PositiveInfinity;
			SR[0, 1, 1, 3, 0, 0, 2] = PositiveInfinity;
			SR[0, 1, 1, 3, 0, 1, 0] = NaN;
			SR[0, 1, 1, 3, 0, 1, 1] = null;
			SR[0, 1, 1, 3, 0, 1, 2] = PositiveZero;
			SR[0, 1, 1, 3, 1, 0, 0] = NegativeInfinity;
			SR[0, 1, 1, 3, 1, 0, 1] = NegativeInfinity;
			SR[0, 1, 1, 3, 1, 0, 2] = NegativeInfinity;
			SR[0, 1, 1, 3, 1, 1, 0] = NaN;
			SR[0, 1, 1, 3, 1, 1, 1] = null;
			SR[0, 1, 1, 3, 1, 1, 2] = NegativeZero;
			SR[0, 1, 2, 0, 0, 0, 0] = NegativeInfinity;
			SR[0, 1, 2, 0, 0, 0, 1] = NegativeInfinity;
			SR[0, 1, 2, 0, 0, 0, 2] = NegativeInfinity;
			SR[0, 1, 2, 0, 0, 1, 0] = NaN;
			SR[0, 1, 2, 0, 0, 1, 1] = NegativeInfinity;
			SR[0, 1, 2, 0, 0, 1, 2] = NegativeInfinity;
			SR[0, 1, 2, 0, 1, 0, 0] = NegativeInfinity;
			SR[0, 1, 2, 0, 1, 0, 1] = NegativeInfinity;
			SR[0, 1, 2, 0, 1, 0, 2] = NegativeInfinity;
			SR[0, 1, 2, 0, 1, 1, 0] = NaN;
			SR[0, 1, 2, 0, 1, 1, 1] = NegativeInfinity;
			SR[0, 1, 2, 0, 1, 1, 2] = NaN;
			SR[0, 1, 2, 1, 0, 0, 0] = NegativeInfinity;
			SR[0, 1, 2, 1, 0, 0, 1] = NegativeInfinity;
			SR[0, 1, 2, 1, 0, 0, 2] = NegativeInfinity;
			SR[0, 1, 2, 1, 0, 1, 0] = NaN;
			SR[0, 1, 2, 1, 0, 1, 1] = NegativeInfinity;
			SR[0, 1, 2, 1, 0, 1, 2] = NaN;
			SR[0, 1, 2, 1, 1, 0, 0] = NegativeInfinity;
			SR[0, 1, 2, 1, 1, 0, 1] = NegativeInfinity;
			SR[0, 1, 2, 1, 1, 0, 2] = NegativeInfinity;
			SR[0, 1, 2, 1, 1, 1, 0] = NaN;
			SR[0, 1, 2, 1, 1, 1, 1] = NegativeInfinity;
			SR[0, 1, 2, 1, 1, 1, 2] = NegativeInfinity;
			SR[0, 1, 2, 2, 0, 0, 0] = NaN;
			SR[0, 1, 2, 2, 0, 0, 1] = NaN;
			SR[0, 1, 2, 2, 0, 0, 2] = NaN;
			SR[0, 1, 2, 2, 0, 1, 0] = NaN;
			SR[0, 1, 2, 2, 0, 1, 1] = PositiveInfinity;
			SR[0, 1, 2, 2, 0, 1, 2] = PositiveInfinity;
			SR[0, 1, 2, 2, 1, 0, 0] = NaN;
			SR[0, 1, 2, 2, 1, 0, 1] = NaN;
			SR[0, 1, 2, 2, 1, 0, 2] = NaN;
			SR[0, 1, 2, 2, 1, 1, 0] = NaN;
			SR[0, 1, 2, 2, 1, 1, 1] = NegativeInfinity;
			SR[0, 1, 2, 2, 1, 1, 2] = NegativeInfinity;
			SR[0, 1, 2, 3, 0, 0, 0] = PositiveInfinity;
			SR[0, 1, 2, 3, 0, 0, 1] = PositiveInfinity;
			SR[0, 1, 2, 3, 0, 0, 2] = PositiveInfinity;
			SR[0, 1, 2, 3, 0, 1, 0] = NaN;
			SR[0, 1, 2, 3, 0, 1, 1] = PositiveInfinity;
			SR[0, 1, 2, 3, 0, 1, 2] = NaN;
			SR[0, 1, 2, 3, 1, 0, 0] = NegativeInfinity;
			SR[0, 1, 2, 3, 1, 0, 1] = NegativeInfinity;
			SR[0, 1, 2, 3, 1, 0, 2] = NegativeInfinity;
			SR[0, 1, 2, 3, 1, 1, 0] = NaN;
			SR[0, 1, 2, 3, 1, 1, 1] = NegativeInfinity;
			SR[0, 1, 2, 3, 1, 1, 2] = NaN;
			SR[1, 0, 0, 0, 0, 0, 0] = PositiveZero;
			SR[1, 0, 0, 0, 0, 0, 1] = PositiveZero;
			SR[1, 0, 0, 0, 0, 0, 2] = PositiveZero;
			SR[1, 0, 0, 0, 0, 1, 0] = NaN;
			SR[1, 0, 0, 0, 0, 1, 1] = null;
			SR[1, 0, 0, 0, 0, 1, 2] = NegativeInfinity;
			SR[1, 0, 0, 0, 1, 0, 0] = PositiveZero;
			SR[1, 0, 0, 0, 1, 0, 1] = PositiveZero;
			SR[1, 0, 0, 0, 1, 0, 2] = PositiveZero;
			SR[1, 0, 0, 0, 1, 1, 0] = NaN;
			SR[1, 0, 0, 0, 1, 1, 1] = null;
			SR[1, 0, 0, 0, 1, 1, 2] = PositiveInfinity;
			SR[1, 0, 0, 1, 0, 0, 0] = PositiveZero;
			SR[1, 0, 0, 1, 0, 0, 1] = PositiveZero;
			SR[1, 0, 0, 1, 0, 0, 2] = PositiveZero;
			SR[1, 0, 0, 1, 0, 1, 0] = NaN;
			SR[1, 0, 0, 1, 0, 1, 1] = null;
			SR[1, 0, 0, 1, 0, 1, 2] = PositiveInfinity;
			SR[1, 0, 0, 1, 1, 0, 0] = PositiveZero;
			SR[1, 0, 0, 1, 1, 0, 1] = PositiveZero;
			SR[1, 0, 0, 1, 1, 0, 2] = PositiveZero;
			SR[1, 0, 0, 1, 1, 1, 0] = NaN;
			SR[1, 0, 0, 1, 1, 1, 1] = null;
			SR[1, 0, 0, 1, 1, 1, 2] = NegativeInfinity;
			SR[1, 0, 0, 2, 0, 0, 0] = NegativeZero;
			SR[1, 0, 0, 2, 0, 0, 1] = NegativeZero;
			SR[1, 0, 0, 2, 0, 0, 2] = NegativeZero;
			SR[1, 0, 0, 2, 0, 1, 0] = NaN;
			SR[1, 0, 0, 2, 0, 1, 1] = NegativeZero;
			SR[1, 0, 0, 2, 0, 1, 2] = NaN;
			SR[1, 0, 0, 2, 1, 0, 0] = PositiveZero;
			SR[1, 0, 0, 2, 1, 0, 1] = PositiveZero;
			SR[1, 0, 0, 2, 1, 0, 2] = PositiveZero;
			SR[1, 0, 0, 2, 1, 1, 0] = NaN;
			SR[1, 0, 0, 2, 1, 1, 1] = PositiveZero;
			SR[1, 0, 0, 2, 1, 1, 2] = NaN;
			SR[1, 0, 0, 3, 0, 0, 0] = NaN;
			SR[1, 0, 0, 3, 0, 0, 1] = NaN;
			SR[1, 0, 0, 3, 0, 0, 2] = NaN;
			SR[1, 0, 0, 3, 0, 1, 0] = NaN;
			SR[1, 0, 0, 3, 0, 1, 1] = NegativeZero;
			SR[1, 0, 0, 3, 0, 1, 2] = NegativeZero;
			SR[1, 0, 0, 3, 1, 0, 0] = NaN;
			SR[1, 0, 0, 3, 1, 0, 1] = NaN;
			SR[1, 0, 0, 3, 1, 0, 2] = NaN;
			SR[1, 0, 0, 3, 1, 1, 0] = NaN;
			SR[1, 0, 0, 3, 1, 1, 1] = PositiveZero;
			SR[1, 0, 0, 3, 1, 1, 2] = PositiveZero;
			SR[1, 0, 1, 0, 0, 0, 0] = PositiveZero;
			SR[1, 0, 1, 0, 0, 0, 1] = PositiveZero;
			SR[1, 0, 1, 0, 0, 0, 2] = PositiveZero;
			SR[1, 0, 1, 0, 0, 1, 0] = NaN;
			SR[1, 0, 1, 0, 0, 1, 1] = null;
			SR[1, 0, 1, 0, 0, 1, 2] = NegativeInfinity;
			SR[1, 0, 1, 0, 1, 0, 0] = PositiveZero;
			SR[1, 0, 1, 0, 1, 0, 1] = PositiveZero;
			SR[1, 0, 1, 0, 1, 0, 2] = PositiveZero;
			SR[1, 0, 1, 0, 1, 1, 0] = NaN;
			SR[1, 0, 1, 0, 1, 1, 1] = null;
			SR[1, 0, 1, 0, 1, 1, 2] = PositiveInfinity;
			SR[1, 0, 1, 1, 0, 0, 0] = PositiveZero;
			SR[1, 0, 1, 1, 0, 0, 1] = PositiveZero;
			SR[1, 0, 1, 1, 0, 0, 2] = PositiveZero;
			SR[1, 0, 1, 1, 0, 1, 0] = NaN;
			SR[1, 0, 1, 1, 0, 1, 1] = null;
			SR[1, 0, 1, 1, 0, 1, 2] = PositiveInfinity;
			SR[1, 0, 1, 1, 1, 0, 0] = PositiveZero;
			SR[1, 0, 1, 1, 1, 0, 1] = PositiveZero;
			SR[1, 0, 1, 1, 1, 0, 2] = PositiveZero;
			SR[1, 0, 1, 1, 1, 1, 0] = NaN;
			SR[1, 0, 1, 1, 1, 1, 1] = null;
			SR[1, 0, 1, 1, 1, 1, 2] = NegativeInfinity;
			SR[1, 0, 1, 2, 0, 0, 0] = NegativeZero;
			SR[1, 0, 1, 2, 0, 0, 1] = NegativeZero;
			SR[1, 0, 1, 2, 0, 0, 2] = NegativeZero;
			SR[1, 0, 1, 2, 0, 1, 0] = NaN;
			SR[1, 0, 1, 2, 0, 1, 1] = NegativeZero;
			SR[1, 0, 1, 2, 0, 1, 2] = NaN;
			SR[1, 0, 1, 2, 1, 0, 0] = PositiveZero;
			SR[1, 0, 1, 2, 1, 0, 1] = PositiveZero;
			SR[1, 0, 1, 2, 1, 0, 2] = PositiveZero;
			SR[1, 0, 1, 2, 1, 1, 0] = NaN;
			SR[1, 0, 1, 2, 1, 1, 1] = PositiveZero;
			SR[1, 0, 1, 2, 1, 1, 2] = NaN;
			SR[1, 0, 1, 3, 0, 0, 0] = NaN;
			SR[1, 0, 1, 3, 0, 0, 1] = NaN;
			SR[1, 0, 1, 3, 0, 0, 2] = NaN;
			SR[1, 0, 1, 3, 0, 1, 0] = NaN;
			SR[1, 0, 1, 3, 0, 1, 1] = NegativeZero;
			SR[1, 0, 1, 3, 0, 1, 2] = NegativeZero;
			SR[1, 0, 1, 3, 1, 0, 0] = NaN;
			SR[1, 0, 1, 3, 1, 0, 1] = NaN;
			SR[1, 0, 1, 3, 1, 0, 2] = NaN;
			SR[1, 0, 1, 3, 1, 1, 0] = NaN;
			SR[1, 0, 1, 3, 1, 1, 1] = PositiveZero;
			SR[1, 0, 1, 3, 1, 1, 2] = PositiveZero;
			SR[1, 0, 2, 0, 0, 0, 0] = PositiveZero;
			SR[1, 0, 2, 0, 0, 0, 1] = PositiveZero;
			SR[1, 0, 2, 0, 0, 0, 2] = PositiveZero;
			SR[1, 0, 2, 0, 0, 1, 0] = NaN;
			SR[1, 0, 2, 0, 0, 1, 1] = null;
			SR[1, 0, 2, 0, 0, 1, 2] = NegativeInfinity;
			SR[1, 0, 2, 0, 1, 0, 0] = PositiveZero;
			SR[1, 0, 2, 0, 1, 0, 1] = PositiveZero;
			SR[1, 0, 2, 0, 1, 0, 2] = PositiveZero;
			SR[1, 0, 2, 0, 1, 1, 0] = NaN;
			SR[1, 0, 2, 0, 1, 1, 1] = null;
			SR[1, 0, 2, 0, 1, 1, 2] = PositiveInfinity;
			SR[1, 0, 2, 1, 0, 0, 0] = PositiveZero;
			SR[1, 0, 2, 1, 0, 0, 1] = PositiveZero;
			SR[1, 0, 2, 1, 0, 0, 2] = PositiveZero;
			SR[1, 0, 2, 1, 0, 1, 0] = NaN;
			SR[1, 0, 2, 1, 0, 1, 1] = null;
			SR[1, 0, 2, 1, 0, 1, 2] = PositiveInfinity;
			SR[1, 0, 2, 1, 1, 0, 0] = PositiveZero;
			SR[1, 0, 2, 1, 1, 0, 1] = PositiveZero;
			SR[1, 0, 2, 1, 1, 0, 2] = PositiveZero;
			SR[1, 0, 2, 1, 1, 1, 0] = NaN;
			SR[1, 0, 2, 1, 1, 1, 1] = null;
			SR[1, 0, 2, 1, 1, 1, 2] = NegativeInfinity;
			SR[1, 0, 2, 2, 0, 0, 0] = NegativeZero;
			SR[1, 0, 2, 2, 0, 0, 1] = NegativeZero;
			SR[1, 0, 2, 2, 0, 0, 2] = NegativeZero;
			SR[1, 0, 2, 2, 0, 1, 0] = NaN;
			SR[1, 0, 2, 2, 0, 1, 1] = NegativeZero;
			SR[1, 0, 2, 2, 0, 1, 2] = NaN;
			SR[1, 0, 2, 2, 1, 0, 0] = PositiveZero;
			SR[1, 0, 2, 2, 1, 0, 1] = PositiveZero;
			SR[1, 0, 2, 2, 1, 0, 2] = PositiveZero;
			SR[1, 0, 2, 2, 1, 1, 0] = NaN;
			SR[1, 0, 2, 2, 1, 1, 1] = PositiveZero;
			SR[1, 0, 2, 2, 1, 1, 2] = NaN;
			SR[1, 0, 2, 3, 0, 0, 0] = NaN;
			SR[1, 0, 2, 3, 0, 0, 1] = NaN;
			SR[1, 0, 2, 3, 0, 0, 2] = NaN;
			SR[1, 0, 2, 3, 0, 1, 0] = NaN;
			SR[1, 0, 2, 3, 0, 1, 1] = NegativeZero;
			SR[1, 0, 2, 3, 0, 1, 2] = NegativeZero;
			SR[1, 0, 2, 3, 1, 0, 0] = NaN;
			SR[1, 0, 2, 3, 1, 0, 1] = NaN;
			SR[1, 0, 2, 3, 1, 0, 2] = NaN;
			SR[1, 0, 2, 3, 1, 1, 0] = NaN;
			SR[1, 0, 2, 3, 1, 1, 1] = PositiveZero;
			SR[1, 0, 2, 3, 1, 1, 2] = PositiveZero;
			SR[1, 1, 0, 0, 0, 0, 0] = NaN;
			SR[1, 1, 0, 0, 0, 0, 1] = NaN;
			SR[1, 1, 0, 0, 0, 0, 2] = NaN;
			SR[1, 1, 0, 0, 0, 1, 0] = NaN;
			SR[1, 1, 0, 0, 0, 1, 1] = NaN;
			SR[1, 1, 0, 0, 0, 1, 2] = NaN;
			SR[1, 1, 0, 0, 1, 0, 0] = NaN;
			SR[1, 1, 0, 0, 1, 0, 1] = NaN;
			SR[1, 1, 0, 0, 1, 0, 2] = NaN;
			SR[1, 1, 0, 0, 1, 1, 0] = NaN;
			SR[1, 1, 0, 0, 1, 1, 1] = NaN;
			SR[1, 1, 0, 0, 1, 1, 2] = NaN;
			SR[1, 1, 0, 1, 0, 0, 0] = NaN;
			SR[1, 1, 0, 1, 0, 0, 1] = NaN;
			SR[1, 1, 0, 1, 0, 0, 2] = NaN;
			SR[1, 1, 0, 1, 0, 1, 0] = NaN;
			SR[1, 1, 0, 1, 0, 1, 1] = NaN;
			SR[1, 1, 0, 1, 0, 1, 2] = NaN;
			SR[1, 1, 0, 1, 1, 0, 0] = NaN;
			SR[1, 1, 0, 1, 1, 0, 1] = NaN;
			SR[1, 1, 0, 1, 1, 0, 2] = NaN;
			SR[1, 1, 0, 1, 1, 1, 0] = NaN;
			SR[1, 1, 0, 1, 1, 1, 1] = NaN;
			SR[1, 1, 0, 1, 1, 1, 2] = NaN;
			SR[1, 1, 0, 2, 0, 0, 0] = NaN;
			SR[1, 1, 0, 2, 0, 0, 1] = NaN;
			SR[1, 1, 0, 2, 0, 0, 2] = NaN;
			SR[1, 1, 0, 2, 0, 1, 0] = NaN;
			SR[1, 1, 0, 2, 0, 1, 1] = NaN;
			SR[1, 1, 0, 2, 0, 1, 2] = NaN;
			SR[1, 1, 0, 2, 1, 0, 0] = NaN;
			SR[1, 1, 0, 2, 1, 0, 1] = NaN;
			SR[1, 1, 0, 2, 1, 0, 2] = NaN;
			SR[1, 1, 0, 2, 1, 1, 0] = NaN;
			SR[1, 1, 0, 2, 1, 1, 1] = NaN;
			SR[1, 1, 0, 2, 1, 1, 2] = NaN;
			SR[1, 1, 0, 3, 0, 0, 0] = NaN;
			SR[1, 1, 0, 3, 0, 0, 1] = NaN;
			SR[1, 1, 0, 3, 0, 0, 2] = NaN;
			SR[1, 1, 0, 3, 0, 1, 0] = NaN;
			SR[1, 1, 0, 3, 0, 1, 1] = NaN;
			SR[1, 1, 0, 3, 0, 1, 2] = NaN;
			SR[1, 1, 0, 3, 1, 0, 0] = NaN;
			SR[1, 1, 0, 3, 1, 0, 1] = NaN;
			SR[1, 1, 0, 3, 1, 0, 2] = NaN;
			SR[1, 1, 0, 3, 1, 1, 0] = NaN;
			SR[1, 1, 0, 3, 1, 1, 1] = NaN;
			SR[1, 1, 0, 3, 1, 1, 2] = NaN;
			SR[1, 1, 1, 0, 0, 0, 0] = null;
			SR[1, 1, 1, 0, 0, 0, 1] = null;
			SR[1, 1, 1, 0, 0, 0, 2] = null;
			SR[1, 1, 1, 0, 0, 1, 0] = NaN;
			SR[1, 1, 1, 0, 0, 1, 1] = null;
			SR[1, 1, 1, 0, 0, 1, 2] = NegativeInfinity;
			SR[1, 1, 1, 0, 1, 0, 0] = null;
			SR[1, 1, 1, 0, 1, 0, 1] = null;
			SR[1, 1, 1, 0, 1, 0, 2] = null;
			SR[1, 1, 1, 0, 1, 1, 0] = NaN;
			SR[1, 1, 1, 0, 1, 1, 1] = null;
			SR[1, 1, 1, 0, 1, 1, 2] = PositiveInfinity;
			SR[1, 1, 1, 1, 0, 0, 0] = null;
			SR[1, 1, 1, 1, 0, 0, 1] = null;
			SR[1, 1, 1, 1, 0, 0, 2] = null;
			SR[1, 1, 1, 1, 0, 1, 0] = NaN;
			SR[1, 1, 1, 1, 0, 1, 1] = null;
			SR[1, 1, 1, 1, 0, 1, 2] = PositiveInfinity;
			SR[1, 1, 1, 1, 1, 0, 0] = null;
			SR[1, 1, 1, 1, 1, 0, 1] = null;
			SR[1, 1, 1, 1, 1, 0, 2] = null;
			SR[1, 1, 1, 1, 1, 1, 0] = NaN;
			SR[1, 1, 1, 1, 1, 1, 1] = null;
			SR[1, 1, 1, 1, 1, 1, 2] = NegativeInfinity;
			SR[1, 1, 1, 2, 0, 0, 0] = NegativeZero;
			SR[1, 1, 1, 2, 0, 0, 1] = NegativeZero;
			SR[1, 1, 1, 2, 0, 0, 2] = NegativeZero;
			SR[1, 1, 1, 2, 0, 1, 0] = NaN;
			SR[1, 1, 1, 2, 0, 1, 1] = null;
			SR[1, 1, 1, 2, 0, 1, 2] = NegativeInfinity;
			SR[1, 1, 1, 2, 1, 0, 0] = PositiveZero;
			SR[1, 1, 1, 2, 1, 0, 1] = PositiveZero;
			SR[1, 1, 1, 2, 1, 0, 2] = PositiveZero;
			SR[1, 1, 1, 2, 1, 1, 0] = NaN;
			SR[1, 1, 1, 2, 1, 1, 1] = null;
			SR[1, 1, 1, 2, 1, 1, 2] = PositiveInfinity;
			SR[1, 1, 1, 3, 0, 0, 0] = NegativeInfinity;
			SR[1, 1, 1, 3, 0, 0, 1] = NegativeInfinity;
			SR[1, 1, 1, 3, 0, 0, 2] = NegativeInfinity;
			SR[1, 1, 1, 3, 0, 1, 0] = NaN;
			SR[1, 1, 1, 3, 0, 1, 1] = null;
			SR[1, 1, 1, 3, 0, 1, 2] = NegativeZero;
			SR[1, 1, 1, 3, 1, 0, 0] = PositiveInfinity;
			SR[1, 1, 1, 3, 1, 0, 1] = PositiveInfinity;
			SR[1, 1, 1, 3, 1, 0, 2] = PositiveInfinity;
			SR[1, 1, 1, 3, 1, 1, 0] = NaN;
			SR[1, 1, 1, 3, 1, 1, 1] = null;
			SR[1, 1, 1, 3, 1, 1, 2] = PositiveZero;
			SR[1, 1, 2, 0, 0, 0, 0] = PositiveInfinity;
			SR[1, 1, 2, 0, 0, 0, 1] = PositiveInfinity;
			SR[1, 1, 2, 0, 0, 0, 2] = PositiveInfinity;
			SR[1, 1, 2, 0, 0, 1, 0] = NaN;
			SR[1, 1, 2, 0, 0, 1, 1] = PositiveInfinity;
			SR[1, 1, 2, 0, 0, 1, 2] = NaN;
			SR[1, 1, 2, 0, 1, 0, 0] = PositiveInfinity;
			SR[1, 1, 2, 0, 1, 0, 1] = PositiveInfinity;
			SR[1, 1, 2, 0, 1, 0, 2] = PositiveInfinity;
			SR[1, 1, 2, 0, 1, 1, 0] = NaN;
			SR[1, 1, 2, 0, 1, 1, 1] = PositiveInfinity;
			SR[1, 1, 2, 0, 1, 1, 2] = PositiveInfinity;
			SR[1, 1, 2, 1, 0, 0, 0] = PositiveInfinity;
			SR[1, 1, 2, 1, 0, 0, 1] = PositiveInfinity;
			SR[1, 1, 2, 1, 0, 0, 2] = PositiveInfinity;
			SR[1, 1, 2, 1, 0, 1, 0] = NaN;
			SR[1, 1, 2, 1, 0, 1, 1] = PositiveInfinity;
			SR[1, 1, 2, 1, 0, 1, 2] = PositiveInfinity;
			SR[1, 1, 2, 1, 1, 0, 0] = PositiveInfinity;
			SR[1, 1, 2, 1, 1, 0, 1] = PositiveInfinity;
			SR[1, 1, 2, 1, 1, 0, 2] = PositiveInfinity;
			SR[1, 1, 2, 1, 1, 1, 0] = NaN;
			SR[1, 1, 2, 1, 1, 1, 1] = PositiveInfinity;
			SR[1, 1, 2, 1, 1, 1, 2] = NaN;
			SR[1, 1, 2, 2, 0, 0, 0] = NaN;
			SR[1, 1, 2, 2, 0, 0, 1] = NaN;
			SR[1, 1, 2, 2, 0, 0, 2] = NaN;
			SR[1, 1, 2, 2, 0, 1, 0] = NaN;
			SR[1, 1, 2, 2, 0, 1, 1] = NegativeInfinity;
			SR[1, 1, 2, 2, 0, 1, 2] = NegativeInfinity;
			SR[1, 1, 2, 2, 1, 0, 0] = NaN;
			SR[1, 1, 2, 2, 1, 0, 1] = NaN;
			SR[1, 1, 2, 2, 1, 0, 2] = NaN;
			SR[1, 1, 2, 2, 1, 1, 0] = NaN;
			SR[1, 1, 2, 2, 1, 1, 1] = PositiveInfinity;
			SR[1, 1, 2, 2, 1, 1, 2] = PositiveInfinity;
			SR[1, 1, 2, 3, 0, 0, 0] = NegativeInfinity;
			SR[1, 1, 2, 3, 0, 0, 1] = NegativeInfinity;
			SR[1, 1, 2, 3, 0, 0, 2] = NegativeInfinity;
			SR[1, 1, 2, 3, 0, 1, 0] = NaN;
			SR[1, 1, 2, 3, 0, 1, 1] = NegativeInfinity;
			SR[1, 1, 2, 3, 0, 1, 2] = NaN;
			SR[1, 1, 2, 3, 1, 0, 0] = PositiveInfinity;
			SR[1, 1, 2, 3, 1, 0, 1] = PositiveInfinity;
			SR[1, 1, 2, 3, 1, 0, 2] = PositiveInfinity;
			SR[1, 1, 2, 3, 1, 1, 0] = NaN;
			SR[1, 1, 2, 3, 1, 1, 1] = PositiveInfinity;
			SR[1, 1, 2, 3, 1, 1, 2] = NaN;
		}

		public string GetFractionString()
		{
			if (IsFinite(this))
			{
				return _fraction.ToString();
			}
			if (IsInfinite(this))
			{
				return "Infinity";
			}
			return "NaN";
		}

		public override string ToString()
		{
			return ToString(0);
		}

		public string ToString(int separateAt)
		{
			return ToString(separateAt, "", false);
		}

		public string ToString(int separateAt, string plusSign, bool floatStyleFormat)
		{
			if (separateAt < 0)
			{
				throw new ArgumentException("argument must be positive");
			}
			if (IsFinite(this) && !IsZero(this))
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (IsNegative(this))
				{
					stringBuilder.Append("-");
				}
				else
				{
					stringBuilder.Append(plusSign);
				}
				if (floatStyleFormat)
				{
					AppendFloatStyle(stringBuilder, separateAt);
				}
				else
				{
					AppendEngineeringStyle(stringBuilder, separateAt);
				}
				return stringBuilder.ToString();
			}
			if (IsPositiveZero(this))
			{
				return plusSign + "0.0";
			}
			if (IsNegativeZero(this))
			{
				return "-0.0";
			}
			if (IsPositiveInfinite(this))
			{
				return plusSign + "Infinity";
			}
			if (IsNegativeInfinite(this))
			{
				return "-Infinity";
			}
			return "NaN";
		}

		private void AppendFloatStyle(StringBuilder sb, int separateAt)
		{
			if (_exponent <= 0)
			{
				sb.Append("0.");
				AppendDigits(sb, new string('0', -_exponent) + _fraction.ToString(), 0, Digits - _exponent, separateAt);
				return;
			}
			int num = _exponent - Digits;
			if (num >= 0)
			{
				AppendDigits(sb, _fraction.ToString() + new string('0', num), 0, _exponent, separateAt);
				sb.Append(".0");
			}
			else
			{
				AppendDigits(sb, _fraction.ToString(), 0, _exponent, separateAt);
				sb.Append(".");
				AppendDigits(sb, _fraction.ToString(), _exponent, Digits - _exponent, separateAt);
			}
		}

		private void AppendEngineeringStyle(StringBuilder sb, int separateAt)
		{
			sb.Append("0.");
			AppendDigits(sb, _fraction.ToString(), 0, Digits, separateAt);
			sb.Append("E");
			sb.AppendFormat("{0}", _exponent);
		}

		private void AppendDigits(StringBuilder sb, string digits, int start, int length, int separateAt)
		{
			int num = start;
			if (separateAt > 0)
			{
				while (num + separateAt < start + length)
				{
					sb.Append(digits.Substring(num, separateAt));
					num += separateAt;
					sb.Append(" ");
				}
			}
			sb.Append(digits.Substring(num, length - (num - start)));
		}

		public bool Equals(BigDecimal other)
		{
			return CompareTo(other) == 0;
		}

		public int? CompareBigDecimal(BigDecimal other)
		{
			int? result;
			if (other == null)
			{
				result = null;
			}
			else
			{
				if (!IsNonZeroFinite(this) || !IsNonZeroFinite(other))
				{
					return CheckSpecialComparison(this, other);
				}
				result = ((_sign == other._sign) ? new int?(_sign * Fraction.Compare(_fraction, other._fraction, _exponent - other._exponent)) : new int?((_sign > other._sign) ? 1 : (-1)));
			}
			return result;
		}

		public int CompareTo(BigDecimal other)
		{
			int? num = CompareBigDecimal(other);
			if (num.HasValue)
			{
				return num.Value;
			}
			if (IsNaN(this))
			{
				if (IsNaN(other))
				{
					return 0;
				}
				return -1;
			}
			return 1;
		}

		public int GetSignCode()
		{
			switch (NumberType)
			{
			case NumberTypes.NaN:
				return 0;
			case NumberTypes.Infinite:
				return Sign * 3;
			default:
				if (IsZero(this))
				{
					return Sign;
				}
				return Sign * 2;
			}
		}

		public override int GetHashCode()
		{
			int num = GetSignCode();
			if (num == 2)
			{
				int hashCode = _fraction.GetHashCode();
				num = (31 * num + hashCode) ^ (hashCode + Exponent);
			}
			return num;
		}
	}
}

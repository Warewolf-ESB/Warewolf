using System;
using System.Globalization;
using System.Text;

namespace IronRuby.StandardLibrary.BigDecimal
{
	public class Fraction
	{
		public const int BASE_FIG = 9;

		public const uint BASE = 1000000000u;

		public const uint HALF_BASE = 500000000u;

		public const uint BASE_DIV_10 = 100000000u;

		public static readonly Fraction Zero;

		public static readonly Fraction One;

		public static readonly Fraction Five;

		private uint[] _words;

		private int _precision;

		private static readonly uint[] _powers;

		public int Precision
		{
			get
			{
				return _precision;
			}
			set
			{
				if (value < 1 || value > _words.Length)
				{
					throw new ArgumentException("Precision must be in [1,MaxPrecision]");
				}
				_precision = value;
			}
		}

		public int MaxPrecision
		{
			get
			{
				return _words.Length;
			}
		}

		public int DigitCount
		{
			get
			{
				int num = Precision - 1;
				while (num > 0 && _words[num] == 0)
				{
					num--;
				}
				int num2 = (num + 1) * 9;
				uint num3 = _words[num];
				uint num4 = 10u;
				while (num4 <= 1000000000 && num3 % num4 == 0)
				{
					num2--;
					num4 *= 10;
				}
				return num2;
			}
		}

		public bool IsOne
		{
			get
			{
				if (Precision == 1)
				{
					return _words[0] == 100000000;
				}
				return false;
			}
		}

		public bool IsZero
		{
			get
			{
				if (Precision == 1)
				{
					return _words[0] == 0;
				}
				return false;
			}
		}

		static Fraction()
		{
			uint[] data = new uint[1];
			Zero = new Fraction(data);
			One = new Fraction(new uint[1] { 100000000u });
			Five = new Fraction(new uint[1] { 500000000u });
			_powers = new uint[9];
			int num = 0;
			for (uint num2 = 100000000u; num2 != 0; num2 /= 10u)
			{
				_powers[num] = num2;
				num++;
			}
		}

		public Fraction(uint[] data)
		{
			_words = data;
			Precision = MaxPrecision;
		}

		public Fraction(int maxPrecision)
			: this(maxPrecision, maxPrecision)
		{
		}

		public Fraction(Fraction copyFrom, int maxPrecision)
			: this(maxPrecision, maxPrecision)
		{
			int length = Math.Min(maxPrecision, copyFrom.MaxPrecision);
			Array.Copy(copyFrom._words, _words, length);
		}

		public Fraction(int maxPrecision, int precision)
		{
			if (maxPrecision <= 0)
			{
				throw new ArgumentException("maxPrecision must be greater than zero");
			}
			_words = new uint[maxPrecision];
			Precision = precision;
		}

		public static Fraction Create(string digits)
		{
			int digitOffset;
			return Parse(digits, out digitOffset);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(Precision * 9);
			for (int i = 0; i < Precision; i++)
			{
				uint num = _words[i];
				stringBuilder.AppendFormat("{0:D" + 9 + "}", num);
			}
			string text = stringBuilder.ToString().TrimEnd('0');
			if (text.Length == 0)
			{
				text = "0";
			}
			return text;
		}

		public override int GetHashCode()
		{
			int num = 0;
			for (int i = 0; i < Precision; i++)
			{
				int num2 = (int)_words[i];
				num = (31 * num + num2) ^ num2;
			}
			return num;
		}

		public static Fraction Add(Fraction x, Fraction y, int exponentDiff, out int exponentOffset)
		{
			Fraction fraction = x;
			Fraction fraction2 = y;
			if (exponentDiff < 0)
			{
				exponentDiff = -exponentDiff;
				fraction = y;
				fraction2 = x;
			}
			int num = exponentDiff / 9;
			int num2 = exponentDiff % 9;
			if (num2 != 0)
			{
				fraction2 = ScrollRight(fraction2, num2);
			}
			int num3 = 0;
			int num4 = num;
			int maxPrecision = Math.Max(fraction.Precision, fraction2.Precision + num) + 1;
			Fraction fraction3 = new Fraction(maxPrecision);
			uint[] words = fraction._words;
			uint[] words2 = fraction2._words;
			uint[] words3 = fraction3._words;
			Array.Copy(words2, 0, words3, num4 + 1, fraction2.Precision);
			ulong num5 = 0uL;
			for (int num6 = fraction.Precision - 1; num6 >= num3; num6--)
			{
				num5 += words[num6] + words3[num6 + 1];
				words3[num6 + 1] = (uint)(num5 % 1000000000uL);
				num5 /= 1000000000uL;
			}
			words3[0] = (uint)(num5 % 1000000000uL);
			fraction3 = Normalize(fraction3, out exponentOffset);
			exponentOffset += 9;
			return fraction3;
		}

		public static Fraction Subtract(Fraction x, Fraction y, int exponentDiff, out int exponentOffset, out int sign)
		{
			Fraction fraction = x;
			Fraction fraction2 = y;
			sign = Compare(x, y, exponentDiff);
			if (sign == 0)
			{
				exponentOffset = 0;
				return new Fraction(1);
			}
			if (sign < 0)
			{
				exponentDiff = -exponentDiff;
				fraction = y;
				fraction2 = x;
			}
			int num = exponentDiff / 9;
			int num2 = exponentDiff % 9;
			if (num2 != 0)
			{
				fraction2 = ScrollRight(fraction2, num2);
			}
			int num3 = num;
			int maxPrecision = Math.Max(fraction.Precision, fraction2.Precision + num3);
			Fraction fraction3 = new Fraction(maxPrecision);
			uint[] words = fraction._words;
			uint[] words2 = fraction2._words;
			uint[] words3 = fraction3._words;
			Array.Copy(words, 0, words3, 0, fraction.Precision);
			SubtractInPlace(words3, words2, fraction2.Precision, num3);
			return Normalize(fraction3, out exponentOffset);
		}

		public static Fraction Multiply(Fraction x, Fraction y, out int offset)
		{
			int precision = x.Precision;
			int precision2 = y.Precision;
			int num = precision + precision2;
			uint[] words = x._words;
			uint[] words2 = y._words;
			uint[] array = new uint[num];
			Fraction f = new Fraction(array);
			for (int num2 = precision - 1; num2 >= 0; num2--)
			{
				uint num3 = words[num2];
				int num4 = num - (precision - num2);
				ulong num5 = 0uL;
				for (int num6 = precision2 - 1; num6 >= 0; num6--)
				{
					num5 = (ulong)((long)num5 + (long)num3 * (long)words2[num6] + array[num4]);
					array[num4--] = (uint)(num5 % 1000000000uL);
					num5 /= 1000000000uL;
				}
				while (num5 != 0)
				{
					num5 += array[num4];
					array[num4--] = (uint)num5;
					num5 /= 1000000000uL;
				}
			}
			return Normalize(f, out offset);
		}

		public static Fraction Divide(Fraction a, Fraction b, int minPrecision, out Fraction r, out int cOffset, out int rOffset)
		{
			int num = Math.Max(a.MaxPrecision + b.MaxPrecision + 1, minPrecision);
			Fraction fraction = new Fraction(num);
			r = new Fraction(num * 2);
			uint[] words = a._words;
			uint[] words2 = b._words;
			uint[] words3 = fraction._words;
			uint[] words4 = r._words;
			int precision = a.Precision;
			int precision2 = b.Precision;
			int maxPrecision = fraction.MaxPrecision;
			int maxPrecision2 = r.MaxPrecision;
			Array.Copy(words, 0, words4, 1, Math.Min(precision, maxPrecision2 - 1));
			ulong num2 = words2[0];
			ulong num3 = ((precision2 <= 1) ? num2 : (num2 + 1));
			ulong num4 = ((precision2 <= 1) ? ((ulong)words2[0] * 1000000000uL) : GetDoubleWord(words2, 0, b.Precision));
			ulong num5 = ((precision2 <= 2) ? num4 : (num4 + 1));
			int num6 = 1;
			int num7 = Math.Min(maxPrecision, maxPrecision2);
			while (num6 < num7)
			{
				if (words4[num6] == 0)
				{
					num6++;
					continue;
				}
				ulong doubleWord = GetDoubleWord(words4, num6, maxPrecision2);
				if (doubleWord == num4)
				{
					int jIndex = 2;
					int iIndex = num6 + 2;
					FindNextNonEqual(words4, words2, precision2, ref iIndex, ref jIndex);
					if (iIndex < maxPrecision2 && jIndex < precision2 && words4[iIndex] < words2[jIndex])
					{
						if (num6 + 1 > maxPrecision2)
						{
							break;
						}
						InternalDivide(words4, maxPrecision2, doubleWord / num3, words2, precision2, num6, words3);
					}
					else
					{
						SubtractInPlace(words4, words2, precision2, num6);
						words3[num6 - 1]++;
						Carry(words3, num6);
					}
				}
				else if (doubleWord >= num5)
				{
					InternalDivide(words4, maxPrecision2, doubleWord / num5, words2, precision2, num6 - 1, words3);
				}
				else
				{
					InternalDivide(words4, maxPrecision2, doubleWord / num3, words2, precision2, num6, words3);
				}
			}
			fraction = Normalize(fraction, out cOffset);
			r = Normalize(r, out rOffset);
			cOffset += 9;
			rOffset += 9;
			return fraction;
		}

		public static int Compare(Fraction x, Fraction y, int exponentDiff)
		{
			if (exponentDiff != 0)
			{
				if (exponentDiff <= 0)
				{
					return -1;
				}
				return 1;
			}
			int i;
			for (i = 0; i < x.Precision && i < y.Precision; i++)
			{
				if (x._words[i] != y._words[i])
				{
					if (x._words[i] <= y._words[i])
					{
						return -1;
					}
					return 1;
				}
			}
			if (i == x.Precision)
			{
				for (; i < y.Precision; i++)
				{
					if (y._words[i] != 0)
					{
						return -1;
					}
				}
			}
			else
			{
				for (; i < x.Precision; i++)
				{
					if (x._words[i] != 0)
					{
						return 1;
					}
				}
			}
			return 0;
		}

		public static Fraction LimitPrecision(int sign, Fraction fraction, int digits, BigDecimal.RoundingModes mode, out int offset)
		{
			if (digits <= 0)
			{
				uint lastDigit = ((!fraction.IsZero) ? 1u : 0u);
				if (RoundDigit(sign, lastDigit, 0u, mode) > 0)
				{
					offset = 1 - digits;
					return One;
				}
				offset = 0;
				return Zero;
			}
			if (digits >= fraction.DigitCount)
			{
				offset = 0;
				return fraction;
			}
			int result;
			int num = Math.DivRem(digits - 1, 9, out result);
			int num2;
			int num3;
			if (result == 8)
			{
				num2 = num + 1;
				num3 = 0;
			}
			else
			{
				num2 = num;
				num3 = result + 1;
			}
			uint num4 = _powers[num3];
			uint num5 = _powers[result];
			uint secondLastDigit = fraction._words[num] / num5 % 10u;
			uint lastDigit2 = ((num2 < fraction.MaxPrecision) ? (fraction._words[num2] / num4 % 10u) : 0u);
			int num6 = RoundDigit(sign, lastDigit2, secondLastDigit, mode);
			Fraction fraction2 = new Fraction(num2 + 1);
			Array.Copy(fraction._words, 0, fraction2._words, 0, Math.Min(num2 + 1, fraction.Precision));
			fraction2._words[num] = fraction2._words[num] - fraction._words[num] % num5;
			if (num6 > 0)
			{
				Fraction fraction3 = new Fraction(num + 1);
				fraction3._words[num] = num5;
				fraction2 = Add(fraction2, fraction3, 0, out offset);
			}
			else
			{
				offset = 0;
			}
			fraction2.Precision = Math.Min(num + 1, fraction2.MaxPrecision);
			return fraction2;
		}

		private static int RoundDigit(int sign, uint lastDigit, uint secondLastDigit, BigDecimal.RoundingModes roundingMode)
		{
			int result = -1;
			switch (roundingMode)
			{
			case BigDecimal.RoundingModes.Up:
				if (lastDigit != 0)
				{
					result = 1;
				}
				break;
			case BigDecimal.RoundingModes.HalfUp:
				if (lastDigit >= 5)
				{
					result = 1;
				}
				break;
			case BigDecimal.RoundingModes.HalfDown:
				if (lastDigit > 5)
				{
					result = 1;
				}
				break;
			case BigDecimal.RoundingModes.HalfEven:
				switch (lastDigit)
				{
				default:
					result = 1;
					break;
				case 5u:
					if (secondLastDigit % 2u != 0)
					{
						result = 1;
					}
					break;
				case 0u:
				case 1u:
				case 2u:
				case 3u:
				case 4u:
					break;
				}
				break;
			case BigDecimal.RoundingModes.Ceiling:
				if (sign == 1 && lastDigit != 0)
				{
					result = 1;
				}
				break;
			case BigDecimal.RoundingModes.Floor:
				if (sign == -1 && lastDigit != 0)
				{
					result = 1;
				}
				break;
			}
			return result;
		}

		private static void InternalDivide(uint[] rWords, int rSize, ulong q, uint[] bWords, int bSize, int index, uint[] cWords)
		{
			cWords[index] += (uint)(int)q;
			SubtractMultiple(rWords, rSize, q, bWords, bSize, index, bSize + index);
		}

		private static void SubtractMultiple(uint[] rWords, int rSize, ulong q, uint[] bWords, int bSize, int index, int rIndex)
		{
			int num = bSize - 1;
			uint num2 = 0u;
			uint num3 = 0u;
			while (num >= 0)
			{
				ulong num4 = q * bWords[num];
				if (num4 < 1000000000)
				{
					num2 = 0u;
				}
				else
				{
					num2 = (uint)(num4 / 1000000000uL);
					num4 -= (ulong)((long)num2 * 1000000000L);
				}
				if (rWords[rIndex] < num4)
				{
					rWords[rIndex] += (uint)(int)(1000000000 - num4);
					num3 += num2 + 1;
				}
				else
				{
					rWords[rIndex] -= (uint)(int)num4;
					num3 += num2;
				}
				if (num3 != 0)
				{
					if (rWords[rIndex - 1] < num3)
					{
						rWords[rIndex - 1] += 1000000000 - num3;
						num3 = 1u;
					}
					else
					{
						rWords[rIndex - 1] -= num3;
						num3 = 0u;
					}
				}
				rIndex--;
				num--;
			}
		}

		private static void FindNextNonEqual(uint[] i, uint[] j, int iSize, ref int iIndex, ref int jIndex)
		{
			while (iIndex < iSize && i[iIndex] == j[jIndex])
			{
				iIndex++;
				jIndex++;
			}
		}

		private static void SubtractInPlace(uint[] x, uint[] y, int count, int offset)
		{
			uint num = 0u;
			int num2 = count - 1;
			int num3 = offset + num2;
			while (num2 >= 0)
			{
				if (x[num3] < y[num2] + num)
				{
					x[num3] += 1000000000 - (y[num2] + num);
					num = 1u;
				}
				else
				{
					x[num3] -= y[num2] + num;
					num = 0u;
				}
				num2--;
				num3--;
			}
			while (num != 0)
			{
				if (x[num3] < num)
				{
					x[num3] += 1000000000 - num;
					num = 1u;
				}
				else
				{
					x[num3] -= num;
					num = 0u;
				}
				num3--;
			}
		}

		private static ulong GetDoubleWord(uint[] a, int i, int precision)
		{
			if (i + 1 == precision)
			{
				return (ulong)a[i] * 1000000000uL;
			}
			return (ulong)((long)a[i] * 1000000000L + a[i + 1]);
		}

		private static void Carry(uint[] a, int i)
		{
			while (a[i] >= 1000000000)
			{
				a[i] -= 1000000000u;
				i--;
				a[i]++;
			}
		}

		private static Fraction ScrollRight(Fraction fraction, int offset)
		{
			if (offset % 9 == 0)
			{
				return fraction;
			}
			int precision = fraction.Precision;
			int maxPrecision = precision + 1;
			Fraction fraction2 = new Fraction(maxPrecision);
			uint num = 1u;
			for (int i = 0; i < offset; i++)
			{
				num *= 10;
			}
			uint num2 = 1000000000u / num;
			uint num3 = 0u;
			uint num4 = 0u;
			int j;
			for (j = 0; j < precision; j++)
			{
				num3 = fraction._words[j] / num;
				fraction2._words[j] = num4 + num3;
				num4 = fraction._words[j] % num * num2;
			}
			fraction2._words[j] = num4;
			return fraction2;
		}

		private static Fraction Parse(string digits, out int digitOffset)
		{
			digits = digits.TrimEnd('0');
			if (digits == "")
			{
				digitOffset = 0;
				return Zero;
			}
			int num = digits.Length / 9;
			int i = digits.Length % 9;
			if (i > 0)
			{
				num++;
			}
			Fraction fraction = new Fraction(num);
			int num2 = 0;
			int j;
			for (j = 0; j + 9 <= digits.Length; j += 9)
			{
				fraction._words[num2] = uint.Parse(digits.Substring(j, 9), CultureInfo.InvariantCulture);
				num2++;
			}
			if (i > 0)
			{
				uint num3 = uint.Parse(digits.Substring(j), CultureInfo.InvariantCulture);
				for (; i < 9; i++)
				{
					num3 *= 10;
				}
				fraction._words[num2] = num3;
			}
			return Normalize(fraction, out digitOffset);
		}

		private static Fraction Normalize(Fraction f, out int digitOffset)
		{
			int i;
			for (i = 0; i < f.MaxPrecision && f._words[i] == 0; i++)
			{
			}
			if (i == f.MaxPrecision)
			{
				digitOffset = 0;
				return f;
			}
			int j;
			for (j = 0; f._words[f.Precision - 1 - j] == 0; j++)
			{
			}
			int num = 9;
			uint num2 = f._words[i];
			uint num3 = 1u;
			while (num2 != 0)
			{
				num2 /= 10u;
				num--;
				num3 *= 10;
			}
			uint num4 = 1000000000u / num3;
			int maxPrecision = f.Precision - i - j;
			Fraction fraction = new Fraction(maxPrecision);
			Array.Copy(f._words, i, fraction._words, 0, fraction.MaxPrecision);
			if (num > 0)
			{
				uint num5 = fraction._words[0] * num4;
				uint num6 = 0u;
				int k;
				for (k = 1; k < fraction.Precision; k++)
				{
					num6 = fraction._words[k] / num3;
					fraction._words[k - 1] = num5 + num6;
					num5 = fraction._words[k] % num3 * num4;
				}
				fraction._words[k - 1] = num5;
				fraction.Precision = k;
			}
			digitOffset = -(i * 9 + num);
			return fraction;
		}
	}
}

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	internal sealed class StringFormatter
	{
		[Flags]
		internal enum FormatOptions
		{
			ZeroPad = 1,
			LeftAdj = 2,
			AltForm = 4,
			Space = 8,
			SignChar = 0x10
		}

		internal struct FormatSettings
		{
			internal FormatOptions Options;

			internal int FieldWidth;

			internal int Precision;

			internal object Value;

			internal int? ArgIndex;

			public bool ZeroPad
			{
				get
				{
					return (Options & FormatOptions.ZeroPad) != 0;
				}
				set
				{
					if (value)
					{
						Options |= FormatOptions.ZeroPad;
					}
					else
					{
						Options &= ~FormatOptions.ZeroPad;
					}
				}
			}

			public bool LeftAdj
			{
				get
				{
					return (Options & FormatOptions.LeftAdj) != 0;
				}
				set
				{
					if (value)
					{
						Options |= FormatOptions.LeftAdj;
					}
					else
					{
						Options &= ~FormatOptions.LeftAdj;
					}
				}
			}

			public bool AltForm
			{
				get
				{
					return (Options & FormatOptions.AltForm) != 0;
				}
				set
				{
					if (value)
					{
						Options |= FormatOptions.AltForm;
					}
					else
					{
						Options &= ~FormatOptions.AltForm;
					}
				}
			}

			public bool Space
			{
				get
				{
					return (Options & FormatOptions.Space) != 0;
				}
				set
				{
					if (value)
					{
						Options |= FormatOptions.Space;
					}
					else
					{
						Options &= ~FormatOptions.Space;
					}
				}
			}

			public bool SignChar
			{
				get
				{
					return (Options & FormatOptions.SignChar) != 0;
				}
				set
				{
					if (value)
					{
						Options |= FormatOptions.SignChar;
					}
					else
					{
						Options &= ~FormatOptions.SignChar;
					}
				}
			}
		}

		private const int UnspecifiedPrecision = -1;

		[ThreadStatic]
		private static NumberFormatInfo NumberFormatInfoForThread;

		private readonly IList _data;

		private readonly string _format;

		private readonly RubyContext _context;

		private bool? _useAbsolute;

		private int _relativeIndex;

		private bool _tainted;

		private int _index;

		private char _curCh;

		private FormatSettings _opts;

		private bool _TrailingZeroAfterWholeFloat;

		private StringBuilder _buf;

		private readonly RubyEncoding _encoding;

		private readonly StringFormatterSiteStorage _siteStorage;

		private static readonly char[] zero = new char[1] { '0' };

		private static uint[] _Mask = new uint[5] { 0u, 1u, 0u, 7u, 15u };

		private static char[] _UpperDigits = new char[16]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F'
		};

		private static char[] _LowerDigits = new char[16]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'a', 'b', 'c', 'd', 'e', 'f'
		};

		private static NumberFormatInfo nfi
		{
			get
			{
				if (NumberFormatInfoForThread == null)
				{
					NumberFormatInfo numberFormat = new CultureInfo("en-US").NumberFormat;
					numberFormat.PositiveInfinitySymbol = "Infinity";
					numberFormat.NegativeInfinitySymbol = "-Infinity";
					numberFormat.NaNSymbol = "NaN";
					NumberFormatInfoForThread = numberFormat;
				}
				return NumberFormatInfoForThread;
			}
		}

		public bool TrailingZeroAfterWholeFloat
		{
			get
			{
				return _TrailingZeroAfterWholeFloat;
			}
			set
			{
				_TrailingZeroAfterWholeFloat = value;
			}
		}

		internal StringFormatter(RubyContext context, string format, RubyEncoding encoding, IList data)
		{
			_context = context;
			_format = format;
			_data = data;
			_encoding = encoding;
		}

		internal StringFormatter(StringFormatterSiteStorage siteStorage, string format, RubyEncoding encoding, IList data)
			: this(siteStorage.Context, format, encoding, data)
		{
			_siteStorage = siteStorage;
		}

		public MutableString Format()
		{
			_index = 0;
			_buf = new StringBuilder();
			_tainted = false;
			int num;
			while ((num = _format.IndexOf('%', _index)) != -1)
			{
				_buf.Append(_format, _index, num - _index);
				_index = num + 1;
				DoFormatCode();
			}
			if (_context.DomainManager.Configuration.DebugMode && (!_useAbsolute.HasValue || !_useAbsolute.Value) && _relativeIndex != _data.Count)
			{
				throw RubyExceptions.CreateArgumentError("too many arguments for format string");
			}
			_buf.Append(_format, _index, _format.Length - _index);
			MutableString mutableString = MutableString.Create(_buf.ToString(), _encoding);
			if (_tainted)
			{
				mutableString.IsTainted = true;
			}
			return mutableString;
		}

		private void DoFormatCode()
		{
			if (_index == _format.Length || _format[_index] == '\n' || _format[_index] == '\0')
			{
				_buf.Append('%');
				return;
			}
			_curCh = _format[_index++];
			if (_curCh == '%')
			{
				_buf.Append('%');
				return;
			}
			_opts = default(FormatSettings);
			ReadConversionFlags();
			ReadArgumentIndex();
			ReadMinimumFieldWidth();
			ReadPrecision();
			ReadArgumentIndex();
			_opts.Value = GetData(_opts.ArgIndex);
			WriteConversion();
		}

		private void ReadConversionFlags()
		{
			while (true)
			{
				switch (_curCh)
				{
				default:
					return;
				case '#':
					_opts.AltForm = true;
					break;
				case '-':
					_opts.LeftAdj = true;
					_opts.ZeroPad = false;
					break;
				case '0':
					if (!_opts.LeftAdj)
					{
						_opts.ZeroPad = true;
					}
					break;
				case '+':
					_opts.SignChar = true;
					_opts.Space = false;
					break;
				case ' ':
					if (!_opts.SignChar)
					{
						_opts.Space = true;
					}
					break;
				}
				if (_index >= _format.Length)
				{
					break;
				}
				_curCh = _format[_index++];
			}
			throw RubyExceptions.CreateArgumentError("illegal format character - %");
		}

		private void ReadArgumentIndex()
		{
			int? argIndex = TryReadArgumentIndex();
			if (argIndex.HasValue)
			{
				if (_opts.ArgIndex.HasValue)
				{
					RubyExceptions.CreateArgumentError("value given twice");
				}
				_opts.ArgIndex = argIndex;
			}
		}

		private int? TryReadArgumentIndex()
		{
			if (char.IsDigit(_curCh))
			{
				int i;
				for (i = _index; i < _format.Length && char.IsDigit(_format[i]); i++)
				{
				}
				if (i < _format.Length && _format[i] == '$')
				{
					int value = int.Parse(_format.Substring(_index - 1, i - _index + 1), CultureInfo.InvariantCulture);
					_index = i + 1;
					if (_index < _format.Length)
					{
						_curCh = _format[_index++];
						return value;
					}
				}
			}
			return null;
		}

		private int ReadNumberOrStar()
		{
			int num = 0;
			if (_curCh == '*')
			{
				_curCh = _format[_index++];
				int? absoluteIndex = TryReadArgumentIndex();
				num = _siteStorage.CastToFixnum(GetData(absoluteIndex));
				if (num < 0)
				{
					_opts.LeftAdj = true;
					num = -num;
				}
			}
			else if (char.IsDigit(_curCh))
			{
				num = 0;
				while (char.IsDigit(_curCh) && _index < _format.Length)
				{
					num = num * 10 + (_curCh - 48);
					_curCh = _format[_index++];
				}
			}
			return num;
		}

		private void ReadMinimumFieldWidth()
		{
			_opts.FieldWidth = ReadNumberOrStar();
			if (_opts.FieldWidth == int.MaxValue)
			{
				throw RubyExceptions.CreateRangeError("bignum too big to convert into `long'");
			}
		}

		private void ReadPrecision()
		{
			if (_curCh == '.')
			{
				_curCh = _format[_index++];
				_opts.Precision = ReadNumberOrStar();
			}
			else
			{
				_opts.Precision = -1;
			}
		}

		private void WriteConversion()
		{
			switch (_curCh)
			{
			case 'B':
			case 'b':
				AppendBinary(_curCh);
				break;
			case 'c':
				AppendChar();
				break;
			case 'd':
			case 'i':
				AppendInt('D');
				break;
			case 'E':
			case 'G':
			case 'e':
			case 'f':
			case 'g':
				AppendFloat(_curCh);
				break;
			case 'o':
				AppendOctal();
				break;
			case 'p':
				AppendInspect();
				break;
			case 's':
				AppendString();
				break;
			case 'u':
				AppendInt(_curCh);
				break;
			case 'X':
			case 'x':
				AppendHex(_curCh);
				break;
			default:
				throw RubyExceptions.CreateArgumentError("malformed format string - %" + _curCh);
			}
		}

		private object GetData(int? absoluteIndex)
		{
			if (_useAbsolute.HasValue)
			{
				if (_useAbsolute.Value && !absoluteIndex.HasValue)
				{
					throw RubyExceptions.CreateArgumentError("unnumbered({0}) mixed with numbered", _relativeIndex + 1);
				}
				if (!_useAbsolute.Value && absoluteIndex.HasValue)
				{
					throw RubyExceptions.CreateArgumentError("numbered({0}) after unnumbered({1})", absoluteIndex.Value, _relativeIndex + 1);
				}
			}
			else
			{
				_useAbsolute = absoluteIndex.HasValue;
			}
			int num = (_useAbsolute.Value ? (absoluteIndex.Value - 1) : _relativeIndex++);
			if (num < _data.Count)
			{
				return _data[num];
			}
			throw RubyExceptions.CreateArgumentError("too few arguments");
		}

		private void AppendChar()
		{
			int num = _siteStorage.CastToFixnum(_opts.Value);
			if (num < 0 && _context.RubyOptions.Compatibility >= RubyCompatibility.Default)
			{
				throw RubyExceptions.CreateArgumentError("invalid character: {0}", num);
			}
			char value = (char)((uint)num & 0xFFu);
			if (_opts.FieldWidth > 1)
			{
				if (!_opts.LeftAdj)
				{
					_buf.Append(' ', _opts.FieldWidth - 1);
				}
				_buf.Append(value);
				if (_opts.LeftAdj)
				{
					_buf.Append(' ', _opts.FieldWidth - 1);
				}
			}
			else
			{
				_buf.Append(value);
			}
		}

		private void AppendInt(char format)
		{
			IntegerValue integerValue = ((_opts.Value == null) ? ((IntegerValue)0) : _siteStorage.ConvertToInteger(_opts.Value));
			bool fPos;
			object val;
			if (integerValue.IsFixnum)
			{
				fPos = integerValue.Fixnum >= 0;
				val = integerValue.Fixnum;
			}
			else
			{
				fPos = integerValue.Bignum.IsZero() || integerValue.Bignum.IsPositive();
				val = integerValue.Bignum;
			}
			if (_opts.LeftAdj)
			{
				AppendLeftAdj(val, fPos, 'D');
			}
			else if (_opts.ZeroPad)
			{
				AppendZeroPad(val, fPos, 'D');
			}
			else
			{
				AppendNumeric(val, fPos, 'D', format == 'u');
			}
		}

		private char AdjustForG(char type, double v)
		{
			if (type != 'G' && type != 'g')
			{
				return type;
			}
			if (double.IsNaN(v) || double.IsInfinity(v))
			{
				return type;
			}
			double num = Math.Abs(v);
			if ((v != 0.0 && num < 0.0001) || num >= Math.Pow(10.0, _opts.Precision))
			{
				int num2 = _opts.Precision - 1;
				string text = num.ToString("E" + num2, CultureInfo.InvariantCulture);
				string text2 = text.Substring(0, text.IndexOf('E')).TrimEnd(zero);
				_opts.Precision = text2.Length - 2;
				type = ((type == 'G') ? 'E' : 'e');
			}
			else
			{
				int num3 = _opts.Precision;
				if (num < 0.001)
				{
					num3 += 3;
				}
				else if (num < 0.01)
				{
					num3 += 2;
				}
				else if (num < 0.1)
				{
					num3++;
				}
				string text3 = num.ToString("F" + num3, CultureInfo.InvariantCulture).TrimEnd(zero);
				string text4 = text3.Substring(text3.IndexOf('.') + 1);
				if (num < 1.0)
				{
					_opts.Precision = text4.Length;
				}
				else
				{
					int num4 = 1 + (int)Math.Log10(num);
					_opts.Precision = Math.Min(_opts.Precision - num4, text4.Length);
				}
				type = 'f';
			}
			return type;
		}

		private void AppendFloat(char type)
		{
			double num = ((_siteStorage == null) ? ((double)_opts.Value) : _siteStorage.CastToDouble(_opts.Value));
			bool flag = false;
			if (_opts.Precision != -1)
			{
				if (_opts.Precision == 0 && _opts.AltForm)
				{
					flag = true;
				}
				if (_opts.Precision > 50)
				{
					_opts.Precision = 50;
				}
			}
			else if (_opts.AltForm)
			{
				_opts.Precision = 0;
				flag = true;
			}
			else
			{
				_opts.Precision = 6;
			}
			type = AdjustForG(type, num);
			nfi.NumberDecimalDigits = _opts.Precision;
			if (_opts.LeftAdj)
			{
				AppendLeftAdj(num, num >= 0.0, type);
			}
			else if (_opts.ZeroPad)
			{
				AppendZeroPadFloat(num, type);
			}
			else
			{
				AppendNumeric(num, num >= 0.0, type, false);
			}
			if (num <= 0.0 && num > -1.0 && _buf[0] != '-')
			{
				FixupFloatMinus(num);
			}
			if (flag)
			{
				FixupAltFormDot();
			}
		}

		private void FixupAltFormDot()
		{
			_buf.Append('.');
			if (_opts.FieldWidth == 0)
			{
				return;
			}
			for (int i = 0; i < _buf.Length; i++)
			{
				switch (_buf[i])
				{
				case ' ':
				case '0':
					_buf.Remove(i, 1);
					return;
				default:
					return;
				case '+':
				case '-':
					break;
				}
			}
		}

		private void FixupFloatMinus(double value)
		{
			bool flag;
			if (value == 0.0)
			{
				flag = MathUtils.IsNegativeZero(value);
			}
			else
			{
				flag = true;
				for (int i = 0; i < _buf.Length; i++)
				{
					char c = _buf[i];
					if (c != '.' && c != '0' && c != ' ')
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			if (_opts.FieldWidth != 0)
			{
				if (_buf[_buf.Length - 1] == ' ')
				{
					_buf.Insert(0, "-");
					_buf.Remove(_buf.Length - 1, 1);
					return;
				}
				int j;
				for (j = 0; _buf[j] == ' '; j++)
				{
				}
				if (j > 0)
				{
					j--;
				}
				_buf[j] = '-';
			}
			else
			{
				_buf.Insert(0, "-");
			}
		}

		private void AppendZeroPad(object val, bool fPos, char format)
		{
			if (fPos && (_opts.SignChar || _opts.Space))
			{
				string text = string.Format(nfi, "{0:" + format + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + "}", new object[1] { val });
				char c = (_opts.SignChar ? '+' : ' ');
				text = ((text[0] != '0' || text.Length <= 1) ? (c + text) : (c + text.Substring(1)));
				_buf.Append(text);
				return;
			}
			string text2 = string.Format(nfi, "{0:" + format + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + "}", new object[1] { val });
			if (text2[0] == '-')
			{
				_buf.Append("-");
				if (text2[1] != '0')
				{
					_buf.Append(text2.Substring(1));
				}
				else
				{
					_buf.Append(text2.Substring(2));
				}
			}
			else
			{
				_buf.Append(text2);
			}
		}

		private void AppendZeroPadFloat(double val, char format)
		{
			if (val >= 0.0)
			{
				StringBuilder stringBuilder = new StringBuilder(val.ToString(format.ToString(), nfi));
				if (stringBuilder.Length < _opts.FieldWidth)
				{
					stringBuilder.Insert(0, new string('0', _opts.FieldWidth - stringBuilder.Length));
				}
				if (_opts.SignChar || _opts.Space)
				{
					char value = (_opts.SignChar ? '+' : ' ');
					if (stringBuilder[0] == '0' && stringBuilder[1] != '.')
					{
						stringBuilder[0] = value;
					}
					else
					{
						stringBuilder.Insert(0, value.ToString());
					}
				}
				_buf.Append(stringBuilder);
			}
			else
			{
				StringBuilder stringBuilder2 = new StringBuilder(val.ToString(format.ToString(), nfi));
				if (stringBuilder2.Length < _opts.FieldWidth)
				{
					stringBuilder2.Insert(1, new string('0', _opts.FieldWidth - stringBuilder2.Length));
				}
				_buf.Append(stringBuilder2);
			}
		}

		private void AppendNumeric(object val, bool fPos, char format, bool unsigned)
		{
			bool flag = false;
			if (val is BigInteger && ((BigInteger)val).Sign == -1)
			{
				flag = true;
			}
			else if (val is int && (int)val < 0)
			{
				flag = true;
			}
			else if (val is float && (float)val < 0f)
			{
				flag = true;
			}
			if (flag && unsigned)
			{
				val = ((val is BigInteger) ? CastToUnsignedBigInteger(val as BigInteger) : ((object)(uint)(int)val));
			}
			if (fPos && (_opts.SignChar || _opts.Space))
			{
				string text = (_opts.SignChar ? "+" : " ") + string.Format(nfi, "{0:" + format + "}", new object[1] { val });
				if (text.Length < _opts.FieldWidth)
				{
					_buf.Append(' ', _opts.FieldWidth - text.Length);
				}
				_buf.Append(text);
			}
			else if (_opts.Precision == -1)
			{
				_buf.AppendFormat(nfi, "{0," + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + ":" + format + "}", new object[1] { val });
				if (unsigned && flag)
				{
					_buf.Insert(0, "..");
				}
			}
			else if (_opts.Precision < 100)
			{
				_buf.AppendFormat(nfi, "{0," + _opts.FieldWidth.ToString(CultureInfo.InvariantCulture) + ":" + format + _opts.Precision.ToString(CultureInfo.InvariantCulture) + "}", new object[1] { val });
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("{0:" + format + "}", val);
				if (stringBuilder.Length < _opts.Precision)
				{
					char c = (unsigned ? '.' : '0');
					stringBuilder.Insert(0, new string(c, _opts.Precision - stringBuilder.Length));
				}
				if (stringBuilder.Length < _opts.FieldWidth)
				{
					stringBuilder.Insert(0, new string(' ', _opts.FieldWidth - stringBuilder.Length));
				}
				_buf.Append(stringBuilder.ToString());
			}
			if (_TrailingZeroAfterWholeFloat && format == 'f' && _opts.Precision == 0)
			{
				_buf.Append(".0");
			}
		}

		private void AppendLeftAdj(object val, bool fPos, char type)
		{
			string text = string.Format(nfi, "{0:" + type + "}", new object[1] { val });
			if (fPos)
			{
				if (_opts.SignChar)
				{
					text = '+' + text;
				}
				else if (_opts.Space)
				{
					text = ' ' + text;
				}
			}
			_buf.Append(text);
			if (text.Length < _opts.FieldWidth)
			{
				_buf.Append(' ', _opts.FieldWidth - text.Length);
			}
		}

		private static bool NeedsAltForm(char format, char last)
		{
			if (format == 'X' || format == 'x')
			{
				return true;
			}
			if (last == '0')
			{
				return false;
			}
			return true;
		}

		private static string GetAltFormPrefixForRadix(char format, int radix)
		{
			switch (radix)
			{
			case 2:
				if (format != 'b')
				{
					return "B0";
				}
				return "b0";
			case 8:
				return "0";
			case 16:
				return format + "0";
			default:
				return "";
			}
		}

		private StringBuilder AppendBase(object value, int bitsToShift, bool lowerCase)
		{
			if (value is BigInteger)
			{
				return AppendBaseBigInteger(value as BigInteger, bitsToShift, lowerCase);
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = IsNegative(value);
			uint num = (uint)(int)value;
			uint num2 = (flag ? uint.MaxValue : 0u);
			uint num3 = _Mask[bitsToShift];
			char[] array = (lowerCase ? _LowerDigits : _UpperDigits);
			if (IsZero(value))
			{
				stringBuilder.Append(array[0]);
				return stringBuilder;
			}
			while (num != num2)
			{
				stringBuilder.Append(array[num & num3]);
				num >>= bitsToShift;
				num2 >>= bitsToShift;
			}
			if (flag)
			{
				stringBuilder.Append(array[num3]);
			}
			return stringBuilder;
		}

		private StringBuilder AppendBaseInt(int value, int radix)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (value == 0)
			{
				stringBuilder.Append('0');
			}
			while (value != 0)
			{
				int num = value % radix;
				stringBuilder.Append(_LowerDigits[num]);
				value /= radix;
			}
			return stringBuilder;
		}

		private StringBuilder AppendBaseUnsignedInt(uint value, uint radix)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (value == 0)
			{
				stringBuilder.Append('0');
			}
			while (value != 0)
			{
				uint num = value % radix;
				stringBuilder.Append(_LowerDigits[num]);
				value /= radix;
			}
			return stringBuilder;
		}

		private StringBuilder AppendBase2(object value, int radix, bool unsigned)
		{
			if (value is BigInteger)
			{
				return AppendBaseBigInteger(value as BigInteger, radix);
			}
			if (unsigned)
			{
				return AppendBaseInt((int)value, radix);
			}
			return AppendBaseUnsignedInt((uint)value, (uint)radix);
		}

		private StringBuilder AppendBaseBigInteger(BigInteger value, int radix)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (value == 0)
			{
				stringBuilder.Append('0');
			}
			while (value != 0)
			{
				int num = (int)(value % radix);
				stringBuilder.Append(_LowerDigits[num]);
				value /= (BigInteger)radix;
			}
			return stringBuilder;
		}

		private BigInteger MakeBigIntegerFromByteArray(byte[] bytes)
		{
			uint[] array = new uint[bytes.Length / 4 + 1];
			int num = 0;
			for (int i = 0; i < bytes.Length; i += 4)
			{
				uint num2 = 0u;
				int num3 = bytes.Length - i;
				if (num3 > 3)
				{
					num2 = (uint)(bytes[i] | (bytes[i + 1] << 8) | (bytes[i + 2] << 16) | (bytes[i + 3] << 24));
				}
				else
				{
					switch (num3)
					{
					case 3:
						num2 = (uint)(bytes[i] | (bytes[i + 1] << 8) | (bytes[i + 2] << 16));
						break;
					case 2:
						num2 = (uint)(bytes[i] | (bytes[i + 1] << 8));
						break;
					case 1:
						num2 = bytes[i];
						break;
					}
				}
				array[num++] = num2;
			}
			return new BigInteger(1, array);
		}

		private BigInteger CastToUnsignedBigInteger(BigInteger value)
		{
			return MakeBigIntegerFromByteArray(value.ToByteArray());
		}

		private BigInteger GenerateMask(BigInteger value)
		{
			byte[] array = new byte[value.ToByteArray().Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = byte.MaxValue;
			}
			return MakeBigIntegerFromByteArray(array);
		}

		private StringBuilder AppendBaseBigInteger(BigInteger value, int bitsToShift, bool lowerCase)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = value.Sign == -1;
			BigInteger bigInteger = CastToUnsignedBigInteger(value);
			BigInteger bigInteger2 = (flag ? GenerateMask(value) : BigInteger.Zero);
			uint num = _Mask[bitsToShift];
			char[] array = (lowerCase ? _LowerDigits : _UpperDigits);
			for (; bigInteger != bigInteger2; bigInteger2 >>= bitsToShift)
			{
				stringBuilder.Append(array[(int)(bigInteger & num)]);
				bigInteger >>= bitsToShift;
			}
			if (flag)
			{
				stringBuilder.Append(array[num]);
			}
			return stringBuilder;
		}

		private object Negate(object value)
		{
			if (value is BigInteger)
			{
				return ((BigInteger)value).OnesComplement();
			}
			return -(int)value;
		}

		private bool IsZero(object value)
		{
			if (value is BigInteger)
			{
				return ((BigInteger)value).IsZero();
			}
			return (int)value == 0;
		}

		private bool IsNegative(object value)
		{
			if (value is BigInteger)
			{
				return ((BigInteger)value).Sign == -1;
			}
			return (int)value < 0;
		}

		private void AppendBase(char format, int radix)
		{
			IntegerValue integerValue = ((_opts.Value == null) ? ((IntegerValue)0) : _siteStorage.ConvertToInteger(_opts.Value));
			object value = (integerValue.IsFixnum ? ((object)integerValue.Fixnum) : integerValue.Bignum);
			bool flag = IsNegative(value);
			if (flag)
			{
				if (_opts.Space || _opts.SignChar)
				{
					value = Negate(value);
				}
				if (radix != 2 && radix != 8 && radix != 16)
				{
					_opts.Space = false;
				}
			}
			StringBuilder stringBuilder;
			switch (radix)
			{
			case 2:
				stringBuilder = AppendBase(value, 1, true);
				break;
			case 8:
				stringBuilder = AppendBase(value, 3, true);
				break;
			case 16:
				stringBuilder = AppendBase(value, 4, format == 'x');
				break;
			default:
				stringBuilder = AppendBase2(value, 10, format == 'u');
				break;
			}
			if (stringBuilder.Length < _opts.Precision)
			{
				int repeatCount = _opts.Precision - stringBuilder.Length;
				char value2 = '0';
				if (radix == 2 && flag)
				{
					value2 = '1';
				}
				else if (radix == 8 && flag)
				{
					value2 = '7';
				}
				else if (radix == 16 && flag)
				{
					value2 = ((format == 'x') ? 'f' : 'F');
				}
				stringBuilder.Append(value2, repeatCount);
			}
			if (_opts.FieldWidth != 0)
			{
				int num = ((flag || _opts.SignChar) ? 1 : 0);
				int num2 = (_opts.Space ? 1 : 0);
				int num3 = _opts.FieldWidth - (stringBuilder.Length + num + num2);
				if (num3 > 0)
				{
					if (_opts.AltForm && NeedsAltForm(format, (!_opts.LeftAdj && _opts.ZeroPad) ? '0' : stringBuilder[stringBuilder.Length - 1]))
					{
						num3 -= GetAltFormPrefixForRadix(format, radix).Length;
					}
					if (num3 > 0)
					{
						if (_opts.LeftAdj)
						{
							stringBuilder.Insert(0, " ", num3);
						}
						else if (_opts.ZeroPad)
						{
							stringBuilder.Append('0', num3);
						}
						else
						{
							_buf.Append(' ', num3);
						}
					}
				}
			}
			if (_opts.AltForm && NeedsAltForm(format, stringBuilder[stringBuilder.Length - 1]))
			{
				stringBuilder.Append(GetAltFormPrefixForRadix(format, radix));
			}
			if (flag)
			{
				if (radix == 2 || radix == 8 || radix == 16)
				{
					if (_opts.SignChar || _opts.Space)
					{
						_buf.Append('-');
					}
					else if (!_opts.ZeroPad && _opts.Precision == -1)
					{
						_buf.Append("..");
					}
				}
				else
				{
					_buf.Append("-");
				}
			}
			else if (_opts.SignChar)
			{
				_buf.Append('+');
			}
			else if (_opts.Space)
			{
				_buf.Append(' ');
			}
			for (int num4 = stringBuilder.Length - 1; num4 >= 0; num4--)
			{
				_buf.Append(stringBuilder[num4]);
			}
		}

		private void AppendBinary(char format)
		{
			AppendBase(format, 2);
		}

		private void AppendHex(char format)
		{
			AppendBase(format, 16);
		}

		private void AppendOctal()
		{
			AppendBase('o', 8);
		}

		private void AppendInspect()
		{
			MutableString mutableString = _context.Inspect(_opts.Value);
			if (KernelOps.Tainted(_context, mutableString))
			{
				_tainted = true;
			}
			AppendString(mutableString);
		}

		private void AppendString()
		{
			MutableString mutableString = _siteStorage.ConvertToString(_opts.Value);
			if (KernelOps.Tainted(_context, mutableString))
			{
				_tainted = true;
			}
			AppendString(mutableString);
		}

		private void AppendString(MutableString mutable)
		{
			string text = mutable.ConvertToString();
			if (_opts.Precision != -1 && text.Length > _opts.Precision)
			{
				text = text.Substring(0, _opts.Precision);
			}
			if (!_opts.LeftAdj && _opts.FieldWidth > text.Length)
			{
				_buf.Append(' ', _opts.FieldWidth - text.Length);
			}
			_buf.Append(text);
			if (_opts.LeftAdj && _opts.FieldWidth > text.Length)
			{
				_buf.Append(' ', _opts.FieldWidth - text.Length);
			}
		}
	}
}

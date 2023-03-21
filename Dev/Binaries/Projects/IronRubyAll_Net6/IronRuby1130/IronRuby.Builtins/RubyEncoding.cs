using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Utils.Extensions;

namespace IronRuby.Builtins
{
	[Serializable]
	public class RubyEncoding : ISerializable, IExpressionSerializable
	{
		[Serializable]
		internal sealed class Deserializer : ISerializable, IObjectReference
		{
			private readonly int _codePage;

			private Deserializer(SerializationInfo info, StreamingContext context)
			{
				_codePage = info.GetInt32("CodePage");
			}

			public object GetRealObject(StreamingContext context)
			{
				return GetRubyEncoding(_codePage);
			}

			void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
			{
				throw Assert.Unreachable;
			}
		}

		public const int CodePageBinary = 0;

		public const int CodePageSJIS = 932;

		public const int CodePageBig5 = 950;

		public const int CodePageAscii = 20127;

		public const int CodePageEUCJP = 51932;

		public const int CodePageUTF7 = 65000;

		public const int CodePageUTF8 = 65001;

		public const int CodePageUTF16BE = 1201;

		public const int CodePageUTF16LE = 1200;

		public const int CodePageUTF32BE = 12001;

		public const int CodePageUTF32LE = 12000;

		public static readonly RubyEncoding Binary = new RubyEncoding(BinaryEncoding.Instance, BinaryEncoding.Instance, -4);

		public static readonly RubyEncoding UTF8 = new RubyEncoding(CreateEncoding(65001, false), CreateEncoding(65001, true), -3);

		public static readonly RubyEncoding Ascii = new RubyEncoding(CreateEncoding(20127, false), CreateEncoding(20127, true), -2);

		public static readonly RubyEncoding EUCJP = new RubyEncoding(CreateEncoding(51932, false), CreateEncoding(51932, true), -1);

		public static readonly RubyEncoding SJIS = new RubyEncoding(CreateEncoding(932, false), CreateEncoding(932, true), 0);

		private readonly Encoding _encoding;

		private readonly Encoding _strictEncoding;

		private Expression _expression;

		private readonly int _ordinal;

		private readonly int _maxBytesPerChar;

		private readonly bool _isAsciiIdentity;

		private bool? _isSingleByteCharacterSet;

		private bool? _isDoubleByteCharacterSet;

		private static Dictionary<int, RubyEncoding> _Encodings;

		private static string _AllAscii;

		private static int[] _sbsc;

		private static int[] _dbsc;

		private static ReadOnlyDictionary<string, string> _aliases;

		internal Expression Expression
		{
			get
			{
				return _expression ?? (_expression = Expression.Constant(this));
			}
		}

		public bool IsAsciiIdentity
		{
			get
			{
				return _isAsciiIdentity;
			}
		}

		public int MaxBytesPerChar
		{
			get
			{
				return _maxBytesPerChar;
			}
		}

		public Encoding Encoding
		{
			get
			{
				return _encoding;
			}
		}

		public Encoding StrictEncoding
		{
			get
			{
				return _strictEncoding;
			}
		}

		public string Name
		{
			get
			{
				return GetRubySpecificName(CodePage) ?? _encoding.WebName;
			}
		}

		public int CodePage
		{
			get
			{
				return GetCodePage(_encoding);
			}
		}

		public bool IsSingleByteCharacterSet
		{
			get
			{
				if (!_isSingleByteCharacterSet.HasValue)
				{
					_isSingleByteCharacterSet = IsSBCS(CodePage);
				}
				return _isSingleByteCharacterSet.Value;
			}
		}

		public bool IsDoubleByteCharacterSet
		{
			get
			{
				if (!_isDoubleByteCharacterSet.HasValue)
				{
					_isDoubleByteCharacterSet = IsDBCS(CodePage);
				}
				return _isDoubleByteCharacterSet.Value;
			}
		}

		public bool InUnicodeBasicPlane
		{
			get
			{
				if (this != Ascii)
				{
					return this == Binary;
				}
				return true;
			}
		}

		public bool IsUnicodeEncoding
		{
			get
			{
				switch (CodePage)
				{
				case 1200:
				case 1201:
				case 12000:
				case 12001:
				case 65000:
				case 65001:
					return true;
				default:
					return false;
				}
			}
		}

		public static ReadOnlyDictionary<string, string> Aliases
		{
			get
			{
				return _aliases ?? (_aliases = CreateAliases());
			}
		}

		private RubyEncoding(Encoding encoding, Encoding strictEncoding, int ordinal)
		{
			_ordinal = ordinal;
			_encoding = encoding;
			_strictEncoding = strictEncoding;
			_maxBytesPerChar = strictEncoding.GetMaxByteCount(1);
			_isAsciiIdentity = AsciiIdentity(encoding);
		}

		public override int GetHashCode()
		{
			return _ordinal;
		}

		private static Encoding CreateEncoding(int codepage, bool throwOnError)
		{
			if (throwOnError)
			{
				return Encoding.GetEncoding(codepage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
			}
			return Encoding.GetEncoding(codepage, EncoderFallback.ReplacementFallback, BinaryDecoderFallback.Instance);
		}

		private RubyEncoding(SerializationInfo info, StreamingContext context)
		{
			throw Assert.Unreachable;
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("CodePage", CodePage);
			info.SetType(typeof(Deserializer));
		}

		public static string GetRubySpecificName(int codepage)
		{
			switch (codepage)
			{
			case 65001:
				return "UTF-8";
			case 65000:
				return "UTF-7";
			case 1201:
				return "UTF-16BE";
			case 1200:
				return "UTF-16LE";
			case 12001:
				return "UTF-32BE";
			case 12000:
				return "UTF-32LE";
			case 932:
				return "Shift_JIS";
			case 20127:
				return "US-ASCII";
			case 51932:
				return "EUC-JP";
			case 20932:
				return "CP20932";
			case 50220:
				return "ISO-2022-JP";
			case 50222:
				return "CP50222";
			default:
				return null;
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public int CompareTo(RubyEncoding other)
		{
			return _ordinal - other._ordinal;
		}

		public static RubyRegexOptions ToRegexOption(RubyEncoding encoding)
		{
			if (encoding == Binary)
			{
				return RubyRegexOptions.FIXED;
			}
			if (encoding == null)
			{
				return RubyRegexOptions.NONE;
			}
			switch (encoding.CodePage)
			{
			case 932:
				return RubyRegexOptions.SJIS;
			case 51932:
				return RubyRegexOptions.EUC;
			case 65001:
				return RubyRegexOptions.UTF8;
			default:
				throw Assert.Unreachable;
			}
		}

		public static RubyEncoding GetRegexEncoding(RubyRegexOptions options)
		{
			switch (options & RubyRegexOptions.EncodingMask)
			{
			case RubyRegexOptions.EUC:
				return EUCJP;
			case RubyRegexOptions.SJIS:
				return SJIS;
			case RubyRegexOptions.UTF8:
				return UTF8;
			case RubyRegexOptions.FIXED:
				return Binary;
			default:
				return null;
			}
		}

		internal static int GetCodePage(int nameInitial)
		{
			switch (nameInitial)
			{
			case 69:
			case 101:
				return 51932;
			case 83:
			case 115:
				return 932;
			case 85:
			case 117:
				return 65001;
			default:
				return -1;
			}
		}

		public static RubyEncoding GetEncodingByNameInitial(int initial)
		{
			int codePage = GetCodePage(initial);
			if (codePage <= 0)
			{
				return null;
			}
			return GetRubyEncoding(codePage);
		}

		public void RequireAsciiIdentity()
		{
			if (!_isAsciiIdentity)
			{
				throw new NotSupportedException(string.Format("Encoding {0} (code page {1}) is not supported", Name, CodePage));
			}
		}

		public static RubyEncoding GetRubyEncoding(Encoding encoding)
		{
			ContractUtils.RequiresNotNull(encoding, "encoding");
			if (encoding.CodePage == 0 && encoding == BinaryEncoding.Instance)
			{
				return Binary;
			}
			return GetRubyEncoding(encoding.CodePage);
		}

		public static RubyEncoding GetRubyEncoding(int codepage)
		{
			switch (codepage)
			{
			case 0:
				return Binary;
			case 20127:
				return Ascii;
			case 65001:
				return UTF8;
			case 932:
				return SJIS;
			case 51932:
				return EUCJP;
			default:
				if (_Encodings == null)
				{
					Interlocked.CompareExchange(ref _Encodings, new Dictionary<int, RubyEncoding>(), null);
				}
				lock (_Encodings)
				{
					RubyEncoding value;
					if (!_Encodings.TryGetValue(codepage, out value))
					{
						value = new RubyEncoding(CreateEncoding(codepage, false), CreateEncoding(codepage, true), codepage);
						_Encodings.Add(codepage, value);
						return value;
					}
					return value;
				}
			}
		}

		private static int GetCodePage(Encoding encoding)
		{
			return encoding.CodePage;
		}

		public static bool AsciiIdentity(Encoding encoding)
		{
			if (encoding == BinaryEncoding.Instance)
			{
				return true;
			}
			switch (encoding.CodePage)
			{
			case 437:
			case 708:
			case 720:
			case 737:
			case 775:
			case 850:
			case 852:
			case 855:
			case 857:
			case 858:
			case 860:
			case 861:
			case 862:
			case 863:
			case 864:
			case 865:
			case 866:
			case 869:
			case 874:
			case 932:
			case 936:
			case 949:
			case 950:
			case 1250:
			case 1251:
			case 1252:
			case 1253:
			case 1254:
			case 1255:
			case 1256:
			case 1257:
			case 1258:
			case 1361:
			case 10000:
			case 10001:
			case 10002:
			case 10003:
			case 10004:
			case 10005:
			case 10006:
			case 10007:
			case 10008:
			case 10010:
			case 10017:
			case 10029:
			case 10079:
			case 10081:
			case 10082:
			case 20000:
			case 20001:
			case 20002:
			case 20003:
			case 20004:
			case 20005:
			case 20127:
			case 20866:
			case 20932:
			case 20936:
			case 20949:
			case 21866:
			case 28591:
			case 28592:
			case 28593:
			case 28594:
			case 28595:
			case 28596:
			case 28597:
			case 28598:
			case 28599:
			case 28603:
			case 28605:
			case 38598:
			case 50220:
			case 50221:
			case 50222:
			case 50225:
			case 50227:
			case 51932:
			case 51936:
			case 51949:
			case 54936:
			case 57002:
			case 57003:
			case 57004:
			case 57005:
			case 57006:
			case 57007:
			case 57008:
			case 57009:
			case 57010:
			case 57011:
			case 65001:
				return true;
			default:
				return IsAsciiIdentityFallback(encoding);
			}
		}

		private static bool IsAsciiIdentityFallback(Encoding encoding)
		{
			if (_AllAscii == null)
			{
				StringBuilder stringBuilder = new StringBuilder(128);
				for (int i = 0; i < 128; i++)
				{
					stringBuilder.Append((char)i);
				}
				_AllAscii = stringBuilder.ToString();
			}
			byte[] bytes = encoding.GetBytes(_AllAscii);
			if (bytes.Length != _AllAscii.Length)
			{
				return false;
			}
			for (int j = 0; j < _AllAscii.Length; j++)
			{
				if (_AllAscii[j] != bytes[j])
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsSBCS(int codepage)
		{
			if (_sbsc == null)
			{
				_sbsc = new int[95]
				{
					0, 37, 437, 500, 708, 720, 737, 775, 850, 852,
					855, 857, 858, 860, 861, 862, 863, 864, 865, 866,
					869, 870, 874, 875, 1026, 1047, 1140, 1141, 1142, 1143,
					1144, 1145, 1146, 1147, 1148, 1149, 1250, 1251, 1252, 1253,
					1254, 1255, 1256, 1257, 1258, 10000, 10004, 10005, 10006, 10007,
					10010, 10017, 10021, 10029, 10079, 10081, 10082, 20105, 20106, 20107,
					20108, 20127, 20269, 20273, 20277, 20278, 20280, 20284, 20285, 20290,
					20297, 20420, 20423, 20424, 20833, 20838, 20866, 20871, 20880, 20905,
					20924, 21025, 21866, 28592, 28593, 28594, 28595, 28596, 28597, 28598,
					28599, 28603, 28605, 29001, 38598
				};
			}
			return Array.BinarySearch(_sbsc, codepage) >= 0;
		}

		private static bool IsDBCS(int codepage)
		{
			if (_dbsc == null)
			{
				_dbsc = new int[22]
				{
					932, 936, 949, 950, 1361, 10001, 10002, 10003, 10008, 20000,
					20001, 20002, 20003, 20004, 20005, 20261, 20932, 20936, 20949, 50227,
					51936, 51949
				};
			}
			return Array.BinarySearch(_dbsc, codepage) >= 0;
		}

		private static ReadOnlyDictionary<string, string> CreateAliases()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			dictionary.Add("646", "US-ASCII");
			dictionary.Add("ASCII", "US-ASCII");
			dictionary.Add("ANSI_X3.4-1968", "US-ASCII");
			dictionary.Add("BINARY", "ASCII-8BIT");
			dictionary.Add("CP437", "IBM437");
			dictionary.Add("CP737", "IBM737");
			dictionary.Add("CP775", "IBM775");
			dictionary.Add("CP857", "IBM857");
			dictionary.Add("CP860", "IBM860");
			dictionary.Add("CP861", "IBM861");
			dictionary.Add("CP862", "IBM862");
			dictionary.Add("CP863", "IBM863");
			dictionary.Add("CP864", "IBM864");
			dictionary.Add("CP865", "IBM865");
			dictionary.Add("CP866", "IBM866");
			dictionary.Add("CP869", "IBM869");
			dictionary.Add("CP874", "Windows-874");
			dictionary.Add("CP878", "KOI8-R");
			dictionary.Add("CP932", "Windows-31J");
			dictionary.Add("CP936", "GBK");
			dictionary.Add("CP950", "Big5");
			dictionary.Add("CP951", "Big5-HKSCS");
			dictionary.Add("CP1258", "Windows-1258");
			dictionary.Add("CP1252", "Windows-1252");
			dictionary.Add("CP1250", "Windows-1250");
			dictionary.Add("CP1256", "Windows-1256");
			dictionary.Add("CP1251", "Windows-1251");
			dictionary.Add("CP1253", "Windows-1253");
			dictionary.Add("CP1255", "Windows-1255");
			dictionary.Add("CP1254", "Windows-1254");
			dictionary.Add("CP1257", "Windows-1257");
			dictionary.Add("CP65000", "UTF-7");
			dictionary.Add("CP65001", "UTF-8");
			dictionary.Add("IBM850", "CP850");
			dictionary.Add("eucJP", "EUC-JP");
			dictionary.Add("eucKR", "EUC-KR");
			dictionary.Add("ISO2022-JP", "ISO-2022-JP");
			dictionary.Add("ISO8859-1", "ISO-8859-1");
			dictionary.Add("ISO8859-2", "ISO-8859-2");
			dictionary.Add("ISO8859-3", "ISO-8859-3");
			dictionary.Add("ISO8859-4", "ISO-8859-4");
			dictionary.Add("ISO8859-5", "ISO-8859-5");
			dictionary.Add("ISO8859-6", "ISO-8859-6");
			dictionary.Add("ISO8859-7", "ISO-8859-7");
			dictionary.Add("ISO8859-8", "ISO-8859-8");
			dictionary.Add("ISO8859-9", "ISO-8859-9");
			dictionary.Add("ISO8859-11", "ISO-8859-11");
			dictionary.Add("ISO8859-13", "ISO-8859-13");
			dictionary.Add("ISO8859-15", "ISO-8859-15");
			dictionary.Add("SJIS", "Shift_JIS");
			dictionary.Add("csWindows31J", "Windows-31J");
			dictionary.Add("UCS-2BE", "UTF-16BE");
			dictionary.Add("UCS-4BE", "UTF-32BE");
			dictionary.Add("UCS-4LE", "UTF-32LE");
			return new ReadOnlyDictionary<string, string>(dictionary);
		}

		Expression IExpressionSerializable.CreateExpression()
		{
			return Methods.CreateEncoding.OpCall(Expression.Constant(CodePage));
		}
	}
}

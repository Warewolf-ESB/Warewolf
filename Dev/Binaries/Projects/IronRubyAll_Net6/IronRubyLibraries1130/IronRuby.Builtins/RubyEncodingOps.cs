using System;
using System.Collections.Generic;
using System.Text;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Encoding", Extends = typeof(RubyEncoding), Inherits = typeof(object), BuildConfig = "!SILVERLIGHT")]
	public static class RubyEncodingOps
	{
		[RubyException("CompatibilityError", Extends = typeof(EncodingCompatibilityError))]
		public static class CompatibilityErrorOps
		{
		}

		[RubyException("UndefinedConversionError", Extends = typeof(UndefinedConversionError))]
		public static class UndefinedConversionErrorOps
		{
		}

		[RubyException("InvalidByteSequenceError", Extends = typeof(InvalidByteSequenceError))]
		public static class InvalidByteSequenceErrorOps
		{
		}

		[RubyException("ConverterNotFoundError", Extends = typeof(ConverterNotFoundError))]
		public static class ConverterNotFoundErrorOps
		{
		}

		[RubyConstant("ASCII")]
		[RubyConstant("ANSI_X3_4_1968")]
		[RubyConstant("US_ASCII")]
		public static readonly RubyEncoding US_ASCII = RubyEncoding.Ascii;

		[RubyConstant]
		public static readonly RubyEncoding UTF_8 = RubyEncoding.UTF8;

		[RubyConstant]
		public static readonly RubyEncoding ASCII_8BIT = RubyEncoding.Binary;

		[RubyConstant]
		public static readonly RubyEncoding BINARY = RubyEncoding.Binary;

		[RubyConstant("SHIFT_JIS")]
		[RubyConstant("Shift_JIS")]
		public static readonly RubyEncoding SHIFT_JIS = RubyEncoding.SJIS;

		[RubyConstant]
		public static readonly RubyEncoding EUC_JP = RubyEncoding.EUCJP;

		[RubyConstant]
		public static readonly RubyEncoding KOI8_R = RubyEncoding.GetRubyEncoding(20866);

		[RubyConstant]
		public static readonly RubyEncoding TIS_620 = RubyEncoding.GetRubyEncoding(874);

		[RubyConstant("ISO8859_9")]
		[RubyConstant("ISO_8859_9")]
		public static readonly RubyEncoding ISO_8859_9 = RubyEncoding.GetRubyEncoding(28599);

		[RubyConstant("ISO8859_15")]
		[RubyConstant("ISO_8859_15")]
		public static readonly RubyEncoding ISO_8859_15 = RubyEncoding.GetRubyEncoding(28605);

		[RubyConstant("BIG5")]
		[RubyConstant("Big5")]
		public static readonly RubyEncoding Big5 = RubyEncoding.GetRubyEncoding(950);

		[RubyConstant]
		public static readonly RubyEncoding UTF_7 = RubyEncoding.UTF8;//RubyEncoding.GetRubyEncoding(Encoding.UTF7);

		[RubyConstant]
		public static readonly RubyEncoding UTF_16BE = RubyEncoding.GetRubyEncoding(Encoding.BigEndianUnicode);

		[RubyConstant]
		public static readonly RubyEncoding UTF_16LE = RubyEncoding.GetRubyEncoding(Encoding.Unicode);

		[RubyConstant]
		public static readonly RubyEncoding UTF_32BE = RubyEncoding.GetRubyEncoding(12001);

		[RubyConstant]
		public static readonly RubyEncoding UTF_32LE = RubyEncoding.GetRubyEncoding(Encoding.UTF32);

		[RubyMethod("name")]
		[RubyMethod("to_s")]
		public static MutableString ToS(RubyEncoding self)
		{
			return MutableString.CreateAscii(self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, RubyEncoding self)
		{
			MutableString mutableString = MutableString.CreateMutable(context.GetIdentifierEncoding());
			mutableString.Append("#<");
			mutableString.Append(context.GetClassDisplayName(self));
			mutableString.Append(':');
			mutableString.Append(self.Name);
			mutableString.Append('>');
			return mutableString;
		}

		[RubyMethod("based_encoding")]
		public static RubyEncoding BasedEncoding(RubyEncoding self)
		{
			return null;
		}

		[RubyMethod("dummy?")]
		public static bool IsDummy(RubyEncoding self)
		{
			return false;
		}

		[RubyMethod("ascii_compatible?")]
		public static bool IsAsciiCompatible(RubyEncoding self)
		{
			return self.IsAsciiIdentity;
		}

		[RubyMethod("names")]
		public static RubyArray GetAllNames(RubyContext context, RubyEncoding self)
		{
			RubyArray rubyArray = new RubyArray();
			string name = self.Name;
			rubyArray.Add(MutableString.Create(name));
			foreach (KeyValuePair<string, string> alias in RubyEncoding.Aliases)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(alias.Value, name))
				{
					rubyArray.Add(MutableString.CreateAscii(alias.Key));
				}
			}
			if (self == context.RubyOptions.LocaleEncoding)
			{
				rubyArray.Add(MutableString.CreateAscii("locale"));
			}
			if (self == context.DefaultExternalEncoding)
			{
				rubyArray.Add(MutableString.CreateAscii("external"));
			}
			if (self == context.GetPathEncoding())
			{
				rubyArray.Add(MutableString.CreateAscii("filesystem"));
			}
			return rubyArray;
		}

		[RubyMethod("aliases", RubyMethodAttributes.PublicSingleton)]
		public static Hash GetAliases(RubyClass self)
		{
			RubyContext context = self.Context;
			Hash hash = new Hash(context.EqualityComparer, RubyEncoding.Aliases.Count + 3);
			foreach (KeyValuePair<string, string> alias in RubyEncoding.Aliases)
			{
				hash.Add(MutableString.CreateAscii(alias.Key).Freeze(), MutableString.CreateAscii(alias.Value).Freeze());
			}
			hash.Add(MutableString.CreateAscii("locale").Freeze(), MutableString.Create(context.RubyOptions.LocaleEncoding.Name).Freeze());
			hash.Add(MutableString.CreateAscii("external").Freeze(), MutableString.Create(context.DefaultExternalEncoding.Name).Freeze());
			hash.Add(MutableString.CreateAscii("filesystem").Freeze(), MutableString.Create(context.GetPathEncoding().Name).Freeze());
			return hash;
		}

		[RubyMethod("name_list", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetNameList(RubyClass self)
		{
			EncodingInfo[] encodings = Encoding.GetEncodings();
			RubyArray rubyArray = new RubyArray(1 + encodings.Length);
			rubyArray.Add(MutableString.CreateAscii(RubyEncoding.Binary.Name));
			EncodingInfo[] array = encodings;
			foreach (EncodingInfo encodingInfo in array)
			{
				rubyArray.Add(MutableString.Create(RubyEncoding.GetRubySpecificName(encodingInfo.CodePage) ?? encodingInfo.Name));
			}
			foreach (string key in RubyEncoding.Aliases.Keys)
			{
				rubyArray.Add(MutableString.CreateAscii(key));
			}
			rubyArray.Add(MutableString.CreateAscii("locale"));
			rubyArray.Add(MutableString.CreateAscii("external"));
			rubyArray.Add(MutableString.CreateAscii("filesystem"));
			return rubyArray;
		}

		[RubyMethod("list", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetAvailableEncodings(RubyClass self)
		{
			EncodingInfo[] encodings = Encoding.GetEncodings();
			RubyArray rubyArray = new RubyArray(1 + encodings.Length);
			rubyArray.Add(RubyEncoding.Binary);
			EncodingInfo[] array = encodings;
			foreach (EncodingInfo encodingInfo in array)
			{
				rubyArray.Add(RubyEncoding.GetRubyEncoding(encodingInfo.CodePage));
			}
			return rubyArray;
		}

		[RubyMethod("find", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetEncoding(RubyClass self, [NotNull][DefaultProtocol] MutableString name)
		{
			return self.Context.GetRubyEncoding(name);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] MutableString str1, [NotNull] MutableString str2)
		{
			return str1.GetCompatibleEncoding(str2);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] RubyEncoding encoding1, [NotNull] RubyEncoding encoding2)
		{
			return MutableString.GetCompatibleEncoding(encoding1, encoding2);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] RubyEncoding encoding, [NotNull] MutableString str)
		{
			return str.GetCompatibleEncoding(encoding);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] MutableString str, [NotNull] RubyEncoding encoding)
		{
			return str.GetCompatibleEncoding(encoding);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] RubyEncoding encoding, [NotNull] RubySymbol symbol)
		{
			return GetCompatible(self, encoding, symbol.String);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] MutableString str, [NotNull] RubySymbol symbol)
		{
			return GetCompatible(self, str, symbol.String);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] RubySymbol symbol, [NotNull] RubyEncoding encoding)
		{
			return GetCompatible(self, symbol.String, encoding);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] RubySymbol symbol, [NotNull] MutableString str)
		{
			return GetCompatible(self, symbol.String, str);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, [NotNull] RubySymbol encoding1, [NotNull] RubySymbol encoding2)
		{
			return GetCompatible(self, encoding1.String, encoding2.String);
		}

		[RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetCompatible(RubyClass self, object obj1, object obj2)
		{
			return null;
		}

		[RubyMethod("default_external", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetDefaultExternalEncoding(RubyClass self)
		{
			return self.Context.DefaultExternalEncoding;
		}

		[RubyMethod("default_external=", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding SetDefaultExternalEncoding(RubyClass self, RubyEncoding encoding)
		{
			if (encoding == null)
			{
				throw RubyExceptions.CreateArgumentError("default external can not be nil");
			}
			RubyEncoding defaultExternalEncoding = self.Context.DefaultExternalEncoding;
			self.Context.DefaultExternalEncoding = encoding;
			return defaultExternalEncoding;
		}

		[RubyMethod("default_external=", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding SetDefaultExternalEncoding(RubyClass self, [DefaultProtocol][NotNull] MutableString encodingName)
		{
			return SetDefaultExternalEncoding(self, self.Context.GetRubyEncoding(encodingName));
		}

		[RubyMethod("default_internal", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding GetDefaultInternalEncoding(RubyClass self)
		{
			return self.Context.DefaultInternalEncoding;
		}

		[RubyMethod("default_internal=", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding SetDefaultInternalEncoding(RubyClass self, RubyEncoding encoding)
		{
			RubyEncoding defaultInternalEncoding = self.Context.DefaultInternalEncoding;
			self.Context.DefaultInternalEncoding = encoding;
			return defaultInternalEncoding;
		}

		[RubyMethod("default_internal=", RubyMethodAttributes.PublicSingleton)]
		public static RubyEncoding SetDefaultInternalEncoding(RubyClass self, [DefaultProtocol][NotNull] MutableString encodingName)
		{
			return SetDefaultInternalEncoding(self, self.Context.GetRubyEncoding(encodingName));
		}

		[RubyMethod("locale_charmap", RubyMethodAttributes.PublicSingleton)]
		public static MutableString GetDefaultCharmap(RubyClass self)
		{
			return MutableString.Create(self.Context.RubyOptions.LocaleEncoding.Name);
		}
	}
}

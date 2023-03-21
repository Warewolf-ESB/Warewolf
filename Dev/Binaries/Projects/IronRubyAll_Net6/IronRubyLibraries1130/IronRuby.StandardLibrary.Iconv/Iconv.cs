using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Iconv
{
	[RubyClass("Iconv", Inherits = typeof(object))]
	public class Iconv
	{
		[RubyModule("Failure")]
		public static class Failure
		{
		}

		[Serializable]
		[Includes(new Type[] { typeof(Failure) })]
		[RubyException("BrokenLibrary")]
		public class BrokenLibrary : RuntimeError
		{
			public BrokenLibrary()
				: this(null, null)
			{
			}

			public BrokenLibrary(string message)
				: this(message, null)
			{
			}

			public BrokenLibrary(string message, Exception inner)
				: base(message ?? "BrokenLibrary", inner)
			{
			}

			protected BrokenLibrary(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}

			[RubyConstructor]
			public static BrokenLibrary Factory(RubyClass self, object arg1, object arg2, object arg3)
			{
				return new BrokenLibrary(GetMessage(arg2, arg3));
			}
		}

		[Serializable]
		[RubyException("InvalidEncoding")]
		[Includes(new Type[] { typeof(Failure) })]
		public class InvalidEncoding : ArgumentException
		{
			public InvalidEncoding()
				: this(null, null)
			{
			}

			public InvalidEncoding(string message)
				: this(message, null)
			{
			}

			public InvalidEncoding(string message, Exception inner)
				: base(message ?? "InvalidEncoding", inner)
			{
			}

			protected InvalidEncoding(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}

			[RubyConstructor]
			public static InvalidEncoding Factory(RubyClass self, object arg1, object arg2, object arg3)
			{
				return new InvalidEncoding(GetMessage(arg2, arg3));
			}
		}

		[Serializable]
		[Includes(new Type[] { typeof(Failure) })]
		[RubyException("InvalidCharacter")]
		public class InvalidCharacter : ArgumentException
		{
			public InvalidCharacter()
				: this(null, null)
			{
			}

			public InvalidCharacter(string message)
				: this(message, null)
			{
			}

			public InvalidCharacter(string message, Exception inner)
				: base(message ?? "InvalidCharacter", inner)
			{
			}

			protected InvalidCharacter(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}

			[RubyConstructor]
			public static InvalidCharacter Factory(RubyClass self, object arg1, object arg2, object arg3)
			{
				return new InvalidCharacter(GetMessage(arg2, arg3));
			}
		}

		[Serializable]
		[RubyException("IllegalSequence")]
		[Includes(new Type[] { typeof(Failure) })]
		public class IllegalSequence : ArgumentException
		{
			public IllegalSequence()
				: this(null, null)
			{
			}

			public IllegalSequence(string message)
				: this(message, null)
			{
			}

			public IllegalSequence(string message, Exception inner)
				: base(message ?? "IllegalSequence", inner)
			{
			}

			protected IllegalSequence(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}

			[RubyConstructor]
			public static IllegalSequence Factory(RubyClass self, object arg1, object arg2, object arg3)
			{
				return new IllegalSequence(GetMessage(arg2, arg3));
			}
		}

		[Serializable]
		[Includes(new Type[] { typeof(Failure) })]
		[RubyException("OutOfRange")]
		public class OutOfRange : RuntimeError
		{
			public OutOfRange()
				: this(null, null)
			{
			}

			public OutOfRange(string message)
				: this(message, null)
			{
			}

			public OutOfRange(string message, Exception inner)
				: base(message ?? "OutOfRange", inner)
			{
			}

			protected OutOfRange(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}

			[RubyConstructor]
			public static OutOfRange Factory(RubyClass self, object arg1, object arg2, object arg3)
			{
				return new OutOfRange(GetMessage(arg2, arg3));
			}
		}

		private Decoder _fromEncoding;

		private Encoder _toEncoding;

		private string _toEncodingString;

		private bool _emitBom;

		private bool _isClosed;

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static Iconv Create(RubyClass self, [NotNull][DefaultProtocol] MutableString toEncoding, [NotNull][DefaultProtocol] MutableString fromEncoding)
		{
			Iconv self2 = new Iconv();
			return Initialize(self.Context, self2, toEncoding, fromEncoding);
		}

		private void ResetByteOrderMark()
		{
			if (_toEncodingString == "UTF-16")
			{
				_emitBom = true;
			}
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Iconv Initialize(RubyContext context, Iconv self, [DefaultProtocol][NotNull] MutableString toEncoding, [DefaultProtocol][NotNull] MutableString fromEncoding)
		{
			self._toEncodingString = toEncoding.ConvertToString().ToUpperInvariant();
			try
			{
				self._toEncoding = context.GetEncodingByRubyName(self._toEncodingString).GetEncoder();
			}
			catch (ArgumentException inner)
			{
				throw new InvalidEncoding(self._toEncodingString, inner);
			}
			try
			{
				self._fromEncoding = context.GetEncodingByRubyName(fromEncoding.ConvertToString()).GetDecoder();
			}
			catch (ArgumentException inner2)
			{
				throw new InvalidEncoding(fromEncoding.ConvertToString(), inner2);
			}
			self.ResetByteOrderMark();
			return self;
		}

		[RubyMethod("iconv")]
		public static MutableString iconv(Iconv self, [DefaultProtocol] MutableString str, [DefaultProtocol] int startIndex, object length)
		{
			if (length == null)
			{
				return iconv(self, str, startIndex, -1);
			}
			throw new ArgumentException();
		}

		[RubyMethod("iconv")]
		public static MutableString iconv(Iconv self, [DefaultProtocol] MutableString str, [DefaultProtocol] int startIndex, [DefaultProtocol][NotNull] int length)
		{
			if (self._isClosed)
			{
				throw RubyExceptions.CreateArgumentError("closed stream");
			}
			if (str == null)
			{
				return self.Close(true);
			}
			byte[] array = str.ConvertToBytes();
			if (startIndex < 0)
			{
				startIndex = array.Length + startIndex;
				if (startIndex < 0)
				{
					startIndex = 0;
					length = 0;
				}
			}
			else if (startIndex > array.Length)
			{
				startIndex = 0;
				length = 0;
			}
			if (length < 0 || startIndex + length > array.Length)
			{
				length = array.Length - startIndex;
			}
			char[] array2 = new char[self._fromEncoding.GetCharCount(array, startIndex, length)];
			int bytesUsed;
			int charsUsed;
			bool completed;
			self._fromEncoding.Convert(array, startIndex, length, array2, 0, array2.Length, false, out bytesUsed, out charsUsed, out completed);
			byte[] array3 = new byte[self._toEncoding.GetByteCount(array2, 0, array2.Length, false)];
			self._toEncoding.GetBytes(array2, 0, array2.Length, array3, 0, false);
			if (self._emitBom && array3.Length > 0)
			{
				byte[] array4 = new byte[2 + array3.Length];
				array4[0] = byte.MaxValue;
				array4[1] = 254;
				Array.Copy(array3, 0, array4, 2, array3.Length);
				array3 = array4;
				self._emitBom = false;
			}
			return MutableString.CreateBinary(array3);
		}

		private MutableString Close(bool resetEncoder)
		{
			char[] chars = new char[0];
			byte[] bytes = new byte[_toEncoding.GetByteCount(chars, 0, 0, true)];
			_toEncoding.GetBytes(chars, 0, 0, bytes, 0, true);
			if (resetEncoder)
			{
				_toEncoding.Reset();
				ResetByteOrderMark();
			}
			else
			{
				_isClosed = true;
			}
			return MutableString.CreateBinary(bytes);
		}

		[RubyMethod("close")]
		public static MutableString Close(Iconv self)
		{
			if (!self._isClosed)
			{
				return self.Close(false);
			}
			return null;
		}

		private static MutableString[] Convert(RubyClass self, MutableString toEncoding, MutableString fromEncoding, MutableString[] strings)
		{
			Iconv iconv = Create(self, toEncoding, fromEncoding);
			MutableString[] array = new MutableString[strings.Length];
			for (int i = 0; i < strings.Length; i++)
			{
				array[i] = Iconv.iconv(iconv, strings[i], 0, -1);
			}
			MutableString mutableString = iconv.Close(false);
			if (mutableString.IsEmpty)
			{
				return array;
			}
			Array.Resize(ref array, strings.Length + 1);
			array[strings.Length] = mutableString;
			return array;
		}

		[RubyMethod("conv", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Convert(RubyClass self, [NotNull][DefaultProtocol] MutableString toEncoding, [NotNull][DefaultProtocol] MutableString fromEncoding, [DefaultProtocol] MutableString str)
		{
			MutableString[] array = Convert(self, toEncoding, fromEncoding, new MutableString[2] { str, null });
			MutableString mutableString = MutableString.CreateEmpty();
			MutableString[] array2 = array;
			foreach (MutableString value in array2)
			{
				mutableString.Append(value);
			}
			return mutableString;
		}

		[RubyMethod("charset_map", RubyMethodAttributes.PublicSingleton)]
		public static Hash CharsetMap(RubyClass self)
		{
			return new Hash(self.Context);
		}

		[RubyMethod("iconv", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray iconv(RubyClass self, [NotNull][DefaultProtocol] MutableString toEncoding, [NotNull][DefaultProtocol] MutableString fromEncoding, params MutableString[] strings)
		{
			MutableString[] items = Convert(self, toEncoding, fromEncoding, strings);
			return new RubyArray(items);
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static object Open([NotNull] BlockParam block, RubyClass self, [DefaultProtocol][NotNull] MutableString toEncoding, [DefaultProtocol][NotNull] MutableString fromEncoding)
		{
			Iconv iconv = Create(self, toEncoding, fromEncoding);
			if (block == null)
			{
				return iconv;
			}
			try
			{
				object blockResult;
				block.Yield(iconv, out blockResult);
				return blockResult;
			}
			finally
			{
				Close(iconv);
			}
		}

		private static string GetMessage(object arg2, object arg3)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}", new object[2]
			{
				KernelOps.ToS(arg2),
				KernelOps.ToS(arg3)
			});
		}
	}
}

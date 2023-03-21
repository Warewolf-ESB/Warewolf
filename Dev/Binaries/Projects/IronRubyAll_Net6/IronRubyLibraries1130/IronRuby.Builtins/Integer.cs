using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("Integer")]
	[Includes(new Type[] { typeof(Precision) })]
	public class Integer : Numeric
	{
		public Integer(RubyClass cls)
			: base(cls)
		{
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static object InducedFrom(RubyClass self, int obj)
		{
			return obj;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static object InducedFrom(RubyClass self, [NotNull] BigInteger obj)
		{
			return obj;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static object InducedFrom(UnaryOpStorage toiStorage, RubyClass self, double obj)
		{
			CallSite<Func<CallSite, object, object>> callSite = toiStorage.GetCallSite("to_i");
			return callSite.Target(callSite, obj);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static int InducedFrom(RubyClass self, object obj)
		{
			throw RubyExceptions.CreateTypeError("failed to convert {0} into Integer", self.Context.GetClassDisplayName(obj));
		}

		[RubyMethod("floor")]
		[RubyMethod("round")]
		[RubyMethod("truncate")]
		[RubyMethod("to_i")]
		[RubyMethod("to_int")]
		[RubyMethod("ceil")]
		public static object ToInteger(object self)
		{
			return self;
		}

		[RubyMethod("ord")]
		[RubyMethod("numerator")]
		public static object Numerator(object self)
		{
			return self;
		}

		[RubyMethod("denominator")]
		public static object Denominator(object self)
		{
			return ClrInteger.One;
		}

		[RubyMethod("rationalize")]
		[RubyMethod("to_r")]
		public static object ToRational(CallSiteStorage<Func<CallSite, object, object, object, object>> toRational, RubyScope scope, object self)
		{
			return KernelOps.ToRational(toRational, scope, self, self, ClrInteger.One);
		}

		[RubyMethod("chr")]
		public static MutableString ToChr(ConversionStorage<MutableString> toStr, [DefaultProtocol] int self, [Optional] object encoding)
		{
			RubyEncoding resultEncoding;
			RubyEncoding rubyEncoding;
			if (encoding != Missing.Value)
			{
				resultEncoding = (rubyEncoding = Protocols.ConvertToEncoding(toStr, encoding));
			}
			else
			{
				rubyEncoding = toStr.Context.DefaultInternalEncoding ?? RubyEncoding.Ascii;
				resultEncoding = ((self <= 127) ? RubyEncoding.Ascii : ((self <= 255) ? RubyEncoding.Binary : rubyEncoding));
			}
			return ToChr(rubyEncoding, resultEncoding, self);
		}

		internal static MutableString ToChr(RubyEncoding encoding, RubyEncoding resultEncoding, int codepoint)
		{
			if (codepoint < 0)
			{
				throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
			}
			switch (encoding.CodePage)
			{
			case 1200:
			case 1201:
			case 12000:
			case 12001:
			case 65000:
			case 65001:
				if (codepoint > 1114111)
				{
					throw RubyExceptions.CreateRangeError("{0} is not a valid Unicode code point (0..0x10ffff)", codepoint);
				}
				return MutableString.CreateMutable(Tokenizer.UnicodeCodePointToString(codepoint), resultEncoding);
			case 932:
				if ((codepoint >= 129 && codepoint <= 159) || (codepoint >= 224 && codepoint <= 252))
				{
					throw RubyExceptions.CreateArgumentError("invalid codepoint 0x{0:x2} in Shift_JIS", codepoint);
				}
				break;
			case 51932:
				if (codepoint >= 128)
				{
					throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
				}
				break;
			}
			if (codepoint <= 255)
			{
				return MutableString.CreateBinary(new byte[1] { (byte)codepoint }, resultEncoding);
			}
			if (encoding.IsDoubleByteCharacterSet)
			{
				if (codepoint <= 65535)
				{
					return MutableString.CreateBinary(new byte[2]
					{
						(byte)(codepoint >> 8),
						(byte)((uint)codepoint & 0xFFu)
					}, resultEncoding);
				}
				throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
			}
			if (encoding.IsSingleByteCharacterSet)
			{
				throw RubyExceptions.CreateRangeError("{0} out of char range", codepoint);
			}
			throw new NotSupportedException(RubyExceptions.FormatMessage("Encoding {0} code points not supported", encoding));
		}

		[RubyMethod("integer?")]
		public new static bool IsInteger(object self)
		{
			return true;
		}

		[RubyMethod("odd?")]
		public static bool IsOdd(int self)
		{
			return (self & 1) != 0;
		}

		[RubyMethod("odd?")]
		public static bool IsOdd(BigInteger self)
		{
			return !self.IsEven;
		}

		[RubyMethod("even?")]
		public static bool IsEven(int self)
		{
			return (self & 1) == 0;
		}

		[RubyMethod("even?")]
		public static bool IsEven(BigInteger self)
		{
			return self.IsEven;
		}

		[RubyMethod("next")]
		[RubyMethod("succ")]
		public static object Next(int self)
		{
			return ClrInteger.Add(self, 1);
		}

		[RubyMethod("succ")]
		[RubyMethod("next")]
		public static object Next(BinaryOpStorage addStorage, object self)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = addStorage.GetCallSite("+");
			return callSite.Target(callSite, self, ClrInteger.One);
		}

		[RubyMethod("pred")]
		public static object Pred(int self)
		{
			return ClrInteger.Subtract(self, 1);
		}

		[RubyMethod("pred")]
		public static object Pred(BinaryOpStorage subStorage, object self)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = subStorage.GetCallSite("-");
			return callSite.Target(callSite, self, ClrInteger.One);
		}

		[RubyMethod("times")]
		public static object Times(BlockParam block, int self)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => TimesImpl(innerBlock, self));
			}
			return TimesImpl(block, self);
		}

		private static object TimesImpl(BlockParam block, int self)
		{
			for (int i = 0; i < self; i++)
			{
				object blockResult;
				if (block.Yield(i, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("times")]
		public static object Times(BinaryOpStorage lessThanStorage, BinaryOpStorage addStorage, BlockParam block, object self)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => TimesImpl(lessThanStorage, addStorage, innerBlock, self));
			}
			return TimesImpl(lessThanStorage, addStorage, block, self);
		}

		public static object TimesImpl(BinaryOpStorage lessThanStorage, BinaryOpStorage addStorage, BlockParam block, object self)
		{
			object obj = 0;
			CallSite<Func<CallSite, object, object, object>> callSite = lessThanStorage.GetCallSite("<");
			while (RubyOps.IsTrue(callSite.Target(callSite, obj, self)))
			{
				object blockResult;
				if (block.Yield(obj, out blockResult))
				{
					return blockResult;
				}
				CallSite<Func<CallSite, object, object, object>> callSite2 = addStorage.GetCallSite("+");
				obj = callSite2.Target(callSite2, obj, 1);
			}
			return self;
		}

		[RubyMethod("upto")]
		public static object UpTo(BlockParam block, int self, int other)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => UpToImpl(innerBlock, self, other));
			}
			return UpToImpl(block, self, other);
		}

		private static object UpToImpl(BlockParam block, int self, int other)
		{
			for (int i = self; i <= other; i++)
			{
				object blockResult;
				if (block.Yield(i, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("upto")]
		public static object UpTo(BinaryOpStorage greaterThanStorage, BinaryOpStorage addStorage, BlockParam block, object self, object other)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => UpToImpl(greaterThanStorage, addStorage, innerBlock, self, other));
			}
			return UpToImpl(greaterThanStorage, addStorage, block, self, other);
		}

		private static object UpToImpl(BinaryOpStorage greaterThanStorage, BinaryOpStorage addStorage, BlockParam block, object self, object other)
		{
			object obj = self;
			object obj2 = null;
			CallSite<Func<CallSite, object, object, object>> callSite = greaterThanStorage.GetCallSite(">");
			while (RubyOps.IsFalse(obj2))
			{
				obj2 = callSite.Target(callSite, obj, other);
				if (obj2 == null)
				{
					throw RubyExceptions.MakeComparisonError(greaterThanStorage.Context, obj, other);
				}
				if (RubyOps.IsFalse(obj2))
				{
					object blockResult;
					if (block.Yield(obj, out blockResult))
					{
						return blockResult;
					}
					CallSite<Func<CallSite, object, object, object>> callSite2 = addStorage.GetCallSite("+");
					obj = callSite2.Target(callSite2, obj, 1);
				}
			}
			return self;
		}

		[RubyMethod("downto")]
		public static object DownTo(BlockParam block, int self, int other)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => DownToImpl(innerBlock, self, other));
			}
			return DownToImpl(block, self, other);
		}

		private static object DownToImpl(BlockParam block, int self, int other)
		{
			for (int num = self; num >= other; num--)
			{
				object blockResult;
				if (block.Yield(num, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("downto")]
		public static object DownTo(BinaryOpStorage lessThanStorage, BinaryOpStorage subtractStorage, BlockParam block, object self, object other)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => DownToImpl(lessThanStorage, subtractStorage, innerBlock, self, other));
			}
			return DownToImpl(lessThanStorage, subtractStorage, block, self, other);
		}

		private static object DownToImpl(BinaryOpStorage lessThanStorage, BinaryOpStorage subtractStorage, BlockParam block, object self, object other)
		{
			object obj = self;
			object obj2 = null;
			CallSite<Func<CallSite, object, object, object>> callSite = lessThanStorage.GetCallSite("<");
			while (RubyOps.IsFalse(obj2))
			{
				obj2 = callSite.Target(callSite, obj, other);
				if (obj2 == null)
				{
					throw RubyExceptions.MakeComparisonError(lessThanStorage.Context, obj, other);
				}
				if (RubyOps.IsFalse(obj2))
				{
					object blockResult;
					if (block.Yield(obj, out blockResult))
					{
						return blockResult;
					}
					CallSite<Func<CallSite, object, object, object>> callSite2 = subtractStorage.GetCallSite("-");
					obj = callSite2.Target(callSite2, obj, 1);
				}
			}
			return self;
		}

		private static int SignedGcd(int a, int b)
		{
			if (b == -1)
			{
				return -1;
			}
			while (b != 0)
			{
				int num = b;
				b = a % b;
				a = num;
			}
			return a;
		}

		private static BigInteger SignedGcd(BigInteger a, BigInteger b)
		{
			while (!b.IsZero())
			{
				BigInteger bigInteger = b;
				b = a % b;
				a = bigInteger;
			}
			return a;
		}

		private static object Lcm(int self, int other, int gcd)
		{
			if (gcd != 0)
			{
				return Protocols.Normalize(Math.Abs((long)self / (long)gcd * other));
			}
			return ClrInteger.Zero;
		}

		private static object Lcm(BigInteger self, BigInteger other, BigInteger gcd)
		{
			if (!(gcd == 0))
			{
				return Protocols.Normalize((self / gcd * other).Abs());
			}
			return ClrInteger.Zero;
		}

		[RubyMethod("gcd")]
		public static object Gcd(int self, int other)
		{
			return ClrInteger.Abs(SignedGcd(self, other));
		}

		[RubyMethod("gcd")]
		public static object Gcd(BigInteger self, BigInteger other)
		{
			return ClrBigInteger.Abs(SignedGcd(self, other));
		}

		[RubyMethod("gcd")]
		public static object Gcd(object self, object other)
		{
			throw RubyExceptions.CreateTypeError("not an integer");
		}

		[RubyMethod("lcm")]
		public static object Lcm(int self, int other)
		{
			return Lcm(self, other, SignedGcd(self, other));
		}

		[RubyMethod("lcm")]
		public static object Lcm(BigInteger self, BigInteger other)
		{
			return Lcm(self, other, SignedGcd(self, other));
		}

		[RubyMethod("lcm")]
		public static object Lcm(object self, object other)
		{
			throw RubyExceptions.CreateTypeError("not an integer");
		}

		[RubyMethod("gcdlcm")]
		public static RubyArray GcdLcm(int self, int other)
		{
			int num = SignedGcd(self, other);
			RubyArray rubyArray = new RubyArray();
			rubyArray.Add(ClrInteger.Abs(num));
			rubyArray.Add(Lcm(self, other, num));
			return rubyArray;
		}

		[RubyMethod("gcdlcm")]
		public static RubyArray GcdLcm(BigInteger self, BigInteger other)
		{
			BigInteger bigInteger = SignedGcd(self, other);
			RubyArray rubyArray = new RubyArray();
			rubyArray.Add(ClrBigInteger.Abs(bigInteger));
			rubyArray.Add(Lcm(self, other, bigInteger));
			return rubyArray;
		}

		[RubyMethod("gcdlcm")]
		public static RubyArray GcdLcm(object self, object other)
		{
			throw RubyExceptions.CreateTypeError("not an integer");
		}

		public static object TryUnaryMinus(object obj)
		{
			if (obj is int)
			{
				int num = (int)obj;
				if (num == int.MinValue)
				{
					return -BigInteger.Create(num);
				}
				return ScriptingRuntimeHelpers.Int32ToObject(-num);
			}
			BigInteger bigInteger = obj as BigInteger;
			if ((object)bigInteger != null)
			{
				return -bigInteger;
			}
			return null;
		}
	}
}

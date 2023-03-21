using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrInteger) }, Copy = true)]
	[RubyClass(Extends = typeof(byte), Inherits = typeof(Integer), Restrictions = ModuleRestrictions.None)]
	public static class ByteOps
	{
		[RubyMethod("size")]
		public static int Size(byte self)
		{
			return 1;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static byte InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			if (value >= 0 && value <= 255)
			{
				return (byte)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static byte InducedFrom(RubyClass self, [NotNull] BigInteger value)
		{
			if (value >= (byte)0 && value <= byte.MaxValue)
			{
				return (byte)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static byte InducedFrom(RubyClass self, double value)
		{
			if (value >= 0.0 && value <= 255.0)
			{
				return (byte)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (Byte)");
		}

		[RubyMethod("succ")]
		[RubyMethod("next")]
		public static object Next(byte self)
		{
			if (self == byte.MaxValue)
			{
				return self + 1;
			}
			return (byte)(self + 1);
		}
	}
}

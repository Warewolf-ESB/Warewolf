using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass(Extends = typeof(short), Inherits = typeof(Integer), Restrictions = ModuleRestrictions.None)]
	[Includes(new Type[] { typeof(ClrInteger) }, Copy = true)]
	public static class Int16Ops
	{
		[RubyMethod("size")]
		public static int Size(short self)
		{
			return 2;
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static short InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			if (value >= -32768 && value <= 32767)
			{
				return (short)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static short InducedFrom(RubyClass self, [NotNull] BigInteger value)
		{
			if (value >= short.MinValue && value <= short.MaxValue)
			{
				return (short)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static short InducedFrom(RubyClass self, double value)
		{
			if (value >= -32768.0 && value <= 32767.0)
			{
				return (short)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (Int16)");
		}

		[RubyMethod("next")]
		[RubyMethod("succ")]
		public static object Next(short self)
		{
			if (self == short.MaxValue)
			{
				return self + 1;
			}
			return (short)(self + 1);
		}
	}
}

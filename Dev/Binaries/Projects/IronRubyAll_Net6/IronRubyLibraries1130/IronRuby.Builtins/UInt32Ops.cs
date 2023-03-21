using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrBigInteger) }, Copy = true)]
	[RubyClass(Extends = typeof(uint), Inherits = typeof(Integer), Restrictions = ModuleRestrictions.None)]
	public static class UInt32Ops
	{
		[RubyMethod("size")]
		public static int Size(uint self)
		{
			return 4;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static uint InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			if (value >= 0)
			{
				return (uint)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static uint InducedFrom(RubyClass self, [NotNull] BigInteger value)
		{
			if (value >= 0u && value <= uint.MaxValue)
			{
				return (uint)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static uint InducedFrom(RubyClass self, double value)
		{
			if (value >= 0.0 && value <= 4294967295.0)
			{
				return (uint)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (UInt32)");
		}

		[RubyMethod("succ")]
		[RubyMethod("next")]
		public static object Next(uint self)
		{
			if (self == uint.MaxValue)
			{
				return (BigInteger)self + (BigInteger)1;
			}
			return self + 1;
		}
	}
}

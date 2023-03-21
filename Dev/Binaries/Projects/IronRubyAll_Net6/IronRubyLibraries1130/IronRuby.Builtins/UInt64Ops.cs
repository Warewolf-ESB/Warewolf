using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrBigInteger) }, Copy = true)]
	[RubyClass(Extends = typeof(ulong), Inherits = typeof(Integer), Restrictions = ModuleRestrictions.None)]
	public static class UInt64Ops
	{
		[RubyMethod("size")]
		public static int Size(ulong self)
		{
			return 8;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static ulong InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			if (value >= 0)
			{
				return (ulong)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static ulong InducedFrom(RubyClass self, [NotNull] BigInteger value)
		{
			if (value >= 0uL && value <= ulong.MaxValue)
			{
				return (ulong)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static ulong InducedFrom(RubyClass self, double value)
		{
			if (value >= 0.0 && value <= 1.8446744073709552E+19)
			{
				return (ulong)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (UInt64)");
		}

		[RubyMethod("succ")]
		[RubyMethod("next")]
		public static object Next(ulong self)
		{
			if (self == ulong.MaxValue)
			{
				return (BigInteger)self + (BigInteger)1;
			}
			return self + 1;
		}
	}
}

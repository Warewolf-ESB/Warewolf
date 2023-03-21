using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrBigInteger) }, Copy = true)]
	[RubyClass(Extends = typeof(long), Inherits = typeof(Integer), Restrictions = ModuleRestrictions.None)]
	public static class Int64Ops
	{
		[RubyMethod("size")]
		public static int Size(long self)
		{
			return 8;
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static long InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			return value;
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static long InducedFrom(RubyClass self, [NotNull] BigInteger value)
		{
			if (value >= long.MinValue && value <= long.MaxValue)
			{
				return (long)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static long InducedFrom(RubyClass self, double value)
		{
			if (value >= -9.2233720368547758E+18 && value <= 9.2233720368547758E+18)
			{
				return (long)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (Int64)");
		}

		[RubyMethod("succ")]
		[RubyMethod("next")]
		public static object Next(long self)
		{
			if (self == long.MaxValue)
			{
				return (BigInteger)self + (BigInteger)1;
			}
			return self + 1;
		}
	}
}

using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrInteger) }, Copy = true)]
	[RubyClass(Extends = typeof(ushort), Inherits = typeof(Integer), Restrictions = ModuleRestrictions.None)]
	public static class UInt16Ops
	{
		[RubyMethod("size")]
		public static int Size(ushort self)
		{
			return 2;
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static ushort InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			if (value >= 0 && value <= 65535)
			{
				return (ushort)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static ushort InducedFrom(RubyClass self, [NotNull] BigInteger value)
		{
			if (value >= (ushort)0 && value <= ushort.MaxValue)
			{
				return (ushort)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static ushort InducedFrom(RubyClass self, double value)
		{
			if (value >= 0.0 && value <= 65535.0)
			{
				return (ushort)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (UInt16)");
		}

		[RubyMethod("succ")]
		[RubyMethod("next")]
		public static object Next(ushort self)
		{
			if (self == ushort.MaxValue)
			{
				return self + 1;
			}
			return (ushort)(self + 1);
		}
	}
}

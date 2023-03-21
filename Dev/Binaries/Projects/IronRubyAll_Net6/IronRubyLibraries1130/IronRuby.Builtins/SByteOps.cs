using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(ClrInteger) }, Copy = true)]
	[RubyClass(Extends = typeof(sbyte), Inherits = typeof(Integer), Restrictions = ModuleRestrictions.None)]
	public static class SByteOps
	{
		[RubyMethod("size")]
		public static int Size(sbyte self)
		{
			return 1;
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static sbyte InducedFrom(RubyClass self, [DefaultProtocol] int value)
		{
			if (value >= -128 && value <= 127)
			{
				return (sbyte)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyConstructor]
		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		public static sbyte InducedFrom(RubyClass self, [NotNull] BigInteger value)
		{
			if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
			{
				return (sbyte)value;
			}
			throw RubyExceptions.CreateRangeError("Integer {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
		[RubyConstructor]
		public static sbyte InducedFrom(RubyClass self, double value)
		{
			if (value >= -128.0 && value <= 127.0)
			{
				return (sbyte)value;
			}
			throw RubyExceptions.CreateRangeError("Float {0} out of range of {1}", value, self.Name);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (SByte)");
		}

		[RubyMethod("next")]
		[RubyMethod("succ")]
		public static object Next(sbyte self)
		{
			if (self == sbyte.MaxValue)
			{
				return self + 1;
			}
			return (sbyte)(self + 1);
		}
	}
}

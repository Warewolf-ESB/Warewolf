using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule("FlagEnumeration", DefineIn = typeof(IronRubyOps.Clr), Extends = typeof(FlagEnumeration), Restrictions = ModuleRestrictions.NoUnderlyingType)]
	public static class FlagEnumerationOps
	{
		[RubyMethod("|")]
		public static object BitwiseOr(RubyContext context, object self, [NotNull] object other)
		{
			object obj = EnumUtils.BitwiseOr(self, other);
			if (obj != null)
			{
				return obj;
			}
			throw RubyExceptions.CreateUnexpectedTypeError(context, other, context.GetClassDisplayName(self));
		}

		[RubyMethod("&")]
		public static object BitwiseAnd(RubyContext context, object self, [NotNull] object other)
		{
			object obj = EnumUtils.BitwiseAnd(self, other);
			if (obj != null)
			{
				return obj;
			}
			throw RubyExceptions.CreateUnexpectedTypeError(context, other, context.GetClassDisplayName(self));
		}

		[RubyMethod("^")]
		public static object Xor(RubyContext context, object self, [NotNull] object other)
		{
			object obj = EnumUtils.ExclusiveOr(self, other);
			if (obj != null)
			{
				return obj;
			}
			throw RubyExceptions.CreateUnexpectedTypeError(context, other, context.GetClassDisplayName(self));
		}

		[RubyMethod("~")]
		public static object OnesComplement(RubyContext context, object self)
		{
			return EnumUtils.OnesComplement(self);
		}
	}
}

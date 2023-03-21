using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyClass("NilClass", Extends = typeof(DynamicNull))]
	public static class NilClassOps
	{
		[RubyMethod("GetType")]
		public static Type GetClrType(object self)
		{
			return typeof(DynamicNull);
		}

		[RubyMethod("ToString")]
		public static string ToClrString(object self)
		{
			return "nil";
		}

		[RubyMethod("GetHashCode")]
		public static int GetClrHashCode(object self)
		{
			return 0;
		}

		[RubyMethod("&")]
		public static bool And(object self, object obj)
		{
			return false;
		}

		[RubyMethod("^")]
		public static bool Xor(object self, object obj)
		{
			return obj != null;
		}

		[RubyMethod("^")]
		public static bool Xor(object self, bool obj)
		{
			return obj;
		}

		[RubyMethod("|")]
		public static bool Or(object self, object obj)
		{
			return obj != null;
		}

		[RubyMethod("|")]
		public static bool Or(object self, bool obj)
		{
			return obj;
		}

		[RubyMethod("nil?")]
		public static bool IsNil(object self)
		{
			return true;
		}

		[RubyMethod("to_a")]
		public static RubyArray ToArray(object self)
		{
			return new RubyArray();
		}

		[RubyMethod("to_f")]
		public static double ToDouble(object self)
		{
			return 0.0;
		}

		[RubyMethod("to_i")]
		public static int ToInteger(object self)
		{
			return 0;
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(object self)
		{
			return MutableString.CreateAscii("nil");
		}

		[RubyMethod("to_s")]
		public static MutableString ToString(object self)
		{
			return MutableString.CreateEmpty();
		}

        [SpecialName]
        public static bool op_Implicit(DynamicNull self)
        {
            Debug.Assert(self == null);
            return false;
        }
    }
}

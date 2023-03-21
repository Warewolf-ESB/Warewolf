using System;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("Comparable")]
	public static class Comparable
	{
		[RubyMethod("<")]
		public static bool Less(ComparisonStorage comparisonStorage, object self, object other)
		{
			return Compare(comparisonStorage, self, other).GetValueOrDefault(0) < 0;
		}

		[RubyMethod("<=")]
		public static bool LessOrEqual(ComparisonStorage comparisonStorage, object self, object other)
		{
			return Compare(comparisonStorage, self, other).GetValueOrDefault(1) <= 0;
		}

		[RubyMethod(">=")]
		public static bool GreaterOrEqual(ComparisonStorage comparisonStorage, object self, object other)
		{
			return Compare(comparisonStorage, self, other).GetValueOrDefault(-1) >= 0;
		}

		[RubyMethod(">")]
		public static bool Greater(ComparisonStorage comparisonStorage, object self, object other)
		{
			return Compare(comparisonStorage, self, other).GetValueOrDefault(0) > 0;
		}

		private static int? Compare(ComparisonStorage comparisonStorage, object lhs, object rhs)
		{
			CallSite<Func<CallSite, object, object, object>> compareSite = comparisonStorage.CompareSite;
			object obj = compareSite.Target(compareSite, lhs, rhs);
			if (obj != null)
			{
				return Protocols.ConvertCompareResult(comparisonStorage, obj);
			}
			throw RubyExceptions.MakeComparisonError(comparisonStorage.Context, lhs, rhs);
		}

		[RubyMethod("between?")]
		public static bool Between(ComparisonStorage comparisonStorage, object self, object min, object max)
		{
			if (!Less(comparisonStorage, self, min))
			{
				return !Greater(comparisonStorage, self, max);
			}
			return false;
		}

		[RubyMethod("==")]
		public static bool Equal(BinaryOpStorage compareStorage, object self, object other)
		{
			if (self == other)
			{
				return true;
			}
			CallSite<Func<CallSite, object, object, object>> callSite = compareStorage.GetCallSite("<=>");
			object obj;
			try
			{
				obj = callSite.Target(callSite, self, other);
			}
			catch (SystemException)
			{
				return false;
			}
			if (obj is int)
			{
				return (int)obj == 0;
			}
			return false;
		}
	}
}

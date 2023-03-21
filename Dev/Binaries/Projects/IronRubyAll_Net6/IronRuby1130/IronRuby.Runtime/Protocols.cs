using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Math.Extensions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public static class Protocols
	{
		public static object Normalize(BigInteger x)
		{
			int ret;
			if (x.AsInt32(out ret))
			{
				return ScriptingRuntimeHelpers.Int32ToObject(ret);
			}
			return x;
		}

		public static object Normalize(long x)
		{
			if (x >= int.MinValue && x <= int.MaxValue)
			{
				return ScriptingRuntimeHelpers.Int32ToObject((int)x);
			}
			return BigInteger.Create(x);
		}

		[CLSCompliant(false)]
		public static object Normalize(ulong x)
		{
			if (x <= int.MaxValue)
			{
				return ScriptingRuntimeHelpers.Int32ToObject((int)x);
			}
			return BigInteger.Create(x);
		}

		[CLSCompliant(false)]
		public static object Normalize(uint x)
		{
			if (x <= int.MaxValue)
			{
				return ScriptingRuntimeHelpers.Int32ToObject((int)x);
			}
			return BigInteger.Create(x);
		}

		public static object Normalize(decimal x)
		{
			if (x >= -2147483648m && x <= 2147483647m)
			{
				return ScriptingRuntimeHelpers.Int32ToObject(decimal.ToInt32(x));
			}
			return BigInteger.Create(x);
		}

		public static object Normalize(object x)
		{
			int ret;
			if (x is BigInteger && ((BigInteger)x).AsInt32(out ret))
			{
				return ScriptingRuntimeHelpers.Int32ToObject(ret);
			}
			return x;
		}

		public static double ConvertToDouble(RubyContext context, BigInteger bignum)
		{
			double result;
			//if (bignum.TryToFloat64(out result))
			if (BigIntegerExtensions.TryToFloat64(bignum, out result))
			{
				return result;
			}
			context.ReportWarning("Bignum out of Float range");
			if (bignum.Sign <= 0)
			{
				return double.NegativeInfinity;
			}
			return double.PositiveInfinity;
		}

		public static MutableString CastToString(ConversionStorage<MutableString> stringCast, object obj)
		{
			return CastToString(stringCast.GetSite(ProtocolConversionAction<ConvertToStrAction>.Make(stringCast.Context)), obj);
		}

		public static MutableString CastToString(CallSite<Func<CallSite, object, MutableString>> toStrSite, object obj)
		{
			MutableString mutableString = toStrSite.Target(toStrSite, obj);
			if (mutableString == null)
			{
				throw RubyExceptions.CreateTypeConversionError("nil", "String");
			}
			return mutableString;
		}

		public static MutableString CastToPath(ConversionStorage<MutableString> toPath, object obj)
		{
			return CastToPath(toPath.GetSite(CompositeConversionAction.Make(toPath.Context, CompositeConversion.ToPathToStr)), obj);
		}

		public static MutableString CastToPath(CallSite<Func<CallSite, object, MutableString>> toPath, object obj)
		{
			MutableString mutableString = toPath.Target(toPath, obj);
			if (mutableString == null)
			{
				throw RubyExceptions.CreateTypeConversionError("nil", "String");
			}
			return mutableString;
		}

		public static MutableString TryCastToString(ConversionStorage<MutableString> stringTryCast, object obj)
		{
			CallSite<Func<CallSite, object, MutableString>> site = stringTryCast.GetSite(ProtocolConversionAction<TryConvertToStrAction>.Make(stringTryCast.Context));
			return site.Target(site, obj);
		}

		public static MutableString ConvertToString(ConversionStorage<MutableString> tosConversion, object obj)
		{
			CallSite<Func<CallSite, object, MutableString>> site = tosConversion.GetSite(ConvertToSAction.Make(tosConversion.Context));
			return site.Target(site, obj);
		}

		internal static string ToClrStringNoThrow(RubyContext context, object obj)
		{
			try
			{
				MutableString mutableString = obj as MutableString;
				if (mutableString == null)
				{
					CallSite<Func<CallSite, object, MutableString>> stringConversionSite = context.StringConversionSite;
					mutableString = stringConversionSite.Target(stringConversionSite, obj);
				}
				return mutableString.ToStringWithEscapedInvalidCharacters(RubyEncoding.UTF8);
			}
			catch (Exception ex)
			{
				return string.Format(CultureInfo.CurrentCulture, "<{0}.to_s raised an exception: '{1}'>", new object[2] { obj, ex.Message });
			}
		}

		public static RubyEncoding ConvertToEncoding(ConversionStorage<MutableString> toStr, object obj)
		{
			return (obj as RubyEncoding) ?? toStr.Context.GetRubyEncoding(CastToString(toStr, obj));
		}

		public static IList CastToArray(ConversionStorage<IList> arrayCast, object obj)
		{
			CallSite<Func<CallSite, object, IList>> site = arrayCast.GetSite(ProtocolConversionAction<ConvertToArrayAction>.Make(arrayCast.Context));
			return site.Target(site, obj);
		}

		public static IList TryCastToArray(ConversionStorage<IList> arrayTryCast, object obj)
		{
			CallSite<Func<CallSite, object, IList>> site = arrayTryCast.GetSite(ProtocolConversionAction<TryConvertToArrayAction>.Make(arrayTryCast.Context));
			return site.Target(site, obj);
		}

		public static IList TryConvertToArray(ConversionStorage<IList> tryToA, object obj)
		{
			CallSite<Func<CallSite, object, IList>> site = tryToA.GetSite(ProtocolConversionAction<TryConvertToAAction>.Make(tryToA.Context));
			return site.Target(site, obj);
		}

		internal static IList ImplicitTrySplat(RubyContext context, object splattee)
		{
			CallSite<Func<CallSite, object, object>> toImplicitTrySplatSite = context.GetClassOf(splattee).ToImplicitTrySplatSite;
			return toImplicitTrySplatSite.Target(toImplicitTrySplatSite, splattee) as IList;
		}

		public static void TryConvertToOptions(ConversionStorage<IDictionary<object, object>> toHash, ref IDictionary<object, object> options, ref object param1, ref object param2)
		{
			if (options != null || param1 == Missing.Value)
			{
				return;
			}
			CallSite<Func<CallSite, object, IDictionary<object, object>>> site = toHash.GetSite(ProtocolConversionAction<TryConvertToHashAction>.Make(toHash.Context));
			if (param2 != Missing.Value)
			{
				options = site.Target(site, param2);
				if (options != null)
				{
					param2 = Missing.Value;
				}
			}
			else
			{
				options = site.Target(site, param1);
				if (options != null)
				{
					param1 = Missing.Value;
				}
			}
		}

		public static double ConvertStringToFloat(RubyContext context, MutableString value)
		{
			return RubyOps.ConvertStringToFloat(context, value.ConvertToString());
		}

		public static IntegerValue ConvertToInteger(ConversionStorage<IntegerValue> integerConversion, object value)
		{
			CallSite<Func<CallSite, object, IntegerValue>> site = integerConversion.GetSite(CompositeConversionAction.Make(integerConversion.Context, CompositeConversion.ToIntToI));
			return site.Target(site, value);
		}

		public static IntegerValue CastToInteger(ConversionStorage<IntegerValue> integerConversion, object value)
		{
			CallSite<Func<CallSite, object, IntegerValue>> site = integerConversion.GetSite(ProtocolConversionAction<ConvertToIntAction>.Make(integerConversion.Context));
			return site.Target(site, value);
		}

		public static double CastToFloat(ConversionStorage<double> floatConversion, object value)
		{
			CallSite<Func<CallSite, object, double>> site = floatConversion.GetSite(ProtocolConversionAction<ConvertToFAction>.Make(floatConversion.Context));
			return site.Target(site, value);
		}

		public static int CastToFixnum(ConversionStorage<int> conversionStorage, object value)
		{
			CallSite<Func<CallSite, object, int>> site = conversionStorage.GetSite(ProtocolConversionAction<ConvertToFixnumAction>.Make(conversionStorage.Context));
			return site.Target(site, value);
		}

		[CLSCompliant(false)]
		public static uint CastToUInt32Unchecked(ConversionStorage<IntegerValue> integerConversion, object obj)
		{
			if (obj == null)
			{
				throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
			}
			return CastToInteger(integerConversion, obj).ToUInt32Unchecked();
		}

		[CLSCompliant(false)]
		public static long CastToInt64Unchecked(ConversionStorage<IntegerValue> integerConversion, object obj)
		{
			if (obj == null)
			{
				throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
			}
			return CastToInteger(integerConversion, obj).ToInt64();
		}

		public static int Compare(ComparisonStorage comparisonStorage, object lhs, object rhs)
		{
			CallSite<Func<CallSite, object, object, object>> compareSite = comparisonStorage.CompareSite;
			object obj = compareSite.Target(compareSite, lhs, rhs);
			if (obj != null)
			{
				return ConvertCompareResult(comparisonStorage, obj);
			}
			throw RubyExceptions.MakeComparisonError(comparisonStorage.Context, lhs, rhs);
		}

		public static int ConvertCompareResult(ComparisonStorage comparisonStorage, object result)
		{
			CallSite<Func<CallSite, object, object, object>> greaterThanSite = comparisonStorage.GreaterThanSite;
			if (RubyOps.IsTrue(greaterThanSite.Target(greaterThanSite, result, 0)))
			{
				return 1;
			}
			CallSite<Func<CallSite, object, object, object>> lessThanSite = comparisonStorage.LessThanSite;
			if (RubyOps.IsTrue(lessThanSite.Target(lessThanSite, result, 0)))
			{
				return -1;
			}
			return 0;
		}

		public static bool IsTrue(object obj)
		{
			if (!(obj is bool))
			{
				return obj != null;
			}
			return (bool)obj;
		}

		public static bool IsEqual(BinaryOpStorage equals, object lhs, object rhs)
		{
			return IsEqual(equals.GetCallSite("=="), lhs, rhs);
		}

		public static bool IsEqual(CallSite<Func<CallSite, object, object, object>> site, object lhs, object rhs)
		{
			if (lhs == rhs)
			{
				return true;
			}
			return IsTrue(site.Target(site, lhs, rhs));
		}

		public static bool RespondTo(RespondToStorage respondToStorage, object target, string methodName)
		{
			return RespondTo(respondToStorage.GetCallSite(), respondToStorage.Context, target, methodName);
		}

		public static bool RespondTo(CallSite<Func<CallSite, object, object, object>> respondToSite, RubyContext context, object target, string methodName)
		{
			return IsTrue(respondToSite.Target(respondToSite, target, context.EncodeIdentifier(methodName)));
		}

		public static void Write(BinaryOpStorage writeStorage, object target, object value)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = writeStorage.GetCallSite("write");
			callSite.Target(callSite, target, value);
		}

		public static int ToHashCode(object hashResult)
		{
			if (hashResult is int)
			{
				return (int)hashResult;
			}
			if (hashResult is BigInteger)
			{
				return hashResult.GetHashCode();
			}
			if (hashResult != null)
			{
				return RuntimeHelpers.GetHashCode(hashResult);
			}
			return RubyUtils.NilObjectId;
		}

		public static object CoerceAndCompare(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, object self, object other)
		{
			object result;
			if (!TryCoerceAndApply(coercionStorage, comparisonStorage, "<=>", self, other, out result))
			{
				return null;
			}
			return result;
		}

		public static bool CoerceAndRelate(BinaryOpStorage coercionStorage, BinaryOpStorage comparisonStorage, string relationalOp, object self, object other)
		{
			object result;
			if (TryCoerceAndApply(coercionStorage, comparisonStorage, relationalOp, self, other, out result))
			{
				return RubyOps.IsTrue(result);
			}
			throw RubyExceptions.MakeComparisonError(coercionStorage.Context, self, other);
		}

		public static object CoerceAndApply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpStorage, string binaryOp, object self, object other)
		{
			object result;
			if (TryCoerceAndApply(coercionStorage, binaryOpStorage, binaryOp, self, other, out result))
			{
				return result;
			}
			throw RubyExceptions.MakeCoercionError(coercionStorage.Context, other, self);
		}

		public static object TryCoerceAndApply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpStorage, string binaryOp, object self, object other)
		{
			if (other == null)
			{
				return null;
			}
			object result;
			if (TryCoerceAndApply(coercionStorage, binaryOpStorage, binaryOp, self, other, out result) && result != null)
			{
				return RubyOps.IsTrue(result);
			}
			return null;
		}

		private static bool TryCoerceAndApply(BinaryOpStorage coercionStorage, BinaryOpStorage binaryOpStorage, string binaryOp, object self, object other, out object result)
		{
			CallSite<Func<CallSite, object, object, object>> callSite = coercionStorage.GetCallSite("coerce", new RubyCallSignature(1, RubyCallFlags.HasImplicitSelf));
			IList list;
			try
			{
				list = callSite.Target(callSite, other, self) as IList;
			}
			catch (SystemException)
			{
				result = null;
				return false;
			}
			if (list != null && list.Count == 2)
			{
				CallSite<Func<CallSite, object, object, object>> callSite2 = binaryOpStorage.GetCallSite(binaryOp);
				result = callSite2.Target(callSite2, list[0], list[1]);
				return true;
			}
			result = null;
			return false;
		}

		public static Type[] ToTypes(RubyContext context, object[] values)
		{
			Type[] array = new Type[values.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ToType(context, values[i]);
			}
			return array;
		}

		public static Type ToType(RubyContext context, object value)
		{
			TypeTracker typeTracker = value as TypeTracker;
			if (typeTracker != null)
			{
				return typeTracker.Type;
			}
			RubyModule rubyModule = value as RubyModule;
			if (rubyModule != null && (rubyModule.IsClass || rubyModule.IsClrModule))
			{
				return rubyModule.GetUnderlyingSystemType();
			}
			throw RubyExceptions.InvalidValueForType(context, value, "Class");
		}

		public static void CheckSafeLevel(RubyContext context, int level)
		{
			if (level <= context.CurrentSafeLevel)
			{
				throw RubyExceptions.CreateSecurityError("Insecure operation at level " + context.CurrentSafeLevel);
			}
		}

		public static void CheckSafeLevel(RubyContext context, int level, string method)
		{
			if (level <= context.CurrentSafeLevel)
			{
				throw RubyExceptions.CreateSecurityError(string.Format("Insecure operation {0} at level {1}", method, context.CurrentSafeLevel));
			}
		}
	}
}

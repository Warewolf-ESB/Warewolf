using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Compiler;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Conversions
{
	internal static class Converter
	{
		private const NumericTypeCode MaxPrimitive = NumericTypeCode.Decimal;

		private const NumericTypeCode MaxNumeric = NumericTypeCode.BigInteger;

		private static readonly int[] ExplicitConversions = CreateExplicitConversions();

		private static readonly NumericTypeCode[] TypeCodeMapping = CreateTypeCodeMapping();

		internal static Convertibility CanConvertFrom(DynamicMetaObject fromArg, Type fromType, Type toType, bool toNotNullable, NarrowingLevel level, bool explicitProtocolConversions, bool implicitProtocolConversions)
		{
			ContractUtils.RequiresNotNull(fromType, "fromType");
			ContractUtils.RequiresNotNull(toType, "toType");
			IConvertibleMetaObject convertibleMetaObject = fromArg as IConvertibleMetaObject;
			IConvertibleRubyMetaObject convertibleRubyMetaObject = fromArg as IConvertibleRubyMetaObject;
			if (toType == fromType)
			{
				return Convertibility.AlwaysConvertible;
			}
			if (fromType == typeof(DynamicNull))
			{
				if (toNotNullable)
				{
					return Convertibility.NotConvertible;
				}
				if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					return Convertibility.AlwaysConvertible;
				}
				if (!toType.IsValueType)
				{
					return Convertibility.AlwaysConvertible;
				}
				if (toType == typeof(bool))
				{
					return Convertibility.AlwaysConvertible;
				}
				if (!RubyConversionAction.HasDefaultConversion(toType))
				{
					return Convertibility.NotConvertible;
				}
			}
			if (fromType == typeof(MissingBlockParam))
			{
				return new Convertibility(toType == typeof(BlockParam) && !toNotNullable, null);
			}
			if (fromType == typeof(BlockParam) && toType == typeof(MissingBlockParam))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (toType.IsAssignableFrom(fromType))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (HasImplicitNumericConversion(fromType, toType))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (CompilerHelpers.GetImplicitConverter(fromType, toType) != null)
			{
				return Convertibility.AlwaysConvertible;
			}
			if (convertibleRubyMetaObject != null)
			{
				return convertibleRubyMetaObject.IsConvertibleTo(toType, false);
			}
			if (convertibleMetaObject != null)
			{
				return new Convertibility(convertibleMetaObject.CanConvertTo(toType, false), null);
			}
			if (level < NarrowingLevel.One)
			{
				return Convertibility.NotConvertible;
			}
			if (explicitProtocolConversions && RubyConversionAction.HasDefaultConversion(toType))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (level < NarrowingLevel.Two)
			{
				return Convertibility.NotConvertible;
			}
			if (HasExplicitNumericConversion(fromType, toType))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (CompilerHelpers.GetExplicitConverter(fromType, toType) != null)
			{
				return Convertibility.AlwaysConvertible;
			}
			if (CompilerHelpers.HasTypeConverter(fromType, toType))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (fromType == typeof(char) && toType == typeof(string))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (toType == typeof(bool))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (convertibleRubyMetaObject != null)
			{
				return convertibleRubyMetaObject.IsConvertibleTo(toType, true);
			}
			if (convertibleMetaObject != null)
			{
				return new Convertibility(convertibleMetaObject.CanConvertTo(toType, true), null);
			}
			if (level < NarrowingLevel.Three)
			{
				return Convertibility.NotConvertible;
			}
			if (implicitProtocolConversions && RubyConversionAction.HasDefaultConversion(toType))
			{
				return Convertibility.AlwaysConvertible;
			}
			if (TypeUtils.IsComObjectType(fromType) && toType.IsInterface)
			{
				return Convertibility.AlwaysConvertible;
			}
			return Convertibility.NotConvertible;
		}

		internal static Expression ConvertExpression(Expression expr, Type toType, RubyContext context, Expression contextExpression, bool implicitProtocolConversions)
		{
			return ImplicitConvert(expr, expr.Type, toType) ?? ExplicitConvert(expr, expr.Type, toType) ?? Microsoft.Scripting.Ast.Utils.LightDynamic(RubyConversionAction.GetConversionAction(context, toType, implicitProtocolConversions), toType, expr);
		}

		internal static Expression ImplicitConvert(Expression expr, Type fromType, Type toType)
		{
			expr = Microsoft.Scripting.Ast.Utils.Convert(expr, fromType);
			if (toType.IsAssignableFrom(fromType))
			{
				return Microsoft.Scripting.Ast.Utils.Convert(expr, toType);
			}
			if (HasImplicitNumericConversion(fromType, toType))
			{
				return Expression.Convert(expr, toType);
			}
			MethodInfo implicitConverter = CompilerHelpers.GetImplicitConverter(fromType, toType);
			if (implicitConverter != null)
			{
				return Expression.Call(null, implicitConverter, expr);
			}
			return null;
		}

		internal static Expression ExplicitConvert(Expression expr, Type fromType, Type toType)
		{
			expr = Microsoft.Scripting.Ast.Utils.Convert(expr, fromType);
			if (HasExplicitNumericConversion(fromType, toType))
			{
				if (fromType == typeof(BigInteger))
				{
					if (toType == typeof(int))
					{
						return Methods.ConvertBignumToFixnum.OpCall(expr);
					}
					if (toType == typeof(double))
					{
						return Methods.ConvertBignumToFloat.OpCall(expr);
					}
				}
				else if (fromType == typeof(double) && toType == typeof(int))
				{
					return Methods.ConvertDoubleToFixnum.OpCall(expr);
				}
				return Expression.ConvertChecked(expr, toType);
			}
			MethodInfo explicitConverter = CompilerHelpers.GetExplicitConverter(fromType, toType);
			if (explicitConverter != null)
			{
				return Expression.Call(null, explicitConverter, expr);
			}
			if (fromType == typeof(char) && toType == typeof(string))
			{
				return Expression.Call(null, fromType.GetMethod("ToString", BindingFlags.Static | BindingFlags.Public), expr);
			}
			if (toType == typeof(bool))
			{
				if (!fromType.IsValueType)
				{
					return Expression.NotEqual(expr, Microsoft.Scripting.Ast.Utils.Constant(null));
				}
				return Microsoft.Scripting.Ast.Utils.Constant(true);
			}
			return null;
		}

		internal static Candidate PreferConvert(Type t1, Type t2)
		{
			switch (Type.GetTypeCode(t1))
			{
			case TypeCode.SByte:
				switch (Type.GetTypeCode(t2))
				{
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return Candidate.Two;
				default:
					return Candidate.Equivalent;
				}
			case TypeCode.Int16:
				switch (Type.GetTypeCode(t2))
				{
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return Candidate.Two;
				default:
					return Candidate.Equivalent;
				}
			case TypeCode.Int32:
				switch (Type.GetTypeCode(t2))
				{
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return Candidate.Two;
				default:
					return Candidate.Equivalent;
				}
			case TypeCode.Int64:
			{
				TypeCode typeCode = Type.GetTypeCode(t2);
				if (typeCode == TypeCode.UInt64)
				{
					return Candidate.Two;
				}
				return Candidate.Equivalent;
			}
			case TypeCode.Boolean:
				if (t2 == typeof(int))
				{
					return Candidate.Two;
				}
				return Candidate.Equivalent;
			case TypeCode.Double:
			case TypeCode.Decimal:
				if (t2 == typeof(BigInteger))
				{
					return Candidate.Two;
				}
				return Candidate.Equivalent;
			case TypeCode.Char:
				if (t2 == typeof(string))
				{
					return Candidate.Two;
				}
				return Candidate.Equivalent;
			default:
				return Candidate.Equivalent;
			}
		}

		internal static byte ToByte(int value)
		{
			if (value >= 0 && value <= 255)
			{
				return (byte)value;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::Byte");
		}

		internal static sbyte ToSByte(int value)
		{
			if (value >= -128 && value <= 127)
			{
				return (sbyte)value;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::SByte");
		}

		internal static short ToInt16(int value)
		{
			if (value >= -32768 && value <= 32767)
			{
				return (short)value;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::Int16");
		}

		internal static ushort ToUInt16(int value)
		{
			if (value >= 0 && value <= 65535)
			{
				return (ushort)value;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt16");
		}

		internal static uint ToUInt32(int value)
		{
			if ((long)value >= 0L)
			{
				return (uint)value;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt32");
		}

		internal static uint ToUInt32(BigInteger value)
		{
			uint ret;
			if (value.AsUInt32(out ret))
			{
				return ret;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt32");
		}

		internal static long ToInt64(BigInteger value)
		{
			long ret;
			if (value.AsInt64(out ret))
			{
				return ret;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::Int64");
		}

		internal static ulong ToUInt64(int value)
		{
			if (value >= 0)
			{
				return (ulong)value;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt64");
		}

		internal static ulong ToUInt64(BigInteger value)
		{
			ulong ret;
			if (value.AsUInt64(out ret))
			{
				return ret;
			}
			throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt64");
		}

		private static NumericTypeCode GetNumericTypeCode(Type type)
		{
			TypeCode typeCode = Type.GetTypeCode(type);
			if (typeCode == TypeCode.Object && type == typeof(BigInteger))
			{
				return NumericTypeCode.BigInteger;
			}
			return TypeCodeMapping[(int)typeCode];
		}

		private static NumericTypeCode[] CreateTypeCodeMapping()
		{
			return new NumericTypeCode[20]
			{
				NumericTypeCode.Invalid,
				NumericTypeCode.Invalid,
				NumericTypeCode.Invalid,
				NumericTypeCode.Invalid,
				NumericTypeCode.Invalid,
				NumericTypeCode.SByte,
				NumericTypeCode.Byte,
				NumericTypeCode.Int16,
				NumericTypeCode.UInt16,
				NumericTypeCode.Int32,
				NumericTypeCode.UInt32,
				NumericTypeCode.Int64,
				NumericTypeCode.UInt64,
				NumericTypeCode.Single,
				NumericTypeCode.Double,
				NumericTypeCode.Decimal,
				NumericTypeCode.Invalid,
				NumericTypeCode.Invalid,
				NumericTypeCode.Invalid,
				NumericTypeCode.Invalid
			};
		}

		internal static bool HasImplicitNumericConversion(Type fromType, Type toType)
		{
			NumericTypeCode numericTypeCode = GetNumericTypeCode(toType);
			NumericTypeCode numericTypeCode2 = GetNumericTypeCode(fromType);
			if (numericTypeCode != 0 && numericTypeCode2 != 0)
			{
				return (ExplicitConversions[(int)numericTypeCode2] & (1 << (int)numericTypeCode)) == 0;
			}
			return false;
		}

		internal static bool HasExplicitNumericConversion(Type fromType, Type toType)
		{
			return (ExplicitConversions[(int)GetNumericTypeCode(fromType)] & (1 << (int)GetNumericTypeCode(toType))) != 0;
		}

		private static int[] CreateExplicitConversions()
		{
			return new int[13]
			{
				0, 340, 2, 342, 14, 350, 62, 382, 254, 6654,
				7166, 2046, 4094
			};
		}
	}
}

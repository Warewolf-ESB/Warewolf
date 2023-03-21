using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;

namespace IronRuby.Runtime.Conversions
{
	public abstract class RubyConversionAction : RubyMetaBinder
	{
		public override RubyCallSignature Signature
		{
			get
			{
				return RubyCallSignature.WithImplicitSelf(0);
			}
		}

		protected RubyConversionAction()
			: base(null)
		{
		}

		protected RubyConversionAction(RubyContext context)
			: base(context)
		{
		}

		public override string ToString()
		{
			return GetType().Name + ((base.Context != null) ? (" @" + base.Context.RuntimeId) : null);
		}

		public static RubyConversionAction TryGetDefaultConversionAction(RubyContext context, Type parameterType)
		{
			Func<RubyMetaBinderFactory, RubyConversionAction> func = TryGetDefaultConversionAction(parameterType);
			if (func == null)
			{
				return null;
			}
			return func((context != null) ? context.MetaBinderFactory : RubyMetaBinderFactory.Shared);
		}

		public static RubyConversionAction GetConversionAction(RubyContext context, Type parameterType, bool allowProtocolConversions)
		{
			Func<RubyMetaBinderFactory, RubyConversionAction> func = TryGetConversionAction(parameterType, allowProtocolConversions);
			if (func == null)
			{
				return null;
			}
			return func((context != null) ? context.MetaBinderFactory : RubyMetaBinderFactory.Shared);
		}

		public static bool HasDefaultConversion(Type parameterType)
		{
			return TryGetDefaultConversionAction(parameterType) != null;
		}

		private static Func<RubyMetaBinderFactory, RubyConversionAction> TryGetConversionAction(Type parameterType, bool allowProtocolConversion)
		{
			if (allowProtocolConversion)
			{
				Func<RubyMetaBinderFactory, RubyConversionAction> func = TryGetDefaultConversionAction(parameterType);
				if (func != null)
				{
					return func;
				}
			}
			return (RubyMetaBinderFactory factory) => factory.GenericConversionAction(parameterType);
		}

		private static Func<RubyMetaBinderFactory, RubyConversionAction> TryGetDefaultConversionAction(Type parameterType)
		{
			if (parameterType.IsEnum)
			{
				return null;
			}
			switch (Type.GetTypeCode(parameterType))
			{
			case TypeCode.SByte:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToSByteAction>();
			case TypeCode.Byte:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToByteAction>();
			case TypeCode.Int16:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToInt16Action>();
			case TypeCode.UInt16:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToUInt16Action>();
			case TypeCode.Int32:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToFixnumAction>();
			case TypeCode.UInt32:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToUInt32Action>();
			case TypeCode.Int64:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToInt64Action>();
			case TypeCode.UInt64:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToUInt64Action>();
			case TypeCode.Single:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToSingleAction>();
			case TypeCode.Double:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToFAction>();
			case TypeCode.String:
				return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToSymbolAction>();
			default:
				if (parameterType == typeof(MutableString))
				{
					return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToStrAction>();
				}
				if (parameterType == typeof(BigInteger))
				{
					return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToBignumAction>();
				}
				if (parameterType == typeof(IntegerValue))
				{
					return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToIntAction>();
				}
				if (parameterType == typeof(Union<int, MutableString>))
				{
					return (RubyMetaBinderFactory factory) => factory.CompositeConversion(CompositeConversion.ToFixnumToStr);
				}
				if (parameterType == typeof(Union<MutableString, int>))
				{
					return (RubyMetaBinderFactory factory) => factory.CompositeConversion(CompositeConversion.ToStrToFixnum);
				}
				if (parameterType == typeof(RubyRegex))
				{
					return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToRegexAction>();
				}
				if (parameterType == typeof(IList))
				{
					return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToArrayAction>();
				}
				if (parameterType == typeof(IDictionary<object, object>))
				{
					return (RubyMetaBinderFactory factory) => factory.Conversion<ConvertToHashAction>();
				}
				if (parameterType == typeof(int?))
				{
					return (RubyMetaBinderFactory factory) => factory.Conversion<TryConvertToFixnumAction>();
				}
				return null;
			}
		}

		internal static Expression ImplicitConvert(Type toType, CallArguments args)
		{
			return Converter.ImplicitConvert(args.TargetExpression, CompilerHelpers.GetType(args.Target), toType);
		}

		internal static Expression ExplicitConvert(Type toType, CallArguments args)
		{
			return Converter.ExplicitConvert(args.TargetExpression, CompilerHelpers.GetType(args.Target), toType);
		}

		internal static Expression Convert(Type toType, CallArguments args)
		{
			Type type = CompilerHelpers.GetType(args.Target);
			return Converter.ImplicitConvert(args.TargetExpression, type, toType) ?? Converter.ExplicitConvert(args.TargetExpression, type, toType);
		}
	}
}

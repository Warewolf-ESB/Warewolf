using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Conversions
{
	public sealed class CompositeConversionAction : RubyConversionAction
	{
		private readonly CompositeConversion _conversion;

		private readonly ProtocolConversionAction[] _conversions;

		private readonly Type _resultType;

		public override Type ReturnType
		{
			get
			{
				return _resultType;
			}
		}

		private CompositeConversionAction(CompositeConversion conversion, Type resultType, params ProtocolConversionAction[] conversions)
			: base(conversions[0].Context)
		{
			_conversions = conversions;
			_conversion = conversion;
			_resultType = resultType;
		}

		public static CompositeConversionAction Make(RubyContext context, CompositeConversion conversion)
		{
			switch (conversion)
			{
			case CompositeConversion.ToFixnumToStr:
				return new CompositeConversionAction(conversion, typeof(Union<int, MutableString>), ProtocolConversionAction<ConvertToFixnumAction>.Make(context), ProtocolConversionAction<ConvertToStrAction>.Make(context));
			case CompositeConversion.ToStrToFixnum:
				return new CompositeConversionAction(conversion, typeof(Union<MutableString, int>), ProtocolConversionAction<ConvertToStrAction>.Make(context), ProtocolConversionAction<ConvertToFixnumAction>.Make(context));
			case CompositeConversion.ToIntToI:
				return new CompositeConversionAction(conversion, typeof(IntegerValue), ProtocolConversionAction<ConvertToIntAction>.Make(context), ProtocolConversionAction<ConvertToIAction>.Make(context));
			case CompositeConversion.ToAryToInt:
				return new CompositeConversionAction(conversion, typeof(Union<IList, int>), ProtocolConversionAction<ConvertToArrayAction>.Make(context), ProtocolConversionAction<ConvertToFixnumAction>.Make(context));
			case CompositeConversion.ToPathToStr:
				return new CompositeConversionAction(conversion, typeof(MutableString), ProtocolConversionAction<ConvertToPathAction>.Make(context), ProtocolConversionAction<ConvertToStrAction>.Make(context));
			case CompositeConversion.ToHashToStr:
				return new CompositeConversionAction(conversion, typeof(Union<IDictionary<object, object>, MutableString>), ProtocolConversionAction<ConvertToHashAction>.Make(context), ProtocolConversionAction<ConvertToStrAction>.Make(context));
			default:
				throw Assert.Unreachable;
			}
		}

		public static CompositeConversionAction MakeShared(CompositeConversion conversion)
		{
			return Make(null, conversion);
		}

		public override Expression CreateExpression()
		{
			return Methods.GetMethod(GetType(), "MakeShared").OpCall(Expression.Constant(_conversion));
		}

		protected override bool Build(MetaObjectBuilder metaBuilder, CallArguments args, bool defaultFallback)
		{
			ProtocolConversionAction.BuildConversion(metaBuilder, args, _resultType, _conversions);
			return true;
		}

		public override string ToString()
		{
			return _conversion.ToString() + ((base.Context != null) ? (" @" + base.Context.RuntimeId) : null);
		}
	}
}

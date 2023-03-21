using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public abstract class ConvertToFloatingPointAction<TSelf> : ProtocolConversionAction<TSelf> where TSelf : ConvertToFloatingPointAction<TSelf>, new()
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToF;
			}
		}

		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (args.Target == null)
			{
				metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(Microsoft.Scripting.Ast.Utils.Constant("nil"), Microsoft.Scripting.Ast.Utils.Constant(TargetTypeName)));
				return true;
			}
			metaBuilder.Result = RubyConversionAction.Convert(ReturnType, args) ?? FromString(args);
			return metaBuilder.Result != null;
		}

		private static Expression FromString(CallArguments args)
		{
			Expression expression = RubyConversionAction.ImplicitConvert(typeof(MutableString), args);
			if (expression != null)
			{
				return Expression.Call(Methods.ConvertMutableStringToFloat, args.MetaContext.Expression, expression);
			}
			expression = RubyConversionAction.ImplicitConvert(typeof(string), args);
			if (expression != null)
			{
				return Expression.Call(Methods.ConvertStringToFloat, args.MetaContext.Expression, expression);
			}
			return null;
		}
	}
}

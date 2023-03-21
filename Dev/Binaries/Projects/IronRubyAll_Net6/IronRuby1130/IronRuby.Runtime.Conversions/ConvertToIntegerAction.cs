using System.Linq.Expressions;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public abstract class ConvertToIntegerAction<TSelf> : ProtocolConversionAction<TSelf> where TSelf : ConvertToIntegerAction<TSelf>, new()
	{
		protected override string ToMethodName
		{
			get
			{
				return Symbols.ToInt;
			}
		}

		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			object target = args.Target;
			if (target == null)
			{
				metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(Microsoft.Scripting.Ast.Utils.Constant("nil"), Microsoft.Scripting.Ast.Utils.Constant(TargetTypeName)));
				return true;
			}
			Expression expression2 = (metaBuilder.Result = RubyConversionAction.Convert(ReturnType, args));
			return expression2 != null;
		}
	}
}

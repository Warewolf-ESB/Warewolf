using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;

namespace IronRuby.Runtime.Conversions
{
	public abstract class ConvertToIntegerValueAction<TSelf> : ProtocolConversionAction<TSelf> where TSelf : ConvertToIntegerValueAction<TSelf>, new()
	{
		protected override string TargetTypeName
		{
			get
			{
				return "Integer";
			}
		}

		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (args.Target == null)
			{
				metaBuilder.SetError(Methods.CreateTypeConversionError.OpCall(Microsoft.Scripting.Ast.Utils.Constant("nil"), Microsoft.Scripting.Ast.Utils.Constant(TargetTypeName)));
				return true;
			}
			metaBuilder.Result = RubyConversionAction.ImplicitConvert(typeof(int), args) ?? RubyConversionAction.ImplicitConvert(typeof(BigInteger), args);
			return metaBuilder.Result != null;
		}
	}
}

using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public abstract class ConvertToReferenceTypeAction<TSelf, TTargetType> : ProtocolConversionAction<TSelf> where TSelf : ConvertToReferenceTypeAction<TSelf, TTargetType>, new()where TTargetType : class
	{
		protected internal override bool TryImplicitConversion(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			return TryImplicitConversionInternal(metaBuilder, args);
		}

		internal static bool TryImplicitConversionInternal(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (args.Target == null)
			{
				metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Constant(null, typeof(TTargetType));
				return true;
			}
			TTargetType val = args.Target as TTargetType;
			if (val != null)
			{
				metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(TTargetType));
				return true;
			}
			return false;
		}
	}
}

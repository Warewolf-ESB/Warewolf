using System;
using System.Linq.Expressions;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Runtime.Conversions
{
	public abstract class TryConvertToReferenceTypeAction<TSelf, TTargetType> : ConvertToReferenceTypeAction<TSelf, TTargetType> where TSelf : TryConvertToReferenceTypeAction<TSelf, TTargetType>, new()where TTargetType : class
	{
		protected override Expression MakeErrorExpression(CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			return Microsoft.Scripting.Ast.Utils.Constant(null, resultType);
		}

		protected override void SetError(MetaObjectBuilder metaBuilder, CallArguments args, Expression targetClassNameConstant, Type resultType)
		{
			metaBuilder.Result = Microsoft.Scripting.Ast.Utils.Constant(null, resultType);
		}
	}
}

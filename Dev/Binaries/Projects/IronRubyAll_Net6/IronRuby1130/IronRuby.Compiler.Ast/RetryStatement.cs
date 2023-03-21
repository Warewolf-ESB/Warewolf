using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class RetryStatement : JumpStatement
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RetryStatement;
			}
		}

		public RetryStatement(SourceSpan location)
			: base(null, location)
		{
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return TransformRetry(gen);
		}

		internal static System.Linq.Expressions.Expression TransformRetry(AstGenerator gen)
		{
			if (gen.CompilerOptions.IsEval)
			{
				return Methods.EvalRetry.OpCall(gen.CurrentScopeVariable);
			}
			if (gen.CurrentRescue != null)
			{
				return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(gen.CurrentRescue.RetryingVariable, Utils.Constant(true)), System.Linq.Expressions.Expression.Goto(gen.CurrentRescue.RetryLabel, typeof(void)));
			}
			if (gen.CurrentBlock != null)
			{
				return gen.Return(Methods.BlockRetry.OpCall(gen.CurrentBlock.BfcVariable));
			}
			return gen.Return(Methods.MethodRetry.OpCall(gen.CurrentScopeVariable, gen.MakeMethodBlockParameterRead()));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

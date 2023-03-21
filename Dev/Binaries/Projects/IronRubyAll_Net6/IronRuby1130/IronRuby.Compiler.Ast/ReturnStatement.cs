using System.Linq.Expressions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class ReturnStatement : JumpStatement
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ReturnStatement;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public ReturnStatement(Arguments arguments, SourceSpan location)
			: base(arguments, location)
		{
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = TransformReturnValue(gen);
			if (gen.CompilerOptions.IsEval)
			{
				return gen.Return(Methods.EvalReturn.OpCall(gen.CurrentScopeVariable, Microsoft.Scripting.Ast.Utils.Box(expression)));
			}
			if (gen.CurrentBlock != null)
			{
				return gen.Return(Methods.BlockReturn.OpCall(gen.CurrentBlock.BfcVariable, Microsoft.Scripting.Ast.Utils.Box(expression)));
			}
			return gen.Return(expression);
		}

		internal static System.Linq.Expressions.Expression Propagate(AstGenerator gen, System.Linq.Expressions.Expression resultVariable)
		{
			if (gen.CompilerOptions.IsEval)
			{
				return Methods.EvalPropagateReturn.OpCall(resultVariable);
			}
			if (gen.CurrentBlock != null)
			{
				return Methods.BlockPropagateReturn.OpCall(gen.CurrentBlock.BfcVariable, resultVariable);
			}
			return Methods.MethodPropagateReturn.OpCall(gen.CurrentScopeVariable, gen.MakeMethodBlockParameterRead(), System.Linq.Expressions.Expression.Convert(resultVariable, typeof(BlockReturnResult)));
		}
	}
}

using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class BreakStatement : JumpStatement
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.BreakStatement;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public BreakStatement(Arguments arguments, SourceSpan location)
			: base(arguments, location)
		{
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = TransformReturnValue(gen);
			if (gen.CompilerOptions.IsEval)
			{
				return Methods.EvalBreak.OpCall(gen.CurrentScopeVariable, Utils.Box(expression));
			}
			if (gen.CurrentLoop != null)
			{
				return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(gen.CurrentLoop.ResultVariable, System.Linq.Expressions.Expression.Convert(expression, gen.CurrentLoop.ResultVariable.Type)), System.Linq.Expressions.Expression.Break(gen.CurrentLoop.BreakLabel), Utils.Empty());
			}
			if (gen.CurrentBlock != null)
			{
				return gen.Return(Methods.BlockBreak.OpCall(gen.CurrentBlock.BfcVariable, Utils.Box(expression)));
			}
			return Methods.MethodBreak.OpCall(Utils.Box(expression));
		}
	}
}

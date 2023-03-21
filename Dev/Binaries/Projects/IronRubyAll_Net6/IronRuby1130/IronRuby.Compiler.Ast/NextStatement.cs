using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class NextStatement : JumpStatement
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.NextStatement;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public NextStatement(Arguments arguments, SourceSpan location)
			: base(arguments, location)
		{
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = TransformReturnValue(gen);
			if (gen.CompilerOptions.IsEval)
			{
				return Methods.EvalNext.OpCall(gen.CurrentScopeVariable, Utils.Box(expression));
			}
			if (gen.CurrentLoop != null)
			{
				return System.Linq.Expressions.Expression.Block(expression, System.Linq.Expressions.Expression.Continue(gen.CurrentLoop.ContinueLabel), Utils.Empty());
			}
			if (gen.CurrentBlock != null)
			{
				return gen.Return(expression);
			}
			return Methods.MethodNext.OpCall(gen.CurrentScopeVariable, Utils.Box(expression));
		}
	}
}

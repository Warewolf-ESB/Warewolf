using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class RedoStatement : JumpStatement
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RedoStatement;
			}
		}

		public RedoStatement(SourceSpan location)
			: base(null, location)
		{
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			if (gen.CompilerOptions.IsEval)
			{
				return Methods.EvalRedo.OpCall(gen.CurrentScopeVariable);
			}
			if (gen.CurrentLoop != null)
			{
				return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(gen.CurrentLoop.RedoVariable, Utils.Constant(true)), System.Linq.Expressions.Expression.Continue(gen.CurrentLoop.ContinueLabel), Utils.Empty());
			}
			if (gen.CurrentBlock != null)
			{
				return System.Linq.Expressions.Expression.Continue(gen.CurrentBlock.RedoLabel);
			}
			return Methods.MethodRedo.OpCall(gen.CurrentScopeVariable);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

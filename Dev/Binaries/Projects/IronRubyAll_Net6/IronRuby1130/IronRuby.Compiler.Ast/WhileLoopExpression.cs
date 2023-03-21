using System.Linq.Expressions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class WhileLoopExpression : Expression
	{
		private readonly Expression _condition;

		private readonly Statements _statements;

		private readonly bool _isWhileLoop;

		private readonly bool _isPostTest;

		public Expression Condition
		{
			get
			{
				return _condition;
			}
		}

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		public bool IsWhileLoop
		{
			get
			{
				return _isWhileLoop;
			}
		}

		public bool IsPostTest
		{
			get
			{
				return _isPostTest;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.WhileLoopExpression;
			}
		}

		public WhileLoopExpression(Expression condition, bool isWhileLoop, bool isPostTest, Statements statements, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(condition, "condition");
			ContractUtils.RequiresNotNull(statements, "statements");
			_condition = condition;
			_isWhileLoop = isWhileLoop;
			_isPostTest = isPostTest;
			_statements = statements;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#loop-result", typeof(object));
			System.Linq.Expressions.Expression expression2 = gen.CurrentScope.DefineHiddenVariable("#skip-condition", typeof(bool));
			bool flag = gen.CurrentLoop != null;
			LabelTarget labelTarget = System.Linq.Expressions.Expression.Label();
			LabelTarget labelTarget2 = System.Linq.Expressions.Expression.Label();
			gen.EnterLoop(expression2, expression, labelTarget, labelTarget2);
			System.Linq.Expressions.Expression expr = gen.TransformStatements(_statements, ResultOperation.Ignore);
			System.Linq.Expressions.Expression test = _condition.TransformCondition(gen, true);
			gen.LeaveLoop();
			System.Linq.Expressions.Expression body;
			System.Linq.Expressions.Expression body2;
			if (_isWhileLoop)
			{
				body = Microsoft.Scripting.Ast.Utils.Empty();
				body2 = System.Linq.Expressions.Expression.Break(labelTarget);
			}
			else
			{
				body = System.Linq.Expressions.Expression.Break(labelTarget);
				body2 = Microsoft.Scripting.Ast.Utils.Empty();
			}
			BlockBuilder blockBuilder = new BlockBuilder();
			blockBuilder.Add(gen.ClearDebugInfo());
			blockBuilder.Add(System.Linq.Expressions.Expression.Assign(expression2, Microsoft.Scripting.Ast.Utils.Constant(_isPostTest)));
			ParameterExpression expression3;
			blockBuilder.Add(AstFactory.Infinite(labelTarget, labelTarget2, Microsoft.Scripting.Ast.Utils.Try(Microsoft.Scripting.Ast.Utils.If(expression2, System.Linq.Expressions.Expression.Assign(expression2, Microsoft.Scripting.Ast.Utils.Constant(false))).ElseIf(test, body).Else(body2), expr, Microsoft.Scripting.Ast.Utils.Empty()).Catch(expression3 = System.Linq.Expressions.Expression.Parameter(typeof(BlockUnwinder), "#u"), System.Linq.Expressions.Expression.Assign(expression2, System.Linq.Expressions.Expression.Field(expression3, BlockUnwinder.IsRedoField)), Microsoft.Scripting.Ast.Utils.Empty()).Filter(expression3 = System.Linq.Expressions.Expression.Parameter(typeof(EvalUnwinder), "#u"), System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Field(expression3, EvalUnwinder.ReasonField), AstFactory.BlockReturnReasonBreak), System.Linq.Expressions.Expression.Assign(expression, System.Linq.Expressions.Expression.Field(expression3, StackUnwinder.ReturnValueField)), System.Linq.Expressions.Expression.Break(labelTarget))));
			blockBuilder.Add(gen.ClearDebugInfo());
			blockBuilder.Add(Microsoft.Scripting.Ast.Utils.Empty());
			System.Linq.Expressions.Expression expression4 = blockBuilder;
			if (!flag)
			{
				expression4 = Microsoft.Scripting.Ast.Utils.Try(Methods.EnterLoop.OpCall(gen.CurrentScopeVariable), expression4).Finally(Methods.LeaveLoop.OpCall(gen.CurrentScopeVariable));
			}
			return System.Linq.Expressions.Expression.Block(expression4, expression);
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return TransformRead(gen);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class Body : Expression
	{
		private readonly Statements _statements;

		private readonly List<RescueClause> _rescueClauses;

		private readonly Statements _elseStatements;

		private readonly Statements _ensureStatements;

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		public List<RescueClause> RescueClauses
		{
			get
			{
				return _rescueClauses;
			}
		}

		public Statements ElseStatements
		{
			get
			{
				return _elseStatements;
			}
		}

		public Statements EnsureStatements
		{
			get
			{
				return _ensureStatements;
			}
		}

		private bool HasExceptionHandling
		{
			get
			{
				if ((_statements.Count <= 0 || _rescueClauses == null) && _elseStatements == null)
				{
					return _ensureStatements != null;
				}
				return true;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.Body;
			}
		}

		public Body(Statements statements, List<RescueClause> rescueClauses, Statements elseStatements, Statements ensureStatements, SourceSpan location)
			: base(location)
		{
			_statements = statements;
			_rescueClauses = rescueClauses;
			_elseStatements = elseStatements;
			_ensureStatements = ensureStatements;
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return TransformRead(gen);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			if (HasExceptionHandling)
			{
				System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#block-result", typeof(object));
				return System.Linq.Expressions.Expression.Block(TransformExceptionHandling(gen, ResultOperation.Store(expression)), expression);
			}
			return gen.TransformStatementsToExpression(_statements);
		}

		internal override System.Linq.Expressions.Expression TransformResult(AstGenerator gen, ResultOperation resultOperation)
		{
			if (HasExceptionHandling)
			{
				return TransformExceptionHandling(gen, resultOperation);
			}
			return gen.TransformStatements(_statements, resultOperation);
		}

		private System.Linq.Expressions.Expression TransformExceptionHandling(AstGenerator gen, ResultOperation resultOperation)
		{
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#exception-thrown", typeof(bool));
			System.Linq.Expressions.Expression expression2 = gen.CurrentScope.DefineHiddenVariable("#exception-rethrow", typeof(bool));
			System.Linq.Expressions.Expression expression3 = gen.CurrentScope.DefineHiddenVariable("#retrying", typeof(bool));
			System.Linq.Expressions.Expression expression4 = gen.CurrentScope.DefineHiddenVariable("#old-exception", typeof(Exception));
			System.Linq.Expressions.Expression body = ((_ensureStatements == null) ? Microsoft.Scripting.Ast.Utils.IfThen(System.Linq.Expressions.Expression.AndAlso(expression2, System.Linq.Expressions.Expression.NotEqual(System.Linq.Expressions.Expression.Assign(expression4, Methods.GetCurrentException.OpCall(gen.CurrentScopeVariable)), Microsoft.Scripting.Ast.Utils.Constant(null, typeof(Exception)))), System.Linq.Expressions.Expression.Throw(expression4)) : System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(expression4, Methods.GetCurrentException.OpCall(gen.CurrentScopeVariable)), gen.TransformStatements(_ensureStatements, ResultOperation.Ignore), Methods.SetCurrentException.OpCall(gen.CurrentScopeVariable, expression4), Microsoft.Scripting.Ast.Utils.IfThen(System.Linq.Expressions.Expression.AndAlso(expression2, System.Linq.Expressions.Expression.NotEqual(expression4, Microsoft.Scripting.Ast.Utils.Constant(null))), System.Linq.Expressions.Expression.Throw(expression4))));
			System.Linq.Expressions.Expression expression5 = ((_elseStatements == null) ? Microsoft.Scripting.Ast.Utils.Empty() : gen.TransformStatements(_elseStatements, resultOperation));
			System.Linq.Expressions.Expression arg = gen.TransformStatements(_statements, (_elseStatements != null) ? ResultOperation.Ignore : resultOperation);
			System.Linq.Expressions.Expression expression6 = null;
			System.Linq.Expressions.Expression expression7 = null;
			LabelTarget labelTarget = System.Linq.Expressions.Expression.Label("retry");
			System.Linq.Expressions.Expression expression9;
			if (_rescueClauses != null)
			{
				if (gen.CurrentRescue == null)
				{
					expression6 = Methods.EnterRescue.OpCall(gen.CurrentScopeVariable);
					expression7 = Methods.LeaveRescue.OpCall(gen.CurrentScopeVariable);
				}
				else
				{
					expression6 = (expression7 = Microsoft.Scripting.Ast.Utils.Empty());
				}
				gen.EnterRescueClause(expression3, labelTarget);
				IfStatementTest[] array = new IfStatementTest[_rescueClauses.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _rescueClauses[i].Transform(gen, resultOperation);
				}
				ParameterExpression expression8;
				expression9 = Microsoft.Scripting.Ast.Utils.Try(expression6, Microsoft.Scripting.Ast.Utils.If(array, System.Linq.Expressions.Expression.Assign(expression2, Microsoft.Scripting.Ast.Utils.Constant(true)))).Filter(expression8 = System.Linq.Expressions.Expression.Parameter(typeof(EvalUnwinder), "#u"), System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Field(expression8, EvalUnwinder.ReasonField), Microsoft.Scripting.Ast.Utils.Constant(BlockReturnReason.Retry)), System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(expression3, Microsoft.Scripting.Ast.Utils.Constant(true)), System.Linq.Expressions.Expression.Continue(labelTarget), Microsoft.Scripting.Ast.Utils.Empty()));
				gen.LeaveRescueClause();
			}
			else
			{
				expression9 = System.Linq.Expressions.Expression.Assign(expression2, Microsoft.Scripting.Ast.Utils.Constant(true));
			}
			if (_elseStatements != null)
			{
				expression5 = Microsoft.Scripting.Ast.Utils.Unless(expression, expression5);
			}
			ParameterExpression arg2;
			return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Label(labelTarget), Microsoft.Scripting.Ast.Utils.Try(System.Linq.Expressions.Expression.Assign(expression, Microsoft.Scripting.Ast.Utils.Constant(false)), System.Linq.Expressions.Expression.Assign(expression2, Microsoft.Scripting.Ast.Utils.Constant(false)), System.Linq.Expressions.Expression.Assign(expression3, Microsoft.Scripting.Ast.Utils.Constant(false)), (_rescueClauses == null) ? ((System.Linq.Expressions.Expression)Microsoft.Scripting.Ast.Utils.Empty()) : ((System.Linq.Expressions.Expression)System.Linq.Expressions.Expression.Assign(expression4, Methods.GetCurrentException.OpCall(gen.CurrentScopeVariable))), Microsoft.Scripting.Ast.Utils.Try(System.Linq.Expressions.Expression.Block(arg, Microsoft.Scripting.Ast.Utils.Empty())).Filter(arg2 = System.Linq.Expressions.Expression.Parameter(typeof(Exception), "#e"), Methods.CanRescue.OpCall(gen.CurrentScopeVariable, arg2), System.Linq.Expressions.Expression.Assign(expression, Microsoft.Scripting.Ast.Utils.Constant(true)), expression9, Microsoft.Scripting.Ast.Utils.Empty()).FinallyIf(_rescueClauses != null, Microsoft.Scripting.Ast.Utils.Unless(expression2, Methods.SetCurrentException.OpCall(gen.CurrentScopeVariable, expression4)), expression7), expression5, Microsoft.Scripting.Ast.Utils.Empty()).FilterIf(_rescueClauses != null || _elseStatements != null, arg2 = System.Linq.Expressions.Expression.Parameter(typeof(Exception), "#e"), Methods.CanRescue.OpCall(gen.CurrentScopeVariable, arg2), System.Linq.Expressions.Expression.Assign(expression2, Microsoft.Scripting.Ast.Utils.Constant(true)), Microsoft.Scripting.Ast.Utils.Empty()).FinallyWithJumps(Microsoft.Scripting.Ast.Utils.Unless(expression3, body)));
		}

		internal override Expression ToCondition(LexicalScope currentScope)
		{
			if (_statements != null && _statements.Count == 1 && !HasExceptionHandling)
			{
				return _statements.First.ToCondition(currentScope);
			}
			return this;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

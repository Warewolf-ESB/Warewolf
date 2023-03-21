using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class CaseExpression : Expression
	{
		private readonly Expression _value;

		private readonly WhenClause[] _whenClauses;

		private readonly Statements _elseStatements;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.CaseExpression;
			}
		}

		public Expression Value
		{
			get
			{
				return _value;
			}
		}

		public WhenClause[] WhenClauses
		{
			get
			{
				return _whenClauses;
			}
		}

		public Statements ElseStatements
		{
			get
			{
				return _elseStatements;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		internal CaseExpression(Expression value, WhenClause[] whenClauses, ElseIfClause elseClause, SourceSpan location)
			: this(value, whenClauses, (elseClause != null) ? elseClause.Statements : null, location)
		{
		}

		public CaseExpression(Expression value, WhenClause[] whenClauses, Statements elseStatements, SourceSpan location)
			: base(location)
		{
			_value = value;
			_whenClauses = whenClauses ?? WhenClause.EmptyArray;
			_elseStatements = elseStatements;
		}

		private static System.Linq.Expressions.Expression MakeTest(AstGenerator gen, Expression expr, System.Linq.Expressions.Expression value)
		{
			System.Linq.Expressions.Expression expression = expr.TransformRead(gen);
			if (expr is SplattedArgument)
			{
				if (value != null)
				{
					return Methods.ExistsUnsplatCompare.OpCall(System.Linq.Expressions.Expression.Constant(CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(gen.Context, "===", RubyCallSignature.WithImplicitSelf(2)))), Utils.LightDynamic(ProtocolConversionAction<ExplicitTrySplatAction>.Make(gen.Context), expression), Utils.Box(value));
				}
				return Methods.ExistsUnsplat.OpCall(Utils.LightDynamic(ProtocolConversionAction<ExplicitTrySplatAction>.Make(gen.Context), expression));
			}
			if (value != null)
			{
				return AstFactory.IsTrue(CallSiteBuilder.InvokeMethod(gen.Context, "===", RubyCallSignature.WithScope(1), gen.CurrentScopeVariable, expression, value));
			}
			return AstFactory.IsTrue(expression);
		}

		internal static System.Linq.Expressions.Expression TransformWhenCondition(AstGenerator gen, Expression[] comparisons, System.Linq.Expressions.Expression value)
		{
			System.Linq.Expressions.Expression expression;
			if (comparisons.Length > 0)
			{
				expression = MakeTest(gen, comparisons[comparisons.Length - 1], value);
				for (int num = comparisons.Length - 2; num >= 0; num--)
				{
					expression = System.Linq.Expressions.Expression.OrElse(MakeTest(gen, comparisons[num], value), expression);
				}
			}
			else
			{
				expression = System.Linq.Expressions.Expression.Constant(false);
			}
			return expression;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = ((_elseStatements == null) ? Utils.Constant(null) : gen.TransformStatementsToExpression(_elseStatements));
			System.Linq.Expressions.Expression expression2 = ((_value == null) ? null : gen.CurrentScope.DefineHiddenVariable("#case-compare-value", typeof(object)));
			for (int num = _whenClauses.Length - 1; num >= 0; num--)
			{
				expression = AstFactory.Condition(TransformWhenCondition(gen, _whenClauses[num].Comparisons, expression2), gen.TransformStatementsToExpression(_whenClauses[num].Statements), expression);
			}
			if (_value != null)
			{
				expression = System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(expression2, System.Linq.Expressions.Expression.Convert(_value.TransformRead(gen), typeof(object))), expression);
			}
			return expression;
		}
	}
}

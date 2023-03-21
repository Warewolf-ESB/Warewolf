using System.Linq.Expressions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class RescueClause : Node
	{
		private readonly Expression[] _types;

		private readonly LeftValue _target;

		private readonly Statements _statements;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RescueClause;
			}
		}

		public Expression[] Types
		{
			get
			{
				return _types;
			}
		}

		public LeftValue Target
		{
			get
			{
				return _target;
			}
		}

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public RescueClause(Expression[] types, LeftValue target, Statements statements, SourceSpan location)
			: base(location)
		{
			_types = types;
			_target = target;
			_statements = statements;
		}

		internal IfStatementTest Transform(AstGenerator gen, ResultOperation resultOperation)
		{
			System.Linq.Expressions.Expression test;
			if (_types.Length != 0)
			{
				ConstantExpression comparisonSiteStorage = System.Linq.Expressions.Expression.Constant(new BinaryOpStorage(gen.Context));
				if (_types.Length == 1)
				{
					test = MakeCompareException(gen, comparisonSiteStorage, _types[0].TransformRead(gen), _types[0] is SplattedArgument);
				}
				else
				{
					System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[_types.Length];
					BlockBuilder blockBuilder = new BlockBuilder();
					for (int i = 0; i < _types.Length; i++)
					{
						System.Linq.Expressions.Expression expression = _types[i].TransformRead(gen);
						blockBuilder.Add(System.Linq.Expressions.Expression.Assign(array[i] = gen.CurrentScope.DefineHiddenVariable("#type_" + i, expression.Type), expression));
					}
					test = MakeCompareException(gen, comparisonSiteStorage, array[0], _types[0] is SplattedArgument);
					for (int j = 1; j < _types.Length; j++)
					{
						test = System.Linq.Expressions.Expression.OrElse(test, MakeCompareException(gen, comparisonSiteStorage, array[j], _types[j] is SplattedArgument));
					}
					blockBuilder.Add(test);
					test = blockBuilder;
				}
			}
			else
			{
				test = Methods.CompareDefaultException.OpCall(gen.CurrentScopeVariable);
			}
			return Microsoft.Scripting.Ast.Utils.IfCondition(test, gen.TransformStatements((_target != null) ? _target.TransformWrite(gen, Methods.GetCurrentException.OpCall(gen.CurrentScopeVariable)) : null, _statements, resultOperation));
		}

		private System.Linq.Expressions.Expression MakeCompareException(AstGenerator gen, System.Linq.Expressions.Expression comparisonSiteStorage, System.Linq.Expressions.Expression expression, bool isSplatted)
		{
			if (isSplatted)
			{
				return Methods.CompareSplattedExceptions.OpCall(comparisonSiteStorage, gen.CurrentScopeVariable, expression);
			}
			return Methods.CompareException.OpCall(comparisonSiteStorage, gen.CurrentScopeVariable, Microsoft.Scripting.Ast.Utils.Box(expression));
		}
	}
}

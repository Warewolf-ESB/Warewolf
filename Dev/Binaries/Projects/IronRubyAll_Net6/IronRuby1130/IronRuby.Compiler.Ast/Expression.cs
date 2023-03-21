using System.Collections.Generic;
using System.Linq.Expressions;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public abstract class Expression : Node
	{
		public static readonly Expression[] EmptyArray = new Expression[0];

		internal static readonly List<Expression> _EmptyList = new List<Expression>();

		internal static readonly Statements _EmptyStatements = new Statements();

		internal static List<Expression> EmptyList
		{
			get
			{
				return _EmptyList;
			}
		}

		internal static Statements EmptyStatements
		{
			get
			{
				return _EmptyStatements;
			}
		}

		protected Expression(SourceSpan location)
			: base(location)
		{
		}

		internal abstract System.Linq.Expressions.Expression TransformRead(AstGenerator gen);

		internal System.Linq.Expressions.Expression TransformReadStep(AstGenerator gen)
		{
			return gen.AddDebugInfo(TransformRead(gen), base.Location);
		}

		internal virtual System.Linq.Expressions.Expression TransformReadBoolean(AstGenerator gen, bool positive)
		{
			return (positive ? Methods.IsTrue : Methods.IsFalse).OpCall(Utils.Box(TransformRead(gen)));
		}

		internal System.Linq.Expressions.Expression TransformCondition(AstGenerator gen, bool positive)
		{
			return gen.AddDebugInfo(TransformReadBoolean(gen, positive), base.Location);
		}

		internal virtual System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return gen.AddDebugInfo(TransformRead(gen), base.Location);
		}

		internal virtual System.Linq.Expressions.Expression TransformResult(AstGenerator gen, ResultOperation resultOperation)
		{
			System.Linq.Expressions.Expression expression = TransformRead(gen);
			System.Linq.Expressions.Expression expression2 = ((resultOperation.Variable == null) ? gen.Return(expression) : System.Linq.Expressions.Expression.Assign(resultOperation.Variable, System.Linq.Expressions.Expression.Convert(expression, resultOperation.Variable.Type)));
			return gen.AddDebugInfo(expression2, base.Location);
		}

		internal virtual System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return null;
		}

		internal virtual string GetNodeName(AstGenerator gen)
		{
			return "expression";
		}

		internal virtual System.Linq.Expressions.Expression TransformIsDefined(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = TransformDefinedCondition(gen);
			System.Linq.Expressions.Expression expression2 = Methods.CreateMutableStringL.OpCall(Utils.Constant(GetNodeName(gen)), System.Linq.Expressions.Expression.Constant(RubyEncoding.Binary));
			if (expression == null)
			{
				return expression2;
			}
			return System.Linq.Expressions.Expression.Condition(expression, expression2, AstFactory.NullOfMutableString);
		}

		internal System.Linq.Expressions.Expression TransformBooleanIsDefined(AstGenerator gen, bool positive)
		{
			System.Linq.Expressions.Expression expression = TransformDefinedCondition(gen);
			if (expression == null)
			{
				if (!positive)
				{
					return AstFactory.False;
				}
				return AstFactory.True;
			}
			if (!positive)
			{
				return System.Linq.Expressions.Expression.Not(expression);
			}
			return expression;
		}

		internal virtual Expression ToCondition(LexicalScope currentScope)
		{
			return this;
		}
	}
}

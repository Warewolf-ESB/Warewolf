using System.Linq.Expressions;
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class RangeExpression : Expression
	{
		private readonly Expression _begin;

		private readonly Expression _end;

		private readonly bool _isExclusive;

		private static int _flipFlopVariableId;

		public Expression Begin
		{
			get
			{
				return _begin;
			}
		}

		public Expression End
		{
			get
			{
				return _end;
			}
		}

		public bool IsExclusive
		{
			get
			{
				return _isExclusive;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RangeExpression;
			}
		}

		public RangeExpression(Expression begin, Expression end, bool isExclusive, SourceSpan location)
			: base(location)
		{
			_begin = begin;
			_end = end;
			_isExclusive = isExclusive;
		}

		private bool IsIntegerRange(out int intBegin, out int intEnd)
		{
			Literal literal;
			Literal literal2;
			if ((literal = _begin as Literal) != null && literal.Value is int && (literal2 = _end as Literal) != null && literal2.Value is int)
			{
				intBegin = (int)literal.Value;
				intEnd = (int)literal2.Value;
				return true;
			}
			intBegin = (intEnd = 0);
			return false;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			int intBegin;
			int intEnd;
			if (IsIntegerRange(out intBegin, out intEnd))
			{
				return (_isExclusive ? Methods.CreateExclusiveIntegerRange : Methods.CreateInclusiveIntegerRange).OpCall(Microsoft.Scripting.Ast.Utils.Constant(intBegin), Microsoft.Scripting.Ast.Utils.Constant(intEnd));
			}
			return (_isExclusive ? Methods.CreateExclusiveRange : Methods.CreateInclusiveRange).OpCall(Microsoft.Scripting.Ast.Utils.Box(_begin.TransformRead(gen)), Microsoft.Scripting.Ast.Utils.Box(_end.TransformRead(gen)), gen.CurrentScopeVariable, Microsoft.Scripting.Ast.Utils.Constant(new BinaryOpStorage(gen.Context)));
		}

		internal override Expression ToCondition(LexicalScope currentScope)
		{
			int intBegin;
			int intEnd;
			if (!IsIntegerRange(out intBegin, out intEnd))
			{
				return new RangeCondition(this, currentScope.GetInnermostStaticTopScope().AddVariable("#FlipFlopState" + Interlocked.Increment(ref _flipFlopVariableId), base.Location));
			}
			return this;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

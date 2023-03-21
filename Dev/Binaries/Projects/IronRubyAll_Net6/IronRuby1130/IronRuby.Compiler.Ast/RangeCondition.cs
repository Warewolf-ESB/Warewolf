using System.Linq.Expressions;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class RangeCondition : Expression
	{
		private readonly RangeExpression _range;

		private readonly LocalVariable _stateVariable;

		public RangeExpression Range
		{
			get
			{
				return _range;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RangeCondition;
			}
		}

		internal RangeCondition(RangeExpression range, LocalVariable stateVariable)
			: base(range.Location)
		{
			_range = range;
			_stateVariable = stateVariable;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression arg = Utils.Box(_range.Begin.TransformRead(gen));
			System.Linq.Expressions.Expression arg2 = Utils.Box(_range.End.TransformRead(gen));
			if (_range.IsExclusive)
			{
				return System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.ReferenceNotEqual(_stateVariable.TransformReadVariable(gen, false), Utils.Constant(null)), System.Linq.Expressions.Expression.Block(_stateVariable.TransformWriteVariable(gen, Methods.NullIfTrue.OpCall(arg2)), Utils.Constant(true)), System.Linq.Expressions.Expression.ReferenceNotEqual(_stateVariable.TransformWriteVariable(gen, Methods.NullIfFalse.OpCall(arg)), Utils.Constant(null)));
			}
			return System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.OrElse(System.Linq.Expressions.Expression.ReferenceNotEqual(_stateVariable.TransformReadVariable(gen, false), Utils.Constant(null)), Methods.IsTrue.OpCall(arg)), System.Linq.Expressions.Expression.Block(_stateVariable.TransformWriteVariable(gen, Methods.NullIfTrue.OpCall(arg2)), Utils.Constant(true)), Utils.Constant(false));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

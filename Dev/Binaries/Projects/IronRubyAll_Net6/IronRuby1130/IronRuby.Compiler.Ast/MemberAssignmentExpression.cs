using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class MemberAssignmentExpression : AssignmentExpression
	{
		private readonly Expression _leftTarget;

		private readonly Expression _right;

		private readonly string _memberName;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.MemberAssignmentExpression;
			}
		}

		public Expression LeftTarget
		{
			get
			{
				return _leftTarget;
			}
		}

		public Expression Right
		{
			get
			{
				return _right;
			}
		}

		public string MemberName
		{
			get
			{
				return _memberName;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public MemberAssignmentExpression(Expression leftTarget, string memberName, string operation, Expression right, SourceSpan location)
			: base(operation, location)
		{
			ContractUtils.RequiresNotNull(leftTarget, "leftTarget");
			ContractUtils.RequiresNotNull(operation, "operation");
			ContractUtils.RequiresNotNull(right, "right");
			ContractUtils.RequiresNotNull(memberName, "memberName");
			ContractUtils.RequiresNotNull(operation, "operation");
			_memberName = memberName;
			_leftTarget = leftTarget;
			_right = right;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			string methodName = _memberName + "=";
			System.Linq.Expressions.Expression expression = _leftTarget.TransformRead(gen);
			System.Linq.Expressions.Expression expression2 = _right.TransformRead(gen);
			System.Linq.Expressions.Expression expression3 = gen.CurrentScope.DefineHiddenVariable(string.Empty, expression.Type);
			bool hasImplicitSelf = _leftTarget.NodeType == NodeTypes.SelfReference;
			if (base.Operation == Symbols.And || base.Operation == Symbols.Or)
			{
				System.Linq.Expressions.Expression left = MethodCall.TransformRead(this, gen, false, _memberName, System.Linq.Expressions.Expression.Assign(expression3, expression), null, null, null, null);
				System.Linq.Expressions.Expression right = MethodCall.TransformRead(this, gen, hasImplicitSelf, methodName, expression3, null, null, null, expression2);
				if (base.Operation == Symbols.And)
				{
					return AndExpression.TransformRead(gen, left, right);
				}
				return OrExpression.TransformRead(gen, left, right);
			}
			System.Linq.Expressions.Expression transformedTarget = MethodCall.TransformRead(this, gen, false, _memberName, expression3, null, null, null, null);
			System.Linq.Expressions.Expression assignmentRhsArgument = MethodCall.TransformRead(this, gen, false, base.Operation, transformedTarget, null, null, expression2, null);
			return MethodCall.TransformRead(this, gen, hasImplicitSelf, methodName, System.Linq.Expressions.Expression.Assign(expression3, expression), null, null, null, assignmentRhsArgument);
		}
	}
}

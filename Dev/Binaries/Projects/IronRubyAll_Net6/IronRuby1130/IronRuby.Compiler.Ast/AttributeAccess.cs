using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class AttributeAccess : LeftValue
	{
		private Expression _qualifier;

		private string _name;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.AttributeAccess;
			}
		}

		public Expression Qualifier
		{
			get
			{
				return _qualifier;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public AttributeAccess(Expression qualifier, string name, SourceSpan location)
			: base(location)
		{
			_name = name + "=";
			_qualifier = qualifier;
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "assignment";
		}

		internal override System.Linq.Expressions.Expression TransformTargetRead(AstGenerator gen)
		{
			return _qualifier.TransformRead(gen);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen, System.Linq.Expressions.Expression targetValue, bool tryRead)
		{
			throw Assert.Unreachable;
		}

		internal override System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, System.Linq.Expressions.Expression targetValue, System.Linq.Expressions.Expression rightValue)
		{
			return MethodCall.TransformRead(this, gen, _qualifier.NodeType == NodeTypes.SelfReference, _name, targetValue, null, null, null, rightValue);
		}
	}
}

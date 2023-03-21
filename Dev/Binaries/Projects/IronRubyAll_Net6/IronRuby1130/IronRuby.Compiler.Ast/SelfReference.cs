using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class SelfReference : Expression
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.SelfReference;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public SelfReference(SourceSpan location)
			: base(location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return gen.CurrentSelfVariable;
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "self";
		}
	}
}

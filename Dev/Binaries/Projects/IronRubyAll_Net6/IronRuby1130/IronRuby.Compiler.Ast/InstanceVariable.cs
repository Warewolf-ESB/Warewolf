using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class InstanceVariable : Variable
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.InstanceVariable;
			}
		}

		public InstanceVariable(string name, SourceSpan location)
			: base(name, location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformReadVariable(AstGenerator gen, bool tryRead)
		{
			return Methods.GetInstanceVariable.OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, Utils.Constant(base.Name));
		}

		internal override System.Linq.Expressions.Expression TransformWriteVariable(AstGenerator gen, System.Linq.Expressions.Expression rightValue)
		{
			return Methods.SetInstanceVariable.OpCall(gen.CurrentSelfVariable, Utils.Box(rightValue), gen.CurrentScopeVariable, Utils.Constant(base.Name));
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return Methods.IsDefinedInstanceVariable.OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, Utils.Constant(base.Name));
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "instance-variable";
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

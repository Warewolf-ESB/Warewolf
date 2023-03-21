using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class GlobalVariable : Variable
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.GlobalVariable;
			}
		}

		public string FullName
		{
			get
			{
				return "$" + base.Name;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public GlobalVariable(string name, SourceSpan location)
			: base(name, location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformReadVariable(AstGenerator gen, bool tryRead)
		{
			return Methods.GetGlobalVariable.OpCall(gen.CurrentScopeVariable, TransformName(gen));
		}

		internal override System.Linq.Expressions.Expression TransformWriteVariable(AstGenerator gen, System.Linq.Expressions.Expression rightValue)
		{
			return Methods.SetGlobalVariable.OpCall(Utils.Box(rightValue), gen.CurrentScopeVariable, TransformName(gen));
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return Methods.IsDefinedGlobalVariable.OpCall(gen.CurrentScopeVariable, TransformName(gen));
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "global-variable";
		}
	}
}

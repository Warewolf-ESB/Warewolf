using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ClassVariable : Variable
	{
		private const int OpTryGet = 0;

		private const int OpGet = 1;

		private const int OpIsDefined = 2;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ClassVariable;
			}
		}

		public ClassVariable(string name, SourceSpan location)
			: base(name, location)
		{
		}

		private System.Linq.Expressions.Expression TransformRead(AstGenerator gen, int opKind)
		{
			return GetOp(gen, opKind).OpCall(gen.CurrentScopeVariable, Utils.Constant(base.Name));
		}

		private static MethodInfo GetOp(AstGenerator gen, int opKind)
		{
			switch (opKind)
			{
			case 0:
				return Methods.TryGetClassVariable;
			case 1:
				return Methods.GetClassVariable;
			case 2:
				return Methods.IsDefinedClassVariable;
			default:
				throw Assert.Unreachable;
			}
		}

		internal override System.Linq.Expressions.Expression TransformReadVariable(AstGenerator gen, bool tryRead)
		{
			return TransformRead(gen, (!tryRead) ? 1 : 0);
		}

		internal override System.Linq.Expressions.Expression TransformWriteVariable(AstGenerator gen, System.Linq.Expressions.Expression rightValue)
		{
			return Methods.SetClassVariable.OpCall(Utils.Box(rightValue), gen.CurrentScopeVariable, Utils.Constant(base.Name));
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return TransformRead(gen, 2);
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "class variable";
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

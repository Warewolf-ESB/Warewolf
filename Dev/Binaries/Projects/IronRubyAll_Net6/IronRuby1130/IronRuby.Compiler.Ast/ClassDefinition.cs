using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ClassDefinition : ModuleDefinition
	{
		private readonly Expression _superClass;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ClassDefinition;
			}
		}

		public Expression SuperClass
		{
			get
			{
				return _superClass;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public ClassDefinition(LexicalScope definedScope, ConstantVariable name, Expression superClass, Body body, SourceSpan location)
			: base(definedScope, name, body, location)
		{
			ContractUtils.RequiresNotNull(name, "name");
			_superClass = superClass;
		}

		internal override System.Linq.Expressions.Expression MakeDefinitionExpression(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = base.QualifiedName.TransformName(gen);
			System.Linq.Expressions.Expression expression2 = ((_superClass != null) ? Utils.Box(_superClass.TransformRead(gen)) : Utils.Constant(null));
			System.Linq.Expressions.Expression transformedQualifier;
			switch (base.QualifiedName.TransformQualifier(gen, out transformedQualifier))
			{
			case StaticScopeKind.Global:
				return Methods.DefineGlobalClass.OpCall(gen.CurrentScopeVariable, expression, expression2);
			case StaticScopeKind.EnclosingModule:
				return Methods.DefineNestedClass.OpCall(gen.CurrentScopeVariable, expression, expression2);
			case StaticScopeKind.Explicit:
				return Methods.DefineClass.OpCall(gen.CurrentScopeVariable, transformedQualifier, expression, expression2);
			default:
				throw Assert.Unreachable;
			}
		}
	}
}

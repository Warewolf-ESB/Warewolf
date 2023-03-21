using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ModuleDefinition : DefinitionExpression
	{
		private readonly ConstantVariable _qualifiedName;

		public ConstantVariable QualifiedName
		{
			get
			{
				return _qualifiedName;
			}
		}

		protected virtual bool IsSingletonDeclaration
		{
			get
			{
				return false;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ModuleDefinition;
			}
		}

		public ModuleDefinition(LexicalScope definedScope, ConstantVariable qualifiedName, Body body, SourceSpan location)
			: base(definedScope, body, location)
		{
			ContractUtils.RequiresNotNull(qualifiedName, "qualifiedName");
			_qualifiedName = qualifiedName;
		}

		protected ModuleDefinition(LexicalScope definedScope, Body body, SourceSpan location)
			: base(definedScope, body, location)
		{
			_qualifiedName = null;
		}

		internal virtual System.Linq.Expressions.Expression MakeDefinitionExpression(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = QualifiedName.TransformName(gen);
			System.Linq.Expressions.Expression transformedQualifier;
			switch (QualifiedName.TransformQualifier(gen, out transformedQualifier))
			{
			case StaticScopeKind.Global:
				return Methods.DefineGlobalModule.OpCall(gen.CurrentScopeVariable, expression);
			case StaticScopeKind.EnclosingModule:
				return Methods.DefineNestedModule.OpCall(gen.CurrentScopeVariable, expression);
			case StaticScopeKind.Explicit:
				return Methods.DefineModule.OpCall(gen.CurrentScopeVariable, Microsoft.Scripting.Ast.Utils.Box(transformedQualifier), expression);
			default:
				throw Assert.Unreachable;
			}
		}

		private ScopeBuilder DefineLocals()
		{
			return new ScopeBuilder(base.DefinedScope.AllocateClosureSlotsForLocals(0), null, base.DefinedScope);
		}

		internal sealed override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			string text = (IsSingletonDeclaration ? "SINGLETON" : (((this is ClassDefinition) ? "CLASS" : "MODULE") + " " + QualifiedName.Name));
			ScopeBuilder currentScope = gen.CurrentScope;
			System.Linq.Expressions.Expression right = MakeDefinitionExpression(gen);
			ParameterExpression parameterExpression = currentScope.DefineHiddenVariable("#module", typeof(RubyModule));
			ParameterExpression currentScopeVariable = gen.CurrentScopeVariable;
			ScopeBuilder scopeBuilder = DefineLocals();
			ParameterExpression parameterExpression2 = scopeBuilder.DefineHiddenVariable("#scope", typeof(RubyScope));
			gen.EnterModuleDefinition(scopeBuilder, parameterExpression, parameterExpression2, IsSingletonDeclaration);
			System.Linq.Expressions.Expression expression = base.Body.TransformRead(gen);
			System.Linq.Expressions.Expression expression2 = currentScope.DefineHiddenVariable("#result", expression.Type);
			BlockBuilder blockBuilder = new BlockBuilder();
			blockBuilder.Add(gen.DebugMarker(text));
			blockBuilder.Add(System.Linq.Expressions.Expression.Assign(parameterExpression, right));
			blockBuilder.Add(scopeBuilder.CreateScope(parameterExpression2, Methods.CreateModuleScope.OpCall(scopeBuilder.MakeLocalsStorage(), scopeBuilder.GetVariableNamesExpression(), currentScopeVariable, parameterExpression), System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(expression2, expression), Microsoft.Scripting.Ast.Utils.Empty())));
			blockBuilder.Add(gen.DebugMarker("END OF " + text));
			blockBuilder.Add(expression2);
			System.Linq.Expressions.Expression result = blockBuilder;
			gen.LeaveModuleDefinition();
			return result;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

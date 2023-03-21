using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class SingletonDefinition : ModuleDefinition
	{
		private readonly Expression _singleton;

		public Expression Singleton
		{
			get
			{
				return _singleton;
			}
		}

		protected override bool IsSingletonDeclaration
		{
			get
			{
				return true;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.SingletonDefinition;
			}
		}

		public SingletonDefinition(LexicalScope definedScope, Expression singleton, Body body, SourceSpan location)
			: base(definedScope, body, location)
		{
			ContractUtils.RequiresNotNull(singleton, "singleton");
			_singleton = singleton;
		}

		internal override System.Linq.Expressions.Expression MakeDefinitionExpression(AstGenerator gen)
		{
			return Methods.DefineSingletonClass.OpCall(gen.CurrentScopeVariable, Utils.Box(_singleton.TransformRead(gen)));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public abstract class DefinitionExpression : Expression
	{
		private readonly LexicalScope _definedScope;

		private readonly Body _body;

		public LexicalScope DefinedScope
		{
			get
			{
				return _definedScope;
			}
		}

		public Body Body
		{
			get
			{
				return _body;
			}
		}

		protected DefinitionExpression(LexicalScope definedScope, Body body, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(definedScope, "definedScope");
			ContractUtils.RequiresNotNull(body, "body");
			_definedScope = definedScope;
			_body = body;
		}
	}
}

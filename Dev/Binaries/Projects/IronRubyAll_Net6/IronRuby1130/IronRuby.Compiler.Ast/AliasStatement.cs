using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class AliasStatement : Expression
	{
		private readonly ConstructedSymbol _newName;

		private readonly ConstructedSymbol _oldName;

		private readonly bool _isMethodAlias;

		public ConstructedSymbol NewName
		{
			get
			{
				return _newName;
			}
		}

		public ConstructedSymbol OldName
		{
			get
			{
				return _oldName;
			}
		}

		public bool IsMethodAlias
		{
			get
			{
				return _isMethodAlias;
			}
		}

		public bool IsGlobalVariableAlias
		{
			get
			{
				return !_isMethodAlias;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.AliasStatement;
			}
		}

		public AliasStatement(bool isMethodAlias, ConstructedSymbol newName, ConstructedSymbol oldName, SourceSpan location)
			: base(location)
		{
			_newName = newName;
			_oldName = oldName;
			_isMethodAlias = isMethodAlias;
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return (_isMethodAlias ? Methods.AliasMethod : Methods.AliasGlobalVariable).OpCall(gen.CurrentScopeVariable, _newName.Transform(gen), _oldName.Transform(gen));
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return System.Linq.Expressions.Expression.Block(Transform(gen), Utils.Constant(null));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

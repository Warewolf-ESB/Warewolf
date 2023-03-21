using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class UndefineStatement : Expression
	{
		private readonly List<ConstructedSymbol> _items;

		public List<ConstructedSymbol> Items
		{
			get
			{
				return _items;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.UndefineStatement;
			}
		}

		public UndefineStatement(List<ConstructedSymbol> items, SourceSpan location)
			: base(location)
		{
			_items = items;
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[_items.Count + 1];
			for (int i = 0; i < _items.Count; i++)
			{
				array[i] = Methods.UndefineMethod.OpCall(gen.CurrentScopeVariable, _items[i].Transform(gen));
			}
			array[_items.Count] = Utils.Empty();
			return System.Linq.Expressions.Expression.Block(array);
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

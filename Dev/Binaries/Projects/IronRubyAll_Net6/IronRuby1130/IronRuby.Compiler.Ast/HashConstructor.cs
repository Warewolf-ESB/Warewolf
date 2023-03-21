using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class HashConstructor : Expression
	{
		private readonly Maplet[] _maplets;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.HashConstructor;
			}
		}

		public Maplet[] Maplets
		{
			get
			{
				return _maplets;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public HashConstructor(Maplet[] maplets, SourceSpan location)
			: base(location)
		{
			_maplets = maplets;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return gen.MakeHashOpCall(gen.TransformMapletsToExpressions(_maplets));
		}
	}
}

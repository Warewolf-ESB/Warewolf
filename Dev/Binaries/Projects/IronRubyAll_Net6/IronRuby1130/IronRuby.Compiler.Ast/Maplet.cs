using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class Maplet : Node
	{
		public static Maplet[] EmptyArray = new Maplet[0];

		private readonly Expression _key;

		private readonly Expression _value;

		public Expression Key
		{
			get
			{
				return _key;
			}
		}

		public Expression Value
		{
			get
			{
				return _value;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.Maplet;
			}
		}

		public Maplet(Expression key, Expression value, SourceSpan location)
			: base(location)
		{
			_key = key;
			_value = value;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

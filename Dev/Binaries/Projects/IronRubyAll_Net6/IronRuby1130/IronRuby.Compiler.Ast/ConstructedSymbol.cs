using System.Linq.Expressions;

namespace IronRuby.Compiler.Ast
{
	public struct ConstructedSymbol
	{
		private readonly object _value;

		public object Value
		{
			get
			{
				return _value;
			}
		}

		public ConstructedSymbol(string value)
		{
			_value = value;
		}

		public ConstructedSymbol(StringConstructor value)
		{
			_value = value;
		}

		internal ConstructedSymbol(object value)
		{
			_value = value;
		}

		internal System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			if (_value is string)
			{
				return System.Linq.Expressions.Expression.Constant(_value, typeof(string));
			}
			return Methods.ConvertSymbolToClrString.OpCall(((StringConstructor)_value).TransformRead(gen));
		}
	}
}

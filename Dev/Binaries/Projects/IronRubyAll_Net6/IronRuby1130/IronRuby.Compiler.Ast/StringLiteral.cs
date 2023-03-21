using System.Linq.Expressions;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class StringLiteral : Expression
	{
		private readonly object _value;

		private readonly RubyEncoding _encoding;

		public object Value
		{
			get
			{
				return _value;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return _encoding;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.StringLiteral;
			}
		}

		internal StringLiteral(object value, RubyEncoding encoding, SourceSpan location)
			: base(location)
		{
			_value = value;
			_encoding = encoding;
		}

		public MutableString GetMutableString()
		{
			string text = _value as string;
			if (text != null)
			{
				return MutableString.Create(text, _encoding);
			}
			return MutableString.CreateBinary((byte[])_value, _encoding);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Transform(_value, _encoding);
		}

		internal static System.Linq.Expressions.Expression Transform(object value, RubyEncoding encoding)
		{
			if (value is string)
			{
				return Methods.CreateMutableStringL.OpCall(Utils.Constant(value), encoding.Expression);
			}
			return Methods.CreateMutableStringB.OpCall(Utils.Constant(value), encoding.Expression);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

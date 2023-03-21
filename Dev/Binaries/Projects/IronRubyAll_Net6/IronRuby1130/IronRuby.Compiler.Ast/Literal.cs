using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Compiler.Ast
{
	public class Literal : Expression
	{
		private readonly object _value;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.Literal;
			}
		}

		public object Value
		{
			get
			{
				return _value;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		private Literal(object value, SourceSpan location)
			: base(location)
		{
			_value = value;
		}

		public static Literal Integer(int value, SourceSpan location)
		{
			return new Literal(ScriptingRuntimeHelpers.Int32ToObject(value), location);
		}

		public static Literal Double(double value, SourceSpan location)
		{
			return new Literal(value, location);
		}

		public static Literal BigInteger(BigInteger value, SourceSpan location)
		{
			return new Literal(value, location);
		}

		public static Literal Nil(SourceSpan location)
		{
			return new Literal(null, location);
		}

		public static Literal True(SourceSpan location)
		{
			return new Literal(ScriptingRuntimeHelpers.True, location);
		}

		public static Literal False(SourceSpan location)
		{
			return new Literal(ScriptingRuntimeHelpers.False, location);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Utils.Constant(_value);
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			if (_value == null)
			{
				return "nil";
			}
			if (_value is bool)
			{
				if (!(bool)_value)
				{
					return "false";
				}
				return "true";
			}
			return base.GetNodeName(gen);
		}
	}
}

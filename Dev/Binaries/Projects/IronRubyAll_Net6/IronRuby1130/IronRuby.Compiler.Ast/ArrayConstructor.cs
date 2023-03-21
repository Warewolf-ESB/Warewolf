using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class ArrayConstructor : Expression
	{
		private readonly Arguments _arguments;

		public Arguments Arguments
		{
			get
			{
				return _arguments;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ArrayConstructor;
			}
		}

		public ArrayConstructor(Arguments arguments, SourceSpan location)
			: base(location)
		{
			_arguments = arguments ?? Arguments.Empty;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return _arguments.TransformToArray(gen);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

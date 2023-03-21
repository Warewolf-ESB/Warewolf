using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public abstract class JumpStatement : Expression
	{
		private readonly Arguments _arguments;

		public Arguments Arguments
		{
			get
			{
				return _arguments;
			}
		}

		public JumpStatement(Arguments arguments, SourceSpan location)
			: base(location)
		{
			_arguments = arguments;
		}

		internal System.Linq.Expressions.Expression TransformReturnValue(AstGenerator gen)
		{
			if (_arguments == null)
			{
				return Utils.Constant(null);
			}
			return _arguments.TransformToReturnValue(gen);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Utils.Convert(Transform(gen), typeof(object));
		}

		internal override System.Linq.Expressions.Expression TransformResult(AstGenerator gen, ResultOperation resultOperation)
		{
			return Transform(gen);
		}
	}
}

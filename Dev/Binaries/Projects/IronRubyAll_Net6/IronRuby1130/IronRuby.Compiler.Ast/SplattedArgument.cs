using System.Collections;
using System.Linq.Expressions;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class SplattedArgument : Expression
	{
		private readonly Expression _argument;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.SplattedArgument;
			}
		}

		public Expression Argument
		{
			get
			{
				return _argument;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public SplattedArgument(Expression argument)
			: base(argument.Location)
		{
			_argument = argument;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Utils.LightDynamic(ProtocolConversionAction<ExplicitSplatAction>.Make(gen.Context), typeof(IList), _argument.TransformRead(gen));
		}
	}
}

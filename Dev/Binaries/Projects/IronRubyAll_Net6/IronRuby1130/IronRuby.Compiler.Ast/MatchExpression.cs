using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class MatchExpression : Expression
	{
		private readonly RegularExpression _regex;

		private readonly Expression _expression;

		public RegularExpression Regex
		{
			get
			{
				return _regex;
			}
		}

		public Expression Expression
		{
			get
			{
				return _expression;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.MatchExpression;
			}
		}

		public MatchExpression(RegularExpression regex, Expression expression, SourceSpan location)
			: base(location)
		{
			_regex = regex;
			_expression = expression;
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "method";
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Methods.MatchString.OpCall(Utils.LightDynamic(ProtocolConversionAction<ConvertToStrAction>.Make(gen.Context), typeof(MutableString), _expression.Transform(gen)), _regex.Transform(gen), gen.CurrentScopeVariable);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class BlockReference : Block
	{
		private readonly Expression _expression;

		public sealed override bool IsDefinition
		{
			get
			{
				return false;
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
				return NodeTypes.BlockReference;
			}
		}

		public BlockReference(Expression expression, SourceSpan location)
			: base(location)
		{
			_expression = expression;
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return Utils.LightDynamic(ProtocolConversionAction<ConvertToProcAction>.Make(gen.Context), typeof(Proc), _expression.TransformRead(gen));
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

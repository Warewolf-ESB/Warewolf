using System.Linq.Expressions;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public abstract class LeftValue : Expression
	{
		internal new static readonly LeftValue[] EmptyArray = new LeftValue[0];

		public LeftValue(SourceSpan location)
			: base(location)
		{
		}

		internal abstract System.Linq.Expressions.Expression TransformTargetRead(AstGenerator gen);

		internal sealed override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return TransformRead(gen, TransformTargetRead(gen), false);
		}

		internal System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, System.Linq.Expressions.Expression rightValue)
		{
			return TransformWrite(gen, TransformTargetRead(gen), rightValue);
		}

		internal abstract System.Linq.Expressions.Expression TransformRead(AstGenerator gen, System.Linq.Expressions.Expression targetValue, bool tryRead);

		internal abstract System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, System.Linq.Expressions.Expression targetValue, System.Linq.Expressions.Expression rightValue);
	}
}

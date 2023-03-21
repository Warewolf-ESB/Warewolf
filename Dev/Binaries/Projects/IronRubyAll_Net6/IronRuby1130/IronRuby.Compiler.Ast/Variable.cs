using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public abstract class Variable : LeftValue
	{
		private readonly string _name;

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public Variable(string name, SourceSpan location)
			: base(location)
		{
			_name = name;
		}

		internal System.Linq.Expressions.Expression TransformName(AstGenerator gen)
		{
			return Utils.Constant(_name);
		}

		internal sealed override System.Linq.Expressions.Expression TransformTargetRead(AstGenerator gen)
		{
			return null;
		}

		internal abstract System.Linq.Expressions.Expression TransformReadVariable(AstGenerator gen, bool tryRead);

		internal abstract System.Linq.Expressions.Expression TransformWriteVariable(AstGenerator gen, System.Linq.Expressions.Expression rightValue);

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen, System.Linq.Expressions.Expression targetValue, bool tryRead)
		{
			return TransformReadVariable(gen, tryRead);
		}

		internal override System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, System.Linq.Expressions.Expression targetValue, System.Linq.Expressions.Expression rightValue)
		{
			return TransformWriteVariable(gen, rightValue);
		}

		public override string ToString()
		{
			return _name.ToString();
		}
	}
}

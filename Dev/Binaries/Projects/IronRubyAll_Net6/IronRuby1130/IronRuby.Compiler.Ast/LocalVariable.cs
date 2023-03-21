using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class LocalVariable : Variable
	{
		public new static LocalVariable[] EmptyArray = new LocalVariable[0];

		private readonly int _definitionLexicalDepth;

		private int _closureIndex;

		internal int ClosureIndex
		{
			get
			{
				return _closureIndex;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.LocalVariable;
			}
		}

		internal LocalVariable(string name, SourceSpan location, int definitionLexicalDepth)
			: base(name, location)
		{
			_definitionLexicalDepth = definitionLexicalDepth;
			_closureIndex = -1;
		}

		internal void SetClosureIndex(int index)
		{
			_closureIndex = index;
		}

		internal override System.Linq.Expressions.Expression TransformReadVariable(AstGenerator gen, bool tryRead)
		{
			if (_definitionLexicalDepth >= 0)
			{
				return gen.CurrentScope.GetVariableAccessor(_definitionLexicalDepth, _closureIndex);
			}
			return Methods.GetLocalVariable.OpCall(gen.CurrentScopeVariable, Utils.Constant(base.Name));
		}

		internal override System.Linq.Expressions.Expression TransformWriteVariable(AstGenerator gen, System.Linq.Expressions.Expression rightValue)
		{
			if (_definitionLexicalDepth >= 0)
			{
				return System.Linq.Expressions.Expression.Assign(gen.CurrentScope.GetVariableAccessor(_definitionLexicalDepth, _closureIndex), Utils.Box(rightValue));
			}
			return Methods.SetLocalVariable.OpCall(Utils.Box(rightValue), gen.CurrentScopeVariable, Utils.Constant(base.Name));
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "local-variable";
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

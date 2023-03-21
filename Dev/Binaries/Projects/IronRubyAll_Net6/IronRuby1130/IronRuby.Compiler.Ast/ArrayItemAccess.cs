using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ArrayItemAccess : LeftValue
	{
		private readonly Expression _array;

		private readonly Arguments _arguments;

		private readonly Block _block;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ArrayItemAccess;
			}
		}

		public Expression Array
		{
			get
			{
				return _array;
			}
		}

		public Arguments Arguments
		{
			get
			{
				return _arguments;
			}
		}

		public Block Block
		{
			get
			{
				return _block;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public ArrayItemAccess(Expression array, Arguments arguments, Block block, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNull(array, "array");
			_array = array;
			_arguments = arguments ?? Arguments.Empty;
			_block = block;
		}

		internal override System.Linq.Expressions.Expression TransformTargetRead(AstGenerator gen)
		{
			return _array.TransformRead(gen);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen, System.Linq.Expressions.Expression targetValue, bool tryRead)
		{
			return MethodCall.TransformRead(this, gen, false, "[]", targetValue, _arguments, _block, null, null);
		}

		internal override System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, System.Linq.Expressions.Expression target, System.Linq.Expressions.Expression rightValue)
		{
			return MethodCall.TransformRead(this, gen, _array.NodeType == NodeTypes.SelfReference, "[]=", target, _arguments, _block, null, rightValue);
		}
	}
}

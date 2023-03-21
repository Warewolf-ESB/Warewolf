using System.Linq.Expressions;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class Parameters : Node
	{
		internal static readonly Parameters Empty = new Parameters(null, 0, null, null, null, SourceSpan.None);

		private readonly LeftValue[] _mandatory;

		private readonly int _leadingMandatoryCount;

		private readonly SimpleAssignmentExpression[] _optional;

		private readonly LeftValue _unsplat;

		private readonly LocalVariable _block;

		public LeftValue[] Mandatory
		{
			get
			{
				return _mandatory;
			}
		}

		public int LeadingMandatoryCount
		{
			get
			{
				return _leadingMandatoryCount;
			}
		}

		public SimpleAssignmentExpression[] Optional
		{
			get
			{
				return _optional;
			}
		}

		public LeftValue Unsplat
		{
			get
			{
				return _unsplat;
			}
		}

		public LocalVariable Block
		{
			get
			{
				return _block;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.Parameters;
			}
		}

		public Parameters(LeftValue[] mandatory, int leadingMandatoryCount, SimpleAssignmentExpression[] optional, LeftValue unsplat, LocalVariable block, SourceSpan location)
			: base(location)
		{
			mandatory = mandatory ?? LeftValue.EmptyArray;
			optional = optional ?? SimpleAssignmentExpression.EmptyArray;
			_mandatory = mandatory;
			_leadingMandatoryCount = leadingMandatoryCount;
			_optional = optional;
			_unsplat = unsplat;
			_block = block;
		}

		internal System.Linq.Expressions.Expression TransformOptionalsInitialization(AstGenerator gen)
		{
			if (_optional.Length == 0)
			{
				return Utils.Empty();
			}
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#default", typeof(object));
			System.Linq.Expressions.Expression expression2 = Utils.Empty();
			for (int i = 0; i < _optional.Length; i++)
			{
				expression2 = Utils.IfThen(System.Linq.Expressions.Expression.Equal(_optional[i].Left.TransformRead(gen), expression), System.Linq.Expressions.Expression.Block(expression2, _optional[i].TransformRead(gen)));
			}
			return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(expression, System.Linq.Expressions.Expression.Field(null, Fields.DefaultArgument)), expression2, Utils.Empty());
		}

		internal void TransformForSuperCall(AstGenerator gen, CallSiteBuilder siteBuilder)
		{
			for (int i = 0; i < _leadingMandatoryCount; i++)
			{
				siteBuilder.Add(_mandatory[i].TransformRead(gen));
			}
			SimpleAssignmentExpression[] optional = _optional;
			foreach (SimpleAssignmentExpression simpleAssignmentExpression in optional)
			{
				siteBuilder.Add(simpleAssignmentExpression.Left.TransformRead(gen));
			}
			for (int k = _leadingMandatoryCount; k < _mandatory.Length; k++)
			{
				siteBuilder.Add(_mandatory[k].TransformRead(gen));
			}
			if (_unsplat != null)
			{
				siteBuilder.SplattedArgument = _unsplat.TransformRead(gen);
			}
		}

		internal BlockSignatureAttributes GetBlockSignatureAttributes()
		{
			BlockSignatureAttributes blockSignatureAttributes = BlockSignatureAttributes.None;
			if (_unsplat != null)
			{
				blockSignatureAttributes |= BlockSignatureAttributes.HasUnsplatParameter;
			}
			if (_block != null)
			{
				blockSignatureAttributes |= BlockSignatureAttributes.HasProcParameter;
			}
			int num = _mandatory.Length;
			if (_unsplat != null)
			{
				num = -(num + 1);
			}
			else if (num > 0 && _mandatory[_mandatory.Length - 1] is Placeholder)
			{
				num--;
			}
			return BlockDispatcher.MakeAttributes(blockSignatureAttributes, num);
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

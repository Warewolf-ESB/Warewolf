using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class CompoundLeftValue : LeftValue
	{
		internal static readonly CompoundLeftValue EmptyBlockSignature = new CompoundLeftValue(LeftValue.EmptyArray);

		internal static readonly CompoundLeftValue UnspecifiedBlockSignature = new CompoundLeftValue(LeftValue.EmptyArray);

		private readonly LeftValue[] _leftValues;

		private readonly int _unsplattedValueIndex;

		public LeftValue[] LeftValues
		{
			get
			{
				return _leftValues;
			}
		}

		public int UnsplattedValueIndex
		{
			get
			{
				return _unsplattedValueIndex;
			}
		}

		public LeftValue UnsplattedValue
		{
			get
			{
				if (!HasUnsplattedValue)
				{
					return null;
				}
				return _leftValues[_unsplattedValueIndex];
			}
		}

		public bool HasUnsplattedValue
		{
			get
			{
				return _unsplattedValueIndex < _leftValues.Length;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.CompoundLeftValue;
			}
		}

		public CompoundLeftValue(LeftValue[] leftValues)
			: this(leftValues, leftValues.Length)
		{
		}

		public CompoundLeftValue(LeftValue[] leftValues, int unsplattedValueIndex)
			: base(SourceSpan.None)
		{
			_leftValues = leftValues;
			_unsplattedValueIndex = unsplattedValueIndex;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen, System.Linq.Expressions.Expression targetValue, bool tryRead)
		{
			throw new NotSupportedException("TODO: reading compound l-value");
		}

		internal override System.Linq.Expressions.Expression TransformTargetRead(AstGenerator gen)
		{
			return null;
		}

		internal override System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, System.Linq.Expressions.Expression targetValue, System.Linq.Expressions.Expression rightValue)
		{
			return TransformWrite(gen, rightValue, true);
		}

		internal System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, Arguments rightValues)
		{
			if (_leftValues.Length == 1)
			{
				CompoundLeftValue compoundLeftValue = _leftValues[0] as CompoundLeftValue;
				if (compoundLeftValue != null)
				{
					return compoundLeftValue.TransformWrite(gen, rightValues);
				}
				if (!HasUnsplattedValue)
				{
					return _leftValues[0].TransformWrite(gen, rightValues.TransformToArray(gen));
				}
			}
			if (rightValues.Expressions.Length == 1)
			{
				return TransformWrite(gen, rightValues.Expressions[0].TransformRead(gen), true);
			}
			return TransformWrite(gen, rightValues.TransformToArray(gen), false);
		}

		private System.Linq.Expressions.Expression TransformWrite(AstGenerator gen, System.Linq.Expressions.Expression transformedRight, bool isSimpleRhs)
		{
			BlockBuilder blockBuilder = new BlockBuilder();
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#rhs", typeof(IList));
			System.Linq.Expressions.Expression expression2;
			if (isSimpleRhs)
			{
				expression2 = gen.CurrentScope.DefineHiddenVariable("#pr", transformedRight.Type);
				blockBuilder.Add(System.Linq.Expressions.Expression.Assign(expression2, transformedRight));
				transformedRight = Utils.LightDynamic(ProtocolConversionAction<ImplicitSplatAction>.Make(gen.Context), typeof(IList), expression2);
			}
			else
			{
				expression2 = expression;
			}
			blockBuilder.Add(System.Linq.Expressions.Expression.Assign(expression, transformedRight));
			for (int i = 0; i < _unsplattedValueIndex; i++)
			{
				blockBuilder.Add(_leftValues[i].TransformWrite(gen, Methods.GetArrayItem.OpCall(expression, Utils.Constant(i))));
			}
			if (HasUnsplattedValue)
			{
				System.Linq.Expressions.Expression arg = Utils.Constant(_leftValues.Length - 1);
				System.Linq.Expressions.Expression rightValue = Methods.GetArrayRange.OpCall(expression, Utils.Constant(_unsplattedValueIndex), arg);
				blockBuilder.Add(_leftValues[_unsplattedValueIndex].TransformWrite(gen, rightValue));
				for (int j = _unsplattedValueIndex + 1; j < _leftValues.Length; j++)
				{
					blockBuilder.Add(_leftValues[j].TransformWrite(gen, Methods.GetTrailingArrayItem.OpCall(expression, Utils.Constant(_leftValues.Length - j), arg)));
				}
			}
			blockBuilder.Add(expression2);
			return blockBuilder;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			for (int i = 0; i < _leftValues.Length; i++)
			{
				if (!flag)
				{
					stringBuilder.Append(',');
				}
				else
				{
					flag = false;
				}
				if (i == _unsplattedValueIndex)
				{
					stringBuilder.Append('*');
					stringBuilder.Append(_leftValues[i].ToString());
					continue;
				}
				CompoundLeftValue compoundLeftValue = _leftValues[i] as CompoundLeftValue;
				if (compoundLeftValue != null)
				{
					stringBuilder.Append('(');
				}
				stringBuilder.Append(_leftValues[i].ToString());
				if (compoundLeftValue != null)
				{
					stringBuilder.Append(')');
				}
			}
			return stringBuilder.ToString();
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

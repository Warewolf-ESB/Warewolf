using System.Collections.Generic;
using IronRuby.Builtins;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting.Math;

namespace IronRuby.Compiler
{
	public struct TokenValue
	{
		private NumericUnion _numeric;

		private object _obj1;

		private object _obj2;

		public int Integer1
		{
			get
			{
				return _numeric.Integer1;
			}
			set
			{
				_numeric.Integer1 = value;
			}
		}

		public int Integer2
		{
			get
			{
				return _numeric.Integer2;
			}
			set
			{
				_numeric.Integer2 = value;
			}
		}

		public double Double
		{
			get
			{
				return _numeric.Double;
			}
			set
			{
				_numeric.Double = value;
			}
		}

		internal int VariableFactory
		{
			get
			{
				return Integer1;
			}
			set
			{
				Integer1 = value;
			}
		}

		public int ArgumentCount
		{
			get
			{
				return _numeric.Integer1;
			}
			set
			{
				_numeric.Integer1 = value;
			}
		}

		public Block Block
		{
			get
			{
				return (Block)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public Arguments Arguments
		{
			get
			{
				return (Arguments)_obj2;
			}
			set
			{
				_obj2 = value;
			}
		}

		public object StringContent
		{
			get
			{
				return _obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public RubyEncoding Encoding
		{
			get
			{
				return (RubyEncoding)_obj2;
			}
			set
			{
				_obj2 = value;
			}
		}

		public Expression Expression
		{
			get
			{
				return (Expression)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public List<Expression> Expressions
		{
			get
			{
				return (List<Expression>)_obj2;
			}
			set
			{
				_obj2 = value;
			}
		}

		public Parameters Parameters
		{
			get
			{
				return (Parameters)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public RubyRegexOptions RegExOptions
		{
			get
			{
				return (RubyRegexOptions)Integer1;
			}
			set
			{
				Integer1 = (int)value;
			}
		}

		public CallExpression CallExpression
		{
			get
			{
				return (CallExpression)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public ElseIfClause ElseIfClause
		{
			get
			{
				return (ElseIfClause)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public RescueClause RescueClause
		{
			get
			{
				return (RescueClause)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public WhenClause WhenClause
		{
			get
			{
				return (WhenClause)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public Statements Statements
		{
			get
			{
				return (Statements)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public BlockReference BlockReference
		{
			get
			{
				return (BlockReference)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public BlockDefinition BlockDefinition
		{
			get
			{
				return (BlockDefinition)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public LambdaDefinition LambdaDefinition
		{
			get
			{
				return (LambdaDefinition)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public ConstantVariable ConstantVariable
		{
			get
			{
				return (ConstantVariable)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public Maplet Maplet
		{
			get
			{
				return (Maplet)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public BigInteger BigInteger
		{
			get
			{
				return (BigInteger)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public string String
		{
			get
			{
				return (string)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public LocalVariable LocalVariable
		{
			get
			{
				return (LocalVariable)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public SimpleAssignmentExpression SimpleAssignmentExpression
		{
			get
			{
				return (SimpleAssignmentExpression)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public LeftValue LeftValue
		{
			get
			{
				return (LeftValue)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public CompoundLeftValue CompoundLeftValue
		{
			get
			{
				return (CompoundLeftValue)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public Body Body
		{
			get
			{
				return (Body)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public JumpStatement JumpStatement
		{
			get
			{
				return (JumpStatement)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public RegexMatchReference RegexMatchReference
		{
			get
			{
				return (RegexMatchReference)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public ConstructedSymbol ConstructedSymbol
		{
			get
			{
				return new ConstructedSymbol(_obj1);
			}
			set
			{
				_obj1 = value.Value;
			}
		}

		public List<ConstructedSymbol> ConstructedSymbols
		{
			get
			{
				return (List<ConstructedSymbol>)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public List<LeftValue> LeftValues
		{
			get
			{
				return (List<LeftValue>)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public List<ElseIfClause> ElseIfClauses
		{
			get
			{
				return (List<ElseIfClause>)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public List<WhenClause> WhenClauses
		{
			get
			{
				return (List<WhenClause>)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public List<RescueClause> RescueClauses
		{
			get
			{
				return (List<RescueClause>)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public List<Maplet> Maplets
		{
			get
			{
				return (List<Maplet>)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		public List<SimpleAssignmentExpression> SimpleAssignmentExpressions
		{
			get
			{
				return (List<SimpleAssignmentExpression>)_obj1;
			}
			set
			{
				_obj1 = value;
			}
		}

		internal void SetInteger(int value)
		{
			Integer1 = value;
		}

		internal void SetBigInteger(BigInteger value)
		{
			BigInteger = value;
		}

		internal void SetDouble(double value)
		{
			Double = value;
		}

		internal void SetString(string value)
		{
			String = value;
		}

		internal void SetStringContent(MutableStringBuilder contentBuilder)
		{
			StringContent = contentBuilder.ToValue();
			Encoding = contentBuilder.Encoding;
		}

		internal void SetRegexOptions(RubyRegexOptions value)
		{
			Integer1 = (int)value;
		}

		public override string ToString()
		{
			return string.Format("O1: {0}, O2: {1}, I1: {2}, I2: {3}, D: {4}", _obj1, _obj2, _numeric.Integer1, _numeric.Integer2, _numeric.Double);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class StringConstructor : Expression
	{
		internal interface IFactory
		{
			System.Linq.Expressions.Expression CreateExpression(AstGenerator gen, string literal, RubyEncoding encoding);

			System.Linq.Expressions.Expression CreateExpression(AstGenerator gen, byte[] literal, RubyEncoding encoding);

			System.Linq.Expressions.Expression CreateExpressionN(AstGenerator gen, IEnumerable<System.Linq.Expressions.Expression> args);

			System.Linq.Expressions.Expression CreateExpressionM(AstGenerator gen, ExpressionCollectionBuilder args);
		}

		private sealed class StringFactory : IFactory
		{
			public static readonly StringFactory Instance = new StringFactory();

			public System.Linq.Expressions.Expression CreateExpression(AstGenerator gen, string literal, RubyEncoding encoding)
			{
				return Methods.CreateMutableStringL.OpCall(System.Linq.Expressions.Expression.Constant(literal), encoding.Expression);
			}

			public System.Linq.Expressions.Expression CreateExpression(AstGenerator gen, byte[] literal, RubyEncoding encoding)
			{
				return Methods.CreateMutableStringB.OpCall(System.Linq.Expressions.Expression.Constant(literal), encoding.Expression);
			}

			public System.Linq.Expressions.Expression CreateExpressionN(AstGenerator gen, IEnumerable<System.Linq.Expressions.Expression> args)
			{
				return Methods.CreateMutableString("N").OpCall(System.Linq.Expressions.Expression.NewArrayInit(typeof(MutableString), args));
			}

			public System.Linq.Expressions.Expression CreateExpressionM(AstGenerator gen, ExpressionCollectionBuilder args)
			{
				string suffix = new string('M', args.Count);
				args.Add(gen.Encoding.Expression);
				return Methods.CreateMutableString(suffix).OpCall(args);
			}
		}

		internal sealed class SymbolFactory : IFactory
		{
			public static readonly SymbolFactory Instance = new SymbolFactory();

			public System.Linq.Expressions.Expression CreateExpression(AstGenerator gen, string literal, RubyEncoding encoding)
			{
				return System.Linq.Expressions.Expression.Constant(gen.Context.CreateSymbol(literal, encoding));
			}

			public System.Linq.Expressions.Expression CreateExpression(AstGenerator gen, byte[] literal, RubyEncoding encoding)
			{
				return System.Linq.Expressions.Expression.Constant(gen.Context.CreateSymbol(literal, encoding));
			}

			public System.Linq.Expressions.Expression CreateExpressionN(AstGenerator gen, IEnumerable<System.Linq.Expressions.Expression> args)
			{
				return Methods.CreateSymbol("N").OpCall(System.Linq.Expressions.Expression.NewArrayInit(typeof(MutableString), args), gen.CurrentScopeVariable);
			}

			public System.Linq.Expressions.Expression CreateExpressionM(AstGenerator gen, ExpressionCollectionBuilder args)
			{
				string suffix = new string('M', args.Count);
				args.Add(gen.Encoding.Expression);
				args.Add(gen.CurrentScopeVariable);
				return Methods.CreateSymbol(suffix).OpCall(args);
			}
		}

		private sealed class LiteralConcatenation : List<object>
		{
			private RubyEncoding _encoding;

			private int _length;

			private bool _isBinary;

			public bool IsBinary
			{
				get
				{
					return _isBinary;
				}
			}

			public RubyEncoding Encoding
			{
				get
				{
					return _encoding;
				}
			}

			private void ObjectInvariant()
			{
			}

			public LiteralConcatenation(RubyEncoding sourceEncoding)
			{
				_encoding = sourceEncoding;
			}

			public new void Clear()
			{
				_length = 0;
				base.Clear();
			}

			public bool Add(StringLiteral literal)
			{
				RubyEncoding compatibleEncoding = MutableString.GetCompatibleEncoding(_encoding, literal.Encoding);
				if (compatibleEncoding == null)
				{
					return false;
				}
				string text = literal.Value as string;
				if (text != null)
				{
					if (_isBinary)
					{
						_length += literal.Encoding.Encoding.GetByteCount(text);
					}
					else
					{
						_length += text.Length;
					}
				}
				else
				{
					byte[] array = (byte[])literal.Value;
					if (!_isBinary)
					{
						_length = 0;
						using (Enumerator enumerator = GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								object current = enumerator.Current;
								_length += _encoding.Encoding.GetByteCount((string)current);
							}
						}
						_isBinary = true;
					}
					_length += array.Length;
				}
				_encoding = compatibleEncoding;
				Add(literal.Value);
				return true;
			}

			public object GetValue()
			{
				if (base.Count == 1)
				{
					return base[0];
				}
				if (_isBinary)
				{
					int num = 0;
					byte[] array = new byte[_length];
					using (Enumerator enumerator = GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							object current = enumerator.Current;
							byte[] array2 = current as byte[];
							if (array2 != null)
							{
								Buffer.BlockCopy(array2, 0, array, num, array2.Length);
								num += array2.Length;
							}
							else
							{
								string text = (string)current;
								num += _encoding.Encoding.GetBytes(text, 0, text.Length, array, num);
							}
						}
						return array;
					}
				}
				StringBuilder stringBuilder = new StringBuilder(_length);
				using (Enumerator enumerator2 = GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						string value = (string)enumerator2.Current;
						stringBuilder.Append(value);
					}
				}
				return stringBuilder.ToString();
			}
		}

		private readonly StringKind _kind;

		private readonly List<Expression> _parts;

		public StringKind Kind
		{
			get
			{
				return _kind;
			}
		}

		public List<Expression> Parts
		{
			get
			{
				return _parts;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.StringConstructor;
			}
		}

		public StringConstructor(List<Expression> parts, StringKind kind, SourceSpan location)
			: base(location)
		{
			ContractUtils.RequiresNotNullItems(parts, "parts");
			_parts = parts;
			_kind = kind;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			switch (_kind)
			{
			case StringKind.Mutable:
				return TransformConcatentation(gen, _parts, StringFactory.Instance);
			case StringKind.Symbol:
				return TransformConcatentation(gen, _parts, SymbolFactory.Instance);
			case StringKind.Command:
				return CallSiteBuilder.InvokeMethod(gen.Context, "`", new RubyCallSignature(1, (RubyCallFlags)17), gen.CurrentScopeVariable, gen.CurrentSelfVariable, TransformConcatentation(gen, _parts, StringFactory.Instance));
			default:
				throw Assert.Unreachable;
			}
		}

		internal static System.Linq.Expressions.Expression MakeConversion(AstGenerator gen, Expression expression)
		{
			return Utils.LightDynamic(ConvertToSAction.Make(gen.Context), typeof(MutableString), expression.TransformRead(gen));
		}

		internal static System.Linq.Expressions.Expression TransformConcatentation(AstGenerator gen, List<Expression> parts, IFactory factory)
		{
			if (parts.Count == 1)
			{
				StringLiteral stringLiteral = parts[0] as StringLiteral;
				if (stringLiteral != null)
				{
					string text = stringLiteral.Value as string;
					if (text != null)
					{
						return factory.CreateExpression(gen, text, stringLiteral.Encoding);
					}
					return factory.CreateExpression(gen, (byte[])stringLiteral.Value, stringLiteral.Encoding);
				}
				return factory.CreateExpressionM(gen, new ExpressionCollectionBuilder { MakeConversion(gen, parts[0]) });
			}
			ExpressionCollectionBuilder expressionCollectionBuilder = new ExpressionCollectionBuilder();
			LiteralConcatenation literalConcatenation = new LiteralConcatenation(gen.Encoding);
			if (!ConcatLiteralsAndTransformRecursive(gen, parts, literalConcatenation, expressionCollectionBuilder))
			{
				return factory.CreateExpressionN(gen, CollectionUtils.ConvertAll(parts, (Expression e) => e.Transform(gen)));
			}
			if (literalConcatenation.Count > 0)
			{
				expressionCollectionBuilder.Add(StringLiteral.Transform(literalConcatenation.GetValue(), literalConcatenation.Encoding));
			}
			if (expressionCollectionBuilder.Count <= 2)
			{
				if (expressionCollectionBuilder.Count == 0)
				{
					return factory.CreateExpression(gen, string.Empty, gen.Encoding);
				}
				return factory.CreateExpressionM(gen, expressionCollectionBuilder);
			}
			return factory.CreateExpressionN(gen, expressionCollectionBuilder);
		}

		private static bool ConcatLiteralsAndTransformRecursive(AstGenerator gen, List<Expression> parts, LiteralConcatenation concat, ExpressionCollectionBuilder result)
		{
			for (int i = 0; i < parts.Count; i++)
			{
				Expression expression = parts[i];
				StringLiteral literal;
				if ((literal = expression as StringLiteral) != null)
				{
					if (!concat.Add(literal))
					{
						return false;
					}
					continue;
				}
				StringConstructor stringConstructor;
				if ((stringConstructor = expression as StringConstructor) != null)
				{
					if (!ConcatLiteralsAndTransformRecursive(gen, stringConstructor.Parts, concat, result))
					{
						return false;
					}
					continue;
				}
				if (concat.Count > 0)
				{
					result.Add(StringLiteral.Transform(concat.GetValue(), concat.Encoding));
					concat.Clear();
				}
				result.Add(MakeConversion(gen, expression));
			}
			return true;
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

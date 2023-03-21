using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class RegularExpression : Expression, StringConstructor.IFactory
	{
		private readonly RubyRegexOptions _options;

		private readonly List<Expression> _pattern;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RegularExpression;
			}
		}

		public RubyRegexOptions Options
		{
			get
			{
				return _options;
			}
		}

		public List<Expression> Pattern
		{
			get
			{
				return _pattern;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public RegularExpression(List<Expression> pattern, RubyRegexOptions options, SourceSpan location)
			: this(pattern, options, false, location)
		{
		}

		public RegularExpression(List<Expression> pattern, RubyRegexOptions options, bool isCondition, SourceSpan location)
			: base(location)
		{
			_pattern = pattern;
			_options = options;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return StringConstructor.TransformConcatentation(gen, _pattern, this);
		}

		System.Linq.Expressions.Expression StringConstructor.IFactory.CreateExpression(AstGenerator gen, string literal, RubyEncoding encoding)
		{
			return Methods.CreateRegexL.OpCall(System.Linq.Expressions.Expression.Constant(literal), encoding.Expression, Utils.Constant(_options), Utils.Constant(new StrongBox<RubyRegex>(null)));
		}

		System.Linq.Expressions.Expression StringConstructor.IFactory.CreateExpression(AstGenerator gen, byte[] literal, RubyEncoding encoding)
		{
			return Methods.CreateRegexB.OpCall(System.Linq.Expressions.Expression.Constant(literal), encoding.Expression, Utils.Constant(_options), Utils.Constant(new StrongBox<RubyRegex>(null)));
		}

		System.Linq.Expressions.Expression StringConstructor.IFactory.CreateExpressionN(AstGenerator gen, IEnumerable<System.Linq.Expressions.Expression> args)
		{
			return Methods.CreateRegex("N").OpCall(System.Linq.Expressions.Expression.NewArrayInit(typeof(MutableString), args), Utils.Constant(_options), Utils.Constant(new StrongBox<RubyRegex>(null)));
		}

		System.Linq.Expressions.Expression StringConstructor.IFactory.CreateExpressionM(AstGenerator gen, ExpressionCollectionBuilder args)
		{
			string suffix = new string('M', args.Count);
			args.Add(gen.Encoding.Expression);
			args.Add(Utils.Constant(_options));
			args.Add(Utils.Constant(new StrongBox<RubyRegex>(null)));
			return Methods.CreateRegex(suffix).OpCall(args);
		}

		internal override Expression ToCondition(LexicalScope currentScope)
		{
			return new RegularExpressionCondition(this);
		}
	}
}

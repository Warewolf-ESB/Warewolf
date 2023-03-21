using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public class ForLoopExpression : Expression
	{
		private readonly BlockDefinition _block;

		private readonly Expression _list;

		public BlockDefinition Block
		{
			get
			{
				return _block;
			}
		}

		public Expression List
		{
			get
			{
				return _list;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ForLoopExpression;
			}
		}

		public ForLoopExpression(LexicalScope definedScope, Parameters variables, Expression list, Statements body, SourceSpan location)
			: base(location)
		{
			_block = new BlockDefinition(definedScope, variables, body, location);
			_list = list;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression transformedBlock = _block.Transform(gen);
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#forloop-block", typeof(Proc));
			System.Linq.Expressions.Expression invoke = CallSiteBuilder.InvokeMethod(gen.Context, "each", RubyCallSignature.WithScopeAndBlock(0), gen.CurrentScopeVariable, _list.TransformRead(gen), expression);
			return gen.DebugMark(MethodCall.MakeCallWithBlockRetryable(gen, invoke, expression, transformedBlock, true), "#RB: method call with a block ('for-loop')");
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

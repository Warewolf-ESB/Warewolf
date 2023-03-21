using System.Linq.Expressions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class YieldCall : CallExpression
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.YieldCall;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public YieldCall(Arguments args, SourceSpan location)
			: base(args, null, location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#yielded-bfc", typeof(BlockParam));
			System.Linq.Expressions.Expression expression2 = gen.CurrentScope.DefineHiddenVariable("#result", typeof(object));
			System.Linq.Expressions.Expression test = (gen.CompilerOptions.IsEval ? Methods.EvalYield.OpCall(gen.CurrentScopeVariable, expression, expression2) : ((gen.CurrentBlock == null) ? Methods.MethodYield.OpCall(gen.CurrentScopeVariable, expression, expression2) : Methods.BlockYield.OpCall(gen.CurrentScopeVariable, gen.CurrentBlock.BfcVariable, expression, expression2)));
			BlockBuilder blockBuilder = new BlockBuilder();
			blockBuilder.Add(gen.DebugMarker("#RB: yield begin"));
			blockBuilder.Add(System.Linq.Expressions.Expression.Assign(expression, Methods.CreateBfcForYield.OpCall(gen.MakeMethodBlockParameterRead())));
			blockBuilder.Add(System.Linq.Expressions.Expression.Assign(expression2, (base.Arguments ?? Arguments.Empty).TransformToYield(gen, expression, gen.MakeMethodBlockParameterSelfRead())));
			blockBuilder.Add(Microsoft.Scripting.Ast.Utils.IfThen(test, gen.Return(expression2)));
			blockBuilder.Add(gen.DebugMarker("#RB: yield end"));
			blockBuilder.Add(expression2);
			return blockBuilder;
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "yield";
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return System.Linq.Expressions.Expression.NotEqual(gen.MakeMethodBlockParameterRead(), Microsoft.Scripting.Ast.Utils.Constant(null));
		}
	}
}

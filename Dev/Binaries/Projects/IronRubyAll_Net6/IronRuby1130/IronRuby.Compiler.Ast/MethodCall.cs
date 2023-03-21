using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class MethodCall : CallExpression
	{
		private string _methodName;

		private readonly Expression _target;

		public string MethodName
		{
			get
			{
				return _methodName;
			}
		}

		public Expression Target
		{
			get
			{
				return _target;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.MethodCall;
			}
		}

		public MethodCall(Expression target, string methodName, Arguments args, SourceSpan location)
			: this(target, methodName, args, null, location)
		{
		}

		public MethodCall(Expression target, string methodName, Arguments args, Block block, SourceSpan location)
			: base(args, block, location)
		{
			_methodName = methodName;
			_target = target;
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression transformedTarget;
			bool hasImplicitSelf;
			if (_target != null)
			{
				transformedTarget = _target.TransformRead(gen);
				hasImplicitSelf = false;
			}
			else
			{
				transformedTarget = gen.CurrentSelfVariable;
				hasImplicitSelf = true;
			}
			return TransformRead(this, gen, hasImplicitSelf, _methodName, transformedTarget, base.Arguments, base.Block, null, null);
		}

		internal static System.Linq.Expressions.Expression TransformRead(Expression node, AstGenerator gen, bool hasImplicitSelf, string methodName, System.Linq.Expressions.Expression transformedTarget, Arguments arguments, Block block, System.Linq.Expressions.Expression singleArgument, System.Linq.Expressions.Expression assignmentRhsArgument)
		{
			System.Linq.Expressions.Expression expression;
			System.Linq.Expressions.Expression transformedBlock;
			if (block != null)
			{
				expression = gen.CurrentScope.DefineHiddenVariable("#block-def", typeof(Proc));
				transformedBlock = block.Transform(gen);
			}
			else
			{
				expression = (transformedBlock = null);
			}
			CallSiteBuilder callSiteBuilder = new CallSiteBuilder(gen, transformedTarget, expression);
			if (arguments != null)
			{
				arguments.TransformToCall(gen, callSiteBuilder);
			}
			else if (singleArgument != null)
			{
				callSiteBuilder.Add(singleArgument);
			}
			System.Linq.Expressions.Expression expression2 = null;
			if (assignmentRhsArgument != null)
			{
				expression2 = gen.CurrentScope.DefineHiddenVariable("#rhs", assignmentRhsArgument.Type);
				callSiteBuilder.RhsArgument = System.Linq.Expressions.Expression.Assign(expression2, assignmentRhsArgument);
			}
			System.Linq.Expressions.Expression expression3 = callSiteBuilder.MakeCallAction(methodName, hasImplicitSelf);
			System.Linq.Expressions.Expression expression4 = gen.DebugMark(expression3, methodName);
			if (block != null)
			{
				expression4 = gen.DebugMark(MakeCallWithBlockRetryable(gen, expression4, expression, transformedBlock, block.IsDefinition), "#RB: method call with a block ('" + methodName + "')");
			}
			if (assignmentRhsArgument != null)
			{
				expression4 = System.Linq.Expressions.Expression.Block(expression4, expression2);
			}
			return expression4;
		}

		internal static System.Linq.Expressions.Expression MakeCallWithBlockRetryable(AstGenerator gen, System.Linq.Expressions.Expression invoke, System.Linq.Expressions.Expression blockArgVariable, System.Linq.Expressions.Expression transformedBlock, bool isBlockDefinition)
		{
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#method-result", typeof(object));
			LabelTarget target = System.Linq.Expressions.Expression.Label("retry");
			BlockBuilder blockBuilder = new BlockBuilder();
			blockBuilder.Add(System.Linq.Expressions.Expression.Assign(blockArgVariable, System.Linq.Expressions.Expression.Convert(transformedBlock, blockArgVariable.Type)));
			blockBuilder.Add(System.Linq.Expressions.Expression.Label(target));
			blockBuilder.Add(isBlockDefinition ? Methods.InitializeBlock.OpCall(blockArgVariable) : null);
			ParameterExpression expression2;
			blockBuilder.Add(Microsoft.Scripting.Ast.Utils.Try(System.Linq.Expressions.Expression.Assign(expression, invoke)).Catch(expression2 = System.Linq.Expressions.Expression.Parameter(typeof(EvalUnwinder), "#u"), System.Linq.Expressions.Expression.Assign(expression, System.Linq.Expressions.Expression.Field(expression2, StackUnwinder.ReturnValueField))));
			blockBuilder.Add(System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.TypeEqual(expression, typeof(BlockReturnResult)), System.Linq.Expressions.Expression.IfThenElse(Methods.IsRetrySingleton.OpCall(expression), Microsoft.Scripting.Ast.Utils.IfThenElse(System.Linq.Expressions.Expression.Equal(gen.MakeMethodBlockParameterRead(), blockArgVariable), RetryStatement.TransformRetry(gen), System.Linq.Expressions.Expression.Goto(target)), gen.Return(ReturnStatement.Propagate(gen, expression)))));
			blockBuilder.Add(expression);
			BlockBuilder blockBuilder2 = blockBuilder;
			return blockBuilder2;
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = Microsoft.Scripting.Ast.Utils.LightDynamic(RubyCallAction.Make(gen.Context, _methodName, RubyCallSignature.IsDefined(_target == null)), typeof(bool), gen.CurrentScopeVariable, (_target != null) ? Microsoft.Scripting.Ast.Utils.Box(_target.TransformRead(gen)) : gen.CurrentSelfVariable);
			if (_target == null)
			{
				return expression;
			}
			return gen.TryCatchAny(expression, AstFactory.False);
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "method";
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}

using System.Linq.Expressions;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class SuperCall : CallExpression
	{
		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.SuperCall;
			}
		}

		public bool HasImplicitArguments
		{
			get
			{
				return base.Arguments == null;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public SuperCall(Arguments args, Block block, SourceSpan location)
			: base(args, block, location)
		{
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			System.Linq.Expressions.Expression expression = gen.CurrentScope.DefineHiddenVariable("#super-call-block", typeof(Proc));
			CallSiteBuilder callSiteBuilder = new CallSiteBuilder(gen, gen.CurrentSelfVariable, expression);
			if (HasImplicitArguments)
			{
				if (gen.CurrentMethod.Parameters != null)
				{
					gen.CurrentMethod.Parameters.TransformForSuperCall(gen, callSiteBuilder);
				}
				else if (gen.CompilerOptions.TopLevelParameterNames != null)
				{
					bool topLevelHasUnsplatParameter = gen.CompilerOptions.TopLevelHasUnsplatParameter;
					string[] topLevelParameterNames = gen.CompilerOptions.TopLevelParameterNames;
					for (int i = 0; i < topLevelParameterNames.Length - (topLevelHasUnsplatParameter ? 1 : 0); i++)
					{
						callSiteBuilder.Add(Methods.GetLocalVariable.OpCall(gen.CurrentScopeVariable, Utils.Constant(topLevelParameterNames[i])));
					}
					if (topLevelHasUnsplatParameter)
					{
						callSiteBuilder.SplattedArgument = Methods.GetLocalVariable.OpCall(gen.CurrentScopeVariable, Utils.Constant(topLevelParameterNames[topLevelParameterNames.Length - 1]));
					}
				}
			}
			else
			{
				base.Arguments.TransformToCall(gen, callSiteBuilder);
			}
			return gen.DebugMark(MethodCall.MakeCallWithBlockRetryable(transformedBlock: (base.Block == null) ? gen.MakeMethodBlockParameterRead() : base.Block.Transform(gen), gen: gen, invoke: callSiteBuilder.MakeSuperCallAction(gen.CurrentFrame.UniqueId, HasImplicitArguments), blockArgVariable: expression, isBlockDefinition: base.Block != null && base.Block.IsDefinition), "#RB: super call ('" + gen.CurrentMethod.MethodName + "')");
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return Utils.LightDynamic(SuperCallAction.Make(gen.Context, RubyCallSignature.IsDefined(true), gen.CurrentFrame.UniqueId), typeof(bool), gen.CurrentScopeVariable, gen.CurrentSelfVariable);
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "super";
		}
	}
}

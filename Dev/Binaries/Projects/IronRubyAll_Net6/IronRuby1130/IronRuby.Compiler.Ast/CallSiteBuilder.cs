using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	internal sealed class CallSiteBuilder : ExpressionCollectionBuilder<System.Linq.Expressions.Expression>
	{
		private readonly AstGenerator _gen;

		private readonly bool _hasBlock;

		public System.Linq.Expressions.Expression SplattedArgument { get; set; }

		public System.Linq.Expressions.Expression RhsArgument { get; set; }

		private int HiddenArgumentCount
		{
			get
			{
				if (!_hasBlock)
				{
					return 2;
				}
				return 3;
			}
		}

		internal CallSiteBuilder(AstGenerator gen, System.Linq.Expressions.Expression instance, System.Linq.Expressions.Expression block)
		{
			_hasBlock = block != null;
			_gen = gen;
			Add(Microsoft.Scripting.Ast.Utils.Convert(_gen.CurrentScopeVariable, typeof(RubyScope)));
			Add(instance);
			Add(block);
		}

		private RubyCallFlags GetSignatureFlags()
		{
			RubyCallFlags rubyCallFlags = RubyCallFlags.HasScope;
			if (_hasBlock)
			{
				rubyCallFlags |= RubyCallFlags.HasBlock;
			}
			if (SplattedArgument != null)
			{
				rubyCallFlags |= RubyCallFlags.HasSplattedArgument;
			}
			if (RhsArgument != null)
			{
				rubyCallFlags |= RubyCallFlags.HasRhsArgument;
			}
			return rubyCallFlags;
		}

		public System.Linq.Expressions.Expression MakeSuperCallAction(int lexicalScopeId, bool hasImplicitArguments)
		{
			RubyCallFlags rubyCallFlags = GetSignatureFlags() | RubyCallFlags.HasImplicitSelf;
			if (hasImplicitArguments)
			{
				rubyCallFlags |= RubyCallFlags.IsSuperCall;
			}
			return MakeCallSite(SuperCallAction.Make(_gen.Context, new RubyCallSignature(base.Count - HiddenArgumentCount, rubyCallFlags), lexicalScopeId));
		}

		public System.Linq.Expressions.Expression MakeCallAction(string name, bool hasImplicitSelf)
		{
			RubyCallFlags rubyCallFlags = GetSignatureFlags();
			if (hasImplicitSelf)
			{
				rubyCallFlags |= RubyCallFlags.HasImplicitSelf;
			}
			return MakeCallSite(RubyCallAction.Make(_gen.Context, name, new RubyCallSignature(base.Count - HiddenArgumentCount, rubyCallFlags)));
		}

		internal System.Linq.Expressions.Expression MakeCallSite(CallSiteBinder binder)
		{
			if (SplattedArgument != null)
			{
				Add(SplattedArgument);
			}
			if (RhsArgument != null)
			{
				Add(RhsArgument);
			}
			return Microsoft.Scripting.Ast.Utils.LightDynamic(binder, this);
		}

		internal static System.Linq.Expressions.Expression InvokeMethod(RubyContext context, string name, RubyCallSignature signature, System.Linq.Expressions.Expression scope, System.Linq.Expressions.Expression target)
		{
			return Microsoft.Scripting.Ast.Utils.LightDynamic(RubyCallAction.Make(context, name, signature), Microsoft.Scripting.Ast.Utils.Convert(scope, typeof(RubyScope)), target);
		}

		internal static System.Linq.Expressions.Expression InvokeMethod(RubyContext context, string name, RubyCallSignature signature, System.Linq.Expressions.Expression scope, System.Linq.Expressions.Expression target, System.Linq.Expressions.Expression arg0)
		{
			return Microsoft.Scripting.Ast.Utils.LightDynamic(RubyCallAction.Make(context, name, signature), Microsoft.Scripting.Ast.Utils.Convert(scope, typeof(RubyScope)), target, arg0);
		}

		internal static System.Linq.Expressions.Expression InvokeMethod(RubyContext context, string name, RubyCallSignature signature, System.Linq.Expressions.Expression scope, System.Linq.Expressions.Expression target, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1)
		{
			return Microsoft.Scripting.Ast.Utils.LightDynamic(RubyCallAction.Make(context, name, signature), Microsoft.Scripting.Ast.Utils.Convert(scope, typeof(RubyScope)), target, arg0, arg1);
		}
	}
}

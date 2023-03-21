using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using IronRuby.Compiler;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	internal class RubyScriptCode : ScriptCode
	{
		private sealed class CustomGenerator : DebugInfoGenerator
		{
			public override void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression node)
			{
				RubyMethodDebugInfo.GetOrCreate(method.Name).AddMapping(ilOffset, node.StartLine);
			}
		}

		private readonly Expression<Func<RubyScope, object, object>> _code;

		private readonly TopScopeFactoryKind _kind;

		private Func<RubyScope, object, object> _target;

		private static bool _HasPdbPermissions = true;

		internal Expression<Func<RubyScope, object, object>> Code
		{
			get
			{
				return _code;
			}
		}

		private Func<RubyScope, object, object> Target
		{
			get
			{
				if (_target == null)
				{
					Func<RubyScope, object, object> value = (Func<RubyScope, object, object>)CompileLambda(_code, base.SourceUnit.LanguageContext);
					Interlocked.CompareExchange(ref _target, value, null);
				}
				return _target;
			}
		}

		public RubyScriptCode(Expression<Func<RubyScope, object, object>> code, SourceUnit sourceUnit, TopScopeFactoryKind kind)
			: base(sourceUnit)
		{
			_code = code;
			_kind = kind;
		}

		internal RubyScriptCode(Func<RubyScope, object, object> target, SourceUnit sourceUnit, TopScopeFactoryKind kind)
			: base(sourceUnit)
		{
			_target = target;
			_kind = kind;
		}

		public override object Run()
		{
			return Run(CreateScope(), false);
		}

		public override object Run(Scope scope)
		{
			return Run(scope, true);
		}

		private object Run(Scope scope, bool bindGlobals)
		{
			RubyContext context = (RubyContext)base.LanguageContext;
			RubyScope rubyScope;
			switch (_kind)
			{
			case TopScopeFactoryKind.Hosted:
				rubyScope = RubyTopLevelScope.CreateHostedTopLevelScope(scope, context, bindGlobals);
				break;
			case TopScopeFactoryKind.Main:
				rubyScope = RubyTopLevelScope.CreateTopLevelScope(scope, context, true);
				break;
			case TopScopeFactoryKind.File:
				rubyScope = RubyTopLevelScope.CreateTopLevelScope(scope, context, false);
				break;
			case TopScopeFactoryKind.WrappedFile:
				rubyScope = RubyTopLevelScope.CreateWrappedTopLevelScope(scope, context);
				break;
			default:
				throw Assert.Unreachable;
			}
			return Target(rubyScope, rubyScope.SelfObject);
		}

		internal static Delegate CompileLambda(LambdaExpression lambda, LanguageContext context)
		{
			return CompileLambda(lambda, context.DomainManager.Configuration.DebugMode, context.Options.NoAdaptiveCompilation, context.Options.CompilationThreshold);
		}

		internal static Delegate CompileLambda(LambdaExpression lambda, bool debugMode, bool noAdaptiveCompilation, int compilationThreshold)
		{
			if (debugMode)
			{
				return CompileDebug(lambda);
			}
			if (noAdaptiveCompilation)
			{
				return lambda.Compile();
			}
			return lambda.LightCompile(compilationThreshold);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Delegate CompileDebug(LambdaExpression lambda)
		{
			if (_HasPdbPermissions)
			{
				try
				{
					return Microsoft.Scripting.Generation2.CompilerHelpers.CompileToMethod(lambda, true);
					//return CompilerHelpers.Compile(lambda, true);

                }
				catch (SecurityException)
				{
					_HasPdbPermissions = false;
				}
			}
            return Microsoft.Scripting.Generation2.CompilerHelpers.CompileToMethod(lambda, false);
            //return CompilerHelpers.Compile(lambda, false);

        }
    }
}

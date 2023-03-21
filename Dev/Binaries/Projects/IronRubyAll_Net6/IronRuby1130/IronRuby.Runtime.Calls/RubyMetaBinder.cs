using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public abstract class RubyMetaBinder : DynamicMetaObjectBinder, ILightCallSiteBinder, IExpressionSerializable
	{
		private RubyContext _context;

		internal RubyContext Context
		{
			get
			{
				return _context;
			}
			set
			{
				_context = value;
			}
		}

		bool ILightCallSiteBinder.AcceptsArgumentArray
		{
			get
			{
				return true;
			}
		}

		public abstract RubyCallSignature Signature { get; }

		protected RubyMetaBinder(RubyContext context)
		{
			_context = context;
		}

		protected abstract bool Build(MetaObjectBuilder metaBuilder, CallArguments args, bool defaultFallback);

		public abstract Expression CreateExpression();

		public sealed override T BindDelegate<T>(CallSite<T> site, object[] args)
		{
			object obj = args[0];
			ArgumentArray argumentArray = obj as ArgumentArray;
			Type typeFromHandle = typeof(T);
			T val;
			if (argumentArray != null)
			{
				obj = argumentArray.GetArgument(0);
			}
			else
			{
				object obj2 = BindPrecompiled(typeFromHandle, args);
				if (obj2 != null)
				{
					val = (T)obj2;
					CacheTarget(val);
					return val;
				}
			}
			RubyContext rubyContext = _context ?? (Signature.HasScope ? ((RubyScope)obj).RubyContext : ((RubyContext)obj));
			if (rubyContext.Options.NoAdaptiveCompilation)
			{
				return null;
			}
			val = this.LightBind<T>(args, rubyContext.Options.CompilationThreshold);
			CacheTarget(val);
			return val;
		}

		protected virtual object BindPrecompiled(Type delegateType, object[] args)
		{
			return null;
		}

		public override DynamicMetaObject Bind(DynamicMetaObject scopeOrContextOrTargetOrArgArray, DynamicMetaObject[] args)
		{
			CallArguments callArguments = new CallArguments(_context, scopeOrContextOrTargetOrArgArray, args, Signature);
			MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(this, args);
			DynamicMetaObject result;
			if (IsForeignMetaObject(callArguments.MetaTarget) && (result = InteropBind(metaObjectBuilder, callArguments)) != null)
			{
				return result;
			}
			Build(metaObjectBuilder, callArguments, true);
			return metaObjectBuilder.CreateMetaObject(this);
		}

		protected virtual DynamicMetaObjectBinder GetInteropBinder(RubyContext context, IList<DynamicMetaObject> args, out MethodInfo postProcessor)
		{
			postProcessor = null;
			return null;
		}

		private DynamicMetaObject InteropBind(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			IList<DynamicMetaObject> list = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, int.MaxValue);
			if (!metaBuilder.Error)
			{
				MethodInfo postProcessor;
				DynamicMetaObjectBinder interopBinder = GetInteropBinder(args.RubyContext, list, out postProcessor);
				if (interopBinder != null)
				{
					DynamicMetaObject metaResult = interopBinder.Bind(args.MetaTarget, ArrayUtils.MakeArray(list));
					metaBuilder.SetMetaResult(metaResult, args);
					Type returnType;
					if (postProcessor != null)
					{
						Type parameterType = postProcessor.GetParameters()[0].ParameterType;
						metaBuilder.Result = Expression.Call(null, postProcessor, Microsoft.Scripting.Ast.Utils.Convert(metaBuilder.Result, parameterType));
						returnType = postProcessor.ReturnType;
					}
					else
					{
						returnType = interopBinder.ReturnType;
					}
					return metaBuilder.CreateMetaObject(interopBinder, returnType);
				}
				return null;
			}
			return metaBuilder.CreateMetaObject(this);
		}

		internal static bool IsForeignMetaObject(DynamicMetaObject metaObject)
		{
			if (!(metaObject.Value is IDynamicMetaObjectProvider) || metaObject is RubyMetaObject)
			{
				return TypeUtils.IsComObjectType(metaObject.LimitType);
			}
			return true;
		}
	}
}

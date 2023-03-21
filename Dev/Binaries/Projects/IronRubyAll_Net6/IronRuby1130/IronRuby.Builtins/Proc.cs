using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public class Proc : IDuplicable, IRubyDynamicMetaObjectProvider, IDynamicMetaObjectProvider
	{
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		public sealed class Subclass : Proc, IRubyObject, IRubyObjectState
		{
			private RubyInstanceData _instanceData;

			private RubyClass _immediateClass;

			public RubyClass ImmediateClass
			{
				get
				{
					return _immediateClass;
				}
				set
				{
					_immediateClass = value;
				}
			}

			public bool IsFrozen
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsFrozen;
					}
					return false;
				}
			}

			public bool IsTainted
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsTainted;
					}
					return false;
				}
				set
				{
					GetInstanceData().IsTainted = value;
				}
			}

			public bool IsUntrusted
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsUntrusted;
					}
					return false;
				}
				set
				{
					GetInstanceData().IsUntrusted = value;
				}
			}

			public RubyInstanceData GetInstanceData()
			{
				return RubyOps.GetInstanceData(ref _instanceData);
			}

			public RubyInstanceData TryGetInstanceData()
			{
				return _instanceData;
			}

			public void Freeze()
			{
				GetInstanceData().Freeze();
			}

			public int BaseGetHashCode()
			{
				return base.GetHashCode();
			}

			public bool BaseEquals(object other)
			{
				return base.Equals(other);
			}

			public string BaseToString()
			{
				return ToString();
			}

			public Subclass(RubyClass rubyClass, Proc proc)
				: base(proc)
			{
				ImmediateClass = rubyClass;
			}

			public override Proc Copy()
			{
				return new Subclass(ImmediateClass.NominalClass, this);
			}
		}

		internal sealed class Meta : RubyMetaObject<Proc>, IConvertibleRubyMetaObject, IConvertibleMetaObject, IInferableInvokable
		{
			public override RubyContext Context
			{
				get
				{
					return base.Value.LocalScope.RubyContext;
				}
			}

			protected override MethodInfo ContextConverter
			{
				get
				{
					return Methods.GetContextFromProc;
				}
			}

			public Meta(System.Linq.Expressions.Expression expression, BindingRestrictions restrictions, Proc value)
				: base(expression, restrictions, value)
			{
			}

			bool IConvertibleMetaObject.CanConvertTo(Type type, bool @explicit)
			{
				return IsConvertibleTo(type, @explicit).IsConvertible;
			}

			public Convertibility IsConvertibleTo(Type type, bool @explicit)
			{
				if (!typeof(Delegate).IsAssignableFrom(type))
				{
					return Convertibility.NotConvertible;
				}
				if (@explicit)
				{
					return Convertibility.AlwaysConvertible;
				}
				if (!base.HasValue)
				{
					return Convertibility.NotConvertible;
				}
				MethodInfo method = type.GetMethod("Invoke");
				if (method == null)
				{
					return Convertibility.NotConvertible;
				}
				int num = method.GetParameters().Length;
				return new Convertibility(num == base.Value.Dispatcher.ParameterCount || (num > base.Value.Dispatcher.ParameterCount && base.Value.Dispatcher.HasUnsplatParameter), System.Linq.Expressions.Expression.Equal(Methods.GetProcArity.OpCall(Microsoft.Scripting.Ast.Utils.Convert(base.Expression, typeof(Proc))), System.Linq.Expressions.Expression.Constant(base.Value.Dispatcher.Arity)));
			}

			public override DynamicMetaObject BindConvert(ConvertBinder binder)
			{
				return InteropBinder.TryBindCovertToDelegate(this, binder, Methods.CreateDelegateFromProc) ?? base.BindConvert(binder);
			}

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			{
				return InteropBinder.Invoke.Bind(binder, this, args, base.Value.BuildInvoke);
			}

			InferenceResult IInferableInvokable.GetInferredType(Type delegateType, Type parameterType)
			{
				return new InferenceResult(typeof(object), BindingRestrictions.GetTypeRestriction(base.Expression, typeof(Proc)));
			}
		}

		private readonly object _self;

		private readonly RubyScope _scope;

		private readonly BlockDispatcher _dispatcher;

		private ProcKind _kind;

		private RubyLambdaMethodInfo _method;

		internal RuntimeFlowControl Converter { get; set; }

		public ProcKind Kind
		{
			get
			{
				return _kind;
			}
			internal set
			{
				_kind = value;
			}
		}

		internal RubyLambdaMethodInfo Method
		{
			get
			{
				return _method;
			}
		}

		public BlockDispatcher Dispatcher
		{
			get
			{
				return _dispatcher;
			}
		}

		public object Self
		{
			get
			{
				return _self;
			}
		}

		public RubyScope LocalScope
		{
			get
			{
				return _scope;
			}
		}

		public string SourcePath
		{
			get
			{
				return _dispatcher.SourcePath;
			}
		}

		public int SourceLine
		{
			get
			{
				return _dispatcher.SourceLine;
			}
		}

		internal Proc(ProcKind kind, object self, RubyScope scope, BlockDispatcher dispatcher)
		{
			_kind = kind;
			_self = self;
			_scope = scope;
			_dispatcher = dispatcher;
		}

		protected Proc(Proc proc)
			: this(proc.Kind, proc.Self, proc.LocalScope, proc.Dispatcher)
		{
			Converter = proc.Converter;
		}

		public Proc Create(Proc proc)
		{
			return new Proc(proc);
		}

		public Proc ToLambda(RubyLambdaMethodInfo method)
		{
			Proc proc = new Proc(this);
			proc.Kind = ProcKind.Lambda;
			proc._method = method;
			return proc;
		}

		public virtual Proc Copy()
		{
			return new Proc(this);
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			Proc proc = Copy();
			context.CopyInstanceData(this, proc, copySingletonMembers);
			return proc;
		}

		public static RubyLambdaMethodInfo ToLambdaMethodInfo(Proc block, string definitionName, RubyMethodVisibility visibility, RubyModule owner)
		{
			return new RubyLambdaMethodInfo(block, definitionName, (RubyMemberFlags)visibility, owner);
		}

		internal void BuildInvoke(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			System.Linq.Expressions.Expression expression = Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(Proc));
			metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
			BuildCall(metaBuilder, expression, Methods.GetProcSelf.OpCall(expression), args);
		}

		internal static void BuildCall(MetaObjectBuilder metaBuilder, System.Linq.Expressions.Expression procExpression, System.Linq.Expressions.Expression selfExpression, CallArguments args)
		{
			ParameterExpression temporary = metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
			metaBuilder.Result = System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(temporary, Methods.CreateBfcForProcCall.OpCall(Microsoft.Scripting.Ast.Utils.Convert(procExpression, typeof(Proc)))), Methods.MethodProcCall.OpCall(temporary, AstFactory.YieldExpression(args.RubyContext, args.GetSimpleArgumentExpressions(), args.GetSplattedArgumentExpression(), args.GetRhsArgumentExpression(), args.GetBlockExpression(), temporary, selfExpression)));
		}

		public object Call(Proc procArg)
		{
			BlockParam blockParam = RubyOps.CreateBfcForProcCall(this);
			return RubyOps.MethodProcCall(blockParam, RubyOps.Yield0(procArg, _self, blockParam));
		}

		public object Call(Proc procArg, object arg1)
		{
			BlockParam blockParam = RubyOps.CreateBfcForProcCall(this);
			object returnValue = ((_kind == ProcKind.Lambda) ? RubyOps.YieldNoAutoSplat1(arg1, procArg, _self, blockParam) : RubyOps.Yield1(arg1, procArg, _self, blockParam));
			return RubyOps.MethodProcCall(blockParam, returnValue);
		}

		public object Call(Proc procArg, object arg1, object arg2)
		{
			BlockParam blockParam = RubyOps.CreateBfcForProcCall(this);
			return RubyOps.MethodProcCall(blockParam, RubyOps.Yield2(arg1, arg2, procArg, _self, blockParam));
		}

		public object Call(Proc procArg, object arg1, object arg2, object arg3)
		{
			BlockParam blockParam = RubyOps.CreateBfcForProcCall(this);
			return RubyOps.MethodProcCall(blockParam, RubyOps.Yield3(arg1, arg2, arg3, procArg, _self, blockParam));
		}

		public object Call(Proc procArg, object arg1, object arg2, object arg3, object arg4)
		{
			BlockParam blockParam = RubyOps.CreateBfcForProcCall(this);
			return RubyOps.MethodProcCall(blockParam, RubyOps.Yield4(arg1, arg2, arg3, arg4, procArg, _self, blockParam));
		}

		public object Call(Proc procArg, params object[] args)
		{
			switch (args.Length)
			{
			case 0:
				return Call(procArg);
			case 1:
				return Call(procArg, args[0]);
			case 2:
				return Call(procArg, args[0], args[1]);
			case 3:
				return Call(procArg, args[0], args[1], args[2]);
			case 4:
				return Call(procArg, args[0], args[1], args[2], args[3]);
			default:
			{
				BlockParam blockParam = RubyOps.CreateBfcForProcCall(this);
				return RubyOps.MethodProcCall(blockParam, RubyOps.YieldN(args, procArg, _self, blockParam));
			}
			}
		}

		public object CallN(Proc procArg, object[] args)
		{
			BlockParam blockParam = RubyOps.CreateBfcForProcCall(this);
			return RubyOps.MethodProcCall(blockParam, RubyOps.YieldN(args, procArg, _self, blockParam));
		}

		public static Proc Create(RubyContext context, Func<BlockParam, object, object, object> clrMethod)
		{
			return Create(context, 1, BlockDispatcher.MakeAttributes(BlockSignatureAttributes.None, -1), clrMethod);
		}

		public static Proc CreateSimple(RubyContext context, Func<BlockParam, object, object, object> clrMethod)
		{
			return Create(context, 1, BlockSignatureAttributes.None, clrMethod);
		}

		public static Proc Create(RubyContext context, Func<BlockParam, object, object, object, object> clrMethod)
		{
			return Create(context, 2, BlockSignatureAttributes.None, clrMethod);
		}

		public static Proc Create(RubyContext context, Func<BlockParam, object, object, object, object, object> clrMethod)
		{
			return Create(context, 3, BlockSignatureAttributes.None, clrMethod);
		}

		public static Proc Create(RubyContext context, int parameterCount, Func<BlockParam, object, object[], RubyArray, object> clrMethod)
		{
			return Create(context, parameterCount, BlockSignatureAttributes.HasUnsplatParameter, clrMethod);
		}

		public static Proc Create(RubyContext context, int parameterCount, BlockSignatureAttributes signatureAttributes, Delegate clrMethod)
		{
			return new Proc(ProcKind.Block, null, context.EmptyScope, BlockDispatcher.Create(parameterCount, signatureAttributes, null, 0).SetMethod(clrMethod));
		}

		public static Proc CreateMethodInvoker(RubyScope scope, string methodName)
		{
			ContractUtils.RequiresNotNull(scope, "scope");
			CallSite<Func<CallSite, object, object, object, object>> site = CallSite<Func<CallSite, object, object, object, object>>.Create(RubyCallAction.Make(scope.RubyContext, methodName, new RubyCallSignature(0, (RubyCallFlags)3)));
			Func<BlockParam, object, object[], RubyArray, object> method = delegate(BlockParam blockParam, object self, object[] args, RubyArray unsplat)
			{
				if (unsplat.Count == 0)
				{
					throw RubyExceptions.CreateArgumentError("no receiver given");
				}
				object arg = unsplat[0];
				unsplat.RemoveAt(0);
				return site.Target(site, scope, arg, unsplat);
			};
			BlockDispatcherUnsplatN blockDispatcherUnsplatN = new BlockDispatcherUnsplatN(0, BlockDispatcher.MakeAttributes(BlockSignatureAttributes.HasUnsplatParameter, -1), null, 0);
			blockDispatcherUnsplatN.SetMethod(method);
			return new Proc(ProcKind.Proc, scope.SelfObject, scope, blockDispatcherUnsplatN);
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}:{2}", _kind, SourcePath ?? "(unknown)", SourceLine);
		}

		public DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
		{
			return new Meta(parameter, BindingRestrictions.Empty, this);
		}
	}
}

using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public sealed class BlockParam : IRubyDynamicMetaObjectProvider, IDynamicMetaObjectProvider
	{
		internal sealed class Meta : RubyMetaObject<BlockParam>
		{
			public override RubyContext Context
			{
				get
				{
					return base.Value.RubyContext;
				}
			}

			protected override MethodInfo ContextConverter
			{
				get
				{
					return Methods.GetContextFromBlockParam;
				}
			}

			public Meta(System.Linq.Expressions.Expression expression, BindingRestrictions restrictions, BlockParam value)
				: base(expression, restrictions, value)
			{
				ContractUtils.RequiresNotNull(value, "value");
			}

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			{
				return InteropBinder.Invoke.Bind(binder, this, args, base.Value.BuildInvoke);
			}
		}

		private readonly Proc _proc;

		private readonly BlockCallerKind _callerKind;

		private RubyModule _methodLookupModule;

		internal readonly bool _isLibProcConverter;

		private BlockReturnReason _returnReason;

		private RuntimeFlowControl _targetFrame;

		private ProcKind _sourceProcKind;

		internal BlockCallerKind CallerKind
		{
			get
			{
				return _callerKind;
			}
		}

		internal ProcKind SourceProcKind
		{
			get
			{
				return _sourceProcKind;
			}
		}

		internal BlockReturnReason ReturnReason
		{
			get
			{
				return _returnReason;
			}
			set
			{
				_returnReason = value;
			}
		}

		internal RuntimeFlowControl TargetFrame
		{
			get
			{
				return _targetFrame;
			}
		}

		internal RubyModule MethodLookupModule
		{
			get
			{
				return _methodLookupModule;
			}
			set
			{
				_methodLookupModule = value;
			}
		}

		internal bool IsLibProcConverter
		{
			get
			{
				return _isLibProcConverter;
			}
		}

		public Proc Proc
		{
			get
			{
				return _proc;
			}
		}

		public object Self
		{
			get
			{
				return _proc.Self;
			}
		}

		public RubyContext RubyContext
		{
			get
			{
				return _proc.LocalScope.RubyContext;
			}
		}

		public bool IsMethod
		{
			get
			{
				return _proc.Method != null;
			}
		}

		internal static PropertyInfo SelfProperty
		{
			get
			{
				return typeof(BlockParam).GetProperty("Self");
			}
		}

		internal BlockParam(Proc proc, BlockCallerKind callerKind, bool isLibProcConverter)
		{
			_callerKind = callerKind;
			_proc = proc;
			_isLibProcConverter = isLibProcConverter;
		}

		internal void SetFlowControl(BlockReturnReason reason, RuntimeFlowControl targetFrame, ProcKind sourceProcKind)
		{
			_returnReason = reason;
			_targetFrame = targetFrame;
			_sourceProcKind = sourceProcKind;
		}

		internal object GetUnwinderResult(EvalUnwinder unwinder)
		{
			SetFlowControl(unwinder.Reason, unwinder.TargetFrame, unwinder.SourceProcKind);
			return unwinder.ReturnValue;
		}

		internal bool BlockJumped(object returnValue)
		{
			return RubyOps.MethodYieldRfc(_isLibProcConverter ? _proc.Converter : null, this, returnValue);
		}

		public bool Returning(object returnValue, out object result)
		{
			if (ReturnReason == BlockReturnReason.Return)
			{
				result = ((BlockReturnResult)returnValue).ReturnValue;
				return true;
			}
			result = null;
			return false;
		}

		public object PropagateFlow(BlockParam yieldedBlock, object returnValue)
		{
			if (yieldedBlock.ReturnReason == BlockReturnReason.Break)
			{
				return Break(returnValue);
			}
			_returnReason = yieldedBlock.ReturnReason;
			return returnValue;
		}

		public object Break(object returnValue)
		{
			SetFlowControl(BlockReturnReason.Break, _proc.Converter, _proc.Kind);
			return returnValue;
		}

		public bool Yield(out object blockResult)
		{
			return BlockJumped(blockResult = RubyOps.Yield0(null, Self, this));
		}

		public bool Yield(object arg1, out object blockResult)
		{
			return BlockJumped(blockResult = RubyOps.Yield1(arg1, null, Self, this));
		}

		public bool Yield(object arg1, object arg2, out object blockResult)
		{
			return BlockJumped(blockResult = RubyOps.Yield2(arg1, arg2, null, Self, this));
		}

		public bool Yield(object arg1, object arg2, object arg3, out object blockResult)
		{
			return BlockJumped(blockResult = RubyOps.Yield3(arg1, arg2, arg3, null, Self, this));
		}

		public bool Yield(object arg1, object arg2, object arg3, object arg4, out object blockResult)
		{
			return BlockJumped(blockResult = RubyOps.Yield4(arg1, arg2, arg3, arg4, null, Self, this));
		}

		public bool Yield(object[] args, out object blockResult)
		{
			return BlockJumped(blockResult = RubyOps.Yield(args, null, Self, this));
		}

		public bool YieldSplat(IList args, out object blockResult)
		{
			return BlockJumped(blockResult = RubyOps.YieldSplat0(args, null, Self, this));
		}

		internal void BuildInvoke(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			System.Linq.Expressions.Expression expression = Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(BlockParam));
			metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
			metaBuilder.Result = AstFactory.YieldExpression(args.RubyContext, args.GetSimpleArgumentExpressions(), args.GetSplattedArgumentExpression(), args.GetRhsArgumentExpression(), args.GetBlockExpression(), expression, System.Linq.Expressions.Expression.Property(expression, SelfProperty));
		}

		public DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
		{
			return new Meta(parameter, BindingRestrictions.Empty, this);
		}
	}
}

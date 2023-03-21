using System.Collections;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public sealed class CallArguments
	{
		private readonly bool _hasScopeOrContextArg;

		private readonly DynamicMetaObject _context;

		private DynamicMetaObject _scope;

		private DynamicMetaObject _target;

		private RubyClass _targetClass;

		private DynamicMetaObject[] _args;

		private bool _copyArgsOnWrite;

		private RubyCallSignature _signature;

		public RubyCallSignature Signature
		{
			get
			{
				return _signature;
			}
		}

		public int CallSiteArgumentCount
		{
			get
			{
				return (_hasScopeOrContextArg ? 1 : 0) + ExplicitArgumentCount;
			}
		}

		public int ExplicitArgumentCount
		{
			get
			{
				return ((_target != null) ? 1 : 0) + _args.Length;
			}
		}

		private int FirstArgumentIndex
		{
			get
			{
				if (_target != null)
				{
					return 0;
				}
				return 1;
			}
		}

		public int SimpleArgumentCount
		{
			get
			{
				return _args.Length - FirstArgumentIndex - (_signature.HasBlock ? 1 : 0) - (_signature.HasSplattedArgument ? 1 : 0) - (_signature.HasRhsArgument ? 1 : 0);
			}
		}

		public DynamicMetaObject MetaScope
		{
			get
			{
				if (_scope == null)
				{
					RubyScope emptyScope = ((RubyContext)_context.Value).EmptyScope;
					_scope = new DynamicMetaObject(Expression.Constant(emptyScope, typeof(RubyScope)), BindingRestrictions.Empty, emptyScope);
				}
				return _scope;
			}
		}

		public DynamicMetaObject MetaContext
		{
			get
			{
				return _context;
			}
		}

		public RubyScope Scope
		{
			get
			{
				return (RubyScope)MetaScope.Value;
			}
		}

		public RubyContext RubyContext
		{
			get
			{
				return (RubyContext)MetaContext.Value;
			}
		}

		public DynamicMetaObject MetaTarget
		{
			get
			{
				return _target ?? _args[0];
			}
		}

		public Expression TargetExpression
		{
			get
			{
				return MetaTarget.Expression;
			}
		}

		public RubyClass TargetClass
		{
			get
			{
				if (_targetClass == null)
				{
					_targetClass = RubyContext.GetImmediateClassOf(Target);
				}
				return _targetClass;
			}
		}

		public object Target
		{
			get
			{
				return MetaTarget.Value;
			}
		}

		public Proc GetBlock()
		{
			return (Proc)_args[GetBlockIndex()].Value;
		}

		public IList GetSplattedArgument()
		{
			return (IList)_args[GetSplattedArgumentIndex()].Value;
		}

		public object GetRhsArgument()
		{
			return _args[GetRhsArgumentIndex()].Value;
		}

		public Expression GetBlockExpression()
		{
			if (!_signature.HasBlock)
			{
				return null;
			}
			return _args[GetBlockIndex()].Expression;
		}

		public DynamicMetaObject GetMetaBlock()
		{
			if (!_signature.HasBlock)
			{
				return null;
			}
			return _args[GetBlockIndex()];
		}

		public DynamicMetaObject GetSplattedMetaArgument()
		{
			if (!_signature.HasSplattedArgument)
			{
				return null;
			}
			return _args[GetSplattedArgumentIndex()];
		}

		public Expression GetSplattedArgumentExpression()
		{
			if (!_signature.HasSplattedArgument)
			{
				return null;
			}
			return _args[GetSplattedArgumentIndex()].Expression;
		}

		public DynamicMetaObject GetRhsMetaArgument()
		{
			if (!_signature.HasRhsArgument)
			{
				return null;
			}
			return _args[GetRhsArgumentIndex()];
		}

		public Expression GetRhsArgumentExpression()
		{
			if (!_signature.HasRhsArgument)
			{
				return null;
			}
			return _args[GetRhsArgumentIndex()].Expression;
		}

		public ReadOnlyCollectionBuilder<Expression> GetSimpleArgumentExpressions()
		{
			int simpleArgumentCount = SimpleArgumentCount;
			ReadOnlyCollectionBuilder<Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<Expression>(simpleArgumentCount);
			int i = 0;
			int num = GetSimpleArgumentIndex(0);
			for (; i < simpleArgumentCount; i++)
			{
				readOnlyCollectionBuilder.Add(_args[num].Expression);
				num++;
			}
			return readOnlyCollectionBuilder;
		}

		internal ReadOnlyCollection<Expression> GetCallSiteArguments(Expression targetExpression)
		{
			ReadOnlyCollectionBuilder<Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<Expression>(CallSiteArgumentCount);
			if (_hasScopeOrContextArg)
			{
				readOnlyCollectionBuilder.Add(_signature.HasScope ? MetaScope.Expression : MetaContext.Expression);
			}
			readOnlyCollectionBuilder.Add(targetExpression);
			for (int i = FirstArgumentIndex; i < _args.Length; i++)
			{
				readOnlyCollectionBuilder.Add(_args[i].Expression);
			}
			return readOnlyCollectionBuilder.ToReadOnlyCollection();
		}

		private int GetSimpleArgumentIndex(int i)
		{
			return FirstArgumentIndex + (_signature.HasBlock ? 1 : 0) + i;
		}

		internal object GetSimpleArgument(int i)
		{
			return GetSimpleMetaArgument(i).Value;
		}

		internal Expression GetSimpleArgumentExpression(int i)
		{
			return GetSimpleMetaArgument(i).Expression;
		}

		internal DynamicMetaObject GetSimpleMetaArgument(int i)
		{
			return _args[GetSimpleArgumentIndex(i)];
		}

		internal int GetBlockIndex()
		{
			return FirstArgumentIndex;
		}

		internal int GetSplattedArgumentIndex()
		{
			return _args.Length - ((!_signature.HasRhsArgument) ? 1 : 2);
		}

		internal int GetRhsArgumentIndex()
		{
			return _args.Length - 1;
		}

		internal CallArguments(RubyContext context, DynamicMetaObject scopeOrContextOrTargetOrArgArray, DynamicMetaObject[] args, RubyCallSignature signature)
		{
			ArgumentArray argumentArray = scopeOrContextOrTargetOrArgArray.Value as ArgumentArray;
			if (argumentArray != null)
			{
				args = new DynamicMetaObject[argumentArray.Count - 1];
				for (int i = 0; i < args.Length; i++)
				{
					args[i] = argumentArray.GetMetaObject(scopeOrContextOrTargetOrArgArray.Expression, 1 + i);
				}
				scopeOrContextOrTargetOrArgArray = argumentArray.GetMetaObject(scopeOrContextOrTargetOrArgArray.Expression, 0);
			}
			if (context != null)
			{
				_context = new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Constant(context), BindingRestrictions.Empty, context);
				if (signature.HasScope)
				{
					_scope = scopeOrContextOrTargetOrArgArray;
					_hasScopeOrContextArg = true;
				}
				else
				{
					_target = scopeOrContextOrTargetOrArgArray;
				}
			}
			else if (signature.HasScope)
			{
				_context = new DynamicMetaObject(Methods.GetContextFromScope.OpCall(scopeOrContextOrTargetOrArgArray.Expression), BindingRestrictions.Empty, ((RubyScope)scopeOrContextOrTargetOrArgArray.Value).RubyContext);
				_scope = scopeOrContextOrTargetOrArgArray;
				_hasScopeOrContextArg = true;
				_target = null;
			}
			else
			{
				_context = scopeOrContextOrTargetOrArgArray;
				_hasScopeOrContextArg = true;
				_target = null;
			}
			_args = args;
			_copyArgsOnWrite = true;
			_signature = signature;
		}

		internal CallArguments(DynamicMetaObject context, DynamicMetaObject target, DynamicMetaObject[] args, RubyCallSignature signature)
		{
			_target = target;
			_context = context;
			_args = args;
			_copyArgsOnWrite = true;
			_signature = signature;
		}

		internal CallArguments(RubyContext context, DynamicMetaObject target, DynamicMetaObject[] args, CallInfo callInfo)
			: this(new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Constant(context), BindingRestrictions.Empty, context), target, args, RubyCallSignature.Interop(callInfo.ArgumentCount))
		{
		}

		public void InsertSimple(int index, DynamicMetaObject arg)
		{
			index = GetSimpleArgumentIndex(index);
			_args = ArrayUtils.InsertAt(_args, index, arg);
			_signature = new RubyCallSignature(_signature.ArgumentCount + 1, _signature.Flags);
		}

		internal void InsertMethodName(string methodName)
		{
			object value = RubyContext.EncodeIdentifier(methodName);
			InsertSimple(0, new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Constant(value), BindingRestrictions.Empty, value));
		}

		public void SetSimpleArgument(int index, DynamicMetaObject arg)
		{
			SetArgument(GetSimpleArgumentIndex(index), arg);
		}

		private void SetArgument(int index, DynamicMetaObject arg)
		{
			if (_copyArgsOnWrite)
			{
				_args = ArrayUtils.Copy(_args);
				_copyArgsOnWrite = false;
			}
			_args[index] = arg;
		}

		public void SetTarget(Expression expression, object value)
		{
			DynamicMetaObject dynamicMetaObject = new DynamicMetaObject(expression, BindingRestrictions.Empty, value);
			if (_target == null)
			{
				if (_copyArgsOnWrite)
				{
					_args = ArrayUtils.RemoveFirst(_args);
					_copyArgsOnWrite = false;
					_target = dynamicMetaObject;
				}
				else
				{
					_args[0] = dynamicMetaObject;
				}
			}
			else
			{
				_target = dynamicMetaObject;
			}
			_targetClass = null;
		}
	}
}

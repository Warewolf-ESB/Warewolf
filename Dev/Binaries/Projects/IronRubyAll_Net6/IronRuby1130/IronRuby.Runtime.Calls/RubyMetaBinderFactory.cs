using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime.Calls
{
	internal sealed class RubyMetaBinderFactory
	{
		private static RubyMetaBinderFactory _Shared;

		private readonly RubyContext _context;

		private readonly Dictionary<Key<string, RubyCallSignature>, RubyCallAction> _callActions;

		private readonly Dictionary<Key<int, RubyCallSignature>, SuperCallAction> _superCallActions;

		private readonly Dictionary<Type, RubyConversionAction> _conversionActions;

		private readonly Dictionary<CompositeConversion, CompositeConversionAction> _compositeConversionActions;

		private readonly Dictionary<Type, GenericConversionAction> _genericConversionActions;

		private Dictionary<CallInfo, InteropBinder.CreateInstance> _interopCreateInstance;

		private Dictionary<CallInfo, InteropBinder.Return> _interopReturn;

		private Dictionary<CallInfo, InteropBinder.Invoke> _interopInvoke;

		private Dictionary<Key<string, CallInfo>, InteropBinder.InvokeMember> _interopInvokeMember;

		private Dictionary<string, InteropBinder.GetMember> _interopGetMember;

		private Dictionary<string, InteropBinder.TryGetMemberExact> _interopTryGetMemberExact;

		private Dictionary<string, InteropBinder.SetMember> _interopSetMember;

		private Dictionary<string, InteropBinder.SetMemberExact> _interopSetMemberExact;

		private Dictionary<CallInfo, InteropBinder.GetIndex> _interopGetIndex;

		private Dictionary<CallInfo, InteropBinder.SetIndex> _interopSetIndex;

		private Dictionary<Key<string, CallInfo>, InteropBinder.SetIndexedProperty> _interopSetIndexedProperty;

		private Dictionary<ExpressionType, DynamicMetaObjectBinder> _interopOperation;

		private Dictionary<Key<Type, bool>, InteropBinder.Convert> _interopConvert;

		private InteropBinder.Splat _interopSplat;

		internal static RubyMetaBinderFactory Shared
		{
			get
			{
				if (_Shared == null)
				{
					Interlocked.CompareExchange(ref _Shared, new RubyMetaBinderFactory(null), null);
				}
				return _Shared;
			}
		}

		internal RubyMetaBinderFactory(RubyContext context)
		{
			_context = context;
			_callActions = new Dictionary<Key<string, RubyCallSignature>, RubyCallAction>();
			_superCallActions = new Dictionary<Key<int, RubyCallSignature>, SuperCallAction>();
			_conversionActions = new Dictionary<Type, RubyConversionAction>();
			_compositeConversionActions = new Dictionary<CompositeConversion, CompositeConversionAction>();
			_genericConversionActions = new Dictionary<Type, GenericConversionAction>();
		}

		public RubyCallAction Call(string methodName, RubyCallSignature signature)
		{
			Key<string, RubyCallSignature> key = Key.Create(methodName, signature);
			lock (_callActions)
			{
				RubyCallAction value;
				if (!_callActions.TryGetValue(key, out value))
				{
					_callActions.Add(key, value = new RubyCallAction(_context, methodName, signature));
				}
				return value;
			}
		}

		public SuperCallAction SuperCall(int lexicalScopeId, RubyCallSignature signature)
		{
			Key<int, RubyCallSignature> key = Key.Create(lexicalScopeId, signature);
			lock (_superCallActions)
			{
				SuperCallAction value;
				if (!_superCallActions.TryGetValue(key, out value))
				{
					_superCallActions.Add(key, value = new SuperCallAction(_context, signature, lexicalScopeId));
				}
				return value;
			}
		}

		public TAction Conversion<TAction>() where TAction : RubyConversionAction, new()
		{
			Type typeFromHandle = typeof(TAction);
			lock (_conversionActions)
			{
				RubyConversionAction value;
				if (!_conversionActions.TryGetValue(typeFromHandle, out value))
				{
					_conversionActions.Add(typeFromHandle, value = new TAction
					{
						Context = _context
					});
				}
				return (TAction)value;
			}
		}

		public CompositeConversionAction CompositeConversion(CompositeConversion conversion)
		{
			lock (_conversionActions)
			{
				CompositeConversionAction value;
				if (!_compositeConversionActions.TryGetValue(conversion, out value))
				{
					_compositeConversionActions.Add(conversion, value = CompositeConversionAction.Make(_context, conversion));
				}
				return value;
			}
		}

		public GenericConversionAction GenericConversionAction(Type type)
		{
			lock (_conversionActions)
			{
				GenericConversionAction value;
				if (!_genericConversionActions.TryGetValue(type, out value))
				{
					_genericConversionActions.Add(type, value = new GenericConversionAction(_context, type));
				}
				return value;
			}
		}

		public InteropBinder.CreateInstance InteropCreateInstance(CallInfo callInfo)
		{
			if (_interopCreateInstance == null)
			{
				Interlocked.CompareExchange(ref _interopCreateInstance, new Dictionary<CallInfo, InteropBinder.CreateInstance>(), null);
			}
			lock (_interopCreateInstance)
			{
				InteropBinder.CreateInstance value;
				if (!_interopCreateInstance.TryGetValue(callInfo, out value))
				{
					_interopCreateInstance.Add(callInfo, value = new InteropBinder.CreateInstance(_context, callInfo));
				}
				return value;
			}
		}

		public InteropBinder.Return InteropReturn(CallInfo callInfo)
		{
			if (_interopReturn == null)
			{
				Interlocked.CompareExchange(ref _interopReturn, new Dictionary<CallInfo, InteropBinder.Return>(), null);
			}
			lock (_interopReturn)
			{
				InteropBinder.Return value;
				if (!_interopReturn.TryGetValue(callInfo, out value))
				{
					_interopReturn.Add(callInfo, value = new InteropBinder.Return(_context, callInfo));
				}
				return value;
			}
		}

		public InteropBinder.Invoke InteropInvoke(CallInfo callInfo)
		{
			if (_interopInvoke == null)
			{
				Interlocked.CompareExchange(ref _interopInvoke, new Dictionary<CallInfo, InteropBinder.Invoke>(), null);
			}
			lock (_interopInvoke)
			{
				InteropBinder.Invoke value;
				if (!_interopInvoke.TryGetValue(callInfo, out value))
				{
					_interopInvoke.Add(callInfo, value = new InteropBinder.Invoke(_context, callInfo));
				}
				return value;
			}
		}

		public InteropBinder.InvokeMember InteropInvokeMember(string name, CallInfo callInfo)
		{
			if (_interopInvokeMember == null)
			{
				Interlocked.CompareExchange(ref _interopInvokeMember, new Dictionary<Key<string, CallInfo>, InteropBinder.InvokeMember>(), null);
			}
			Key<string, CallInfo> key = Key.Create(name, callInfo);
			lock (_interopInvokeMember)
			{
				InteropBinder.InvokeMember value;
				if (!_interopInvokeMember.TryGetValue(key, out value))
				{
					_interopInvokeMember.Add(key, value = new InteropBinder.InvokeMember(_context, name, callInfo, null));
				}
				return value;
			}
		}

		public InteropBinder.GetMember InteropGetMember(string name)
		{
			if (_interopGetMember == null)
			{
				Interlocked.CompareExchange(ref _interopGetMember, new Dictionary<string, InteropBinder.GetMember>(), null);
			}
			lock (_interopGetMember)
			{
				InteropBinder.GetMember value;
				if (!_interopGetMember.TryGetValue(name, out value))
				{
					_interopGetMember.Add(name, value = new InteropBinder.GetMember(_context, name, null));
				}
				return value;
			}
		}

		public InteropBinder.TryGetMemberExact InteropTryGetMemberExact(string name)
		{
			if (_interopTryGetMemberExact == null)
			{
				Interlocked.CompareExchange(ref _interopTryGetMemberExact, new Dictionary<string, InteropBinder.TryGetMemberExact>(), null);
			}
			lock (_interopTryGetMemberExact)
			{
				InteropBinder.TryGetMemberExact value;
				if (!_interopTryGetMemberExact.TryGetValue(name, out value))
				{
					_interopTryGetMemberExact.Add(name, value = new InteropBinder.TryGetMemberExact(_context, name));
				}
				return value;
			}
		}

		public InteropBinder.SetMember InteropSetMember(string name)
		{
			if (_interopSetMember == null)
			{
				Interlocked.CompareExchange(ref _interopSetMember, new Dictionary<string, InteropBinder.SetMember>(), null);
			}
			lock (_interopSetMember)
			{
				InteropBinder.SetMember value;
				if (!_interopSetMember.TryGetValue(name, out value))
				{
					_interopSetMember.Add(name, value = new InteropBinder.SetMember(_context, name));
				}
				return value;
			}
		}

		public InteropBinder.SetMemberExact InteropSetMemberExact(string name)
		{
			if (_interopSetMemberExact == null)
			{
				Interlocked.CompareExchange(ref _interopSetMemberExact, new Dictionary<string, InteropBinder.SetMemberExact>(), null);
			}
			lock (_interopSetMemberExact)
			{
				InteropBinder.SetMemberExact value;
				if (!_interopSetMemberExact.TryGetValue(name, out value))
				{
					_interopSetMemberExact.Add(name, value = new InteropBinder.SetMemberExact(_context, name));
				}
				return value;
			}
		}

		public InteropBinder.GetIndex InteropGetIndex(CallInfo callInfo)
		{
			if (_interopGetIndex == null)
			{
				Interlocked.CompareExchange(ref _interopGetIndex, new Dictionary<CallInfo, InteropBinder.GetIndex>(), null);
			}
			lock (_interopGetIndex)
			{
				InteropBinder.GetIndex value;
				if (!_interopGetIndex.TryGetValue(callInfo, out value))
				{
					_interopGetIndex.Add(callInfo, value = new InteropBinder.GetIndex(_context, callInfo));
				}
				return value;
			}
		}

		public InteropBinder.SetIndex InteropSetIndex(CallInfo callInfo)
		{
			if (_interopSetIndex == null)
			{
				Interlocked.CompareExchange(ref _interopSetIndex, new Dictionary<CallInfo, InteropBinder.SetIndex>(), null);
			}
			lock (_interopSetIndex)
			{
				InteropBinder.SetIndex value;
				if (!_interopSetIndex.TryGetValue(callInfo, out value))
				{
					_interopSetIndex.Add(callInfo, value = new InteropBinder.SetIndex(_context, callInfo));
				}
				return value;
			}
		}

		public InteropBinder.SetIndexedProperty InteropSetIndexedProperty(string name, CallInfo callInfo)
		{
			if (_interopSetIndexedProperty == null)
			{
				Interlocked.CompareExchange(ref _interopSetIndexedProperty, new Dictionary<Key<string, CallInfo>, InteropBinder.SetIndexedProperty>(), null);
			}
			Key<string, CallInfo> key = Key.Create(name, callInfo);
			lock (_interopSetIndexedProperty)
			{
				InteropBinder.SetIndexedProperty value;
				if (!_interopSetIndexedProperty.TryGetValue(key, out value))
				{
					_interopSetIndexedProperty.Add(key, value = new InteropBinder.SetIndexedProperty(_context, name, callInfo));
				}
				return value;
			}
		}

		public InteropBinder.BinaryOperation InteropBinaryOperation(ExpressionType op)
		{
			if (_interopOperation == null)
			{
				Interlocked.CompareExchange(ref _interopOperation, new Dictionary<ExpressionType, DynamicMetaObjectBinder>(), null);
			}
			lock (_interopOperation)
			{
				DynamicMetaObjectBinder value;
				if (!_interopOperation.TryGetValue(op, out value))
				{
					_interopOperation.Add(op, value = new InteropBinder.BinaryOperation(_context, op));
				}
				return (InteropBinder.BinaryOperation)value;
			}
		}

		public InteropBinder.UnaryOperation InteropUnaryOperation(ExpressionType op)
		{
			if (_interopOperation == null)
			{
				Interlocked.CompareExchange(ref _interopOperation, new Dictionary<ExpressionType, DynamicMetaObjectBinder>(), null);
			}
			lock (_interopOperation)
			{
				DynamicMetaObjectBinder value;
				if (!_interopOperation.TryGetValue(op, out value))
				{
					_interopOperation.Add(op, value = new InteropBinder.UnaryOperation(_context, op));
				}
				return (InteropBinder.UnaryOperation)value;
			}
		}

		public InteropBinder.Convert InteropConvert(Type type, bool isExplicit)
		{
			if (_interopConvert == null)
			{
				Interlocked.CompareExchange(ref _interopConvert, new Dictionary<Key<Type, bool>, InteropBinder.Convert>(), null);
			}
			Key<Type, bool> key = Key.Create(type, isExplicit);
			lock (_interopConvert)
			{
				InteropBinder.Convert value;
				if (!_interopConvert.TryGetValue(key, out value))
				{
					_interopConvert.Add(key, value = new InteropBinder.Convert(_context, type, isExplicit));
				}
				return value;
			}
		}

		public InteropBinder.Splat InteropSplat()
		{
			if (_interopSplat == null)
			{
				_interopSplat = new InteropBinder.Splat(_context);
			}
			return _interopSplat;
		}
	}
}

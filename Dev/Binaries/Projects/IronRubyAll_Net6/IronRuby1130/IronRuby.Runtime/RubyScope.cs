using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	[DebuggerTypeProxy(typeof(DebugView))]
	public abstract class RubyScope : RuntimeFlowControl
	{
		internal sealed class DebugView
		{
			[DebuggerDisplay("{_value}", Name = "{_name,nq}", Type = "{_valueClassName,nq}")]
			internal struct VariableView
			{
				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly string _name;

				[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
				private readonly object _value;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly string _valueClassName;

				public VariableView(string name, object value, string valueClassName)
				{
					_name = name;
					_value = value;
					_valueClassName = valueClassName;
				}
			}

			private readonly RubyScope _scope;

			private readonly string _selfClassName;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public VariableView[] A0
			{
				get
				{
					List<VariableView> list = new List<VariableView>();
					RubyScope rubyScope = _scope;
					while (true)
					{
						foreach (KeyValuePair<string, object> declaredLocalVariable in rubyScope.GetDeclaredLocalVariables())
						{
							string text = declaredLocalVariable.Key;
							string classDisplayName = _scope.RubyContext.GetClassDisplayName(declaredLocalVariable.Value);
							if (rubyScope != _scope)
							{
								text += " (outer)";
							}
							list.Add(new VariableView(text, declaredLocalVariable.Value, classDisplayName));
						}
						if (!rubyScope.InheritsLocalVariables)
						{
							break;
						}
						rubyScope = rubyScope.Parent;
					}
					return list.ToArray();
				}
			}

			[DebuggerDisplay("{A1}", Name = "self", Type = "{_selfClassName,nq}")]
			public object A1
			{
				get
				{
					return _scope._selfObject;
				}
			}

			[DebuggerDisplay("{B}", Name = "MethodAttributes", Type = "")]
			public object B
			{
				get
				{
					return _scope._methodAttributes;
				}
			}

			[DebuggerDisplay("{C}", Name = "ParentScope", Type = "")]
			public object C
			{
				get
				{
					return _scope.Parent;
				}
			}

			[DebuggerDisplay("", Name = "Raw Variables", Type = "")]
			public object D
			{
				get
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					foreach (KeyValuePair<string, object> declaredLocalVariable in _scope.GetDeclaredLocalVariables())
					{
						dictionary.Add(declaredLocalVariable.Key, declaredLocalVariable.Value);
					}
					return dictionary;
				}
			}

			public DebugView(RubyScope scope)
			{
				_scope = scope;
				MutableString displayName = _scope.RubyContext.GetImmediateClassOf(_scope._selfObject).GetDisplayName(_scope.RubyContext, true);
				_selfClassName = ((displayName != null) ? displayName.ConvertToString() : null);
			}
		}

		internal bool InLoop;

		internal bool InRescue;

		private Dictionary<string, int> _staticLocalMapping;

		private Dictionary<string, object> _dynamicLocals;

		internal MutableTuple _locals;

		internal string[] _variableNames;

		internal RubyTopLevelScope _top;

		internal RubyScope _parent;

		internal object _selfObject;

		private RubyClass _selfImmediateClass;

		internal RubyMethodAttributes _methodAttributes;

		internal InterpretedFrame InterpretedFrame { get; set; }

		public abstract ScopeKind Kind { get; }

		public abstract bool InheritsLocalVariables { get; }

		public virtual RubyModule Module
		{
			get
			{
				return null;
			}
		}

		public object SelfObject
		{
			get
			{
				return _selfObject;
			}
		}

		internal RubyClass SelfImmediateClass
		{
			get
			{
				if (_selfImmediateClass == null)
				{
					_selfImmediateClass = RubyContext.GetImmediateClassOf(_selfObject);
				}
				return _selfImmediateClass;
			}
		}

		public RubyMethodVisibility Visibility
		{
			get
			{
				return (RubyMethodVisibility)(_methodAttributes & RubyMethodAttributes.VisibilityMask);
			}
		}

		public RubyMethodAttributes MethodAttributes
		{
			get
			{
				return _methodAttributes;
			}
			set
			{
				_methodAttributes = value;
			}
		}

		public RubyGlobalScope GlobalScope
		{
			get
			{
				return _top.RubyGlobalScope;
			}
		}

		public RubyTopLevelScope Top
		{
			get
			{
				return _top;
			}
		}

		public RubyContext RubyContext
		{
			get
			{
				return _top.RubyContext;
			}
		}

		internal MutableTuple Locals
		{
			get
			{
				return _locals;
			}
		}

		internal bool LocalsInitialized
		{
			get
			{
				return _variableNames != null;
			}
		}

		public RubyScope Parent
		{
			get
			{
				return _parent;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return RubyContext.EmptyScope == this;
			}
		}

		protected virtual bool IsClosureScope
		{
			get
			{
				return false;
			}
		}

		protected virtual bool IsFlowControlScope
		{
			get
			{
				return false;
			}
		}

		internal RubyScope()
		{
		}

		internal void SetLocals(MutableTuple locals, string[] variableNames)
		{
			_locals = locals;
			_variableNames = variableNames;
		}

		internal void SetEmptyLocals()
		{
			_variableNames = ArrayUtils.EmptyStrings;
			_locals = null;
		}

		private void EnsureBoxes()
		{
			if (_staticLocalMapping == null)
			{
				int num = _variableNames.Length;
				Dictionary<string, int> dictionary = new Dictionary<string, int>(num);
				for (int i = 0; i < num; i++)
				{
					dictionary[_variableNames[i]] = i;
				}
				_staticLocalMapping = dictionary;
			}
		}

		private bool TryGetLocal(string name, out object value)
		{
			EnsureBoxes();
			int value2;
			if (_staticLocalMapping.TryGetValue(name, out value2))
			{
				value = _locals.GetValue(value2);
				return true;
			}
			if (_dynamicLocals == null)
			{
				value = null;
				return false;
			}
			lock (_dynamicLocals)
			{
				return _dynamicLocals.TryGetValue(name, out value);
			}
		}

		private bool TrySetLocal(string name, object value)
		{
			EnsureBoxes();
			int value2;
			if (_staticLocalMapping.TryGetValue(name, out value2))
			{
				_locals.SetValue(value2, value);
				return true;
			}
			if (_dynamicLocals == null)
			{
				return false;
			}
			lock (_dynamicLocals)
			{
				if (!_dynamicLocals.ContainsKey(name))
				{
					return false;
				}
				_dynamicLocals[name] = value;
			}
			return true;
		}

		private IEnumerable<KeyValuePair<string, object>> GetDeclaredLocalVariables()
		{
			for (int i = 0; i < _variableNames.Length; i++)
			{
				yield return new KeyValuePair<string, object>(_variableNames[i], _locals.GetValue(i));
			}
			if (_dynamicLocals == null)
			{
				yield break;
			}
			bool lockTaken = false;
			Dictionary<string, object> obj = default(Dictionary<string, object>);
			try
			{
				Dictionary<string, object> dynamicLocals;
				obj = (dynamicLocals = _dynamicLocals);
				Monitor.Enter(dynamicLocals, ref lockTaken);
				foreach (KeyValuePair<string, object> dynamicLocal in _dynamicLocals)
				{
					yield return dynamicLocal;
				}
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(obj);
				}
			}
		}

		private IEnumerable<string> GetDeclaredLocalSymbols()
		{
			for (int i = 0; i < _variableNames.Length; i++)
			{
				yield return _variableNames[i];
			}
			if (_dynamicLocals == null)
			{
				yield break;
			}
			bool lockTaken = false;
			Dictionary<string, object> obj = default(Dictionary<string, object>);
			try
			{
				Dictionary<string, object> dynamicLocals;
				obj = (dynamicLocals = _dynamicLocals);
				Monitor.Enter(dynamicLocals, ref lockTaken);
				foreach (string key in _dynamicLocals.Keys)
				{
					yield return key;
				}
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(obj);
				}
			}
		}

		public List<string> GetVisibleLocalNames()
		{
			List<string> list = new List<string>();
			RubyScope rubyScope = this;
			while (true)
			{
				foreach (string declaredLocalSymbol in rubyScope.GetDeclaredLocalSymbols())
				{
					list.Add(declaredLocalSymbol);
				}
				if (!rubyScope.InheritsLocalVariables)
				{
					break;
				}
				rubyScope = rubyScope.Parent;
			}
			return list;
		}

		internal object ResolveLocalVariable(string name)
		{
			RubyScope rubyScope = this;
			while (true)
			{
				object value;
				if (rubyScope.TryGetLocal(name, out value))
				{
					return value;
				}
				if (!rubyScope.InheritsLocalVariables)
				{
					break;
				}
				rubyScope = rubyScope.Parent;
			}
			return null;
		}

		internal object ResolveAndSetLocalVariable(string name, object value)
		{
			RubyScope rubyScope = this;
			while (true)
			{
				if (rubyScope.TrySetLocal(name, value))
				{
					return value;
				}
				if (!rubyScope.InheritsLocalVariables)
				{
					break;
				}
				rubyScope = rubyScope.Parent;
			}
			DefineDynamicVariable(name, value);
			return value;
		}

		internal virtual void DefineDynamicVariable(string name, object value)
		{
			if (_dynamicLocals == null)
			{
				Interlocked.CompareExchange(ref _dynamicLocals, new Dictionary<string, object>(), null);
			}
			lock (_dynamicLocals)
			{
				_dynamicLocals[name] = value;
			}
		}

		public RubyModule GetInnerMostModuleForConstantLookup()
		{
			return GetInnerMostModule(false, RubyContext.ObjectClass);
		}

		public RubyModule GetInnerMostModuleForMethodLookup()
		{
			return GetInnerMostModule(false, Top.MethodLookupModule ?? RubyContext.ObjectClass);
		}

		public RubyModule GetInnerMostModuleForClassVariableLookup()
		{
			return GetInnerMostModule(true, RubyContext.ObjectClass);
		}

		private RubyModule GetInnerMostModule(bool skipSingletons, RubyModule fallbackModule)
		{
			RubyScope rubyScope = this;
			do
			{
				RubyModule module = rubyScope.Module;
				if (module != null && (!skipSingletons || !module.IsSingletonClass))
				{
					return module;
				}
				rubyScope = rubyScope.Parent;
			}
			while (rubyScope != null);
			return fallbackModule;
		}

		public RubyMethodScope GetInnerMostMethodScope()
		{
			RubyScope rubyScope = this;
			while (rubyScope != null && rubyScope.Kind != ScopeKind.Method)
			{
				rubyScope = rubyScope.Parent;
			}
			return (RubyMethodScope)rubyScope;
		}

		public RubyClosureScope GetInnerMostClosureScope()
		{
			RubyScope rubyScope = this;
			while (rubyScope != null && !rubyScope.IsClosureScope)
			{
				rubyScope = rubyScope.Parent;
			}
			return (RubyClosureScope)rubyScope;
		}

		public void GetInnerMostBlockOrMethodScope(out RubyBlockScope blockScope, out RubyMethodScope methodScope)
		{
			methodScope = null;
			blockScope = null;
			for (RubyScope rubyScope = this; rubyScope != null; rubyScope = rubyScope.Parent)
			{
				switch (rubyScope.Kind)
				{
					case ScopeKind.Block:
					case ScopeKind.BlockMethod:
					case ScopeKind.BlockModule:
						blockScope = (RubyBlockScope)rubyScope;
						return;
					case ScopeKind.Method:
						methodScope = (RubyMethodScope)rubyScope;
						return;
				}
			}
		}

		internal int GetSuperCallTarget(out RubyModule declaringModule, out string methodName, out RubyScope targetScope)
		{
			RubyScope rubyScope = this;
			while (true)
			{
				switch (rubyScope.Kind)
				{
					case ScopeKind.Method:
						{
							RubyMethodScope rubyMethodScope = (RubyMethodScope)rubyScope;
							declaringModule = rubyMethodScope.DeclaringModule;
							methodName = rubyMethodScope.DefinitionName;
							targetScope = rubyScope;
							return 0;
						}
					case ScopeKind.BlockMethod:
						{
							RubyLambdaMethodInfo method = ((RubyBlockScope)rubyScope).BlockFlowControl.Proc.Method;
							declaringModule = method.DeclaringModule;
							methodName = method.DefinitionName;
							targetScope = rubyScope;
							return method.Id;
						}
					case ScopeKind.TopLevel:
						declaringModule = null;
						methodName = null;
						targetScope = null;
						return -1;
				}
				rubyScope = rubyScope.Parent;
			}
		}

		public RubyScope GetMethodAttributesDefinitionScope()
		{
			RubyScope rubyScope = this;
			while (true)
			{
				switch (rubyScope.Kind)
				{
					case ScopeKind.TopLevel:
					case ScopeKind.Method:
					case ScopeKind.Module:
					case ScopeKind.BlockModule:
					case ScopeKind.FileInitializer:
						return rubyScope;
				}
				rubyScope = rubyScope.Parent;
			}
		}

		internal RubyModule GetMethodDefinitionOwner()
		{
			RubyScope rubyScope = this;
			while (true)
			{
				switch (rubyScope.Kind)
				{
					case ScopeKind.TopLevel:
						return Top.MethodLookupModule ?? Top.TopModuleOrObject;
					case ScopeKind.Module:
						return rubyScope.Module;
					case ScopeKind.Method:
						return rubyScope.GetInnerMostModuleForMethodLookup();
					case ScopeKind.BlockMethod:
						if (RubyContext.RubyOptions.Compatibility != RubyCompatibility.Default)
						{
							return ((RubyBlockScope)rubyScope).BlockFlowControl.Proc.Method.DeclaringModule;
						}
						break;
					case ScopeKind.BlockModule:
						{
							BlockParam blockFlowControl = ((RubyBlockScope)rubyScope).BlockFlowControl;
							return blockFlowControl.MethodLookupModule;
						}
				}
				rubyScope = rubyScope.Parent;
			}
		}

		internal RubyModule TryResolveConstantNoLock(RubyGlobalScope autoloadScope, string name, out ConstantStorage result)
		{
			RubyContext rubyContext = RubyContext;
			RubyScope rubyScope = this;
			RubyModule rubyModule = null;
			do
			{
				RubyModule module = rubyScope.Module;
				if (module != null)
				{
					if (module.TryGetConstantNoLock(autoloadScope, name, out result))
					{
						return null;
					}
					if (rubyModule == null)
					{
						rubyModule = module;
					}
				}
				rubyScope = rubyScope.Parent;
			}
			while (rubyScope != null);
			if (rubyModule != null)
			{
				if (rubyModule.TryResolveConstantNoLock(autoloadScope, name, out result))
				{
					return null;
				}
			}
			else
			{
				rubyModule = rubyContext.ObjectClass;
			}
			if (rubyContext.ObjectClass.TryResolveConstantNoLock(autoloadScope, name, out result))
			{
				return null;
			}
			return rubyModule;
		}

		[Conditional("DEBUG")]
		public void SetDebugName(string name)
		{
		}
	}
}

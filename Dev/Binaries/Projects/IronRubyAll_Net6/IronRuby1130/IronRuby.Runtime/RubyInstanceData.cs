using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public sealed class RubyInstanceData : IRubyObjectState
	{
		[DebuggerDisplay("{GetValue()}", Name = "{_name,nq}", Type = "{GetClassName(),nq}")]
		public sealed class VariableDebugView
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly RubyContext _context;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private readonly RubyInstanceData _data;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			internal readonly string _name;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public object A
			{
				get
				{
					return GetValue();
				}
			}

			[DebuggerDisplay("{B}", Name = "Raw Value", Type = "{GetClrType()}")]
			public object B
			{
				get
				{
					return GetValue();
				}
				set
				{
					_data.SetInstanceVariable(_name, value);
				}
			}

			private object GetValue()
			{
				return _data.GetInstanceVariable(_name);
			}

			private Type GetClrType()
			{
				object value = GetValue();
				if (value == null)
				{
					return null;
				}
				return value.GetType();
			}

			private string GetClassName()
			{
				return _context.GetClassDisplayName(GetValue());
			}

			internal VariableDebugView(RubyContext context, RubyInstanceData data, string name)
			{
				_context = context;
				_data = data;
				_name = name;
			}
		}

		private static int _CurrentObjectId = 42;

		private int _objectId;

		private bool _frozen;

		private bool _tainted;

		private bool _untrusted;

		private Dictionary<string, object> _instanceVars;

		private RubyClass _immediateClass;

		internal RubyClass ImmediateClass
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

		internal RubyClass InstanceSingleton
		{
			get
			{
				if (_immediateClass == null || !_immediateClass.IsSingletonClass)
				{
					return null;
				}
				return _immediateClass;
			}
		}

		internal int ObjectId
		{
			get
			{
				return _objectId;
			}
		}

		public bool IsTainted
		{
			get
			{
				return _tainted;
			}
			set
			{
				Mutate();
				_tainted = value;
			}
		}

		public bool IsUntrusted
		{
			get
			{
				return _untrusted;
			}
			set
			{
				Mutate();
				_untrusted = value;
			}
		}

		public bool IsFrozen
		{
			get
			{
				return _frozen;
			}
		}

		internal bool HasInstanceVariables
		{
			get
			{
				return _instanceVars != null;
			}
		}

		internal void UpdateImmediateClass(RubyClass immediate)
		{
			Interlocked.CompareExchange(ref _immediateClass, immediate, null);
		}

		internal RubyInstanceData(int id)
		{
			_objectId = id;
		}

		internal RubyInstanceData()
		{
			_objectId = Interlocked.Increment(ref _CurrentObjectId);
		}

		public void Freeze()
		{
			_frozen = true;
		}

		private void Mutate()
		{
			if (_frozen)
			{
				throw RubyExceptions.CreateObjectFrozenError();
			}
		}

		private Dictionary<string, object> GetInstanceVariables()
		{
			if (_instanceVars == null)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				if (Interlocked.CompareExchange(ref _instanceVars, dictionary, null) == null)
				{
					return dictionary;
				}
			}
			return _instanceVars;
		}

		internal void CopyInstanceVariablesTo(RubyInstanceData dup)
		{
			if (_instanceVars == null)
			{
				return;
			}
			lock (_instanceVars)
			{
				Dictionary<string, object> instanceVariables = dup.GetInstanceVariables();
				foreach (KeyValuePair<string, object> instanceVar in _instanceVars)
				{
					instanceVariables.Add(instanceVar.Key, instanceVar.Value);
				}
			}
		}

		internal bool IsInstanceVariableDefined(string name)
		{
			if (_instanceVars == null)
			{
				return false;
			}
			lock (_instanceVars)
			{
				return _instanceVars.ContainsKey(name);
			}
		}

		internal string[] GetInstanceVariableNames()
		{
			if (_instanceVars == null)
			{
				return ArrayUtils.EmptyStrings;
			}
			lock (_instanceVars)
			{
				string[] array = new string[_instanceVars.Count];
				_instanceVars.Keys.CopyTo(array, 0);
				return array;
			}
		}

		internal List<KeyValuePair<string, object>> GetInstanceVariablePairs()
		{
			if (_instanceVars == null)
			{
				return new List<KeyValuePair<string, object>>();
			}
			lock (_instanceVars)
			{
				return new List<KeyValuePair<string, object>>(_instanceVars);
			}
		}

		internal bool TryGetInstanceVariable(string name, out object value)
		{
			if (_instanceVars == null)
			{
				value = null;
				return false;
			}
			lock (_instanceVars)
			{
				return _instanceVars.TryGetValue(name, out value);
			}
		}

		internal bool TryRemoveInstanceVariable(string name, out object value)
		{
			if (_instanceVars == null)
			{
				value = null;
				return false;
			}
			lock (_instanceVars)
			{
				if (!_instanceVars.TryGetValue(name, out value))
				{
					return false;
				}
				_instanceVars.Remove(name);
				return true;
			}
		}

		internal object GetInstanceVariable(string name)
		{
			object value;
			TryGetInstanceVariable(name, out value);
			return value;
		}

		internal void SetInstanceVariable(string name, object value)
		{
			Dictionary<string, object> instanceVariables = GetInstanceVariables();
			lock (instanceVariables)
			{
				instanceVariables[name] = value;
			}
		}

		internal VariableDebugView[] GetInstanceVariablesDebugView(RubyContext context)
		{
			if (_instanceVars == null)
			{
				return new VariableDebugView[0];
			}
			List<VariableDebugView> list = new List<VariableDebugView>();
			lock (_instanceVars)
			{
				foreach (KeyValuePair<string, object> instanceVar in _instanceVars)
				{
					list.Add(new VariableDebugView(context, this, instanceVar.Key));
				}
			}
			list.Sort((VariableDebugView var1, VariableDebugView var2) => string.CompareOrdinal(var1._name, var2._name));
			return list.ToArray();
		}
	}
}

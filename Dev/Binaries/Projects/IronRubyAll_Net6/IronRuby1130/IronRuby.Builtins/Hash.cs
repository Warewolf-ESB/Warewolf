using System.Collections.Generic;
using System.Diagnostics;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public class Hash : Dictionary<object, object>, IRubyObjectState, IDuplicable
	{
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		public sealed class Subclass : Hash, IRubyObject, IRubyObjectState
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

			public Subclass(RubyClass rubyClass)
				: base(rubyClass.Context)
			{
				ImmediateClass = rubyClass;
			}

			protected override Hash CreateInstance()
			{
				return new Subclass(ImmediateClass.NominalClass);
			}

			public RubyInstanceData GetInstanceData()
			{
				return RubyOps.GetInstanceData(ref _instanceData);
			}

			public RubyInstanceData TryGetInstanceData()
			{
				return _instanceData;
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
				return base.ToString();
			}
		}

		private const uint IsFrozenFlag = 1u;

		private const uint IsTaintedFlag = 2u;

		private const uint IsUntrustedFlag = 4u;

		private Proc _defaultProc;

		private object _defaultValue;

		private uint _flags;

		public Proc DefaultProc
		{
			get
			{
				return _defaultProc;
			}
			set
			{
				Mutate();
				_defaultProc = value;
			}
		}

		public object DefaultValue
		{
			get
			{
				return _defaultValue;
			}
			set
			{
				Mutate();
				_defaultValue = value;
			}
		}

		public bool IsTainted
		{
			get
			{
				return (_flags & 2) != 0;
			}
			set
			{
				Mutate();
				_flags = (_flags & 0xFFFFFFFDu) | (value ? 2u : 0u);
			}
		}

		public bool IsUntrusted
		{
			get
			{
				return (_flags & 4) != 0;
			}
			set
			{
				Mutate();
				_flags = (_flags & 0xFFFFFFFBu) | (value ? 4u : 0u);
			}
		}

		public bool IsFrozen
		{
			get
			{
				return (_flags & 1) != 0;
			}
		}

		public Hash(RubyContext context)
			: base((IEqualityComparer<object>)context.EqualityComparer)
		{
		}

		public Hash(IEqualityComparer<object> comparer)
			: base(comparer)
		{
		}

		public Hash(EqualityComparer comparer, Proc defaultProc, object defaultValue)
			: base((IEqualityComparer<object>)comparer)
		{
			_defaultValue = defaultValue;
			_defaultProc = defaultProc;
		}

		public Hash(EqualityComparer comparer, int capacity)
			: base(capacity, (IEqualityComparer<object>)comparer)
		{
		}

		public Hash(IDictionary<object, object> dictionary)
			: base(dictionary)
		{
		}

		public Hash(IDictionary<object, object> dictionary, EqualityComparer comparer)
			: base(dictionary, (IEqualityComparer<object>)comparer)
		{
		}

		public Hash(Hash hash)
			: base((IDictionary<object, object>)hash, hash.Comparer)
		{
			_defaultProc = hash._defaultProc;
			_defaultValue = hash.DefaultValue;
		}

		public static Hash CreateInstance(RubyClass rubyClass)
		{
			if (!(rubyClass.GetUnderlyingSystemType() == typeof(Hash)))
			{
				return new Subclass(rubyClass);
			}
			return new Hash(rubyClass.Context);
		}

		protected virtual Hash CreateInstance()
		{
			return new Hash(base.Comparer);
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			Hash hash = CreateInstance();
			context.CopyInstanceData(this, hash, copySingletonMembers);
			return hash;
		}

		public void RequireNotFrozen()
		{
			if ((_flags & (true ? 1u : 0u)) != 0)
			{
				throw RubyExceptions.CreateObjectFrozenError();
			}
		}

		private void Mutate()
		{
			RequireNotFrozen();
		}

		void IRubyObjectState.Freeze()
		{
			Freeze();
		}

		public Hash Freeze()
		{
			_flags |= 1u;
			return this;
		}
	}
}

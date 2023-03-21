using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public class Range : IDuplicable, ISerializable
	{
		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		public sealed class Subclass : Range, IRubyObject, IRubyObjectState
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

			public Subclass(RubyClass rubyClass)
			{
				ImmediateClass = rubyClass;
			}

			private Subclass(Subclass range)
				: base(range)
			{
				ImmediateClass = range.ImmediateClass.NominalClass;
			}

			protected override Range Copy()
			{
				return new Subclass(this);
			}
		}

		private object _begin;

		private object _end;

		private bool _excludeEnd;

		private bool _initialized;

		public object Begin
		{
			get
			{
				return _begin;
			}
		}

		public object End
		{
			get
			{
				return _end;
			}
		}

		public bool ExcludeEnd
		{
			get
			{
				return _excludeEnd;
			}
		}

		private string Separator
		{
			get
			{
				if (!_excludeEnd)
				{
					return "..";
				}
				return "...";
			}
		}

		protected Range(SerializationInfo info, StreamingContext context)
		{
			_begin = info.GetValue("begin", typeof(object));
			_end = info.GetValue("end", typeof(object));
			_excludeEnd = info.GetBoolean("excl");
			_initialized = true;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("begin", _begin);
			info.AddValue("end", _end);
			info.AddValue("excl", _excludeEnd);
		}

		protected Range(Range range)
		{
			_begin = range._begin;
			_end = range._end;
			_excludeEnd = range._excludeEnd;
		}

		public Range()
		{
		}

		public Range(int begin, int end, bool excludeEnd)
		{
			_begin = begin;
			_end = end;
			_excludeEnd = excludeEnd;
			_initialized = true;
		}

		public Range(MutableString begin, MutableString end, bool excludeEnd)
		{
			_begin = begin;
			_end = end;
			_excludeEnd = excludeEnd;
			_initialized = true;
		}

		public Range(BinaryOpStorage comparisonStorage, RubyContext context, object begin, object end, bool excludeEnd)
		{
			Initialize(comparisonStorage, context, begin, end, excludeEnd);
		}

		public void Initialize(BinaryOpStorage comparisonStorage, RubyContext context, object begin, object end, bool excludeEnd)
		{
			if (_initialized)
			{
				throw RubyExceptions.CreateNameError("`initialize' called twice");
			}
			CallSite<Func<CallSite, object, object, object>> callSite = comparisonStorage.GetCallSite("<=>");
			object obj;
			try
			{
				obj = callSite.Target(callSite, begin, end);
			}
			catch (Exception)
			{
				obj = null;
			}
			if (obj == null)
			{
				throw RubyExceptions.CreateArgumentError("bad value for range");
			}
			_begin = begin;
			_end = end;
			_excludeEnd = excludeEnd;
			_initialized = true;
		}

		protected virtual Range Copy()
		{
			return new Range(this);
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			Range range = Copy();
			context.CopyInstanceData(this, range, copySingletonMembers);
			return range;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_begin.ToString());
			stringBuilder.Append(Separator);
			stringBuilder.Append(_end.ToString());
			return stringBuilder.ToString();
		}

		public MutableString Inspect(RubyContext context)
		{
			MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
			mutableString.Append(context.Inspect(_begin));
			mutableString.Append(Separator);
			mutableString.Append(context.Inspect(_end));
			return mutableString;
		}

		public MutableString ToMutableString(ConversionStorage<MutableString> tosConversion)
		{
			MutableString mutableString = Protocols.ConvertToString(tosConversion, _begin);
			mutableString.Append(Separator);
			mutableString.Append(Protocols.ConvertToString(tosConversion, _end));
			return mutableString;
		}
	}
}

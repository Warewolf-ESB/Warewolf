using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[Serializable]
	[DebuggerDisplay("{GetDebugView()}")]
	public class RubyArray : IList<object>, ICollection<object>, IEnumerable<object>, IList, ICollection, IEnumerable, IRubyObjectState, IDuplicable
	{
		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		public sealed class Subclass : RubyArray, IRubyObject, IRubyObjectState
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
			{
				ImmediateClass = rubyClass;
			}

			private Subclass(Subclass array)
				: base(array)
			{
				ImmediateClass = array.ImmediateClass.NominalClass;
			}

			public override RubyArray CreateInstance()
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

		private object[] _content;

		private int _start;

		private int _count;

		private uint _flags;

		private static RubyUtils.RecursionTracker _HashTracker = new RubyUtils.RecursionTracker();

		private static RubyUtils.RecursionTracker _EqualsTracker = new RubyUtils.RecursionTracker();

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

		public int Count
		{
			get
			{
				return _count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return IsFrozen;
			}
		}

		bool IList.IsFixedSize
		{
			get
			{
				return IsReadOnly;
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				return this;
			}
		}

		public int Capacity
		{
			get
			{
				return _content.Length;
			}
		}

		public object this[int index]
		{
			get
			{
				if (index < 0 || index >= _count)
				{
					throw new IndexOutOfRangeException();
				}
				return _content[_start + index];
			}
			set
			{
				Mutate();
				int num = index - _count;
				if (num >= 0)
				{
					ResizeForAppend(num + 1, true);
				}
				_content[_start + index] = value;
			}
		}

		[Conditional("DEBUG")]
		private void ObjectInvariant()
		{
		}

		[Conditional("DEBUG")]
		internal void RequireNullEmptySlots()
		{
			for (int i = 0; i < _content.Length; i++)
			{
				if (i >= _start)
				{
					int num = _start + _count;
				}
			}
		}

		internal RubyArray(object[] content, int start, int count)
		{
			_start = start;
			_content = content;
			_count = count;
		}

		public RubyArray()
			: this(ArrayUtils.EmptyObjects, 0, 0)
		{
		}

		public RubyArray(int capacity)
			: this(new object[Math.Max(capacity, 4)], 0, 0)
		{
		}

		public RubyArray(RubyArray items)
			: this(items, 0, items.Count)
		{
		}

		public RubyArray(RubyArray items, int start, int count)
			: this(count)
		{
			ContractUtils.RequiresNotNull(items, "items");
			ContractUtils.RequiresArrayRange(items.Count, start, count, "start", "count");
			AddVector(items._content, items._start + start, count);
		}

		public RubyArray(IList items)
			: this(items, 0, items.Count)
		{
		}

		public RubyArray(IList items, int start, int count)
			: this(count)
		{
			AddRange(items, start, count);
		}

		public RubyArray(ICollection items)
			: this(items.Count)
		{
			AddCollection(items);
		}

		public RubyArray(IEnumerable items)
			: this()
		{
			AddRange(items);
		}

		public static RubyArray Create(object item)
		{
			return new RubyArray(new object[4] { item, null, null, null }, 0, 1);
		}

		public static RubyArray CreateInstance(RubyClass rubyClass)
		{
			if (!(rubyClass.GetUnderlyingSystemType() == typeof(RubyArray)))
			{
				return new Subclass(rubyClass);
			}
			return new RubyArray();
		}

		public virtual RubyArray CreateInstance()
		{
			return new RubyArray();
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			RubyArray rubyArray = CreateInstance();
			context.CopyInstanceData(this, rubyArray, copySingletonMembers);
			return rubyArray;
		}

		public void RequireNotFrozen()
		{
			if ((_flags & (true ? 1u : 0u)) != 0)
			{
				ThrowObjectFrozenException();
			}
		}

		private static void ThrowObjectFrozenException()
		{
			throw RubyExceptions.CreateObjectFrozenError();
		}

		private void Mutate()
		{
			RequireNotFrozen();
		}

		void IRubyObjectState.Freeze()
		{
			Freeze();
		}

		public RubyArray Freeze()
		{
			_flags |= 1u;
			return this;
		}

		public static int GetHashCode(UnaryOpStorage hashStorage, ConversionStorage<int> fixnumCast, IList self)
		{
			int num = self.Count;
			using (IDisposable disposable = _HashTracker.TrackObject(self))
			{
				if (disposable == null)
				{
					return 0;
				}
				CallSite<Func<CallSite, object, object>> callSite = hashStorage.GetCallSite("hash");
				CallSite<Func<CallSite, object, int>> site = fixnumCast.GetSite(ProtocolConversionAction<ConvertToFixnumAction>.Make(fixnumCast.Context));
				foreach (object item in self)
				{
					num = (num << 1) ^ site.Target(site, callSite.Target(callSite, item));
				}
				return num;
			}
		}

		public static bool Equals(BinaryOpStorage eqlStorage, IList self, object obj)
		{
			if (object.ReferenceEquals(self, obj))
			{
				return true;
			}
			IList list = obj as IList;
			if (list == null || self.Count != list.Count)
			{
				return false;
			}
			using (IDisposable disposable = _EqualsTracker.TrackObject(self))
			{
				using (IDisposable disposable2 = _EqualsTracker.TrackObject(list))
				{
					if (disposable == null && disposable2 == null)
					{
						return true;
					}
					CallSite<Func<CallSite, object, object, object>> callSite = eqlStorage.GetCallSite("eql?");
					for (int i = 0; i < self.Count; i++)
					{
						if (!Protocols.IsEqual(callSite, self[i], list[i]))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private int ResizeForAppend(int additionalCount, bool clear)
		{
			int count = _count;
			_count += additionalCount;
			if (_start + count > _content.Length - additionalCount)
			{
				object[] array = ((_count <= _content.Length) ? _content : new object[Utils.GetExpandedSize(_content, _count)]);
				Array.Copy(_content, _start, array, 0, count);
				if (array == _content && (additionalCount < _start || clear))
				{
					if (_start < count)
					{
						Utils.Fill(array, count, null, _start);
					}
					else
					{
						Utils.Fill(array, _start, null, count);
					}
				}
				_content = array;
				_start = 0;
			}
			return _start + count;
		}

		public void Add(object item)
		{
			Mutate();
			int num = ResizeForAppend(1, false);
			_content[num] = item;
		}

		int IList.Add(object value)
		{
			int count = _count;
			Add(value);
			return count;
		}

		public RubyArray AddCapacity(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}
			Mutate();
			int count = _count;
			ResizeForAppend(capacity, true);
			_count = count;
			return this;
		}

		public RubyArray AddMultiple(int count, object value)
		{
			Mutate();
			if (value != null)
			{
				int num = ResizeForAppend(count, false);
				int num2 = num + count;
				for (int i = num; i < num2; i++)
				{
					_content[i] = value;
				}
			}
			else
			{
				ResizeForAppend(count, true);
			}
			return this;
		}

		public RubyArray AddRange(IList items, int start, int count)
		{
			ContractUtils.RequiresNotNull(items, "items");
			ContractUtils.RequiresArrayRange(items.Count, start, count, "start", "count");
			Mutate();
			RubyArray rubyArray;
			object[] items2;
			if ((rubyArray = items as RubyArray) != null)
			{
				AddVector(rubyArray._content, rubyArray._start + start, count);
			}
			else if ((items2 = items as object[]) != null)
			{
				AddVector(items2, start, count);
			}
			else
			{
				AddList(items, start, count);
			}
			return this;
		}

		public RubyArray AddRange(IEnumerable items)
		{
			ContractUtils.RequiresNotNull(items, "items");
			Mutate();
			RubyArray rubyArray;
			object[] array;
			ICollection items2;
			if ((rubyArray = items as RubyArray) != null)
			{
				AddVector(rubyArray._content, rubyArray._start, rubyArray._count);
			}
			else if ((array = items as object[]) != null)
			{
				AddVector(array, 0, array.Length);
			}
			else if ((items2 = items as ICollection) != null)
			{
				AddCollection(items2);
			}
			else
			{
				AddSequence(items);
			}
			return this;
		}

		private void AddList(IList items, int start, int count)
		{
			int num = ResizeForAppend(count, false);
			for (int i = 0; i < count; i++)
			{
				_content[num + i] = items[start + i];
			}
		}

		internal void AddVector(object[] items, int start, int count)
		{
			int destinationIndex = ResizeForAppend(count, false);
			Array.Copy(items, start, _content, destinationIndex, count);
		}

		private void AddCollection(ICollection items)
		{
			int num = ResizeForAppend(items.Count, false);
			foreach (object item in items)
			{
				_content[num++] = item;
			}
		}

		private void AddSequence(IEnumerable items)
		{
			foreach (object item in items)
			{
				Add(item);
			}
		}

		private int ResizeForInsertion(int index, int size)
		{
			if (_count + size > _content.Length)
			{
				object[] array = new object[Utils.GetExpandedSize(_content, _count + size)];
				Array.Copy(_content, _start, array, 0, index);
				Array.Copy(_content, _start + index, array, index + size, _count - index);
				_count += size;
				_content = array;
				return index;
			}
			int num = _start + index;
			int num2 = num;
			int num3 = _content.Length - _start - _count;
			int num4 = 0;
			int num5 = 0;
			if (_start >= size)
			{
				if (num3 >= size)
				{
					if (index < _count / 2)
					{
						num4 = size;
						num2 -= size;
					}
					else
					{
						num5 = size;
					}
				}
				else
				{
					num4 = size;
					num2 -= size;
				}
			}
			else if (num3 >= size)
			{
				num5 = size;
			}
			else
			{
				num4 = _start;
				num5 = size - num4;
				num2 -= num4;
			}
			if (num4 > 0)
			{
				int num6 = _start - num4;
				Array.Copy(_content, _start, _content, num6, index);
				_start = num6;
			}
			if (num5 > 0)
			{
				Array.Copy(_content, num, _content, num + num5, _count - index);
			}
			_count += size;
			return num2;
		}

		public void Insert(int index, object item)
		{
			ContractUtils.RequiresArrayInsertIndex(_count, index, "index");
			Mutate();
			int num = ResizeForInsertion(index, 1);
			_content[num] = item;
		}

		public void InsertRange(int index, IList items, int start, int count)
		{
			ContractUtils.RequiresNotNull(items, "items");
			ContractUtils.RequiresArrayInsertIndex(_count, index, "index");
			ContractUtils.RequiresArrayRange(items.Count, start, count, "start", "count");
			Mutate();
			RubyArray rubyArray;
			object[] items2;
			if ((rubyArray = items as RubyArray) != null)
			{
				InsertVector(index, rubyArray._content, start, count);
			}
			else if ((items2 = items as object[]) != null)
			{
				InsertVector(index, items2, start, count);
			}
			else
			{
				InsertList(index, items, start, count);
			}
		}

		public void InsertRange(int index, IEnumerable items)
		{
			ContractUtils.RequiresNotNull(items, "items");
			ContractUtils.RequiresArrayInsertIndex(_count, index, "index");
			Mutate();
			RubyArray array;
			object[] array2;
			IList list;
			if ((array = items as RubyArray) != null)
			{
				InsertArray(index, array);
			}
			else if ((array2 = items as object[]) != null)
			{
				InsertVector(index, array2, 0, array2.Length);
			}
			else if ((list = items as IList) != null)
			{
				InsertList(index, list, 0, list.Count);
			}
			else
			{
				InsertArray(index, new RubyArray(items));
			}
		}

		private void InsertArray(int index, RubyArray array)
		{
			InsertVector(index, array._content, array._start, array._count);
		}

		private void InsertVector(int index, object[] items, int start, int count)
		{
			int destinationIndex = ResizeForInsertion(index, count);
			Array.Copy(items, start, _content, destinationIndex, count);
		}

		private void InsertList(int index, IList items, int start, int count)
		{
			int num = ResizeForInsertion(index, count);
			for (int i = 0; i < count; i++)
			{
				_content[num + i] = items[start + i];
			}
		}

		public void RemoveRange(int index, int size)
		{
			ContractUtils.RequiresArrayRange(_count, index, size, "index", "size");
			Mutate();
			int num = _count - size;
			int num2 = _content.Length;
			int num3 = 2 * num - 1;
			if (num3 <= num2 / 2)
			{
				object[] array = new object[Math.Max(4, num3)];
				Array.Copy(_content, _start, array, 0, index);
				Array.Copy(_content, _start + index + size, array, index, num - index);
				_content = array;
				_start = 0;
			}
			else if (index <= num / 2)
			{
				int num4 = _start + size;
				Array.Copy(_content, _start, _content, num4, index);
				Utils.Fill(_content, _start, null, size);
				_start = num4;
			}
			else
			{
				Array.Copy(_content, _start + index + size, _content, _start + index, num - index);
				Utils.Fill(_content, _start + num, null, size);
			}
			_count = num;
		}

		public void RemoveAt(int index)
		{
			RemoveRange(index, 1);
		}

		public bool Remove(object item)
		{
			int num = IndexOf(item);
			if (num >= 0)
			{
				RemoveAt(num);
				return true;
			}
			return false;
		}

		void IList.Remove(object value)
		{
			Remove(value);
		}

		public void Clear()
		{
			Mutate();
			_content = ArrayUtils.EmptyObjects;
			_start = (_count = 0);
		}

		public void CopyTo(object[] array, int index)
		{
			Array.Copy(_content, _start, array, index, _count);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			Array.Copy(_content, _start, array, index, _count);
		}

		public object[] ToArray()
		{
			object[] array = new object[_count];
			CopyTo(array, 0);
			return array;
		}

		public int IndexOf(object item)
		{
			return IndexOf(item, 0, _count);
		}

		public int IndexOf(object item, int startIndex)
		{
			return Array.IndexOf(_content, item, startIndex, _count - startIndex);
		}

		public int IndexOf(object item, int startIndex, int count)
		{
			return Array.IndexOf(_content, item, _start + startIndex, count);
		}

		public int FindIndex(Predicate<object> match)
		{
			return FindIndex(0, _count, match);
		}

		public int FindIndex(int startIndex, Predicate<object> match)
		{
			return FindIndex(startIndex, _count - startIndex, match);
		}

		public int FindIndex(int startIndex, int count, Predicate<object> match)
		{
			return Array.FindIndex(_content, _start + startIndex, count, match);
		}

		public bool Contains(object item)
		{
			return IndexOf(item) >= 0;
		}

		public IEnumerator<object> GetEnumerator()
		{
			int i = 0;
			int start = _start;
			for (int count = _count; i < count; i++)
			{
				yield return _content[start + i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)this).GetEnumerator();
		}

		public void Reverse()
		{
			Mutate();
			Array.Reverse(_content, _start, _count);
		}

		public void Sort()
		{
			Mutate();
			Array.Sort(_content, _start, _count);
		}

		public void Sort(Comparison<object> comparison)
		{
			Mutate();
			Array.Sort(_content, _start, _count, ArrayUtils.ToComparer(comparison));
		}

		internal string GetDebugView()
		{
			if (RubyContext._Default == null)
			{
				return ToString();
			}
			return RubyContext._Default.Inspect(this).ToString();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule(Extends = typeof(IList), Restrictions = ModuleRestrictions.None)]
	[Includes(new Type[] { typeof(Enumerable) })]
	public static class IListOps
	{
		internal sealed class PermutationEnumerator : IEnumerator
		{
			private struct State
			{
				public readonly int i;

				public readonly int j;

				public State(int i, int j)
				{
					this.i = i;
					this.j = j;
				}
			}

			private readonly IList _list;

			private readonly int? _size;

			public PermutationEnumerator(IList list, int? size)
			{
				_size = size;
				_list = list;
			}

			public object Each(RubyScope scope, BlockParam block)
			{
				int num = _size ?? _list.Count;
				if (num < 0 || num > _list.Count)
				{
					return _list;
				}
				object[] array = new object[num];
				object[] array2 = new object[_list.Count];
				_list.CopyTo(array2, 0);
				Stack<State> stack = new Stack<State>();
				stack.Push(new State(0, -1));
				while (stack.Count > 0)
				{
					State state = stack.Pop();
					int i = state.i;
					int j = state.j;
					if (j < 0)
					{
						if (i == array.Length)
						{
							object blockResult;
							if (block.Yield(RubyOps.MakeArrayN(array), out blockResult))
							{
								return blockResult;
							}
						}
						else
						{
							array[i] = array2[i];
							stack.Push(new State(i, i));
							stack.Push(new State(i + 1, -1));
						}
						continue;
					}
					j++;
					if (j == array2.Length)
					{
						while (j > i)
						{
							j--;
							Xchg(array2, i, j);
						}
					}
					else
					{
						Xchg(array2, i, j);
						array[i] = array2[i];
						stack.Push(new State(i, j));
						stack.Push(new State(i + 1, -1));
					}
				}
				return _list;
			}

			private static void Xchg(object[] values, int i, int j)
			{
				object obj = values[j];
				values[j] = values[i];
				values[i] = obj;
			}
		}

		internal sealed class CombinationEnumerator : IEnumerator
		{
			private struct State
			{
				public readonly int i;

				public readonly int j;

				public readonly bool init;

				public State(int i, int j, bool init)
				{
					this.i = i;
					this.j = j;
					this.init = init;
				}
			}

			private readonly IList _list;

			private readonly int? _size;

			public CombinationEnumerator(IList list, int? size)
			{
				_size = size;
				_list = list;
			}

			public object Each(RubyScope scope, BlockParam block)
			{
				int num = _size ?? _list.Count;
				if (num < 0 || num > _list.Count)
				{
					return _list;
				}
				object[] array = new object[num];
				object[] array2 = new object[_list.Count];
				_list.CopyTo(array2, 0);
				Stack<State> stack = new Stack<State>();
				stack.Push(new State(0, 0, true));
				while (stack.Count > 0)
				{
					State state = stack.Pop();
					int num2 = state.i;
					int j = state.j;
					if (state.init && j == array.Length)
					{
						object blockResult;
						if (block.Yield(RubyOps.MakeArrayN(array), out blockResult))
						{
							return blockResult;
						}
						continue;
					}
					if (!state.init)
					{
						num2++;
					}
					if (num2 <= array2.Length - array.Length + j)
					{
						array[j] = array2[num2];
						stack.Push(new State(num2, j, false));
						stack.Push(new State(num2 + 1, j + 1, true));
					}
				}
				return _list;
			}
		}

		private static RubyUtils.RecursionTracker _EqualsTracker = new RubyUtils.RecursionTracker();

		private static RubyUtils.RecursionTracker _ComparisonTracker = new RubyUtils.RecursionTracker();

		private static void RequireNotFrozen(IList self)
		{
			RubyArray rubyArray = self as RubyArray;
			if (rubyArray != null && rubyArray.IsFrozen)
			{
				throw RubyExceptions.CreateObjectFrozenError();
			}
		}

		internal static int NormalizeIndex(IList list, int index)
		{
			return NormalizeIndex(list.Count, index);
		}

		internal static int NormalizeIndexThrowIfNegative(IList list, int index)
		{
			index = NormalizeIndex(list.Count, index);
			if (index < 0)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of array", index);
			}
			return index;
		}

		internal static int NormalizeIndex(int count, int index)
		{
			if (index >= 0)
			{
				return index;
			}
			return index + count;
		}

		internal static bool NormalizeRange(int listCount, ref int start, ref int count)
		{
			start = NormalizeIndex(listCount, start);
			if (start < 0 || start > listCount || count < 0)
			{
				return false;
			}
			if (count != 0)
			{
				count = ((start + count > listCount) ? (listCount - start) : count);
			}
			return true;
		}

		internal static bool NormalizeRange(ConversionStorage<int> fixnumCast, int listCount, Range range, out int begin, out int count)
		{
			begin = Protocols.CastToFixnum(fixnumCast, range.Begin);
			int index = Protocols.CastToFixnum(fixnumCast, range.End);
			begin = NormalizeIndex(listCount, begin);
			if (begin < 0 || begin > listCount)
			{
				count = 0;
				return false;
			}
			index = NormalizeIndex(listCount, index);
			count = (range.ExcludeEnd ? (index - begin) : (index - begin + 1));
			return true;
		}

		private static bool InRangeNormalized(IList list, ref int index)
		{
			if (index < 0)
			{
				index += list.Count;
			}
			if (index >= 0)
			{
				return index < list.Count;
			}
			return false;
		}

		private static IList GetResultRange(UnaryOpStorage allocateStorage, IList list, int index, int count)
		{
			IList list2 = CreateResultArray(allocateStorage, list);
			int num = index + count;
			for (int i = index; i < num; i++)
			{
				list2.Add(list[i]);
			}
			return list2;
		}

		private static void InsertRange(IList list, int index, IList items, int start, int count)
		{
			RubyArray rubyArray;
			if ((rubyArray = list as RubyArray) != null)
			{
				rubyArray.InsertRange(index, items, start, count);
				return;
			}
			List<object> list2;
			ICollection<object> collection;
			if ((list2 = list as List<object>) != null && (collection = items as ICollection<object>) != null && start == 0 && count == collection.Count)
			{
				list2.InsertRange(index, collection);
				return;
			}
			for (int i = 0; i < count; i++)
			{
				list.Insert(index + i, items[start + i]);
			}
		}

		internal static void RemoveRange(IList collection, int index, int count)
		{
			if (count <= 1)
			{
				if (count > 0)
				{
					collection.RemoveAt(index);
				}
				return;
			}
			RubyArray rubyArray;
			if ((rubyArray = collection as RubyArray) != null)
			{
				rubyArray.RemoveRange(index, count);
				return;
			}
			List<object> list;
			if ((list = collection as List<object>) != null)
			{
				list.RemoveRange(index, count);
				return;
			}
			for (int num = index + count - 1; num >= index; num--)
			{
				collection.RemoveAt(num);
			}
		}

		internal static void AddRange(IList collection, IList items)
		{
			int count = items.Count;
			RubyArray rubyArray;
			if (count <= 1)
			{
				if (count > 0)
				{
					collection.Add(items[0]);
				}
				else if ((rubyArray = collection as RubyArray) != null)
				{
					rubyArray.RequireNotFrozen();
				}
				return;
			}
			rubyArray = collection as RubyArray;
			if (rubyArray != null)
			{
				rubyArray.AddRange(items);
				return;
			}
			for (int i = 0; i < count; i++)
			{
				collection.Add(items[i]);
			}
		}

		private static IList CreateResultArray(UnaryOpStorage allocateStorage, IList list)
		{
			RubyArray rubyArray = list as RubyArray;
			if (rubyArray != null)
			{
				return rubyArray.CreateInstance();
			}
			CallSite<Func<CallSite, object, object>> callSite = allocateStorage.GetCallSite("allocate", 0);
			RubyClass classOf = allocateStorage.Context.GetClassOf(list);
			IList list2 = callSite.Target(callSite, classOf) as IList;
			if (list2 != null)
			{
				return list2;
			}
			throw RubyExceptions.CreateTypeError("{0}#allocate should return IList", classOf.Name);
		}

        internal static IEnumerable<Int32>/*!*/ ReverseEnumerateIndexes(IList/*!*/ collection)
        {
            for (int originalSize = collection.Count, i = originalSize - 1; i >= 0; i--)
            {
                yield return i;
                if (collection.Count < originalSize)
                {
                    i = originalSize - (originalSize - collection.Count);
                    originalSize = collection.Count;
                }
            }
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("replace")]
		public static IList Replace(IList self, [DefaultProtocol][NotNull] IList other)
		{
			self.Clear();
			AddRange(self, other);
			return self;
		}

		[RubyMethod("clear")]
		public static IList Clear(IList self)
		{
			self.Clear();
			return self;
		}

		[RubyMethod("to_ary")]
		[RubyMethod("to_a")]
		public static RubyArray ToArray(IList self)
		{
			RubyArray rubyArray = new RubyArray(self.Count);
			foreach (object item in self)
			{
				rubyArray.Add(item);
			}
			return rubyArray;
		}

		[RubyMethod("*")]
		public static IList Repeat(UnaryOpStorage allocateStorage, IList self, int repeat)
		{
			if (repeat < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative argument");
			}
			IList list = CreateResultArray(allocateStorage, self);
			RubyArray rubyArray = list as RubyArray;
			if (rubyArray != null)
			{
				rubyArray.AddCapacity(self.Count * repeat);
			}
			for (int i = 0; i < repeat; i++)
			{
				AddRange(list, self);
			}
			allocateStorage.Context.TaintObjectBy(list, self);
			return list;
		}

		[RubyMethod("*")]
		public static MutableString Repeat(JoinConversionStorage conversions, IList self, [NotNull] MutableString separator)
		{
			return Join(conversions, self, separator);
		}

		[RubyMethod("*")]
		public static object Repeat(UnaryOpStorage allocateStorage, JoinConversionStorage conversions, IList self, [DefaultProtocol][NotNull] Union<MutableString, int> repeat)
		{
			if (repeat.IsFixnum())
			{
				return Repeat(allocateStorage, self, repeat.Fixnum());
			}
			return Repeat(conversions, self, repeat.String());
		}

		[RubyMethod("+")]
		public static RubyArray Concatenate(IList self, [NotNull][DefaultProtocol] IList other)
		{
			RubyArray rubyArray = new RubyArray(self.Count + other.Count);
			AddRange(rubyArray, self);
			AddRange(rubyArray, other);
			return rubyArray;
		}

		[RubyMethod("concat")]
		public static IList Concat(IList self, [NotNull][DefaultProtocol] IList other)
		{
			AddRange(self, other);
			return self;
		}

		[RubyMethod("-")]
		public static RubyArray Difference(UnaryOpStorage hashStorage, BinaryOpStorage eqlStorage, IList self, [DefaultProtocol][NotNull] IList other)
		{
			RubyArray rubyArray = new RubyArray();
			Dictionary<object, bool> dictionary = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
			bool flag = false;
			foreach (object item in other)
			{
				if (item != null)
				{
					dictionary[item] = true;
				}
				else
				{
					flag = true;
				}
			}
			foreach (object item2 in self)
			{
				if (!((item2 != null) ? dictionary.ContainsKey(item2) : flag))
				{
					rubyArray.Add(item2);
				}
			}
			return rubyArray;
		}

		internal static int IndexOf(CallSite<Func<CallSite, object, object, object>> equalitySite, IList self, object item)
		{
			for (int i = 0; i < self.Count; i++)
			{
				if (Protocols.IsTrue(equalitySite.Target(equalitySite, item, self[i])))
				{
					return i;
				}
			}
			return -1;
		}

		[RubyMethod("product")]
		public static RubyArray Product(IList self, [DefaultProtocol][NotNullItems] params IList[] arrays)
		{
			RubyArray rubyArray = new RubyArray();
			if (self.Count == 0)
			{
				return rubyArray;
			}
			for (int i = 0; i < arrays.Length; i++)
			{
				if (arrays[i].Count == 0)
				{
					return rubyArray;
				}
			}
			int[] array = new int[1 + arrays.Length];
			while (true)
			{
				RubyArray rubyArray2 = new RubyArray(array.Length);
				for (int j = 0; j < array.Length; j++)
				{
					rubyArray2[j] = GetNth(j, self, arrays)[array[j]];
				}
				rubyArray.Add(rubyArray2);
				int num = array.Length - 1;
				while (num >= 0)
				{
					int num2 = array[num] + 1;
					if (num2 < GetNth(num, self, arrays).Count)
					{
						array[num] = num2;
						break;
					}
					if (num > 0)
					{
						array[num] = 0;
						num--;
						continue;
					}
					return rubyArray;
				}
			}
		}

		private static IList GetNth(int n, IList first, IList[] items)
		{
			if (n != 0)
			{
				return items[n - 1];
			}
			return first;
		}

		[RubyMethod("==")]
		public static bool Equals(RespondToStorage respondTo, BinaryOpStorage equals, IList self, object other)
		{
			if (Protocols.RespondTo(respondTo, other, "to_ary"))
			{
				return Protocols.IsEqual(equals, other, self);
			}
			return false;
		}

		[RubyMethod("==")]
		public static bool Equals(BinaryOpStorage equals, IList self, [NotNull] IList other)
		{
			if (object.ReferenceEquals(self, other))
			{
				return true;
			}
			if (self.Count != other.Count)
			{
				return false;
			}
			using (IDisposable disposable = _EqualsTracker.TrackObject(self))
			{
				using (IDisposable disposable2 = _EqualsTracker.TrackObject(other))
				{
					if (disposable == null && disposable2 == null)
					{
						return true;
					}
					CallSite<Func<CallSite, object, object, object>> callSite = equals.GetCallSite("==");
					for (int i = 0; i < self.Count; i++)
					{
						if (!Protocols.IsEqual(callSite, self[i], other[i]))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage comparisonStorage, ConversionStorage<IList> toAry, IList self, object other)
		{
			IList list = Protocols.TryCastToArray(toAry, other);
			if (list == null)
			{
				return null;
			}
			return Compare(comparisonStorage, self, list);
		}

		[RubyMethod("<=>")]
		public static object Compare(BinaryOpStorage comparisonStorage, IList self, [NotNull] IList other)
		{
			using (IDisposable disposable = _ComparisonTracker.TrackObject(self))
			{
				using (IDisposable disposable2 = _ComparisonTracker.TrackObject(other))
				{
					if (disposable == null && disposable2 == null)
					{
						return ScriptingRuntimeHelpers.Int32ToObject(0);
					}
					int num = Math.Min(self.Count, other.Count);
					CallSite<Func<CallSite, object, object, object>> callSite = comparisonStorage.GetCallSite("<=>");
					for (int i = 0; i < num; i++)
					{
						object obj = callSite.Target(callSite, self[i], other[i]);
						if (!(obj is int) || (int)obj != 0)
						{
							return obj;
						}
					}
					return ScriptingRuntimeHelpers.Int32ToObject(Math.Sign(self.Count - other.Count));
				}
			}
		}

		[RubyMethod("eql?")]
		public static bool HashEquals(BinaryOpStorage eqlStorage, IList self, object other)
		{
			return RubyArray.Equals(eqlStorage, self, other);
		}

		[RubyMethod("hash")]
		public static int GetHashCode(UnaryOpStorage hashStorage, ConversionStorage<int> fixnumCast, IList self)
		{
			return RubyArray.GetHashCode(hashStorage, fixnumCast, self);
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static object GetElement(IList self, [DefaultProtocol] int index)
		{
			if (!InRangeNormalized(self, ref index))
			{
				return null;
			}
			return self[index];
		}

		[RubyMethod("slice")]
		[RubyMethod("[]")]
		public static IList GetElements(UnaryOpStorage allocateStorage, IList self, [DefaultProtocol] int index, [DefaultProtocol] int count)
		{
			if (!NormalizeRange(self.Count, ref index, ref count))
			{
				return null;
			}
			return GetResultRange(allocateStorage, self, index, count);
		}

		[RubyMethod("[]")]
		[RubyMethod("slice")]
		public static IList GetElements(ConversionStorage<int> fixnumCast, UnaryOpStorage allocateStorage, IList self, [NotNull] Range range)
		{
			int begin;
			int count;
			if (!NormalizeRange(fixnumCast, self.Count, range, out begin, out count))
			{
				return null;
			}
			if (count >= 0)
			{
				return GetElements(allocateStorage, self, begin, count);
			}
			return CreateResultArray(allocateStorage, self);
		}

		[RubyMethod("at")]
		public static object At(IList self, [DefaultProtocol] int index)
		{
			return GetElement(self, index);
		}

		public static void ExpandList(IList list, int index)
		{
			int num = index - list.Count;
			for (int i = 0; i < num; i++)
			{
				list.Add(null);
			}
		}

		public static void OverwriteOrAdd(IList list, int index, object value)
		{
			if (index < list.Count)
			{
				list[index] = value;
			}
			else
			{
				list.Add(value);
			}
		}

		public static void DeleteItems(IList list, int index, int length)
		{
			if (index >= list.Count)
			{
				ExpandList(list, index);
				return;
			}
			if (index + length > list.Count)
			{
				length = list.Count - index;
			}
			if (length == 0)
			{
				RequireNotFrozen(list);
			}
			else
			{
				RemoveRange(list, index, length);
			}
		}

		[RubyMethod("[]=")]
		public static object SetElement(RubyArray self, [DefaultProtocol] int index, object value)
		{
			index = NormalizeIndexThrowIfNegative(self, index);
			if (index >= self.Count)
			{
				self.AddMultiple(index + 1 - self.Count, null);
			}
			return self[index] = value;
		}

		[RubyMethod("[]=")]
		public static object SetElement(IList self, [DefaultProtocol] int index, object value)
		{
			index = NormalizeIndexThrowIfNegative(self, index);
			if (index < self.Count)
			{
				self[index] = value;
			}
			else
			{
				ExpandList(self, index);
				self.Add(value);
			}
			return value;
		}

		[RubyMethod("[]=")]
		public static object SetElement(ConversionStorage<IList> arrayTryCast, IList self, [DefaultProtocol] int index, [DefaultProtocol] int length, object value)
		{
			if (length < 0)
			{
				throw RubyExceptions.CreateIndexError("negative length ({0})", length);
			}
			index = NormalizeIndexThrowIfNegative(self, index);
			IList list = value as IList;
			if (list == null)
			{
				list = Protocols.TryCastToArray(arrayTryCast, value);
			}
			if (list != null && list.Count == 0)
			{
				DeleteItems(self, index, length);
			}
			else if (list == null)
			{
				Insert(self, index, value);
				if (length > 0)
				{
					RemoveRange(self, index + 1, Math.Min(length, self.Count - index - 1));
				}
			}
			else
			{
				if (value == self)
				{
					object[] array = new object[self.Count];
					self.CopyTo(array, 0);
					list = array;
				}
				ExpandList(self, index);
				int num = ((length > list.Count) ? list.Count : length);
				for (int i = 0; i < num; i++)
				{
					OverwriteOrAdd(self, index + i, list[i]);
				}
				if (length < list.Count)
				{
					InsertRange(self, index + num, list, num, list.Count - num);
				}
				else
				{
					RemoveRange(self, index + num, Math.Min(length - list.Count, self.Count - (index + num)));
				}
			}
			return value;
		}

		[RubyMethod("[]=")]
		public static object SetElement(ConversionStorage<IList> arrayTryCast, ConversionStorage<int> fixnumCast, IList self, [NotNull] Range range, object value)
		{
			int start;
			int count;
			RangeToStartAndCount(fixnumCast, range, self.Count, out start, out count);
			return SetElement(arrayTryCast, self, start, count, value);
		}

		private static void RangeToStartAndCount(ConversionStorage<int> fixnumCast, Range range, int length, out int start, out int count)
		{
			start = Protocols.CastToFixnum(fixnumCast, range.Begin);
			int num = Protocols.CastToFixnum(fixnumCast, range.End);
			start = ((start < 0) ? (start + length) : start);
			if (start < 0)
			{
				throw RubyExceptions.CreateRangeError("{0}..{1} out of range", start, num);
			}
			num = ((num < 0) ? (num + length) : num);
			count = Math.Max(range.ExcludeEnd ? (num - start) : (num - start + 1), 0);
		}

		[RubyMethod("&")]
		public static RubyArray Intersection(UnaryOpStorage hashStorage, BinaryOpStorage eqlStorage, IList self, [DefaultProtocol] IList other)
		{
			Dictionary<object, bool> dictionary = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
			RubyArray rubyArray = new RubyArray();
			foreach (object item in other)
			{
				dictionary[item] = true;
			}
			foreach (object item2 in self)
			{
				if (dictionary.Remove(item2))
				{
					rubyArray.Add(item2);
					if (dictionary.Count == 0)
					{
						return rubyArray;
					}
				}
			}
			return rubyArray;
		}

		private static void AddUniqueItems(IList list, IList result, Dictionary<object, bool> seen, ref bool nilSeen)
		{
			foreach (object item in list)
			{
				if (item == null)
				{
					if (!nilSeen)
					{
						nilSeen = true;
						result.Add(null);
					}
				}
				else if (!seen.ContainsKey(item))
				{
					seen.Add(item, true);
					result.Add(item);
				}
			}
		}

		[RubyMethod("|")]
		public static RubyArray Union(UnaryOpStorage hashStorage, BinaryOpStorage eqlStorage, IList self, [DefaultProtocol] IList other)
		{
			Dictionary<object, bool> seen = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
			bool nilSeen = false;
			RubyArray result = new RubyArray();
			AddUniqueItems(self, result, seen, ref nilSeen);
			AddUniqueItems(other, result, seen, ref nilSeen);
			return result;
		}

		public static IList GetContainerOf(BinaryOpStorage equals, IList list, int index, object item)
		{
			foreach (object item2 in list)
			{
				IList list2 = item2 as IList;
				if (list2 != null && list2.Count > index && Protocols.IsEqual(equals, list2[index], item))
				{
					return list2;
				}
			}
			return null;
		}

		[RubyMethod("assoc")]
		public static IList GetContainerOfFirstItem(BinaryOpStorage equals, IList self, object item)
		{
			return GetContainerOf(equals, self, 0, item);
		}

		[RubyMethod("rassoc")]
		public static IList GetContainerOfSecondItem(BinaryOpStorage equals, IList self, object item)
		{
			return GetContainerOf(equals, self, 1, item);
		}

		[RubyMethod("map!")]
		[RubyMethod("collect!")]
		public static object CollectInPlace(BlockParam collector, IList self)
		{
			if (collector == null)
			{
				return new Enumerator((RubyScope _, BlockParam block) => CollectInPlaceImpl(block, self));
			}
			return CollectInPlaceImpl(collector, self);
		}

		private static object CollectInPlaceImpl(BlockParam collector, IList self)
		{
			for (int i = 0; i < self.Count; i++)
			{
				object blockResult;
				if (collector.Yield(self[i], out blockResult))
				{
					return blockResult;
				}
				self[i] = blockResult;
			}
			return self;
		}

		[RubyMethod("compact")]
		public static IList Compact(UnaryOpStorage allocateStorage, IList self)
		{
			IList list = CreateResultArray(allocateStorage, self);
			foreach (object item in self)
			{
				if (item != null)
				{
					list.Add(item);
				}
			}
			allocateStorage.Context.TaintObjectBy(list, self);
			return list;
		}

		[RubyMethod("compact!")]
		public static IList CompactInPlace(IList self)
		{
			RequireNotFrozen(self);
			bool flag = false;
			int num = 0;
			while (num < self.Count)
			{
				if (self[num] == null)
				{
					flag = true;
					self.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			if (!flag)
			{
				return null;
			}
			return self;
		}

		public static bool Remove(BinaryOpStorage equals, IList self, object item)
		{
			int num = 0;
			bool result = false;
			while (num < self.Count)
			{
				if (Protocols.IsEqual(equals, self[num], item))
				{
					self.RemoveAt(num);
					result = true;
				}
				else
				{
					num++;
				}
			}
			return result;
		}

		[RubyMethod("delete")]
		public static object Delete(BinaryOpStorage equals, IList self, object item)
		{
			if (!Remove(equals, self, item))
			{
				return null;
			}
			return item;
		}

		[RubyMethod("delete")]
		public static object Delete(BinaryOpStorage equals, BlockParam block, IList self, object item)
		{
			bool flag = Remove(equals, self, item);
			if (!flag && block != null)
			{
				object blockResult;
				block.Yield(out blockResult);
				return blockResult;
			}
			if (!flag)
			{
				return null;
			}
			return item;
		}

		[RubyMethod("delete_at")]
		public static object DeleteAt(IList self, [DefaultProtocol] int index)
		{
			index = ((index < 0) ? (index + self.Count) : index);
			if (index < 0 || index >= self.Count)
			{
				return null;
			}
			object element = GetElement(self, index);
			self.RemoveAt(index);
			return element;
		}

		[RubyMethod("delete_if")]
		public static object DeleteIf(BlockParam block, IList self)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => DeleteIfImpl(innerBlock, self));
			}
			return DeleteIfImpl(block, self);
		}

		private static object DeleteIfImpl(BlockParam block, IList self)
		{
			bool changed;
			bool jumped;
			DeleteIf(block, self, out changed, out jumped);
			return self;
		}

		[RubyMethod("reject!")]
		public static object RejectInPlace(BlockParam block, IList self)
		{
			if (block == null)
			{
				return new Enumerator((RubyScope _, BlockParam innerBlock) => RejectInPlaceImpl(innerBlock, self));
			}
			return RejectInPlaceImpl(block, self);
		}

		private static object RejectInPlaceImpl(BlockParam block, IList self)
		{
			bool changed;
			bool jumped;
			object result = DeleteIf(block, self, out changed, out jumped);
			if (!jumped)
			{
				if (!changed)
				{
					return null;
				}
				return self;
			}
			return result;
		}

		[RubyMethod("reject")]
		public static object Reject(CallSiteStorage<Func<CallSite, object, Proc, object>> each, UnaryOpStorage allocate, BlockParam predicate, IList self)
		{
			if (predicate == null)
			{
				return new Enumerator((RubyScope _, BlockParam block) => RejectImpl(each, allocate, block, self));
			}
			return RejectImpl(each, allocate, predicate, self);
		}

		private static object RejectImpl(CallSiteStorage<Func<CallSite, object, Proc, object>> each, UnaryOpStorage allocate, BlockParam predicate, IList self)
		{
			IList list = CreateResultArray(allocate, self);
			for (int i = 0; i < self.Count; i++)
			{
				object obj = self[i];
				object blockResult;
				if (predicate.Yield(obj, out blockResult))
				{
					return blockResult;
				}
				if (RubyOps.IsFalse(blockResult))
				{
					list.Add(obj);
				}
			}
			return list;
		}

		private static object DeleteIf(BlockParam block, IList self, out bool changed, out bool jumped)
		{
			changed = false;
			jumped = false;
			RequireNotFrozen(self);
			int num = 0;
			while (num < self.Count)
			{
				object blockResult;
				if (block.Yield(self[num], out blockResult))
				{
					jumped = true;
					return blockResult;
				}
				if (RubyOps.IsTrue(blockResult))
				{
					changed = true;
					self.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			return null;
		}

		[RubyMethod("each")]
		public static Enumerator Each(IList self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Each(block, self));
		}

		[RubyMethod("each")]
		public static object Each([NotNull] BlockParam block, IList self)
		{
			for (int i = 0; i < self.Count; i++)
			{
				object blockResult;
				if (block.Yield(self[i], out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("each_index")]
		public static Enumerator EachIndex(IList self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachIndex(block, self));
		}

		[RubyMethod("each_index")]
		public static object EachIndex([NotNull] BlockParam block, IList self)
		{
			for (int i = 0; i < self.Count; i++)
			{
				object blockResult;
				if (block.Yield(ScriptingRuntimeHelpers.Int32ToObject(i), out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("reverse_each")]
		public static Enumerator ReverseEach(RubyArray self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => ReverseEach(block, self));
		}

		[RubyMethod("reverse_each")]
		public static object ReverseEach([NotNull] BlockParam block, RubyArray self)
		{
			foreach (int item in ReverseEnumerateIndexes(self))
			{
				object blockResult;
				if (block.Yield(self[item], out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("fetch")]
		public static object Fetch(ConversionStorage<int> fixnumCast, BlockParam outOfRangeValueProvider, IList list, object index, [Optional] object defaultValue)
		{
			int index2 = Protocols.CastToFixnum(fixnumCast, index);
			if (InRangeNormalized(list, ref index2))
			{
				return list[index2];
			}
			if (outOfRangeValueProvider != null)
			{
				if (defaultValue != Missing.Value)
				{
					fixnumCast.Context.ReportWarning("block supersedes default value argument");
				}
				object blockResult;
				outOfRangeValueProvider.Yield(index, out blockResult);
				return blockResult;
			}
			if (defaultValue == Missing.Value)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of array", index2);
			}
			return defaultValue;
		}

		[RubyMethod("fill")]
		public static IList Fill(IList self, object obj, int start)
		{
			start = Math.Max(0, NormalizeIndex(self, start));
			for (int i = start; i < self.Count; i++)
			{
				self[i] = obj;
			}
			return self;
		}

		[RubyMethod("fill")]
		public static IList Fill(IList self, object obj, int start, int length)
		{
			start = Math.Max(0, NormalizeIndex(self, start));
			ExpandList(self, Math.Min(start, start + length));
			for (int i = 0; i < length; i++)
			{
				OverwriteOrAdd(self, start + i, obj);
			}
			return self;
		}

		[RubyMethod("fill")]
		public static IList Fill(ConversionStorage<int> fixnumCast, IList self, object obj, object start, object length)
		{
			int start2 = ((start != null) ? Protocols.CastToFixnum(fixnumCast, start) : 0);
			if (length == null)
			{
				return Fill(self, obj, start2);
			}
			return Fill(self, obj, start2, Protocols.CastToFixnum(fixnumCast, length));
		}

		[RubyMethod("fill")]
		public static IList Fill(ConversionStorage<int> fixnumCast, IList self, object obj, [NotNull] Range range)
		{
			int num = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.Begin));
			int num2 = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.End));
			int length = Math.Max(0, num2 - num + ((!range.ExcludeEnd) ? 1 : 0));
			return Fill(self, obj, num, length);
		}

		[RubyMethod("fill")]
		public static object Fill([NotNull] BlockParam block, IList self, int start)
		{
			start = Math.Max(0, NormalizeIndex(self, start));
			for (int i = start; i < self.Count; i++)
			{
				object blockResult;
				if (block.Yield(i, out blockResult))
				{
					return blockResult;
				}
				self[i] = blockResult;
			}
			return self;
		}

		[RubyMethod("fill")]
		public static object Fill([NotNull] BlockParam block, IList self, int start, int length)
		{
			start = Math.Max(0, NormalizeIndex(self, start));
			ExpandList(self, Math.Min(start, start + length));
			for (int i = start; i < start + length; i++)
			{
				object blockResult;
				if (block.Yield(i, out blockResult))
				{
					return blockResult;
				}
				OverwriteOrAdd(self, i, blockResult);
			}
			return self;
		}

		[RubyMethod("fill")]
		public static object Fill(ConversionStorage<int> fixnumCast, [NotNull] BlockParam block, IList self, object start, object length)
		{
			int start2 = ((start != null) ? Protocols.CastToFixnum(fixnumCast, start) : 0);
			if (length == null)
			{
				return Fill(block, self, start2);
			}
			return Fill(block, self, start2, Protocols.CastToFixnum(fixnumCast, length));
		}

		[RubyMethod("fill")]
		public static object Fill(ConversionStorage<int> fixnumCast, [NotNull] BlockParam block, IList self, [NotNull] Range range)
		{
			int num = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.Begin));
			int num2 = NormalizeIndex(self, Protocols.CastToFixnum(fixnumCast, range.End));
			int length = Math.Max(0, num2 - num + ((!range.ExcludeEnd) ? 1 : 0));
			return Fill(block, self, num, length);
		}

		[RubyMethod("first")]
		public static object First(IList self)
		{
			if (self.Count != 0)
			{
				return self[0];
			}
			return null;
		}

		[RubyMethod("first")]
		public static IList First(IList self, [DefaultProtocol] int count)
		{
			if (count < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
			}
			if (count > self.Count)
			{
				count = self.Count;
			}
			return new RubyArray(self, 0, count);
		}

		[RubyMethod("last")]
		public static object Last(IList self)
		{
			if (self.Count != 0)
			{
				return self[self.Count - 1];
			}
			return null;
		}

		[RubyMethod("last")]
		public static IList Last(IList self, [DefaultProtocol] int count)
		{
			if (count < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative array size (or size too big)");
			}
			if (count > self.Count)
			{
				count = self.Count;
			}
			return new RubyArray(self, self.Count - count, count);
		}

		private static int IndexOfList(ConversionStorage<IList> tryToAry, IList list, int start, out IList listItem)
		{
			for (int i = start; i < list.Count; i++)
			{
				listItem = Protocols.TryCastToArray(tryToAry, list[i]);
				if (listItem != null)
				{
					return i;
				}
			}
			listItem = null;
			return -1;
		}

		public static IEnumerable<object> EnumerateRecursively(ConversionStorage<IList> tryToAry, IList list, int maxDepth, Func<IList, object> loopDetected)
		{
			if (maxDepth == 0)
			{
				return null;
			}
			IList listItem;
			int num = IndexOfList(tryToAry, list, 0, out listItem);
			if (num == -1)
			{
				return null;
			}
			return EnumerateRecursively(tryToAry, list, list, listItem, num, maxDepth, loopDetected);
		}

        private static IEnumerable<object>/*!*/ EnumerateRecursively(ConversionStorage<IList>/*!*/ tryToAry, IList/*!*/ root,
            IList/*!*/ list, IList/*!*/ nested, int nestedIndex, int maxDepth, Func<IList, object>/*!*/ loopDetected)
        {

            Debug.Assert(nested != null);
            Debug.Assert(nestedIndex != -1);

            if (maxDepth < 0)
            {
                maxDepth = Int32.MaxValue;
            }

            var worklist = new Stack<KeyValuePair<IList, int>>();
            var recursionPath = new HashSet<object>(ReferenceEqualityComparer<object>.Instance);
            recursionPath.Add(root);
            int start = 0;

            while (true)
            {
                // "list" is the list being visited by the current work item (there might be more work items visiting the same list)
                // "nestedIndex" is the index of "nested" in the "list"

                if (nestedIndex >= 0)
                {
                    // push a work item that will process the items following the nested list:
                    worklist.Push(new KeyValuePair<IList, int>(list, nestedIndex + 1));

                    // yield items preceding the nested list:
                    for (int i = start; i < nestedIndex; i++)
                    {
                        yield return list[i];
                    }

                    // push a workitem for the nested list:
                    worklist.Push(new KeyValuePair<IList, int>(nested, 0));
                }
                else
                {
                    // there is no nested list => yield all remaining items:
                    for (int i = start; i < list.Count; i++)
                    {
                        yield return list[i];
                    }
                }

            next:
                if (worklist.Count == 0)
                {
                    break;
                }

                var workitem = worklist.Pop();
                list = workitem.Key;
                start = workitem.Value;

                // finishing nested list:
                if (start == list.Count)
                {
                    recursionPath.Remove(list);
                    goto next;
                }

                // starting nested list:
                if (start == 0 && recursionPath.Contains(list))
                {
                    yield return loopDetected(list);
                    goto next;
                }

                // set the index to -1 if we would go deeper then we should:
                nestedIndex = (recursionPath.Count < maxDepth) ? IndexOfList(tryToAry, list, start, out nested) : -1;

                // starting nested list:
                if (start == 0 && nestedIndex != -1)
                {
                    recursionPath.Add(list);
                }
            }
        }

        internal static IList Flatten(ConversionStorage<IList> tryToAry, IList list, int maxDepth, IList result)
		{
			IEnumerable<object> enumerable = EnumerateRecursively(tryToAry, list, maxDepth, delegate
			{
				throw RubyExceptions.CreateArgumentError("tried to flatten recursive array");
			});
			if (enumerable != null)
			{
				foreach (object item in enumerable)
				{
					result.Add(item);
				}
				return result;
			}
			AddRange(result, list);
			return result;
		}

		[RubyMethod("flatten")]
		public static IList Flatten(UnaryOpStorage allocateStorage, ConversionStorage<IList> tryToAry, IList self, [DefaultProtocol] int maxDepth)
		{
			return Flatten(tryToAry, self, maxDepth, CreateResultArray(allocateStorage, self));
		}

		[RubyMethod("flatten!")]
		public static IList FlattenInPlace(ConversionStorage<IList> tryToAry, RubyArray self, [DefaultProtocol] int maxDepth)
		{
			self.RequireNotFrozen();
			return FlattenInPlace(tryToAry, (IList)self, maxDepth);
		}

		[RubyMethod("flatten!")]
		public static IList FlattenInPlace(ConversionStorage<IList> tryToAry, IList self, [DefaultProtocol] int maxDepth)
		{
			if (maxDepth == 0)
			{
				return null;
			}
			IList listItem;
			int num = IndexOfList(tryToAry, self, 0, out listItem);
			if (num == -1)
			{
				return null;
			}
			object[] array = new object[self.Count - num];
			int i = 0;
			int num2 = num;
			for (; i < array.Length; i++)
			{
				array[i] = self[num2++];
			}
			bool isRecursive = false;
			IEnumerable<object> enumerable = EnumerateRecursively(tryToAry, self, array, listItem, 0, maxDepth, delegate(IList rec)
			{
				isRecursive = true;
				return rec;
			});
			int num3 = num;
			foreach (object item in enumerable)
			{
				if (num3 < self.Count)
				{
					self[num3] = item;
				}
				else
				{
					self.Add(item);
				}
				num3++;
			}
			while (self.Count > num3)
			{
				self.RemoveAt(self.Count - 1);
			}
			if (isRecursive)
			{
				throw RubyExceptions.CreateArgumentError("tried to flatten recursive array");
			}
			return self;
		}

		[RubyMethod("include?")]
		public static bool Include(BinaryOpStorage equals, IList self, object item)
		{
			return FindIndex(equals, null, self, item) != null;
		}

		[RubyMethod("find_index")]
		[RubyMethod("index")]
		public static Enumerator GetFindIndexEnumerator(BlockParam predicate, IList self)
		{
			throw new NotImplementedError("TODO: find_index enumerator");
		}

		[RubyMethod("index")]
		[RubyMethod("find_index")]
		public static object FindIndex([NotNull] BlockParam predicate, IList self)
		{
			for (int i = 0; i < self.Count; i++)
			{
				object blockResult;
				if (predicate.Yield(self[i], out blockResult))
				{
					return blockResult;
				}
				if (Protocols.IsTrue(blockResult))
				{
					return ScriptingRuntimeHelpers.Int32ToObject(i);
				}
			}
			return null;
		}

		[RubyMethod("find_index")]
		[RubyMethod("index")]
		public static object FindIndex(BinaryOpStorage equals, BlockParam predicate, IList self, object value)
		{
			if (predicate != null)
			{
				equals.Context.ReportWarning("given block not used");
			}
			for (int i = 0; i < self.Count; i++)
			{
				if (Protocols.IsEqual(equals, self[i], value))
				{
					return ScriptingRuntimeHelpers.Int32ToObject(i);
				}
			}
			return null;
		}

		[RubyMethod("rindex")]
		public static object ReverseIndex([NotNull] BlockParam predicate, IList self)
		{
			foreach (int item in ReverseEnumerateIndexes(self))
			{
				object blockResult;
				if (predicate.Yield(self[item], out blockResult))
				{
					return blockResult;
				}
				if (Protocols.IsTrue(blockResult))
				{
					return ScriptingRuntimeHelpers.Int32ToObject(item);
				}
			}
			return null;
		}

		[RubyMethod("rindex")]
		public static object ReverseIndex(BinaryOpStorage equals, BlockParam predicate, IList self, object item)
		{
			if (predicate != null)
			{
				equals.Context.ReportWarning("given block not used");
			}
			foreach (int item2 in ReverseEnumerateIndexes(self))
			{
				if (Protocols.IsEqual(equals, self[item2], item))
				{
					return ScriptingRuntimeHelpers.Int32ToObject(item2);
				}
			}
			return null;
		}

		[RubyMethod("indexes")]
		[RubyMethod("indices")]
		public static object Indexes(ConversionStorage<int> fixnumCast, UnaryOpStorage allocateStorage, IList self, params object[] values)
		{
			fixnumCast.Context.ReportWarning("Array#indexes and Array#indices are deprecated; use Array#values_at");
			RubyArray rubyArray = new RubyArray();
			for (int i = 0; i < values.Length; i++)
			{
				Range range = values[i] as Range;
				if (range != null)
				{
					IList elements = GetElements(fixnumCast, allocateStorage, self, range);
					if (elements != null)
					{
						rubyArray.Add(elements);
					}
				}
				else
				{
					rubyArray.Add(GetElement(self, Protocols.CastToFixnum(fixnumCast, values[i])));
				}
			}
			return rubyArray;
		}

		[RubyMethod("values_at")]
		public static RubyArray ValuesAt(ConversionStorage<int> fixnumCast, UnaryOpStorage allocateStorage, IList self, params object[] values)
		{
			RubyArray rubyArray = new RubyArray();
			for (int i = 0; i < values.Length; i++)
			{
				Range range = values[i] as Range;
				if (range != null)
				{
					int begin;
					int count;
					if (NormalizeRange(fixnumCast, self.Count, range, out begin, out count) && count > 0)
					{
						rubyArray.AddRange(GetElements(allocateStorage, self, begin, count));
						if (begin + count >= self.Count)
						{
							rubyArray.Add(null);
						}
					}
				}
				else
				{
					rubyArray.Add(GetElement(self, Protocols.CastToFixnum(fixnumCast, values[i])));
				}
			}
			return rubyArray;
		}

		private static void JoinRecursive(JoinConversionStorage conversions, IList list, List<MutableString> parts, ref bool? isBinary, ref Dictionary<object, bool> seen)
		{
			foreach (object item in list)
			{
				if (item == null)
				{
					parts.Add(null);
					continue;
				}
				IList list2 = conversions.ToAry.Target(conversions.ToAry, item);
				if (list2 != null)
				{
					bool value;
					if (object.ReferenceEquals(list2, list) || (seen != null && seen.TryGetValue(list2, out value)))
					{
						throw RubyExceptions.CreateArgumentError("recursive array join");
					}
					if (seen == null)
					{
						seen = new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance);
					}
					seen.Add(list2, true);
					JoinRecursive(conversions, list2, parts, ref isBinary, ref seen);
					seen.Remove(list2);
				}
				else
				{
					MutableString mutableString = conversions.ToStr.Target(conversions.ToStr, item) ?? conversions.ToS.Target(conversions.ToS, item);
					parts.Add(mutableString);
					bool? obj;
					if (!isBinary.HasValue)
					{
						obj = mutableString.IsBinary;
					}
					else
					{
						bool? flag = isBinary;
						obj = mutableString.IsBinary | flag;
					}
					isBinary = obj;
				}
			}
		}

		public static MutableString Join(JoinConversionStorage conversions, IList self, MutableString separator)
		{
			List<MutableString> list = new List<MutableString>(self.Count);
			bool? isBinary = ((separator != null) ? new bool?(separator.IsBinary) : null);
			Dictionary<object, bool> seen = null;
			JoinRecursive(conversions, self, list, ref isBinary, ref seen);
			if (list.Count == 0)
			{
				return MutableString.CreateEmpty();
			}
			if (separator != null && separator.IsBinary != isBinary && !separator.IsAscii())
			{
				isBinary = true;
			}
			MutableString mutableString = separator;
			int num = ((separator != null) ? (((isBinary.HasValue && isBinary.Value) ? separator.GetByteCount() : separator.GetCharCount()) * (list.Count - 1)) : 0);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				MutableString mutableString2 = list[i];
				if (mutableString2 != null)
				{
					num += ((isBinary.HasValue && isBinary.Value) ? mutableString2.GetByteCount() : mutableString2.GetCharCount());
					if (mutableString == null)
					{
						mutableString = mutableString2;
					}
				}
			}
			if (mutableString == null)
			{
				return MutableString.CreateEmpty();
			}
			MutableString mutableString3 = ((isBinary.HasValue && isBinary.Value) ? MutableString.CreateBinary(num, mutableString.Encoding) : MutableString.CreateMutable(num, mutableString.Encoding));
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				MutableString mutableString4 = list[j];
				if (separator != null && j > 0)
				{
					mutableString3.Append(separator);
				}
				if (mutableString4 != null)
				{
					mutableString3.Append(mutableString4);
					mutableString3.TaintBy(mutableString4);
				}
			}
			if (separator != null)
			{
				mutableString3.TaintBy(separator);
			}
			if (!mutableString3.IsTainted || !mutableString3.IsUntrusted)
			{
				mutableString3.TaintBy(self, conversions.Context);
			}
			return mutableString3;
		}

		[RubyMethod("join")]
		public static MutableString Join(JoinConversionStorage conversions, IList self)
		{
			return Join(conversions, self, conversions.Context.ItemSeparator);
		}

		[RubyMethod("join")]
		public static MutableString JoinWithLazySeparatorConversion(JoinConversionStorage conversions, ConversionStorage<MutableString> toStr, IList self, object separator)
		{
			if (self.Count == 0)
			{
				return MutableString.CreateEmpty();
			}
			return Join(conversions, self, (separator != null) ? Protocols.CastToString(toStr, separator) : null);
		}

		[RubyMethod("inspect")]
		[RubyMethod("to_s")]
		public static MutableString Inspect(RubyContext context, IList self)
		{
			using (IDisposable disposable = RubyUtils.InfiniteInspectTracker.TrackObject(self))
			{
				if (disposable == null)
				{
					return MutableString.CreateAscii("[...]");
				}
				MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
				mutableString.Append('[');
				bool flag = true;
				foreach (object item in self)
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						mutableString.Append(", ");
					}
					mutableString.Append(context.Inspect(item));
				}
				mutableString.Append(']');
				return mutableString;
			}
		}

		[RubyMethod("count")]
		[RubyMethod("length")]
		[RubyMethod("size")]
		public static int Length(IList self)
		{
			return self.Count;
		}

		[RubyMethod("empty?")]
		[RubyMethod("none?")]
		public static bool Empty(IList self)
		{
			return self.Count == 0;
		}

		[RubyMethod("nitems")]
		public static int NumberOfNonNilItems(IList self)
		{
			int num = 0;
			foreach (object item in self)
			{
				if (item != null)
				{
					num++;
				}
			}
			return num;
		}

		[RubyMethod("insert")]
		public static IList Insert(IList self, [DefaultProtocol] int index, params object[] args)
		{
			if (args.Length == 0)
			{
				RubyArray rubyArray = self as RubyArray;
				if (rubyArray != null)
				{
					rubyArray.RequireNotFrozen();
				}
				return self;
			}
			if (index == -1)
			{
				AddRange(self, args);
				return self;
			}
			index = ((index < 0) ? (index + self.Count + 1) : index);
			if (index < 0)
			{
				throw RubyExceptions.CreateIndexError("index {0} out of array", index);
			}
			if (index >= self.Count)
			{
				ExpandList(self, index);
				AddRange(self, args);
				return self;
			}
			InsertRange(self, index, args, 0, args.Length);
			return self;
		}

		[RubyMethod("push")]
		public static IList Push(IList self, params object[] values)
		{
			AddRange(self, values);
			return self;
		}

		[RubyMethod("pop")]
		public static object Pop(IList self)
		{
			if (self.Count == 0)
			{
				return null;
			}
			object result = self[self.Count - 1];
			self.RemoveAt(self.Count - 1);
			return result;
		}

		[RubyMethod("pop")]
		public static object Pop(RubyContext context, IList self, [DefaultProtocol] int count)
		{
			RequireNotFrozen(self);
			if (count < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative array size");
			}
			if (count == 0 || self.Count == 0)
			{
				return new RubyArray();
			}
			int num = ((count <= self.Count) ? count : self.Count);
			int num2 = self.Count - num;
			RubyArray result = new RubyArray(self, num2, num);
			RemoveRange(self, num2, num);
			return result;
		}

		[RubyMethod("shift")]
		public static object Shift(IList self)
		{
			if (self.Count == 0)
			{
				return null;
			}
			object result = self[0];
			self.RemoveAt(0);
			return result;
		}

		[RubyMethod("unshift")]
		public static IList Unshift(IList self, object arg)
		{
			self.Insert(0, arg);
			return self;
		}

		[RubyMethod("unshift")]
		public static IList Unshift(IList self, params object[] args)
		{
			InsertRange(self, 0, args, 0, args.Length);
			return self;
		}

		[RubyMethod("<<")]
		public static IList Append(IList self, object value)
		{
			self.Add(value);
			return self;
		}

		[RubyMethod("slice!")]
		public static object SliceInPlace(IList self, [DefaultProtocol] int index)
		{
			index = ((index < 0) ? (index + self.Count) : index);
			if (index >= 0 && index < self.Count)
			{
				object result = self[index];
				DeleteElements(self, index, 1);
				return result;
			}
			return null;
		}

		[RubyMethod("slice!")]
		public static IList SliceInPlace(ConversionStorage<int> fixnumCast, UnaryOpStorage allocateStorage, IList self, [NotNull] Range range)
		{
			IList elements = GetElements(fixnumCast, allocateStorage, self, range);
			int start;
			int count;
			RangeToStartAndCount(fixnumCast, range, self.Count, out start, out count);
			DeleteElements(self, start, count);
			return elements;
		}

		[RubyMethod("slice!")]
		public static IList SliceInPlace(UnaryOpStorage allocateStorage, IList self, [DefaultProtocol] int start, [DefaultProtocol] int length)
		{
			IList elements = GetElements(allocateStorage, self, start, length);
			DeleteElements(self, start, length);
			return elements;
		}

		private static void DeleteElements(IList self, int start, int count)
		{
			if (count < 0)
			{
				throw RubyExceptions.CreateIndexError("negative length ({0})", count);
			}
			DeleteItems(self, NormalizeIndexThrowIfNegative(self, start), count);
		}

		[RubyMethod("sort")]
		public static object Sort(UnaryOpStorage allocateStorage, ComparisonStorage comparisonStorage, BlockParam block, IList self)
		{
			IList list = CreateResultArray(allocateStorage, self);
			StrongBox<object> breakResult;
			RubyArray other = ArrayOps.SortInPlace(comparisonStorage, block, ToArray(self), out breakResult);
			if (breakResult == null)
			{
				Replace(list, other);
				return list;
			}
			return breakResult.Value;
		}

		[RubyMethod("sort!")]
		public static object SortInPlace(ComparisonStorage comparisonStorage, BlockParam block, IList self)
		{
			StrongBox<object> breakResult;
			RubyArray other = ArrayOps.SortInPlace(comparisonStorage, block, ToArray(self), out breakResult);
			if (breakResult == null)
			{
				Replace(self, other);
				return self;
			}
			return breakResult.Value;
		}

		[RubyMethod("shuffle")]
		public static IList Shuffle(UnaryOpStorage allocateStorage, RubyArray self)
		{
			IList list = CreateResultArray(allocateStorage, self);
			if (self.Count == 0)
			{
				return list;
			}
			RubyArray rubyArray = list as RubyArray;
			if (rubyArray != null && rubyArray.Count < self.Count)
			{
				rubyArray.AddCapacity(self.Count - rubyArray.Count);
			}
			Random randomNumberGenerator = allocateStorage.Context.RandomNumberGenerator;
			list.Add(self[0]);
			for (int i = 1; i < self.Count; i++)
			{
				int num = randomNumberGenerator.Next(i + 1);
				list.Add((num < list.Count) ? list[num] : null);
				list[num] = self[i];
			}
			return list;
		}

		[RubyMethod("shuffle!")]
		public static RubyArray ShuffleInPlace(RubyContext context, RubyArray self)
		{
			Random randomNumberGenerator = context.RandomNumberGenerator;
			for (int num = self.Count - 1; num >= 0; num--)
			{
				int index = randomNumberGenerator.Next(num + 1);
				object value = self[num];
				self[num] = self[index];
				self[index] = value;
			}
			return self;
		}

		[RubyMethod("reverse")]
		public static IList Reverse(UnaryOpStorage allocateStorage, IList self)
		{
			IList list = CreateResultArray(allocateStorage, self);
			if (list is RubyArray)
			{
				(list as RubyArray).AddCapacity(self.Count);
			}
			for (int i = 0; i < self.Count; i++)
			{
				list.Add(self[self.Count - i - 1]);
			}
			return list;
		}

		[RubyMethod("reverse!")]
		public static IList InPlaceReverse(IList self)
		{
			int num = self.Count / 2;
			int num2 = self.Count - 1;
			for (int i = 0; i < num; i++)
			{
				int index = num2 - i;
				object value = self[i];
				self[i] = self[index];
				self[index] = value;
			}
			return self;
		}

		[RubyMethod("transpose")]
		public static RubyArray Transpose(ConversionStorage<IList> arrayCast, IList self)
		{
			RubyArray rubyArray = new RubyArray();
			for (int i = 0; i < self.Count; i++)
			{
				IList list = Protocols.CastToArray(arrayCast, self[i]);
				if (i == 0)
				{
					rubyArray.AddCapacity(list.Count);
					for (int j = 0; j < list.Count; j++)
					{
						rubyArray.Add(new RubyArray());
					}
				}
				else if (list.Count != rubyArray.Count)
				{
					throw RubyExceptions.CreateIndexError("element size differs ({0} should be {1})", list.Count, rubyArray.Count);
				}
				for (int k = 0; k < rubyArray.Count; k++)
				{
					((RubyArray)rubyArray[k]).Add(list[k]);
				}
			}
			return rubyArray;
		}

		[RubyMethod("uniq")]
		public static IList Unique(UnaryOpStorage allocateStorage, IList self)
		{
			IList result = CreateResultArray(allocateStorage, self);
			Dictionary<object, bool> seen = new Dictionary<object, bool>(allocateStorage.Context.EqualityComparer);
			bool nilSeen = false;
			AddUniqueItems(self, result, seen, ref nilSeen);
			return result;
		}

		[RubyMethod("uniq!")]
		public static IList UniqueSelf(UnaryOpStorage hashStorage, BinaryOpStorage eqlStorage, RubyArray self)
		{
			self.RequireNotFrozen();
			return UniqueSelf(hashStorage, eqlStorage, (IList)self);
		}

		[RubyMethod("uniq!")]
		public static IList UniqueSelf(UnaryOpStorage hashStorage, BinaryOpStorage eqlStorage, IList self)
		{
			Dictionary<object, bool> dictionary = new Dictionary<object, bool>(new EqualityComparer(hashStorage, eqlStorage));
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			while (num < self.Count)
			{
				object obj = self[num];
				if (obj != null && !dictionary.ContainsKey(obj))
				{
					dictionary.Add(obj, true);
					num++;
				}
				else if (obj == null && !flag)
				{
					flag = true;
					num++;
				}
				else
				{
					self.RemoveAt(num);
					flag2 = true;
				}
			}
			if (!flag2)
			{
				return null;
			}
			return self;
		}

		[RubyMethod("permutation")]
		public static object GetPermutations(BlockParam block, IList self, [Optional][DefaultProtocol] int? size)
		{
			PermutationEnumerator permutationEnumerator = new PermutationEnumerator(self, size);
			if (block == null)
			{
				return new Enumerator(permutationEnumerator);
			}
			return permutationEnumerator.Each(null, block);
		}

		[RubyMethod("combination")]
		public static object GetCombinations(BlockParam block, IList self, [Optional][DefaultProtocol] int? size)
		{
			CombinationEnumerator combinationEnumerator = new CombinationEnumerator(self, size);
			if (block == null)
			{
				return new Enumerator(combinationEnumerator);
			}
			return combinationEnumerator.Each(null, block);
		}
	}
}

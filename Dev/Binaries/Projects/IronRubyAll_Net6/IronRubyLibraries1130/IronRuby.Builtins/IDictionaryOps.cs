using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule(Extends = typeof(IDictionary<object, object>), Restrictions = ModuleRestrictions.None)]
	[Includes(new Type[] { typeof(Enumerable) })]
	public static class IDictionaryOps
	{
		private static RubyUtils.RecursionTracker _EqualsTracker = new RubyUtils.RecursionTracker();

		internal static RubyArray MakeArray(KeyValuePair<object, object> pair)
		{
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(CustomStringDictionary.ObjToNull(pair.Key));
			rubyArray.Add(pair.Value);
			return rubyArray;
		}

		internal static RubyArray MakeArray(object key, object value)
		{
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(CustomStringDictionary.ObjToNull(key));
			rubyArray.Add(value);
			return rubyArray;
		}

		internal static T ReplaceData<T>(T dest, IEnumerable<KeyValuePair<object, object>> src) where T : IDictionary<object, object>
		{
			dest.Clear();
			foreach (KeyValuePair<object, object> item in src)
			{
				dest[item.Key] = item.Value;
			}
			return dest;
		}

		private static IEnumerable<KeyValuePair<object, object>> CopyKeyValuePairs(IDictionary<object, object> dict)
		{
			KeyValuePair<object, object>[] array = new KeyValuePair<object, object>[dict.Count];
			dict.CopyTo(array, 0);
			return array;
		}

		[RubyMethod("==")]
		public static bool Equals(RespondToStorage respondTo, BinaryOpStorage equals, IDictionary<object, object> self, object other)
		{
			if (Protocols.RespondTo(respondTo, other, "to_hash"))
			{
				return Protocols.IsEqual(equals, other, self);
			}
			return false;
		}

		[RubyMethod("==")]
		public static bool Equals(BinaryOpStorage equals, IDictionary<object, object> self, [NotNull] IDictionary<object, object> other)
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
					foreach (KeyValuePair<object, object> item in self)
					{
						object value;
						if (!other.TryGetValue(item.Key, out value) || !Protocols.IsEqual(callSite, item.Value, value))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		[RubyMethod("[]")]
		public static object GetElement(RubyContext context, IDictionary<object, object> self, object key)
		{
			object value;
			if (!self.TryGetValue(CustomStringDictionary.NullToObj(key), out value))
			{
				return null;
			}
			return value;
		}

		[RubyMethod("[]=")]
		[RubyMethod("store")]
		public static object SetElement(RubyContext context, Hash self, object key, object value)
		{
			self.RequireNotFrozen();
			return RubyUtils.SetHashElement(context, self, key, value);
		}

		[RubyMethod("store")]
		[RubyMethod("[]=")]
		public static object SetElement(RubyContext context, IDictionary<object, object> self, object key, object value)
		{
			return RubyUtils.SetHashElement(context, self, key, value);
		}

		[RubyMethod("clear")]
		public static IDictionary<object, object> Clear(Hash self)
		{
			self.RequireNotFrozen();
			self.Clear();
			return self;
		}

		[RubyMethod("clear")]
		public static IDictionary<object, object> Clear(IDictionary<object, object> self)
		{
			self.Clear();
			return self;
		}

		private static IDictionary<object, object> Duplicate(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, IDictionary<object, object> self)
		{
			IDictionary<object, object> dest = (IDictionary<object, object>)KernelOps.Duplicate(initializeCopyStorage, allocateStorage, self);
			return ReplaceData(dest, self);
		}

		[RubyMethod("default")]
		public static object GetDefaultValue(RubyContext context, IDictionary<object, object> self, [Optional] object key)
		{
			return null;
		}

		[RubyMethod("default_proc")]
		public static Proc GetDefaultProc(IDictionary<object, object> self)
		{
			return null;
		}

		[RubyMethod("delete")]
		public static object Delete(BlockParam block, Hash self, object key)
		{
			self.RequireNotFrozen();
			return Delete(block, (IDictionary<object, object>)self, key);
		}

		[RubyMethod("delete")]
		public static object Delete(BlockParam block, IDictionary<object, object> self, object key)
		{
			object value;
			if (!self.TryGetValue(CustomStringDictionary.NullToObj(key), out value))
			{
				if (block != null)
				{
					object blockResult;
					block.Yield(key, out blockResult);
					return blockResult;
				}
				return null;
			}
			self.Remove(CustomStringDictionary.NullToObj(key));
			return value;
		}

		[RubyMethod("delete_if")]
		public static object DeleteIf(BlockParam block, Hash self)
		{
			self.RequireNotFrozen();
			return DeleteIf(block, (IDictionary<object, object>)self);
		}

		[RubyMethod("delete_if")]
		public static object DeleteIf(BlockParam block, IDictionary<object, object> self)
		{
			if (self.Count > 0 && block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			RubyArray rubyArray = new RubyArray();
			foreach (KeyValuePair<object, object> item in self)
			{
				object blockResult;
				if (block.Yield(CustomStringDictionary.ObjToNull(item.Key), item.Value, out blockResult))
				{
					return blockResult;
				}
				if (RubyOps.IsTrue(blockResult))
				{
					rubyArray.Add(item.Key);
				}
			}
			foreach (object item2 in rubyArray)
			{
				self.Remove(item2);
			}
			return self;
		}

		[RubyMethod("each_pair")]
		[RubyMethod("each")]
		public static Enumerator Each(RubyContext context, IDictionary<object, object> self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Each(context, block, self));
		}

		[RubyMethod("each_pair")]
		[RubyMethod("each")]
		public static object Each(RubyContext context, [NotNull] BlockParam block, IDictionary<object, object> self)
		{
			if (self.Count > 0)
			{
				object[] array = new object[self.Count];
				self.Keys.CopyTo(array, 0);
				for (int i = 0; i < array.Length; i++)
				{
					object blockResult;
					if (block.Yield(MakeArray(array[i], self[array[i]]), out blockResult))
					{
						return blockResult;
					}
				}
			}
			return self;
		}

		[RubyMethod("each_key")]
		public static Enumerator EachKey(RubyContext context, IDictionary<object, object> self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachKey(context, block, self));
		}

		[RubyMethod("each_key")]
		public static object EachKey(RubyContext context, [NotNull] BlockParam block, IDictionary<object, object> self)
		{
			if (self.Count > 0)
			{
				object[] array = new object[self.Count];
				self.Keys.CopyTo(array, 0);
				for (int i = 0; i < array.Length; i++)
				{
					object blockResult;
					if (block.Yield(CustomStringDictionary.ObjToNull(array[i]), out blockResult))
					{
						return blockResult;
					}
				}
			}
			return self;
		}

		[RubyMethod("each_value")]
		public static Enumerator EachValue(RubyContext context, IDictionary<object, object> self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => EachValue(context, block, self));
		}

		[RubyMethod("each_value")]
		public static object EachValue(RubyContext context, [NotNull] BlockParam block, IDictionary<object, object> self)
		{
			if (self.Count > 0)
			{
				object[] array = new object[self.Count];
				self.Values.CopyTo(array, 0);
				for (int i = 0; i < array.Length; i++)
				{
					object blockResult;
					if (block.Yield(array[i], out blockResult))
					{
						return blockResult;
					}
				}
			}
			return self;
		}

		[RubyMethod("empty?")]
		public static bool Empty(IDictionary<object, object> self)
		{
			return self.Count == 0;
		}

		[RubyMethod("fetch")]
		public static object Fetch(RubyContext context, BlockParam block, IDictionary<object, object> self, object key, [Optional] object defaultValue)
		{
			object value;
			if (self.TryGetValue(CustomStringDictionary.NullToObj(key), out value))
			{
				return value;
			}
			if (block != null)
			{
				if (defaultValue != Missing.Value)
				{
					context.ReportWarning("block supersedes default value argument");
				}
				block.Yield(key, out value);
				return value;
			}
			if (defaultValue == Missing.Value)
			{
				throw RubyExceptions.CreateIndexError("key not found");
			}
			return defaultValue;
		}

		[RubyMethod("has_key?")]
		[RubyMethod("member?")]
		[RubyMethod("include?")]
		[RubyMethod("key?")]
		public static bool HasKey(IDictionary<object, object> self, object key)
		{
			return self.ContainsKey(CustomStringDictionary.NullToObj(key));
		}

		[RubyMethod("has_value?")]
		[RubyMethod("value?")]
		public static bool HasValue(BinaryOpStorage equals, IDictionary<object, object> self, object value)
		{
			foreach (KeyValuePair<object, object> item in self)
			{
				if (Protocols.IsEqual(equals, item.Value, value))
				{
					return true;
				}
			}
			return false;
		}

		[RubyMethod("index")]
		public static object Index(BinaryOpStorage equals, IDictionary<object, object> self, object value)
		{
			foreach (KeyValuePair<object, object> item in self)
			{
				if (Protocols.IsEqual(equals, item.Value, value))
				{
					return CustomStringDictionary.ObjToNull(item.Key);
				}
			}
			return null;
		}

		[RubyMethod("invert")]
		public static Hash Invert(RubyContext context, IDictionary<object, object> self)
		{
			Hash hash = new Hash(context.EqualityComparer, self.Count);
			foreach (KeyValuePair<object, object> item in self)
			{
				hash[CustomStringDictionary.NullToObj(item.Value)] = CustomStringDictionary.ObjToNull(item.Key);
			}
			return hash;
		}

		[RubyMethod("keys")]
		public static RubyArray GetKeys(IDictionary<object, object> self)
		{
			RubyArray rubyArray = new RubyArray(self.Count);
			foreach (object key in self.Keys)
			{
				rubyArray.Add(CustomStringDictionary.ObjToNull(key));
			}
			return rubyArray;
		}

		[RubyMethod("length")]
		[RubyMethod("size")]
		public static int Length(IDictionary<object, object> self)
		{
			return self.Count;
		}

		[RubyMethod("merge")]
		public static object Merge(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, BlockParam block, IDictionary<object, object> self, [NotNull][DefaultProtocol] IDictionary<object, object> hash)
		{
			return Update(block, Duplicate(initializeCopyStorage, allocateStorage, self), hash);
		}

		[RubyMethod("update")]
		[RubyMethod("merge!")]
		public static object Update(BlockParam block, Hash self, [DefaultProtocol][NotNull] IDictionary<object, object> hash)
		{
			self.RequireNotFrozen();
			return Update(block, (IDictionary<object, object>)self, hash);
		}

		[RubyMethod("merge!")]
		[RubyMethod("update")]
		public static object Update(BlockParam block, IDictionary<object, object> self, [NotNull][DefaultProtocol] IDictionary<object, object> hash)
		{
			if (block == null)
			{
				foreach (KeyValuePair<object, object> item in CopyKeyValuePairs(hash))
				{
					self[CustomStringDictionary.NullToObj(item.Key)] = item.Value;
				}
				return self;
			}
			foreach (KeyValuePair<object, object> item2 in CopyKeyValuePairs(hash))
			{
				object key = item2.Key;
				object blockResult = item2.Value;
				object value;
				if (self.TryGetValue(key, out value) && block.Yield(CustomStringDictionary.ObjToNull(key), value, item2.Value, out blockResult))
				{
					return blockResult;
				}
				self[key] = blockResult;
			}
			return self;
		}

		[RubyMethod("rehash")]
		public static IDictionary<object, object> Rehash(Hash self)
		{
			self.RequireNotFrozen();
			return Rehash((IDictionary<object, object>)self);
		}

		[RubyMethod("rehash")]
		public static IDictionary<object, object> Rehash(IDictionary<object, object> self)
		{
			return ReplaceData(self, CopyKeyValuePairs(self));
		}

		[RubyMethod("reject")]
		public static object Reject(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, BlockParam block, IDictionary<object, object> self)
		{
			return DeleteIf(block, Duplicate(initializeCopyStorage, allocateStorage, self));
		}

		[RubyMethod("reject!")]
		public static object RejectMutate(BlockParam block, Hash self)
		{
			self.RequireNotFrozen();
			return RejectMutate(block, (IDictionary<object, object>)self);
		}

		[RubyMethod("reject!")]
		public static object RejectMutate(BlockParam block, IDictionary<object, object> self)
		{
			RubyArray rubyArray = new RubyArray();
			foreach (KeyValuePair<object, object> item in self)
			{
				object blockResult;
				if (block.Yield(CustomStringDictionary.ObjToNull(item.Key), item.Value, out blockResult))
				{
					return blockResult;
				}
				if (RubyOps.IsTrue(blockResult))
				{
					rubyArray.Add(item.Key);
				}
			}
			foreach (object item2 in rubyArray)
			{
				self.Remove(item2);
			}
			if (rubyArray.Count != 0)
			{
				return self;
			}
			return null;
		}

		[RubyMethod("replace")]
		public static Hash Replace(RubyContext context, Hash self, [DefaultProtocol][NotNull] IDictionary<object, object> other)
		{
			self.RequireNotFrozen();
			return ReplaceData(self, other);
		}

		[RubyMethod("select")]
		public static Enumerator Select(RubyContext context, IDictionary<object, object> self)
		{
			return new Enumerator((RubyScope _, BlockParam block) => Select(context, block, self));
		}

		[RubyMethod("select")]
		public static object Select(RubyContext context, [NotNull] BlockParam block, IDictionary<object, object> self)
		{
			Hash hash = new Hash(context);
			foreach (KeyValuePair<object, object> item in CopyKeyValuePairs(self))
			{
				object blockResult;
				if (block.Yield(CustomStringDictionary.ObjToNull(item.Key), item.Value, out blockResult))
				{
					return blockResult;
				}
				if (RubyOps.IsTrue(blockResult))
				{
					hash[item.Key] = item.Value;
				}
			}
			return hash;
		}

		[RubyMethod("shift")]
		public static object Shift(Hash self)
		{
			self.RequireNotFrozen();
			return Shift((IDictionary<object, object>)self);
		}

		[RubyMethod("shift")]
		public static object Shift(IDictionary<object, object> self)
		{
			if (self.Count == 0)
			{
				return null;
			}
			IEnumerator<KeyValuePair<object, object>> enumerator = self.GetEnumerator();
			enumerator.MoveNext();
			KeyValuePair<object, object> current = enumerator.Current;
			self.Remove(current.Key);
			return MakeArray(current);
		}

		[RubyMethod("sort")]
		public static object Sort(ComparisonStorage comparisonStorage, BlockParam block, IDictionary<object, object> self)
		{
			return ArrayOps.SortInPlace(comparisonStorage, block, ToArray(self));
		}

		[RubyMethod("to_a")]
		public static RubyArray ToArray(IDictionary<object, object> self)
		{
			RubyArray rubyArray = new RubyArray(self.Count);
			foreach (KeyValuePair<object, object> item in self)
			{
				rubyArray.Add(MakeArray(item));
			}
			return rubyArray;
		}

		[RubyMethod("flatten")]
		public static IList Flatten(ConversionStorage<IList> tryToAry, IDictionary<object, object> self, [DefaultProtocol] int maxDepth)
		{
			if (maxDepth == 0)
			{
				return ToArray(self);
			}
			if (maxDepth > 0)
			{
				maxDepth--;
			}
			RubyArray rubyArray = new RubyArray();
			foreach (KeyValuePair<object, object> item in self)
			{
				IList list;
				if (maxDepth != 0 && (list = Protocols.TryCastToArray(tryToAry, item.Key)) != null)
				{
					IListOps.Flatten(tryToAry, list, maxDepth - 1, rubyArray);
				}
				else
				{
					rubyArray.Add(item.Key);
				}
				if (maxDepth != 0 && (list = Protocols.TryCastToArray(tryToAry, item.Value)) != null)
				{
					IListOps.Flatten(tryToAry, list, maxDepth - 1, rubyArray);
				}
				else
				{
					rubyArray.Add(item.Value);
				}
			}
			return rubyArray;
		}

		[RubyMethod("to_hash")]
		public static IDictionary<object, object> ToHash(IDictionary<object, object> self)
		{
			return self;
		}

		[RubyMethod("inspect")]
		[RubyMethod("to_s")]
		public static MutableString ToMutableString(RubyContext context, IDictionary<object, object> self)
		{
			using (IDisposable disposable = RubyUtils.InfiniteInspectTracker.TrackObject(self))
			{
				if (disposable == null)
				{
					return MutableString.CreateAscii("{...}");
				}
				MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
				mutableString.Append('{');
				bool flag = true;
				foreach (KeyValuePair<object, object> item in self)
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						mutableString.Append(", ");
					}
					mutableString.Append(context.Inspect(CustomStringDictionary.ObjToNull(item.Key)));
					mutableString.Append("=>");
					mutableString.Append(context.Inspect(item.Value));
				}
				mutableString.Append('}');
				return mutableString;
			}
		}

		[RubyMethod("values")]
		public static RubyArray GetValues(IDictionary<object, object> self)
		{
			return new RubyArray(self.Values);
		}

		[RubyMethod("values_at")]
		public static RubyArray ValuesAt(RubyContext context, IDictionary<object, object> self, params object[] keys)
		{
			RubyArray rubyArray = new RubyArray();
			foreach (object key in keys)
			{
				rubyArray.Add(GetElement(context, self, key));
			}
			return rubyArray;
		}
	}
}

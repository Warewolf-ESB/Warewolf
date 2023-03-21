using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(Enumerable) })]
	[RubyClass("Struct", Extends = typeof(RubyStruct), Inherits = typeof(object))]
	public static class RubyStructOps
	{
		internal static readonly object TmsStructClassKey = new object();

		[RubyConstant("Tms", BuildConfig = "!SILVERLIGHT")]
		internal static RubyClass CreateTmsClass(RubyModule module)
		{
			return (RubyClass)module.Context.GetOrCreateLibraryData(TmsStructClassKey, () => RubyStruct.DefineStruct((RubyClass)module, "Tms", new string[4] { "utime", "stime", "cutime", "cstime" }));
		}

		public static RubyClass GetTmsClass(RubyContext context)
		{
			ContractUtils.RequiresNotNull(context, "context");
			object value;
			if (context.TryGetLibraryData(TmsStructClassKey, out value))
			{
				return (RubyClass)value;
			}
			context.GetClass(typeof(RubyStruct)).TryGetConstant(null, "Tms", out value);
			if (context.TryGetLibraryData(TmsStructClassKey, out value))
			{
				return (RubyClass)value;
			}
			throw Assert.Unreachable;
		}

		public static void TmsSetUserTime(RubyStruct tms, double value)
		{
			tms[0] = value;
		}

		public static void TmsSetSystemTime(RubyStruct tms, double value)
		{
			tms[1] = value;
		}

		public static void TmsSetChildUserTime(RubyStruct tms, double value)
		{
			tms[2] = value;
		}

		public static void TmsSetChildSystemTime(RubyStruct tms, double value)
		{
			tms[3] = value;
		}

		[RubyConstructor]
		public static void AllocatorUndefined(RubyClass self, params object[] args)
		{
			throw RubyExceptions.CreateAllocatorUndefinedError(self);
		}

		[RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
		public static object NewAnonymousStruct(BlockParam block, RubyClass self, [NotNull] RubySymbol firstAttibuteName, [DefaultProtocol][NotNullItems] params string[] attributeNames)
		{
			return CreateAnonymousWithFirstAttribute(block, self, RubyOps.ConvertSymbolToClrString(firstAttibuteName), attributeNames);
		}

		[RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
		public static object NewAnonymousStruct(BlockParam block, RubyClass self, [NotNull] string firstAttibuteName, [DefaultProtocol][NotNullItems] params string[] attributeNames)
		{
			return CreateAnonymousWithFirstAttribute(block, self, firstAttibuteName, attributeNames);
		}

		[RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
		public static object NewStruct(BlockParam block, RubyClass self, [DefaultProtocol] MutableString className, [DefaultProtocol][NotNullItems] params string[] attributeNames)
		{
			if (className == null)
			{
				return Create(block, self, null, attributeNames);
			}
			string text = className.ConvertToString();
			RubyUtils.CheckConstantName(text);
			return Create(block, self, text, attributeNames);
		}

		public static object CreateAnonymousWithFirstAttribute(BlockParam block, RubyClass self, string firstAttribute, string[] attributeNames)
		{
			return Create(block, self, null, ArrayUtils.Insert(firstAttribute, attributeNames));
		}

		private static object Create(BlockParam block, RubyClass self, string className, string[] attributeNames)
		{
			RubyClass rubyClass = RubyStruct.DefineStruct(self, className, attributeNames);
			if (block != null)
			{
				return RubyUtils.EvaluateInModule(rubyClass, block, null, rubyClass);
			}
			return rubyClass;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static void Reinitialize(RubyStruct self, params object[] items)
		{
			self.SetValues(items);
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static RubyStruct InitializeCopy(RubyStruct self, [NotNull] RubyStruct source)
		{
			if (self.ImmediateClass.GetNonSingletonClass() != source.ImmediateClass.GetNonSingletonClass())
			{
				throw RubyExceptions.CreateTypeError("wrong argument class");
			}
			self.SetValues(source.Values);
			return self;
		}

		[RubyMethod("members")]
		public static RubyArray GetMembers(RubyStruct self)
		{
			return RubyStruct.GetMembers(self);
		}

		[RubyMethod("length")]
		[RubyMethod("size")]
		public static int GetSize(RubyStruct self)
		{
			return self.ItemCount;
		}

		[RubyMethod("[]")]
		public static object GetValue(RubyStruct self, int index)
		{
			return self[NormalizeIndex(self.ItemCount, index)];
		}

		[RubyMethod("[]")]
		public static object GetValue(RubyStruct self, [NotNull] RubySymbol name)
		{
			return self[name.ToString()];
		}

		[RubyMethod("[]")]
		public static object GetValue(RubyStruct self, [NotNull] MutableString name)
		{
			return self[name.ConvertToString()];
		}

		[RubyMethod("[]")]
		public static object GetValue(ConversionStorage<int> conversionStorage, RubyStruct self, object index)
		{
			return self[NormalizeIndex(self.ItemCount, Protocols.CastToFixnum(conversionStorage, index))];
		}

		[RubyMethod("[]=")]
		public static object SetValue(RubyStruct self, int index, object value)
		{
			return self[NormalizeIndex(self.ItemCount, index)] = value;
		}

		[RubyMethod("[]=")]
		public static object SetValue(RubyStruct self, [NotNull] RubySymbol name, object value)
		{
			return self[name.ToString()] = value;
		}

		[RubyMethod("[]=")]
		public static object SetValue(RubyStruct self, [NotNull] MutableString name, object value)
		{
			return self[name.ConvertToString()] = value;
		}

		[RubyMethod("[]=")]
		public static object SetValue(ConversionStorage<int> conversionStorage, RubyStruct self, object index, object value)
		{
			return self[NormalizeIndex(self.ItemCount, Protocols.CastToFixnum(conversionStorage, index))] = value;
		}

		[RubyMethod("each")]
		public static object Each(BlockParam block, RubyStruct self)
		{
			if (block == null && self.ItemCount > 0)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			object[] values = self.Values;
			foreach (object arg in values)
			{
				object blockResult;
				if (block.Yield(arg, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("each_pair")]
		public static object EachPair(BlockParam block, RubyStruct self)
		{
			if (block == null && self.ItemCount > 0)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			RubyContext context = self.ImmediateClass.Context;
			foreach (KeyValuePair<string, object> item in self.GetItems())
			{
				object blockResult;
				if (block.Yield(context.EncodeIdentifier(item.Key), item.Value, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("to_a")]
		[RubyMethod("values")]
		public static RubyArray Values(RubyStruct self)
		{
			return new RubyArray(self.Values);
		}

		[RubyMethod("hash")]
		public static int Hash(UnaryOpStorage hashStorage, ConversionStorage<int> fixnumCast, RubyStruct self)
		{
			return self.GetHashCode(hashStorage, fixnumCast);
		}

		[RubyMethod("eql?")]
		public static bool Equal(BinaryOpStorage eqlStorage, RubyStruct self, object other)
		{
			return self.Equals(eqlStorage, other);
		}

		[RubyMethod("==")]
		public static bool Equals(BinaryOpStorage equals, RubyStruct self, object obj)
		{
			RubyStruct rubyStruct = obj as RubyStruct;
			if (!self.StructReferenceEquals(rubyStruct))
			{
				return false;
			}
			return IListOps.Equals(equals, self.Values, rubyStruct.Values);
		}

		[RubyMethod("inspect")]
		[RubyMethod("to_s")]
		public static MutableString Inspect(RubyStruct self)
		{
			RubyContext context = self.ImmediateClass.Context;
			using (IDisposable disposable = RubyUtils.InfiniteInspectTracker.TrackObject(self))
			{
				MutableString mutableString = MutableString.CreateMutable(RubyEncoding.Binary);
				mutableString.Append("#<struct ");
				mutableString.Append(context.Inspect(context.GetClassOf(self)));
				if (disposable == null)
				{
					return mutableString.Append(":...>");
				}
				mutableString.Append(' ');
				object[] values = self.Values;
				ReadOnlyCollection<string> names = self.GetNames();
				for (int i = 0; i < values.Length; i++)
				{
					if (i != 0)
					{
						mutableString.Append(", ");
					}
					mutableString.Append(names[i]);
					mutableString.Append('=');
					mutableString.Append(context.Inspect(values[i]));
				}
				mutableString.Append('>');
				return mutableString;
			}
		}

		[RubyMethod("select")]
		public static object Select(CallSiteStorage<Func<CallSite, object, Proc, object>> each, BlockParam predicate, RubyStruct self)
		{
			return Enumerable.Select(each, predicate, self);
		}

		[RubyMethod("values_at")]
		public static RubyArray ValuesAt(ConversionStorage<int> fixnumCast, RubyStruct self, params object[] values)
		{
			RubyArray rubyArray = new RubyArray();
			object[] values2 = self.Values;
			for (int i = 0; i < values.Length; i++)
			{
				Range range = values[i] as Range;
				if (range != null)
				{
					int index = Protocols.CastToFixnum(fixnumCast, range.Begin);
					int num = Protocols.CastToFixnum(fixnumCast, range.End);
					if (range.ExcludeEnd)
					{
						num--;
					}
					index = NormalizeIndex(values2.Length, index);
					num = NormalizeIndex(values2.Length, num);
					if (num - index > 0)
					{
						rubyArray.AddCapacity(num - index);
						for (int j = index; j <= num; j++)
						{
							rubyArray.Add(values2[j]);
						}
					}
				}
				else
				{
					int num2 = NormalizeIndex(values2.Length, Protocols.CastToFixnum(fixnumCast, values[i]));
					rubyArray.Add(values2[num2]);
				}
			}
			return rubyArray;
		}

		private static int NormalizeIndex(int itemCount, int index)
		{
			int num = index;
			if (num < 0)
			{
				num += itemCount;
			}
			if (num >= 0 && num < itemCount)
			{
				return num;
			}
			throw RubyExceptions.CreateIndexError("offset {0} too small for struct (size:{1})", index, itemCount);
		}
	}
}

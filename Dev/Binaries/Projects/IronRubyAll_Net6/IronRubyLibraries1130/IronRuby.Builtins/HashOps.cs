using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(IDictionary<object, object>) }, Copy = true)]
	[RubyClass("Hash", Extends = typeof(Hash), Inherits = typeof(object))]
	public static class HashOps
	{
		[RubyConstructor]
		public static Hash CreateHash(RubyClass self)
		{
			return new Hash(self.Context.EqualityComparer);
		}

		[RubyConstructor]
		public static Hash CreateHash(BlockParam block, RubyClass self, object defaultValue)
		{
			if (block != null)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of arguments");
			}
			return new Hash(self.Context.EqualityComparer, null, defaultValue);
		}

		[RubyConstructor]
		public static Hash CreateHash([NotNull] BlockParam defaultProc, RubyClass self)
		{
			return new Hash(self.Context.EqualityComparer, defaultProc.Proc, null);
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
		public static Hash CreateSubclass(RubyClass self)
		{
			return Hash.CreateInstance(self);
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
		public static Hash CreateSubclass(ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<IList> toAry, RubyClass self, object listOrHash)
		{
			CallSite<Func<CallSite, object, IDictionary<object, object>>> site = toHash.GetSite(ProtocolConversionAction<TryConvertToHashAction>.Make(toHash.Context));
			IDictionary<object, object> dictionary = site.Target(site, listOrHash);
			if (dictionary != null)
			{
				return CreateSubclass(self, dictionary);
			}
			CallSite<Func<CallSite, object, IList>> site2 = toAry.GetSite(ProtocolConversionAction<TryConvertToArrayAction>.Make(toAry.Context));
			IList list = site2.Target(site2, listOrHash);
			if (list != null)
			{
				return CreateSubclass(toAry, self, list);
			}
			throw RubyExceptions.CreateArgumentError("odd number of arguments for Hash");
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
		public static Hash CreateSubclass(ConversionStorage<IList> toAry, RubyClass self, [NotNull] IList list)
		{
			Hash hash = Hash.CreateInstance(self);
			CallSite<Func<CallSite, object, IList>> site = toAry.GetSite(ProtocolConversionAction<TryConvertToArrayAction>.Make(toAry.Context));
			foreach (object item in list)
			{
				IList list2 = site.Target(site, item);
				if (list2 != null && list2.Count >= 1 && list2.Count <= 2)
				{
					RubyUtils.SetHashElement(self.Context, hash, list2[0], (list2.Count == 2) ? list2[1] : null);
				}
			}
			return hash;
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
		public static Hash CreateSubclass(RubyClass self, [NotNull] IDictionary<object, object> hash)
		{
			return IDictionaryOps.ReplaceData(Hash.CreateInstance(self), hash);
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
		public static Hash CreateSubclass(RubyClass self, params object[] items)
		{
			if (items.Length % 2 != 0)
			{
				throw RubyExceptions.CreateArgumentError("odd number of arguments for Hash");
			}
			return RubyUtils.SetHashElements(self.Context, Hash.CreateInstance(self), items);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Hash Initialize(Hash self)
		{
			self.RequireNotFrozen();
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Hash Initialize(BlockParam block, Hash self, object defaultValue)
		{
			if (block != null)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of arguments");
			}
			self.DefaultProc = null;
			self.DefaultValue = defaultValue;
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Hash Initialize([NotNull] BlockParam defaultProc, Hash self)
		{
			self.DefaultProc = defaultProc.Proc;
			self.DefaultValue = null;
			return self;
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static Hash InitializeCopy(RubyContext context, Hash self, [NotNull] Hash source)
		{
			self.DefaultProc = source.DefaultProc;
			self.DefaultValue = source.DefaultValue;
			IDictionaryOps.ReplaceData(self, source);
			return self;
		}

		[RubyMethod("try_convert", RubyMethodAttributes.PublicSingleton)]
		public static IDictionary<object, object> TryConvert(ConversionStorage<IDictionary<object, object>> toHash, RubyClass self, object obj)
		{
			CallSite<Func<CallSite, object, IDictionary<object, object>>> site = toHash.GetSite(ProtocolConversionAction<TryConvertToHashAction>.Make(toHash.Context));
			return site.Target(site, obj);
		}

		[RubyMethod("[]")]
		public static object GetElement(BinaryOpStorage storage, IDictionary<object, object> self, object key)
		{
			object value;
			if (!self.TryGetValue(CustomStringDictionary.NullToObj(key), out value))
			{
				CallSite<Func<CallSite, object, object, object>> callSite = storage.GetCallSite("default", 1);
				return callSite.Target(callSite, self, key);
			}
			return value;
		}

		[RubyMethod("default")]
		public static object GetDefaultValue(RubyContext context, Hash self)
		{
			return self.DefaultValue;
		}

		[RubyMethod("default")]
		public static object GetDefaultValue(CallSiteStorage<Func<CallSite, Proc, Hash, object, object>> storage, Hash self, object key)
		{
			if (self.DefaultProc != null)
			{
				CallSite<Func<CallSite, Proc, Hash, object, object>> callSite = storage.GetCallSite("call", 2);
				return callSite.Target(callSite, self.DefaultProc, self, key);
			}
			return self.DefaultValue;
		}

		[RubyMethod("default=")]
		public static object SetDefaultValue(RubyContext context, Hash self, object value)
		{
			self.DefaultProc = null;
			return self.DefaultValue = value;
		}

		[RubyMethod("default_proc")]
		public static Proc GetDefaultProc(Hash self)
		{
			return self.DefaultProc;
		}

		[RubyMethod("replace")]
		public static Hash Replace(RubyContext context, Hash self, [DefaultProtocol][NotNull] IDictionary<object, object> other)
		{
			if (object.ReferenceEquals(self, other))
			{
				self.RequireNotFrozen();
				return self;
			}
			Hash hash = other as Hash;
			if (hash != null)
			{
				self.DefaultValue = hash.DefaultValue;
				self.DefaultProc = hash.DefaultProc;
			}
			return IDictionaryOps.ReplaceData(self, other);
		}

		[RubyMethod("shift")]
		public static object Shift(CallSiteStorage<Func<CallSite, Hash, object, object>> storage, Hash self)
		{
			self.RequireNotFrozen();
			if (self.Count == 0)
			{
				CallSite<Func<CallSite, Hash, object, object>> callSite = storage.GetCallSite("default", 1);
				return callSite.Target(callSite, self, null);
			}
			IEnumerator<KeyValuePair<object, object>> enumerator = self.GetEnumerator();
			enumerator.MoveNext();
			KeyValuePair<object, object> current = enumerator.Current;
			self.Remove(current.Key);
			return IDictionaryOps.MakeArray(current);
		}
	}
}

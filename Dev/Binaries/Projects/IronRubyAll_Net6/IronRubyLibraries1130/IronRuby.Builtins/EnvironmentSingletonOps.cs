using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyConstant("ENV")]
	[RubySingleton]
	[Includes(new Type[] { typeof(Enumerable) })]
	public static class EnvironmentSingletonOps
	{
		private static MutableString FrozenString(RubyContext context, object value)
		{
			return MutableString.Create(((string)value) ?? "", context.GetPathEncoding()).Freeze();
		}

		private static void SetEnvironmentVariable(RubyContext context, string name, string value)
		{
			context.DomainManager.Platform.SetEnvironmentVariable(name, value);
			if (name == "TZ")
			{
				TimeZone timeZone;
				if (RubyTime.TryParseTimeZone(value, out timeZone))
				{
					RubyTime._CurrentTimeZone = timeZone;
					return;
				}
				context.ReportWarning(string.Format(CultureInfo.InvariantCulture, "`{0}' is not a valid time zone specification; using the current time zone `{1}'", new object[2]
				{
					value,
					RubyTime._CurrentTimeZone.StandardName
				}));
			}
		}

		[RubyMethod("[]", RubyMethodAttributes.PublicInstance)]
		[RubyMethod("fetch", RubyMethodAttributes.PublicInstance)]
		public static MutableString GetVariable(RubyContext context, object self, [NotNull][DefaultProtocol] MutableString name)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			string environmentVariable = platform.GetEnvironmentVariable(name.ConvertToString());
			if (environmentVariable == null)
			{
				return null;
			}
			return FrozenString(context, environmentVariable);
		}

		[RubyMethod("[]=", RubyMethodAttributes.PublicInstance)]
		[RubyMethod("store", RubyMethodAttributes.PublicInstance)]
		public static MutableString SetVariable(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString name, [DefaultProtocol] MutableString value)
		{
			SetEnvironmentVariable(context, name.ConvertToString(), (value != null) ? value.ConvertToString() : null);
			return value;
		}

		[RubyMethod("clear")]
		public static object Clear(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			foreach (var environmentVariable in platform.GetEnvironmentVariables())
			{
				SetEnvironmentVariable(context, environmentVariable.Key.ToString(), null);
			}
			return self;
		}

		[RubyMethod("delete")]
		public static object Delete(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString name)
		{
			MutableString variable = GetVariable(context, self, name);
			if (variable != null)
			{
				SetVariable(context, self, name, null);
			}
			return variable;
		}

		[RubyMethod("delete_if")]
		[RubyMethod("reject!")]
		public static object DeleteIf(RubyContext context, BlockParam block, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			IDictionary environmentVariables = platform.GetEnvironmentVariables();
			if (environmentVariables.Count > 0 && block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			foreach (DictionaryEntry item in environmentVariables)
			{
				MutableString mutableString = FrozenString(context, item.Key);
				MutableString arg = FrozenString(context, item.Value);
				object blockResult;
				if (block.Yield(mutableString, arg, out blockResult))
				{
					return blockResult;
				}
				if (RubyOps.IsTrue(blockResult))
				{
					SetVariable(context, self, mutableString, null);
				}
			}
			return self;
		}

		[RubyMethod("each_pair")]
		[RubyMethod("each")]
		public static object Each(RubyContext context, BlockParam block, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			IDictionary environmentVariables = platform.GetEnvironmentVariables();
			if (environmentVariables.Count > 0 && block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			foreach (DictionaryEntry item in environmentVariables)
			{
				RubyArray rubyArray = new RubyArray(2);
				rubyArray.Add(FrozenString(context, item.Key));
				rubyArray.Add(FrozenString(context, item.Value));
				object blockResult;
				if (block.Yield(rubyArray, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("each_key")]
		public static object EachKey(RubyContext context, BlockParam block, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			IDictionary environmentVariables = platform.GetEnvironmentVariables();
			if (environmentVariables.Count > 0 && block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			foreach (DictionaryEntry item in environmentVariables)
			{
				MutableString arg = FrozenString(context, item.Key);
				object blockResult;
				if (block.Yield(arg, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("each_value")]
		public static object EachValue(RubyContext context, BlockParam block, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			IDictionary environmentVariables = platform.GetEnvironmentVariables();
			if (environmentVariables.Count > 0 && block == null)
			{
				throw RubyExceptions.NoBlockGiven();
			}
			foreach (DictionaryEntry item in environmentVariables)
			{
				MutableString arg = FrozenString(context, item.Value);
				object blockResult;
				if (block.Yield(arg, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("empty?")]
		public static bool IsEmpty(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			return platform.GetEnvironmentVariables().Count == 0;
		}

		[RubyMethod("key?")]
		[RubyMethod("has_key?")]
		[RubyMethod("include?")]
		public static bool HasKey(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString key)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			return platform.GetEnvironmentVariable(key.ConvertToString()) != null;
		}

		[RubyMethod("value?")]
		[RubyMethod("has_value?")]
		public static bool HasValue(RubyContext context, object self, object value)
		{
			MutableString mutableString = value as MutableString;
			if (value == null)
			{
				return false;
			}
			string text = mutableString.ConvertToString();
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			foreach (var environmentVariable in platform.GetEnvironmentVariables())
			{
				if (text.Equals(environmentVariable.Value))
				{
					return true;
				}
			}
			return false;
		}

		[RubyMethod("index")]
		public static MutableString Index(RubyContext context, object self, [DefaultProtocol][NotNull] MutableString value)
		{
			string text = value.ConvertToString();
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			foreach (var environmentVariable in platform.GetEnvironmentVariables())
			{
				if (text.Equals(environmentVariable.Value))
				{
					return FrozenString(context, environmentVariable.Key);
				}
			}
			return null;
		}

		[RubyMethod("indices")]
		public static RubyArray Indices(RubyContext context, object self, [DefaultProtocol][NotNullItems] params MutableString[] keys)
		{
			context.ReportWarning("ENV.indices is deprecated; use ENV.values_at");
			return ValuesAt(context, self, keys);
		}

		[RubyMethod("indexes")]
		public static RubyArray Index(RubyContext context, object self, [DefaultProtocol][NotNullItems] params MutableString[] keys)
		{
			context.ReportWarning("ENV.indexes is deprecated; use ENV.values_at");
			return ValuesAt(context, self, keys);
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyContext context, object self)
		{
			return IDictionaryOps.ToMutableString(context, ToHash(context, self));
		}

		[RubyMethod("invert")]
		public static Hash Invert(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			Hash hash = new Hash(context);
			foreach (var environmentVariable in platform.GetEnvironmentVariables())
			{
				hash.Add(FrozenString(context, environmentVariable.Value), FrozenString(context, environmentVariable.Key));
			}
			return hash;
		}

		[RubyMethod("keys")]
		public static RubyArray Keys(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			IDictionary environmentVariables = platform.GetEnvironmentVariables();
			RubyArray rubyArray = new RubyArray(environmentVariables.Count);
			foreach (DictionaryEntry item in environmentVariables)
			{
				rubyArray.Add(FrozenString(context, item.Key));
			}
			return rubyArray;
		}

		[RubyMethod("length")]
		[RubyMethod("size")]
		public static int Length(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			return platform.GetEnvironmentVariables().Count;
		}

		[RubyMethod("rehash")]
		public static object Rehash(object self)
		{
			return null;
		}

		private static void Update(ConversionStorage<MutableString> stringCast, Hash values)
		{
			foreach (KeyValuePair<object, object> value2 in values)
			{
				string name = Protocols.CastToString(stringCast, value2.Key).ToString();
				string value = Protocols.CastToString(stringCast, value2.Value).ToString();
				SetEnvironmentVariable(stringCast.Context, name, value);
			}
		}

		[RubyMethod("replace")]
		public static object Replace(ConversionStorage<MutableString> stringCast, object self, [NotNull] Hash values)
		{
			Clear(stringCast.Context, self);
			Update(stringCast, values);
			return self;
		}

		[RubyMethod("update")]
		public static object Update(ConversionStorage<MutableString> stringCast, object self, [NotNull] Hash values)
		{
			Update(stringCast, values);
			return self;
		}

		[RubyMethod("shift")]
		public static object Shift(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			IDictionary environmentVariables = platform.GetEnvironmentVariables();
			if (environmentVariables.Count == 0)
			{
				return null;
			}
			RubyArray rubyArray = new RubyArray(2);
			IDictionaryEnumerator dictionaryEnumerator = platform.GetEnvironmentVariables().GetEnumerator();
			try
			{
				if (dictionaryEnumerator.MoveNext())
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)dictionaryEnumerator.Current;
					rubyArray.Add(FrozenString(context, dictionaryEntry.Key));
					rubyArray.Add(FrozenString(context, dictionaryEntry.Value));
					SetEnvironmentVariable(context, (string)dictionaryEntry.Key, null);
					return rubyArray;
				}
				return rubyArray;
			}
			finally
			{
				IDisposable disposable = dictionaryEnumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		[RubyMethod("to_hash")]
		public static Hash ToHash(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			Hash hash = new Hash(context);
			foreach (var environmentVariable in platform.GetEnvironmentVariables())
			{
				hash.Add(FrozenString(context, environmentVariable.Key), FrozenString(context, environmentVariable.Value));
			}
			return hash;
		}

		[RubyMethod("to_s")]
		public static MutableString ToString(object self)
		{
			return MutableString.CreateAscii("ENV");
		}

		[RubyMethod("values")]
		public static RubyArray Values(RubyContext context, object self)
		{
			PlatformAdaptationLayer platform = context.DomainManager.Platform;
			IDictionary environmentVariables = platform.GetEnvironmentVariables();
			RubyArray rubyArray = new RubyArray(environmentVariables.Count);
			foreach (DictionaryEntry item in environmentVariables)
			{
				rubyArray.Add(FrozenString(context, item.Value));
			}
			return rubyArray;
		}

		[RubyMethod("values_at")]
		public static RubyArray ValuesAt(RubyContext context, object self, [DefaultProtocol][NotNullItems] params MutableString[] keys)
		{
			RubyArray rubyArray = new RubyArray(keys.Length);
			foreach (MutableString name in keys)
			{
				rubyArray.Add(GetVariable(context, self, name));
			}
			return rubyArray;
		}
	}
}

using System.Collections.Generic;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyModule("IronRuby", Extends = typeof(Ruby), Restrictions = ModuleRestrictions.NotPublished)]
	public static class IronRubyOps
	{
		[RubyModule("Clr", Restrictions = ModuleRestrictions.NoUnderlyingType)]
		public static class Clr
		{
			[RubyMethod("profile", RubyMethodAttributes.PublicSingleton)]
			public static Hash GetProfile(RubyContext context, object self)
			{
				if (!context.RubyOptions.Profile)
				{
					throw RubyExceptions.CreateSystemCallError("You must enable profiling to use Clr.profile");
				}
				Hash hash = new Hash(context);
				foreach (Profiler.MethodCounter item in Profiler.Instance.GetProfile())
				{
					hash[item.Id] = Utils.DateTimeTicksFromStopwatch(item.Ticks);
				}
				return hash;
			}

			[RubyMethod("profile", RubyMethodAttributes.PublicSingleton)]
			public static object GetProfile(RubyContext context, BlockParam block, object self)
			{
				if (!context.RubyOptions.Profile)
				{
					throw RubyExceptions.CreateSystemCallError("You must enable profiling to use Clr.profile");
				}
				List<Profiler.MethodCounter> profile = Profiler.Instance.GetProfile();
				object blockResult;
				if (block.Yield(out blockResult))
				{
					return blockResult;
				}
				Dictionary<string, long> dictionary = new Dictionary<string, long>();
				foreach (Profiler.MethodCounter item in profile)
				{
					dictionary[item.Id] = item.Ticks;
				}
				Hash hash = new Hash(context);
				foreach (Profiler.MethodCounter item2 in Profiler.Instance.GetProfile())
				{
					long value;
					if (!dictionary.TryGetValue(item2.Id, out value))
					{
						value = 0L;
					}
					long num = item2.Ticks - value;
					if (num > 0)
					{
						hash[item2.Id] = Utils.DateTimeTicksFromStopwatch(num);
					}
				}
				return hash;
			}
		}

		[RubyMethod("configuration", RubyMethodAttributes.PublicSingleton)]
		public static DlrConfiguration GetConfiguration(RubyContext context, RubyModule self)
		{
			return context.DomainManager.Configuration;
		}

		[RubyMethod("globals", RubyMethodAttributes.PublicSingleton)]
		public static Scope GetGlobalScope(RubyContext context, RubyModule self)
		{
			return context.DomainManager.Globals;
		}

		[RubyMethod("loaded_assemblies", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetLoadedAssemblies(RubyContext context, RubyModule self)
		{
			return new RubyArray(context.DomainManager.GetLoadedAssemblyList());
		}

		[RubyMethod("loaded_scripts", RubyMethodAttributes.PublicSingleton)]
		public static IDictionary<string, Scope> GetLoadedScripts(RubyContext context, RubyModule self)
		{
			return new Microsoft.Scripting.Utils.Extensions.ReadOnlyDictionary<string, Scope>(context.Loader.LoadedScripts);
		}

		[RubyMethod("require", RubyMethodAttributes.PublicSingleton)]
		public static object Require(RubyScope scope, RubyModule self, MutableString libraryName)
		{
			object loaded;
			scope.RubyContext.Loader.LoadFile(null, self, libraryName, LoadFlags.Require | LoadFlags.ResolveLoaded | LoadFlags.AnyLanguage, out loaded);
			return loaded;
		}

		[RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
		public static object Load(RubyScope scope, RubyModule self, MutableString libraryName)
		{
			object loaded;
			scope.RubyContext.Loader.LoadFile(null, self, libraryName, LoadFlags.ResolveLoaded | LoadFlags.AnyLanguage, out loaded);
			return loaded;
		}
	}
}

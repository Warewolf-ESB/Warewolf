using System;
using IronRuby.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace IronRuby
{
	public static class Ruby
	{
		public static ScriptRuntime CreateRuntime()
		{
			ScriptRuntimeSetup scriptRuntimeSetup = ScriptRuntimeSetup.ReadConfiguration();
			scriptRuntimeSetup.AddRubySetup();
			return new ScriptRuntime(scriptRuntimeSetup);
		}

		public static ScriptRuntime CreateRuntime(ScriptRuntimeSetup setup)
		{
			return new ScriptRuntime(setup);
		}

		public static ScriptEngine CreateEngine()
		{
			return GetEngine(CreateRuntime());
		}

		public static ScriptEngine CreateEngine(Action<LanguageSetup> setupInitializer)
		{
			ContractUtils.RequiresNotNull(setupInitializer, "setupInitializer");
			ScriptRuntimeSetup scriptRuntimeSetup = ScriptRuntimeSetup.ReadConfiguration();
			int num = IndexOfRubySetup(scriptRuntimeSetup);
			if (num != -1)
			{
				setupInitializer(scriptRuntimeSetup.LanguageSetups[num]);
			}
			else
			{
				scriptRuntimeSetup.LanguageSetups.Add(CreateRubySetup(setupInitializer));
			}
			return GetEngine(CreateRuntime(scriptRuntimeSetup));
		}

		public static LanguageSetup CreateRubySetup()
		{
			return new LanguageSetup(typeof(RubyContext).AssemblyQualifiedName, "IronRuby", "IronRuby;Ruby;rb".Split(';'), ".rb".Split(';'));
		}

		public static LanguageSetup CreateRubySetup(Action<LanguageSetup> initializer)
		{
			ContractUtils.RequiresNotNull(initializer, "initializer");
			LanguageSetup languageSetup = CreateRubySetup();
			initializer(languageSetup);
			return languageSetup;
		}

		public static ScriptEngine GetEngine(ScriptRuntime runtime)
		{
			ContractUtils.RequiresNotNull(runtime, "runtime");
			return runtime.GetEngineByTypeName(typeof(RubyContext).AssemblyQualifiedName);
		}

		internal static int IndexOfRubySetup(ScriptRuntimeSetup runtimeSetup)
		{
			for (int i = 0; i < runtimeSetup.LanguageSetups.Count; i++)
			{
				LanguageSetup languageSetup = runtimeSetup.LanguageSetups[i];
				if (languageSetup.TypeName == typeof(RubyContext).AssemblyQualifiedName)
				{
					return i;
				}
			}
			return -1;
		}
	}
}

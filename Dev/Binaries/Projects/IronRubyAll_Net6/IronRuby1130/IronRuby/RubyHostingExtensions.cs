using System;
using System.ComponentModel;
using IronRuby.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;

namespace IronRuby
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class RubyHostingExtensions
	{
		public static bool RequireFile(this ScriptEngine engine, string path)
		{
			return engine.GetService<RubyService>(new object[1] { engine }).RequireFile(path);
		}

		public static bool RequireFile(this ScriptEngine engine, string path, ScriptScope scope)
		{
			return engine.GetService<RubyService>(new object[1] { engine }).RequireFile(path, scope);
		}

		public static ScriptEngine GetRubyEngine(this ScriptRuntime runtime)
		{
			return Ruby.GetEngine(runtime);
		}

		public static LanguageSetup GetRubySetup(this ScriptRuntimeSetup runtimeSetup)
		{
			int num = Ruby.IndexOfRubySetup(runtimeSetup);
			if (num == -1)
			{
				return null;
			}
			return runtimeSetup.LanguageSetups[num];
		}

		public static LanguageSetup AddRubySetup(this ScriptRuntimeSetup runtimeSetup)
		{
			return runtimeSetup.AddRubySetup(null);
		}

		public static LanguageSetup AddRubySetup(this ScriptRuntimeSetup runtimeSetup, Action<LanguageSetup> newSetupInitializer)
		{
			ContractUtils.RequiresNotNull(runtimeSetup, "runtimeSetup");
			int num = Ruby.IndexOfRubySetup(runtimeSetup);
			LanguageSetup languageSetup;
			if (num == -1)
			{
				languageSetup = Ruby.CreateRubySetup();
				if (newSetupInitializer != null)
				{
					newSetupInitializer(languageSetup);
				}
				runtimeSetup.LanguageSetups.Add(languageSetup);
			}
			else
			{
				languageSetup = runtimeSetup.LanguageSetups[num];
			}
			return languageSetup;
		}
	}
}

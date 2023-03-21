using System;
using System.Security;
using System.Security.Permissions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;

namespace IronRuby.Hosting
{
	public abstract class RubyConsoleHost : ConsoleHost
	{
		protected override Type Provider
		{
			get
			{
				return typeof(RubyContext);
			}
		}

		protected RubyConsoleHost()
		{
			SetHomeEnvironmentVariable();
		}

		protected override CommandLine CreateCommandLine()
		{
			return new RubyCommandLine();
		}

		protected override OptionsParser CreateOptionsParser()
		{
			return new RubyOptionsParser();
		}

		protected override LanguageSetup CreateLanguageSetup()
		{
			return Ruby.CreateRubySetup();
		}

		protected override ConsoleOptions ParseOptions(string[] args, ScriptRuntimeSetup runtimeSetup, LanguageSetup languageSetup)
		{
			languageSetup.Options["ApplicationBase"] = AppDomain.CurrentDomain.BaseDirectory;
			return base.ParseOptions(args, runtimeSetup, languageSetup);
		}

		private static void SetHomeEnvironmentVariable()
		{
			try
			{
				PlatformAdaptationLayer @default = PlatformAdaptationLayer.Default;
				string homeDirectory = RubyUtils.GetHomeDirectory(@default);
				@default.SetEnvironmentVariable("HOME", homeDirectory);
			}
			catch (SecurityException ex)
			{
				if (ex.PermissionType != typeof(EnvironmentPermission))
				{
					throw;
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Hosting
{
	public sealed class RubyOptionsParser : OptionsParser<RubyConsoleOptions>
	{
		private readonly List<string> _loadPaths = new List<string>();

		private readonly List<string> _requiredPaths = new List<string>();

		private RubyEncoding _defaultEncoding;

		private bool _disableRubyGems;

		private static string[] GetPaths(string input)
		{
			string[] array = StringUtils.Split(input, new char[1] { Path.PathSeparator }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = StringUtils.Split(array[i], new char[1] { '"' }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
				array[i] = string.Concat(array2);
			}
			return array;
		}

		protected override void ParseArgument(string arg)
		{
			ContractUtils.RequiresNotNull(arg, "arg");
			string text = null;
			if (arg.StartsWith("-e", StringComparison.Ordinal))
			{
				string text2 = ((!(arg == "-e")) ? arg.Substring(2) : PopNextArg());
				base.LanguageSetup.Options["MainFile"] = "-e";
				if (CommonConsoleOptions.Command == null)
				{
					CommonConsoleOptions.Command = string.Empty;
				}
				else
				{
					CommonConsoleOptions.Command += "\n";
				}
				CommonConsoleOptions.Command += text2;
				return;
			}
			if (arg.StartsWith("-S", StringComparison.Ordinal))
			{
				text = ((arg == "-S") ? PopNextArg() : arg.Substring(2));
			}
			if (arg.StartsWith("-I", StringComparison.Ordinal))
			{
				string input = ((!(arg == "-I")) ? arg.Substring(2) : PopNextArg());
				_loadPaths.AddRange(GetPaths(input));
				return;
			}
			if (arg.StartsWith("-K", StringComparison.Ordinal))
			{
				_defaultEncoding = ((arg.Length >= 3) ? RubyEncoding.GetEncodingByNameInitial(arg[2]) : null);
				return;
			}
			if (arg.StartsWith("-r", StringComparison.Ordinal))
			{
				_requiredPaths.Add((arg == "-r") ? PopNextArg() : arg.Substring(2));
				return;
			}
			if (arg.StartsWith("-C", StringComparison.Ordinal))
			{
				base.ConsoleOptions.ChangeDirectory = arg.Substring(2);
				return;
			}
			if (arg.StartsWith("-0", StringComparison.Ordinal) || arg.StartsWith("-C", StringComparison.Ordinal) || arg.StartsWith("-F", StringComparison.Ordinal) || arg.StartsWith("-i", StringComparison.Ordinal) || arg.StartsWith("-T", StringComparison.Ordinal) || arg.StartsWith("-x", StringComparison.Ordinal))
			{
				throw new InvalidOptionException(string.Format("Option `{0}' not supported", arg));
			}
			int num = arg.IndexOf(':');
			string text3;
			string text4;
			if (num >= 0)
			{
				text3 = arg.Substring(0, num);
				text4 = arg.Substring(num + 1);
			}
			else
			{
				text3 = arg;
				text4 = null;
			}
			switch (text3)
			{
			case "-a":
			case "-c":
			case "--copyright":
			case "-l":
			case "-n":
			case "-p":
			case "-s":
				throw new InvalidOptionException(string.Format("Option `{0}' not supported", text3));
			case "-d":
				base.LanguageSetup.Options["DebugVariable"] = true;
				return;
			case "--version":
				base.ConsoleOptions.PrintVersion = true;
				base.ConsoleOptions.Exit = true;
				return;
			case "-v":
				base.ConsoleOptions.DisplayVersion = true;
				goto case "-w";
			case "-W0":
				base.LanguageSetup.Options["Verbosity"] = 0;
				return;
			case "-W1":
				base.LanguageSetup.Options["Verbosity"] = 1;
				return;
			case "-w":
			case "-W2":
				base.LanguageSetup.Options["Verbosity"] = 2;
				return;
			case "-trace":
				base.LanguageSetup.Options["EnableTracing"] = ScriptingRuntimeHelpers.True;
				return;
			case "-profile":
				base.LanguageSetup.Options["Profile"] = ScriptingRuntimeHelpers.True;
				return;
			case "-1.8.6":
			case "-1.8.7":
			case "-1.9":
			case "-2.0":
				throw new InvalidOptionException(string.Format("Option `{0}' is no longer supported. The compatible Ruby version is 1.9.", text3));
			case "--disable-gems":
				_disableRubyGems = true;
				return;
			case "-X":
				switch (text4)
				{
				case "AutoIndent":
				case "TabCompletion":
				case "ColorfulConsole":
					throw new InvalidOptionException(string.Format("Option `{0}' not supported", text3));
				}
				break;
			}
			base.ParseArgument(arg);
			if (base.ConsoleOptions.FileName != null)
			{
				if (text != null)
				{
					base.ConsoleOptions.FileName = FindMainFileFromPath(text);
				}
				if (base.ConsoleOptions.Command == null)
				{
					SetupOptionsForMainFile();
				}
				else
				{
					SetupOptionsForCommand();
				}
			}
		}

		private void SetupOptionsForMainFile()
		{
			base.LanguageSetup.Options["MainFile"] = RubyUtils.CanonicalizePath(base.ConsoleOptions.FileName);
			base.LanguageSetup.Options["Arguments"] = PopRemainingArgs();
		}

		private void SetupOptionsForCommand()
		{
			string fileName = base.ConsoleOptions.FileName;
			base.ConsoleOptions.FileName = null;
			List<string> list = new List<string>(new string[1] { fileName });
			list.AddRange(PopRemainingArgs());
			base.LanguageSetup.Options["MainFile"] = "-e";
			base.LanguageSetup.Options["Arguments"] = list.ToArray();
		}

		private string FindMainFileFromPath(string mainFileFromPath)
		{
			string environmentVariable = base.Platform.GetEnvironmentVariable("PATH");
			string[] array = environmentVariable.Split(';');
			foreach (string basePath in array)
			{
				string text = RubyUtils.CombinePaths(basePath, mainFileFromPath);
				if (base.Platform.FileExists(text))
				{
					return text;
				}
			}
			return mainFileFromPath;
		}

		protected override void AfterParse()
		{
			ReadOnlyCollection<string> readOnlyCollection = LanguageOptions.GetSearchPathsOption(base.LanguageSetup.Options) ?? LanguageOptions.GetSearchPathsOption(base.RuntimeSetup.Options);
			if (readOnlyCollection != null)
			{
				_loadPaths.InsertRange(0, readOnlyCollection);
			}
			try
			{
				string environmentVariable = Environment.GetEnvironmentVariable("RUBYLIB");
				if (environmentVariable != null)
				{
					_loadPaths.AddRange(GetPaths(environmentVariable));
				}
			}
			catch (SecurityException)
			{
			}
			base.LanguageSetup.Options["SearchPaths"] = _loadPaths;
			if (!_disableRubyGems)
			{
				_requiredPaths.Insert(0, "gem_prelude.rb");
			}
			base.LanguageSetup.Options["RequiredPaths"] = _requiredPaths;
			base.LanguageSetup.Options["DefaultEncoding"] = _defaultEncoding;
			base.LanguageSetup.Options["LocaleEncoding"] = _defaultEncoding ?? RubyEncoding.GetRubyEncoding(Console.InputEncoding);
			if (base.ConsoleOptions.DisplayVersion && base.ConsoleOptions.Command == null && base.ConsoleOptions.FileName == null)
			{
				base.ConsoleOptions.PrintVersion = true;
				base.ConsoleOptions.Exit = true;
			}
		}

		public override void GetHelp(out string commandLine, out string[,] options, out string[,] environmentVariables, out string comments)
		{
			commandLine = "[options] [file] [arguments]";
			environmentVariables = null;
			comments = null;
			options = new string[22, 2]
			{
				{ "-Cdirectory", "cd to directory, before executing your script" },
				{ "-d", "set debugging flags (set $DEBUG to true)" },
				{ "-D", "emit debugging information (PDBs) for Visual Studio debugger" },
				{ "-e 'command'", "one line of script. Several -e's allowed. Omit [file]" },
				{ "-h[elp]", "Display usage" },
				{ "-Idirectory", "specify $LOAD_PATH directory (may be used more than once)" },
				{ "-Kkcode", "specifies KANJI (Japanese) code-set" },
				{ "-rlibrary", "require the library, before executing your script" },
				{ "-S", "look for the script using PATH environment variable" },
				{ "-v", "print version number, then turn on verbose mode" },
				{ "-w", "turn warnings on for your script" },
				{ "-W[level]", "set warning level; 0=silence, 1=medium (default), 2=verbose" },
				{ "--version", "print the version" },
				{ "-trace", "enable support for set_trace_func" },
				{ "-profile", "enable support for 'pi = IronRuby::Clr.profile { block_to_profile }'" },
				{ "-X:ExceptionDetail", "enable ExceptionDetail mode" },
				{ "-X:NoAdaptiveCompilation", "disable adaptive compilation - all code will be compiled" },
				{ "-X:CompilationThreshold", "the number of iterations before the interpreter starts compiling" },
				{ "-X:PassExceptions", "do not catch exceptions that are unhandled by script code" },
				{ "-X:PrivateBinding", "enable binding to private members" },
				{ "-X:ShowClrExceptions", "display CLS Exception information" },
				{ "-X:RemoteRuntimeChannel", "remote console channel" }
			};
		}
	}
}

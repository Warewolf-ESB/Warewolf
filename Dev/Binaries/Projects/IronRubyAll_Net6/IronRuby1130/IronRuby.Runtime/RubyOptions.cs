using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IronRuby.Builtins;
using Microsoft.Scripting;

namespace IronRuby.Runtime
{
	[Serializable]
	public sealed class RubyOptions : LanguageOptions
	{
		private readonly ReadOnlyCollection<string> _arguments;

		private readonly RubyEncoding _localeEncoding;

		private readonly RubyEncoding _defaultEncoding;

		private readonly string _standardLibraryPath;

		private readonly string _applicationBase;

		private readonly ReadOnlyCollection<string> _requirePaths;

		private readonly string _mainFile;

		private readonly bool _enableTracing;

		private readonly int _verbosity;

		private readonly bool _debugVariable;

		private readonly string _savePath;

		private readonly bool _loadFromDisk;

		private readonly bool _profile;

		private readonly bool _hasSearchPaths;

		private readonly bool _noAssemblyResolveHook;

		public ReadOnlyCollection<string> Arguments
		{
			get
			{
				return _arguments;
			}
		}

		public RubyEncoding LocaleEncoding
		{
			get
			{
				return _localeEncoding;
			}
		}

		public RubyEncoding DefaultEncoding
		{
			get
			{
				return _defaultEncoding;
			}
		}

		public string MainFile
		{
			get
			{
				return _mainFile;
			}
		}

		public int Verbosity
		{
			get
			{
				return _verbosity;
			}
		}

		public bool EnableTracing
		{
			get
			{
				return _enableTracing;
			}
		}

		public string SavePath
		{
			get
			{
				return _savePath;
			}
		}

		public bool LoadFromDisk
		{
			get
			{
				return _loadFromDisk;
			}
		}

		public bool Profile
		{
			get
			{
				return _profile;
			}
		}

		public bool NoAssemblyResolveHook
		{
			get
			{
				return _noAssemblyResolveHook;
			}
		}

		public string StandardLibraryPath
		{
			get
			{
				return _standardLibraryPath;
			}
		}

		public string ApplicationBase
		{
			get
			{
				return _applicationBase;
			}
		}

		public ReadOnlyCollection<string> RequirePaths
		{
			get
			{
				return _requirePaths;
			}
		}

		public bool HasSearchPaths
		{
			get
			{
				return _hasSearchPaths;
			}
		}

		public RubyCompatibility Compatibility
		{
			get
			{
				return RubyCompatibility.Default;
			}
		}

		public bool DebugVariable
		{
			get
			{
				return _debugVariable;
			}
		}

		public RubyOptions(IDictionary<string, object> options)
			: base(options)
		{
			_arguments = LanguageOptions.GetStringCollectionOption(options, "Arguments") ?? LanguageOptions.EmptyStringCollection;
			_localeEncoding = LanguageOptions.GetOption(options, "LocaleEncoding", RubyEncoding.UTF8);
			_defaultEncoding = LanguageOptions.GetOption<RubyEncoding>(options, "DefaultEncoding", null);
			_mainFile = LanguageOptions.GetOption<string>(options, "MainFile", null);
			_verbosity = LanguageOptions.GetOption(options, "Verbosity", 1);
			_debugVariable = LanguageOptions.GetOption(options, "DebugVariable", false);
			_enableTracing = LanguageOptions.GetOption(options, "EnableTracing", false);
			_savePath = LanguageOptions.GetOption<string>(options, "SavePath", null);
			_loadFromDisk = LanguageOptions.GetOption(options, "LoadFromDisk", false);
			_profile = LanguageOptions.GetOption(options, "Profile", false);
			_noAssemblyResolveHook = LanguageOptions.GetOption(options, "NoAssemblyResolveHook", false);
			_requirePaths = LanguageOptions.GetStringCollectionOption(options, "RequiredPaths", ';', ',');
			_hasSearchPaths = LanguageOptions.GetOption<object>(options, "SearchPaths", null) != null;
			_standardLibraryPath = LanguageOptions.GetOption<string>(options, "StandardLibrary", null);
			_applicationBase = LanguageOptions.GetOption<string>(options, "ApplicationBase", null);
		}
	}
}

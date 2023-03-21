using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public sealed class Loader
	{
		internal enum FileKind
		{
			RubySourceFile,
			NonRubySourceFile,
			Assembly,
			Type,
			Unknown
		}

		private struct CompiledFile
		{
			public readonly ScriptCode CompiledCode;

			public CompiledFile(ScriptCode compiledCode)
			{
				CompiledCode = compiledCode;
			}
		}

		private sealed class AssemblyResolveHolder
		{
			private readonly WeakReference _loader;

			[ThreadStatic]
			private static HashSet<string> _assembliesBeingResolved;

			public AssemblyResolveHolder(Loader loader)
			{
				_loader = new WeakReference(loader);
			}

			internal void HookAssemblyResolve()
			{
				try
				{
					HookAssemblyResolveInternal();
				}
				catch (SecurityException)
				{
				}
			}

			private void HookAssemblyResolveInternal()
			{
				AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveEvent;
			}

			private Assembly AssemblyResolveEvent(object sender, ResolveEventArgs args)
			{
				Loader loader = (Loader)_loader.Target;
				if (loader != null)
				{
					string name = args.Name;
					if (_assembliesBeingResolved == null)
					{
						_assembliesBeingResolved = new HashSet<string>();
					}
					else if (_assembliesBeingResolved.Contains(name))
					{
						return null;
					}
					_assembliesBeingResolved.Add(name);
					try
					{
						return loader.ResolveAssembly(name);
					}
					catch (Exception ex)
					{
						loader._context.ReportWarning(string.Format("An exception was risen while resolving an assembly `{0}': {1}", name, ex.Message));
						throw;
					}
					finally
					{
						_assembliesBeingResolved.Remove(name);
					}
				}
				AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveEvent;
				return null;
			}
		}

		private class ResolvedFile
		{
			public readonly SourceUnit SourceUnit;

			public readonly string Path;

			public readonly string AppendedExtension;

			public ResolvedFile(SourceUnit sourceUnit, string fullPath, string appendedExtension)
			{
				SourceUnit = sourceUnit;
				Path = fullPath;
				AppendedExtension = appendedExtension;
			}

			public ResolvedFile(string fullLibraryPath, string appendedExtension)
			{
				Path = fullLibraryPath;
				AppendedExtension = appendedExtension;
			}
		}

		private RubyContext _context;

		private readonly RubyArray _loadPaths;

		private readonly RubyArray _loadedFiles;

		private readonly Stack<string> _unfinishedFiles;

		private SynchronizedDictionary<string, Scope> _loadedScripts;

		private Dictionary<string, CompiledFile> _compiledFiles;

		private readonly object _compiledFileMutex = new object();

		private int _cacheHitCount;

		private int _compiledFileCount;

		private static Regex _AssemblyNameRegex = new Regex("\r\n            \\s*((?<type>[\\w.+]+)\\s*,)?\\s* # type name\r\n            (?<assembly>\r\n              [^,=]+\\s*                   # assembly name\r\n              (,\\s*[\\w]+\\s*=\\s*[^,]+\\s*)+ # properties\r\n            )", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

		private static readonly string[] _LibraryExtensions = new string[3] { ".dll", ".so", ".exe" };

		private readonly ConversionStorage<MutableString> _toStrStorage;

		private readonly HashSet<Type> _loadedTypes = new HashSet<Type>();

		public RubyArray LoadPaths
		{
			get
			{
				return _loadPaths;
			}
		}

		public RubyArray LoadedFiles
		{
			get
			{
				return _loadedFiles;
			}
		}

		public IDictionary<string, Scope> LoadedScripts
		{
			get
			{
				if (_loadedScripts == null)
				{
					Interlocked.CompareExchange(ref _loadedScripts, new SynchronizedDictionary<string, Scope>(new Dictionary<string, Scope>(DomainManager.Platform.PathComparer)), null);
				}
				return _loadedScripts;
			}
		}

		private PlatformAdaptationLayer Platform
		{
			get
			{
				return DomainManager.Platform;
			}
		}

		private ScriptDomainManager DomainManager
		{
			get
			{
				return _context.DomainManager;
			}
		}

		internal Loader(RubyContext context)
		{
			_context = context;
			_toStrStorage = new ConversionStorage<MutableString>(context);
			_loadPaths = MakeLoadPaths(context.RubyOptions);
			_loadedFiles = new RubyArray();
			_unfinishedFiles = new Stack<string>();
			if (!context.RubyOptions.NoAssemblyResolveHook)
			{
				new AssemblyResolveHolder(this).HookAssemblyResolve();
			}
		}

		private RubyArray MakeLoadPaths(RubyOptions options)
		{
			RubyArray rubyArray = new RubyArray();
			if (options.HasSearchPaths)
			{
				foreach (string searchPath in options.SearchPaths)
				{
					rubyArray.Add(_context.EncodePath(searchPath));
				}
			}
			AddStandardLibraryPath(rubyArray, options.StandardLibraryPath, options.ApplicationBase);
			rubyArray.Add(MutableString.CreateAscii("."));
			return rubyArray;
		}

		private void AddStandardLibraryPath(RubyArray loadPaths, string path, string applicationBaseDir)
		{
			bool flag;
			if (path != null)
			{
				try
				{
					flag = Platform.IsAbsolutePath(path);
				}
				catch
				{
					loadPaths.Add(_context.EncodePath(path));
					return;
				}
			}
			else
			{
				path = "../Lib";
				flag = false;
			}
			if (!flag)
			{
				try
				{
					if (string.IsNullOrEmpty(applicationBaseDir))
					{
						applicationBaseDir = _context.Platform.GetEnvironmentVariable("IRONRUBY_11");
						if (!Directory.Exists(applicationBaseDir))
						{
							applicationBaseDir = AppDomain.CurrentDomain.BaseDirectory;
						}
					}
				}
				catch (SecurityException)
				{
					applicationBaseDir = null;
				}
				try
				{
					path = Platform.GetFullPath(RubyUtils.CombinePaths(applicationBaseDir, path));
				}
				catch
				{
					loadPaths.Add(_context.EncodePath(path));
					return;
				}
			}
			path = path.Replace('\\', '/');
			loadPaths.Add(_context.EncodePath(RubyUtils.CombinePaths(path, "ironruby")));
			loadPaths.Add(_context.EncodePath(RubyUtils.CombinePaths(path, "ruby/site_ruby/" + _context.StandardLibraryVersion)));
			loadPaths.Add(_context.EncodePath(RubyUtils.CombinePaths(path, "ruby/" + _context.StandardLibraryVersion)));
		}

		private void AddAbsoluteLibraryPaths(RubyArray result, string applicationBaseDir, ICollection<string> paths)
		{
			foreach (string path in paths)
			{
				string text;
				if (applicationBaseDir != null)
				{
					try
					{
						text = (Platform.IsAbsolutePath(path) ? path : Platform.GetFullPath(Path.Combine(applicationBaseDir, path)));
					}
					catch (Exception)
					{
						text = path;
					}
				}
				else
				{
					text = path;
				}
				result.Add(_context.EncodePath(text.Replace('\\', '/')));
			}
		}

		private Dictionary<string, CompiledFile> LoadCompiledCode()
		{
			Dictionary<string, CompiledFile> dictionary = new Dictionary<string, CompiledFile>();
			ScriptCode[] array = SavableScriptCode.LoadFromAssembly(_context.DomainManager, Assembly.Load(Path.GetFileName(_context.RubyOptions.MainFile)));
			for (int i = 0; i < array.Length; i++)
			{
				string path = array[i].SourceUnit.Path;
				string fullPath = Platform.GetFullPath(path);
				dictionary[fullPath] = new CompiledFile(array[i]);
			}
			return dictionary;
		}

		internal void SaveCompiledCode()
		{
			string savePath = _context.RubyOptions.SavePath;
			if (savePath == null)
			{
				return;
			}
			lock (_compiledFileMutex)
			{
				string assemblyName = Path.Combine(savePath, (Path.GetFileName(_context.RubyOptions.MainFile) ?? "snippets") + ".dll");
				if (_compiledFiles == null)
				{
					_compiledFiles = new Dictionary<string, CompiledFile>();
				}
				SavableScriptCode[] array = new SavableScriptCode[_compiledFiles.Count];
				int num = 0;
				foreach (CompiledFile value in _compiledFiles.Values)
				{
					array[num++] = (SavableScriptCode)value.CompiledCode;
				}
				SavableScriptCode.SaveToAssembly(assemblyName, array);
			}
		}

		private bool TryGetCompiledFile(string fullPath, out CompiledFile compiledFile)
		{
			if (!_context.RubyOptions.LoadFromDisk)
			{
				compiledFile = default(CompiledFile);
				return false;
			}
			lock (_compiledFileMutex)
			{
				if (_compiledFiles == null)
				{
					_compiledFiles = LoadCompiledCode();
				}
				return _compiledFiles.TryGetValue(fullPath, out compiledFile);
			}
		}

		private void AddCompiledFile(string fullPath, ScriptCode compiledCode)
		{
			if (_context.RubyOptions.SavePath == null)
			{
				return;
			}
			lock (_compiledFileMutex)
			{
				if (_compiledFiles == null)
				{
					_compiledFiles = new Dictionary<string, CompiledFile>();
				}
				_compiledFiles[fullPath] = new CompiledFile(compiledCode);
			}
		}

		public bool LoadFile(Scope globalScope, object self, MutableString path, LoadFlags flags)
		{
			object loaded;
			return LoadFile(globalScope, self, path, flags, out loaded);
		}

		public bool LoadFile(Scope globalScope, object self, MutableString path, LoadFlags flags, out object loaded)
		{
			string path2 = path.ConvertToString();
			string typeName;
			string assemblyName;
			if (TryParseAssemblyName(path2, out typeName, out assemblyName))
			{
				if (AlreadyLoaded(path2, (string)null, flags))
				{
					loaded = (((flags & LoadFlags.ResolveLoaded) != 0) ? GetAssembly(assemblyName, true, false) : null);
					return false;
				}
				Assembly assembly = LoadAssembly(assemblyName, typeName, false, false);
				if (assembly != null)
				{
					FileLoaded(path.Clone(), flags);
					loaded = assembly;
					return true;
				}
			}
			return LoadFromPath(globalScope, self, path2, path.Encoding, flags, out loaded);
		}

		public Assembly LoadAssembly(string assemblyName, string typeName, bool throwOnError, bool tryPartialName)
		{
			Assembly assembly = GetAssembly(assemblyName, throwOnError, tryPartialName);
			if (!(assembly != null) || !LoadAssembly(assembly, typeName, throwOnError))
			{
				return null;
			}
			return assembly;
		}

		private Assembly GetAssembly(string assemblyName, bool throwOnError, bool tryPartialName)
		{
			try
			{
				return Platform.LoadAssembly(assemblyName);
			}
			catch (Exception ex)
			{
				if (!tryPartialName || !(ex is FileNotFoundException))
				{
					if (throwOnError)
					{
						throw RubyExceptions.CreateLoadError(ex);
					}
					return null;
				}
			}
			Assembly assembly;
			try
			{
				assembly = Assembly.LoadWithPartialName(assemblyName);
			}
			catch (Exception innerException)
			{
				if (throwOnError)
				{
					throw RubyExceptions.CreateLoadError(innerException);
				}
				return null;
			}
			if (assembly == null && throwOnError)
			{
				throw RubyExceptions.CreateLoadError(string.Format("Assembly '{0}' not found", assemblyName));
			}
			return assembly;
		}

		private bool LoadAssembly(Assembly assembly, string typeName, bool throwOnError)
		{
			if (typeName != null)
			{
				Type type;
				try
				{
					type = assembly.GetType(typeName, true);
				}
				catch (Exception ex)
				{
					if (throwOnError)
					{
						throw new LoadError(ex.Message, ex);
					}
					return false;
				}
				LoadLibrary(type, false);
			}
			else
			{
				try
				{
					DomainManager.LoadAssembly(assembly);
				}
				catch (Exception innerException)
				{
					if (throwOnError)
					{
						throw RubyExceptions.CreateLoadError(innerException);
					}
					return false;
				}
			}
			return true;
		}

		internal static bool TryParseAssemblyName(string path, out string typeName, out string assemblyName)
		{
			Match match = _AssemblyNameRegex.Match(path);
			if (match.Success)
			{
				Group group = match.Groups["type"];
				Group group2 = match.Groups["assembly"];
				typeName = (group.Success ? group.Value : null);
				assemblyName = group2.Value;
				return true;
			}
			if (path.Trim() == "mscorlib")
			{
				typeName = null;
				assemblyName = path;
				return true;
			}
			typeName = null;
			assemblyName = null;
			return false;
		}

		internal Assembly ResolveAssembly(string fullName)
		{
			AssemblyName assemblyName = new AssemblyName(fullName);
			ResolvedFile resolvedFile = FindFile(assemblyName.Name, true, ArrayUtils.EmptyStrings).FirstOrDefault();
			if (resolvedFile == null || resolvedFile.SourceUnit != null)
			{
				return null;
			}
			try
			{
				Assembly assembly = Platform.LoadAssemblyFromPath(resolvedFile.Path);
				if (AssemblyName.ReferenceMatchesDefinition(assemblyName, assembly.GetName()))
				{
					DomainManager.LoadAssembly(assembly);
					return assembly;
				}
			}
			catch (Exception innerException)
			{
				throw RubyExceptions.CreateLoadError(innerException);
			}
			return null;
		}

		private bool LoadFromPath(Scope globalScope, object self, string path, RubyEncoding pathEncoding, LoadFlags flags, out object loaded)
		{
			string[] sourceFileExtensions = (((flags & LoadFlags.AnyLanguage) == 0) ? DomainManager.Configuration.GetFileExtensions(_context) : DomainManager.Configuration.GetFileExtensions());
			IList<ResolvedFile> list = FindFile(path, (flags & LoadFlags.AppendExtensions) != 0, sourceFileExtensions);
			if (list.Count == 0)
			{
				if (AlreadyLoaded(path, null, flags, sourceFileExtensions))
				{
					loaded = null;
					return false;
				}
				throw RubyExceptions.CreateLoadError(string.Format("no such file to load -- {0}", path));
			}
			ResolvedFile resolvedFile = list.First();
			string text = path;
			if (resolvedFile.AppendedExtension != null)
			{
				text += resolvedFile.AppendedExtension;
			}
			if (AlreadyLoaded(path, list, flags) || _unfinishedFiles.Contains(resolvedFile.Path))
			{
				if ((flags & LoadFlags.ResolveLoaded) != 0)
				{
					if (resolvedFile.SourceUnit != null)
					{
						Scope value;
						if (!LoadedScripts.TryGetValue(resolvedFile.Path, out value))
						{
							throw RubyExceptions.CreateLoadError(string.Format("no such file to load -- {0}", resolvedFile.Path));
						}
						loaded = value;
					}
					else
					{
						loaded = Platform.LoadAssemblyFromPath(resolvedFile.Path);
					}
				}
				else
				{
					loaded = null;
				}
				return false;
			}
			try
			{
				_unfinishedFiles.Push(resolvedFile.Path);
				if (resolvedFile.SourceUnit != null)
				{
					AddScriptLines(resolvedFile.SourceUnit);
					ScriptCode code = ((resolvedFile.SourceUnit.LanguageContext != _context) ? resolvedFile.SourceUnit.Compile() : CompileRubySource(resolvedFile.SourceUnit, flags));
					loaded = Execute(globalScope, code);
				}
				else
				{
					try
					{
						Assembly assembly = Platform.LoadAssemblyFromPath(resolvedFile.Path);
						DomainManager.LoadAssembly(assembly);
						loaded = assembly;
					}
					catch (Exception innerException)
					{
						throw RubyExceptions.CreateLoadError(innerException);
					}
				}
				FileLoaded(MutableString.Create(resolvedFile.Path, pathEncoding), flags);
			}
			finally
			{
				_unfinishedFiles.Pop();
			}
			return true;
		}

		private ScriptCode CompileRubySource(SourceUnit sourceUnit, LoadFlags flags)
		{
			string fullPath = Platform.GetFullPath(sourceUnit.Path);
			CompiledFile compiledFile;
			if (TryGetCompiledFile(fullPath, out compiledFile))
			{
				return compiledFile.CompiledCode;
			}
			RubyCompilerOptions rubyCompilerOptions = new RubyCompilerOptions(_context.RubyOptions);
			rubyCompilerOptions.FactoryKind = (((flags & LoadFlags.LoadIsolated) != 0) ? TopScopeFactoryKind.WrappedFile : TopScopeFactoryKind.File);
			RubyCompilerOptions options = rubyCompilerOptions;
			ScriptCode scriptCode = sourceUnit.Compile(options, _context.RuntimeErrorSink);
			AddCompiledFile(fullPath, scriptCode);
			return scriptCode;
		}

		internal Scope Execute(Scope globalScope, ScriptCode code)
		{
			if (globalScope == null || code.LanguageContext != _context)
			{
				if (globalScope == null)
				{
					globalScope = code.CreateScope();
				}
				if (code.SourceUnit.Path != null)
				{
					LoadedScripts[Platform.GetFullPath(code.SourceUnit.Path)] = globalScope;
				}
				code.Run(globalScope);
				return globalScope;
			}
			code.Run(globalScope);
			return null;
		}

		private IList<ResolvedFile> FindFile(string path, bool appendExtensions, string[] sourceFileExtensions)
		{
			bool flag;
			if (path.StartsWith("~/", StringComparison.Ordinal) || path.StartsWith("~\\", StringComparison.Ordinal))
			{
				path = RubyUtils.ExpandPath(_context.Platform, path);
				flag = true;
			}
			else
			{
				try
				{
					flag = Platform.IsAbsolutePath(path);
				}
				catch (ArgumentException innerException)
				{
					throw RubyExceptions.CreateLoadError(innerException);
				}
			}
			string extension = RubyUtils.GetExtension(path);
			if (flag)
			{
				ResolvedFile resolvedFile = ResolveFile(path, extension, appendExtensions, sourceFileExtensions);
				if (resolvedFile == null)
				{
					return new ResolvedFile[0];
				}
				return new ResolvedFile[1] { resolvedFile };
			}
			string[] loadPathStrings = GetLoadPathStrings();
			if (loadPathStrings.Length == 0)
			{
				return new ResolvedFile[0];
			}
			if (path.StartsWith("./", StringComparison.Ordinal) || path.StartsWith("../", StringComparison.Ordinal) || path.StartsWith(".\\", StringComparison.Ordinal) || path.StartsWith("..\\", StringComparison.Ordinal))
			{
				ResolvedFile resolvedFile2 = ResolveFile(path, extension, appendExtensions, sourceFileExtensions);
				if (resolvedFile2 == null)
				{
					return new ResolvedFile[0];
				}
				return new ResolvedFile[1] { resolvedFile2 };
			}
			List<ResolvedFile> list = new List<ResolvedFile>();
			string[] array = loadPathStrings;
			foreach (string basePath in array)
			{
				ResolvedFile resolvedFile3 = ResolveFile(RubyUtils.CombinePaths(basePath, path), extension, appendExtensions, sourceFileExtensions);
				if (resolvedFile3 != null)
				{
					list.Add(resolvedFile3);
				}
			}
			return list;
		}

		internal string[] GetLoadPathStrings()
		{
			object[] loadPaths = GetLoadPaths();
			string[] array = new string[loadPaths.Length];
			CallSite<Func<CallSite, object, MutableString>> site = _toStrStorage.GetSite(CompositeConversionAction.Make(_context, CompositeConversion.ToPathToStr));
			for (int i = 0; i < loadPaths.Length; i++)
			{
				if (loadPaths[i] == null)
				{
					throw RubyExceptions.CreateTypeConversionError("nil", "String");
				}
				array[i] = Protocols.CastToPath(site, loadPaths[i]).ConvertToString();
			}
			return array;
		}

		private ResolvedFile ResolveFile(string path, string extension, bool appendExtensions, string[] knownExtensions)
		{
			string text = RubyUtils.ExpandPath(_context.Platform, path);
			if (IsKnownExtension(extension, knownExtensions))
			{
				return GetSourceUnit(path, text, extension, false);
			}
			if (_LibraryExtensions.IndexOf(extension, DlrConfiguration.FileExtensionComparer) != -1)
			{
				if (Platform.FileExists(text))
				{
					return new ResolvedFile(text, null);
				}
			}
			else if (!appendExtensions)
			{
				return GetSourceUnit(path, text, extension, false);
			}
			if (appendExtensions)
			{
				List<string> extensionsOfExistingFiles = GetExtensionsOfExistingFiles(text, knownExtensions);
				if (extensionsOfExistingFiles.Count == 1)
				{
					return GetSourceUnit(path + extensionsOfExistingFiles[0], text + extensionsOfExistingFiles[0], extensionsOfExistingFiles[0], true);
				}
				if (extensionsOfExistingFiles.Count > 1)
				{
					Exception innerException = new AmbiguousFileNameException(text + extensionsOfExistingFiles[0], text + extensionsOfExistingFiles[1]);
					throw RubyExceptions.CreateLoadError(innerException);
				}
				string[] libraryExtensions = _LibraryExtensions;
				foreach (string text2 in libraryExtensions)
				{
					if (Platform.FileExists(text + text2))
					{
						return new ResolvedFile(text + text2, text2);
					}
				}
			}
			return null;
		}

		private static bool IsKnownExtension(string extension, string[] knownExtensions)
		{
			if (extension.Length > 0)
			{
				return knownExtensions.IndexOf(extension, DlrConfiguration.FileExtensionComparer) >= 0;
			}
			return false;
		}

		private ResolvedFile GetSourceUnit(string path, string fullPath, string extension, bool extensionAppended)
		{
			LanguageContext language;
			if (extension.Length == 0 || !DomainManager.TryGetLanguageByFileExtension(extension, out language))
			{
				language = _context;
			}
			if (!DomainManager.Platform.FileExists(fullPath))
			{
				return null;
			}
			SourceUnit sourceUnit = language.CreateFileUnit(path, RubyEncoding.Ascii.Encoding, SourceCodeKind.File);
			return new ResolvedFile(sourceUnit, fullPath, extensionAppended ? extension : null);
		}

		private List<string> GetExtensionsOfExistingFiles(string path, IEnumerable<string> extensions)
		{
			List<string> list = new List<string>();
			foreach (string extension in extensions)
			{
				string path2 = path + extension;
				if (Platform.FileExists(path2))
				{
					list.Add(extension);
				}
			}
			return list;
		}

		internal object[] GetLoadPaths()
		{
			lock (_loadedFiles)
			{
				return _loadPaths.ToArray();
			}
		}

		public void SetLoadPaths(IEnumerable<string> paths)
		{
			ContractUtils.RequiresNotNullItems(paths, "paths");
			lock (_loadPaths)
			{
				_loadPaths.Clear();
				foreach (string path in paths)
				{
					_loadPaths.Add(_context.EncodePath(path));
				}
			}
		}

		internal void AddLoadPaths(IEnumerable<string> paths)
		{
			lock (_loadPaths)
			{
				foreach (string path in paths)
				{
					_loadPaths.Add(_context.EncodePath(path));
				}
			}
		}

		internal void InsertLoadPaths(IEnumerable<string> paths, int index)
		{
			lock (_loadPaths)
			{
				foreach (string path in paths)
				{
					_loadPaths.Insert(0, _context.EncodePath(path));
				}
			}
		}

		internal void InsertLoadPaths(IEnumerable<string> paths)
		{
			InsertLoadPaths(paths, 0);
		}

		private void AddLoadedFile(MutableString path)
		{
			lock (_loadedFiles)
			{
				_loadedFiles.Add(path);
			}
		}

		private void FileLoaded(MutableString path, LoadFlags flags)
		{
			if ((flags & LoadFlags.LoadOnce) != 0)
			{
				AddLoadedFile(path);
			}
		}

		private void AddScriptLines(SourceUnit file)
		{
			ConstantStorage value;
			if (!_context.ObjectClass.TryResolveConstant(null, "SCRIPT_LINES__", out value))
			{
				return;
			}
			IDictionary dictionary = value.Value as IDictionary;
			if (dictionary == null)
			{
				return;
			}
			lock (dictionary)
			{
				RubyArray rubyArray = new RubyArray();
				SourceCodeReader reader = file.GetReader();
				RubyEncoding rubyEncoding = RubyEncoding.GetRubyEncoding(reader.Encoding);
				using (reader)
				{
					reader.SeekLine(1);
					while (true)
					{
						string text = reader.ReadLine();
						if (text != null)
						{
							MutableString mutableString = MutableString.CreateMutable(text.Length + 1, rubyEncoding);
							mutableString.Append(text).Append('\n');
							rubyArray.Add(mutableString);
							continue;
						}
						break;
					}
				}
				MutableString key = MutableString.Create(file.Document.FileName, _context.GetPathEncoding());
				dictionary[key] = rubyArray;
			}
		}

		internal object[] GetLoadedFiles()
		{
			lock (_loadedFiles)
			{
				return _loadedFiles.ToArray();
			}
		}

		private bool AlreadyLoaded(string path, string fullPath, LoadFlags flags)
		{
			return AlreadyLoaded(path, fullPath, flags, ArrayUtils.EmptyStrings);
		}

		private bool AlreadyLoaded(string path, string fullPath, LoadFlags flags, string[] sourceFileExtensions)
		{
			if ((flags & LoadFlags.LoadOnce) != 0)
			{
				return AnyFileLoaded(GetPathsToTestLoaded(path, fullPath, flags, sourceFileExtensions));
			}
			return false;
		}

		private IEnumerable<MutableString> GetPathsToTestLoaded(string path, string fullPath, LoadFlags flags, string[] sourceFileExtensions)
		{
			List<MutableString> list = new List<MutableString>();
			list.Add(_context.EncodePath(path));
			if (fullPath != null)
			{
				list.Add(_context.EncodePath(path));
			}
			if ((flags & LoadFlags.AppendExtensions) != 0 && RubyUtils.GetExtension(path).Length == 0)
			{
				foreach (string text in sourceFileExtensions)
				{
					list.Add(_context.EncodePath(path + text));
				}
				string[] libraryExtensions = _LibraryExtensions;
				foreach (string text2 in libraryExtensions)
				{
					list.Add(_context.EncodePath(path + text2));
				}
			}
			return list;
		}

		private bool AlreadyLoaded(string path, IEnumerable<ResolvedFile> files, LoadFlags flags)
		{
			if ((flags & LoadFlags.LoadOnce) != 0)
			{
				return AnyFileLoaded(new MutableString[1] { _context.EncodePath(path) }.Concat(files.Select((ResolvedFile file) => _context.EncodePath(file.Path))));
			}
			return false;
		}

		private bool AnyFileLoaded(IEnumerable<MutableString> paths)
		{
			CallSite<Func<CallSite, object, MutableString>> site = _toStrStorage.GetSite(CompositeConversionAction.Make(_context, CompositeConversion.ToPathToStr));
			object[] loadedFiles = GetLoadedFiles();
			foreach (object obj in loadedFiles)
			{
				if (obj == null)
				{
					throw RubyExceptions.CreateTypeConversionError("nil", "String");
				}
				MutableString loadedPath = Protocols.CastToPath(site, obj);
				if (paths.Any((MutableString path) => loadedPath.Equals(path)))
				{
					return true;
				}
			}
			return false;
		}

		internal void LoadBuiltins()
		{
			Type type;
			try
			{
				Assembly assembly = _context.DomainManager.Platform.LoadAssembly(GetIronRubyAssemblyLongName("IronRuby.Libraries"));
				var typename = LibraryInitializer.GetBuiltinsFullTypeName();
                type = assembly.GetType(typename);
			}
			catch (Exception innerException)
			{
				throw RubyExceptions.CreateLoadError(innerException);
			}
			LoadLibrary(type, true);
		}

		public static string GetIronRubyAssemblyLongName(string baseName)
		{
			ContractUtils.RequiresNotNull(baseName, "baseName");
			string fullName = typeof(RubyContext).Assembly.FullName;
			int num = fullName.IndexOf(',');
			if (num <= 0)
			{
				return baseName;
			}
			return baseName + fullName.Substring(num);
		}

		private void LoadLibrary(Type initializerType, bool builtin)
		{
			lock (_loadedTypes)
			{
				if (_loadedTypes.Contains(initializerType))
				{
					return;
				}
			}
			LibraryInitializer libraryInitializer;
			try
			{
				libraryInitializer = Activator.CreateInstance(initializerType) as LibraryInitializer;
			}
			catch (TargetInvocationException ex)
			{
				throw RubyExceptions.CreateLoadError(ex.InnerException);
			}
			catch (Exception innerException)
			{
				throw RubyExceptions.CreateLoadError(innerException);
			}
			if (libraryInitializer == null)
			{
				throw RubyExceptions.CreateLoadError(string.Format("Specified type {0} is not a subclass of {1}", initializerType.FullName, typeof(LibraryInitializer).FullName));
			}
			libraryInitializer.LoadModules(_context, builtin);
			lock (_loadedTypes)
			{
				_loadedTypes.Add(initializerType);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Compiler.Generation;
using IronRuby.Hosting;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public sealed class RubyContext : LanguageContext
	{
		private sealed class FileDescriptor
		{
			public int DuplicateCount;

			public readonly Stream Stream;

			public FileDescriptor(Stream stream)
			{
				Stream = stream;
				DuplicateCount = 1;
			}

			public void Close()
			{
				DuplicateCount--;
				if (DuplicateCount == 0)
				{
					Stream.Close();
				}
			}
		}

		public const string BinDirEnvironmentVariable = "IRONRUBY_11";

		public const string IronRubyInformationalVersion = "1.1.3";

		public const string IronRubyVersionString = "1.1.3.0";

		internal const string IronRubyDisplayName = "IronRuby";

		internal const string IronRubyNames = "IronRuby;Ruby;rb";

		internal const string IronRubyFileExtensions = ".rb";

		public const int StandardInputDescriptor = 0;

		public const int StandardOutputDescriptor = 1;

		public const int StandardErrorOutputDescriptor = 2;

		internal static readonly Guid RubyLanguageGuid = new Guid("F03C4640-DABA-473f-96F1-391400714DAB");

		private static readonly Guid LanguageVendor_Microsoft = new Guid(-1723120188, -6423, 4562, 144, 63, 0, 192, 79, 163, 2, 161);

		private static int _RuntimeIdGenerator = 0;

		public static readonly Version IronRubyVersion = new Version(1, 1, 3, 0);

		internal static RubyContext _Default;

		private readonly int _runtimeId;

		private readonly RubyScope _emptyScope;

		private RubyOptions _options;

		private readonly TopNamespaceTracker _namespaces;

		private readonly Loader _loader;

		private readonly Scope _globalScope;

		private readonly RubyMetaBinderFactory _metaBinderFactory;

		private readonly RubyBinder _binder;

		private DynamicDelegateCreator _delegateCreator;

		private RubyService _rubyService;

		[ThreadStatic]
		private static object _childProcessExitStatus;

		private MutableString _inputSeparator;

		private MutableString _outputSeparator;

		private object _stringSeparator;

		private MutableString _itemSeparator;

		private readonly Dictionary<string, GlobalVariable> _globalVariables;

		private readonly object _randomNumberGeneratorLock = new object();

		private Random _randomNumberGenerator;

		private object _randomNumberGeneratorSeed = ScriptingRuntimeHelpers.Int32ToObject(0);

		private readonly Thread _mainThread;

		private Thread _criticalThread;

		private readonly object _criticalMonitor = new object();

		private readonly RubyInputProvider _inputProvider;

		private Proc _traceListener;

		[ThreadStatic]
		private bool _traceListenerSuspended;

		private readonly Stopwatch _upTime;

		private readonly Dictionary<Type, RubyModule> _moduleCache;

		private readonly Dictionary<NamespaceTracker, RubyModule> _namespaceCache;

		private readonly WeakTable<object, RubyInstanceData> _referenceTypeInstanceData;

		private readonly Dictionary<object, RubyInstanceData> _valueTypeInstanceData;

		private readonly RubyInstanceData _nilInstanceData = new RubyInstanceData(RubyUtils.NilObjectId);

		private readonly CheckedMonitor _classHierarchyLock = new CheckedMonitor();

		public int ConstantAccessVersion = 1;

		private RubyClass _basicObjectClass;

		private RubyModule _kernelModule;

		private RubyClass _objectClass;

		private RubyClass _classClass;

		private RubyClass _moduleClass;

		private RubyClass _nilClass;

		private RubyClass _trueClass;

		private RubyClass _falseClass;

		private RubyClass _exceptionClass;

		private RubyClass _standardErrorClass;

		private RubyClass _comObjectClass;

		private Action<RubyModule> _mainSingletonTrait;

		private EqualityComparer _equalityComparer;

		private RubyEncoding _defaultExternalEncoding;

		private readonly object ExtensionsLock = new object();

		private List<Assembly> _potentialExtensionAssemblies = new List<Assembly>();

		private Dictionary<string, List<IEnumerable<ExtensionMethodInfo>>> _availableExtensions;

		[ThreadStatic]
		private static Exception _currentException;

		[ThreadStatic]
		private static int _currentSafeLevel;

		private readonly Dictionary<MutableString, RubySymbol> _symbols;

		private readonly List<FileDescriptor> _fileDescriptors = new List<FileDescriptor>(10);

		private readonly RuntimeErrorSink _runtimeErrorSink;

		private Dictionary<object, object> _libraryData;

		private readonly List<Proc> _shutdownHandlers = new List<Proc>();

		private CallSite<Func<CallSite, object, MutableString>> _stringConversionSite;

		private readonly Dictionary<Key<string, RubyCallSignature>, CallSite> _sendSites = new Dictionary<Key<string, RubyCallSignature>, CallSite>();

		private CallSite<Func<CallSite, object, object, object>> _respondTo;

		public string MriVersion
		{
			get
			{
				return "1.9.2";
			}
		}

		public string StandardLibraryVersion
		{
			get
			{
				return "1.9.1";
			}
		}

		public string MriReleaseDate
		{
			get
			{
				return "2010-08-18";
			}
		}

		public int MriPatchLevel
		{
			get
			{
				return 0;
			}
		}

		public MutableString CommandLineProgramPath { get; set; }

		public object GlobalVariablesLock
		{
			get
			{
				return _globalVariables;
			}
		}

		public IEnumerable<KeyValuePair<string, GlobalVariable>> GlobalVariables
		{
			get
			{
				return _globalVariables;
			}
		}

		public object RandomNumberGeneratorSeed
		{
			get
			{
				return _randomNumberGeneratorSeed;
			}
		}

		public Random RandomNumberGenerator
		{
			get
			{
				if (_randomNumberGenerator == null)
				{
					lock (_randomNumberGeneratorLock)
					{
						if (_randomNumberGenerator == null)
						{
							_randomNumberGenerator = new Random();
						}
					}
				}
				return _randomNumberGenerator;
			}
		}

		internal Action<IronRuby.Compiler.Ast.Expression, DynamicExpression> CallSiteCreated { get; set; }

		private object ModuleCacheLock
		{
			get
			{
				return _moduleCache;
			}
		}

		private object NamespaceCacheLock
		{
			get
			{
				return _namespaceCache;
			}
		}

		private object ReferenceTypeInstanceDataLock
		{
			get
			{
				return _referenceTypeInstanceData;
			}
		}

		private object ValueTypeInstanceDataLock
		{
			get
			{
				return _valueTypeInstanceData;
			}
		}

		public RubyClass BasicObjectClass
		{
			get
			{
				return _basicObjectClass;
			}
		}

		public RubyModule KernelModule
		{
			get
			{
				return _kernelModule;
			}
		}

		public RubyClass ObjectClass
		{
			get
			{
				return _objectClass;
			}
		}

		public RubyClass ClassClass
		{
			get
			{
				return _classClass;
			}
			set
			{
				_classClass = value;
			}
		}

		public RubyClass ModuleClass
		{
			get
			{
				return _moduleClass;
			}
			set
			{
				_moduleClass = value;
			}
		}

		public RubyClass NilClass
		{
			get
			{
				return _nilClass;
			}
			set
			{
				_nilClass = value;
			}
		}

		public RubyClass TrueClass
		{
			get
			{
				return _trueClass;
			}
			set
			{
				_trueClass = value;
			}
		}

		public RubyClass FalseClass
		{
			get
			{
				return _falseClass;
			}
			set
			{
				_falseClass = value;
			}
		}

		public RubyClass ExceptionClass
		{
			get
			{
				return _exceptionClass;
			}
			set
			{
				_exceptionClass = value;
			}
		}

		public RubyClass StandardErrorClass
		{
			get
			{
				return _standardErrorClass;
			}
			set
			{
				_standardErrorClass = value;
			}
		}

		internal RubyClass ComObjectClass
		{
			get
			{
				if (_comObjectClass == null)
				{
					GetOrCreateClass(TypeUtils.ComObjectType);
				}
				return _comObjectClass;
			}
		}

		internal HashSet<string> MissingMethodsCachedInSites { get; set; }

		public PlatformAdaptationLayer Platform
		{
			get
			{
				return base.DomainManager.Platform;
			}
		}

		public override LanguageOptions Options
		{
			get
			{
				return _options;
			}
		}

		public RubyOptions RubyOptions
		{
			get
			{
				return _options;
			}
		}

		internal RubyScope EmptyScope
		{
			get
			{
				return _emptyScope;
			}
		}

		public Thread MainThread
		{
			get
			{
				return _mainThread;
			}
		}

		public MutableString InputSeparator
		{
			get
			{
				return _inputSeparator;
			}
			set
			{
				_inputSeparator = value;
			}
		}

		public MutableString OutputSeparator
		{
			get
			{
				return _outputSeparator;
			}
			set
			{
				_outputSeparator = value;
			}
		}

		public object StringSeparator
		{
			get
			{
				return _stringSeparator;
			}
			set
			{
				_stringSeparator = value;
			}
		}

		public MutableString ItemSeparator
		{
			get
			{
				return _itemSeparator;
			}
			set
			{
				_itemSeparator = value;
			}
		}

		public object CriticalMonitor
		{
			get
			{
				return _criticalMonitor;
			}
		}

		public Thread CriticalThread
		{
			get
			{
				return _criticalThread;
			}
			set
			{
				_criticalThread = value;
			}
		}

		public Proc TraceListener
		{
			get
			{
				return _traceListener;
			}
			set
			{
				_traceListener = value;
			}
		}

		public RubyInputProvider InputProvider
		{
			get
			{
				return _inputProvider;
			}
		}

		public object ChildProcessExitStatus
		{
			get
			{
				return _childProcessExitStatus;
			}
			set
			{
				_childProcessExitStatus = value;
			}
		}

		public Scope TopGlobalScope
		{
			get
			{
				return _globalScope;
			}
		}

		internal RubyMetaBinderFactory MetaBinderFactory
		{
			get
			{
				return _metaBinderFactory;
			}
		}

		public Loader Loader
		{
			get
			{
				return _loader;
			}
		}

		public bool ShowCls
		{
			get
			{
				return false;
			}
		}

		public EqualityComparer EqualityComparer
		{
			get
			{
				if (_equalityComparer == null)
				{
					_equalityComparer = new EqualityComparer(this);
				}
				return _equalityComparer;
			}
		}

		public override Version LanguageVersion
		{
			get
			{
				return IronRubyVersion;
			}
		}

		public override Guid LanguageGuid
		{
			get
			{
				return RubyLanguageGuid;
			}
		}

		public override Guid VendorGuid
		{
			get
			{
				return LanguageVendor_Microsoft;
			}
		}

		public int RuntimeId
		{
			get
			{
				return _runtimeId;
			}
		}

		internal TopNamespaceTracker Namespaces
		{
			get
			{
				return _namespaces;
			}
		}

		public object Verbose { get; set; }

		public RubyEncoding DefaultExternalEncoding
		{
			get
			{
				return _defaultExternalEncoding;
			}
			set
			{
				ContractUtils.RequiresNotNull(value, "value");
				_defaultExternalEncoding = value;
			}
		}

		public RubyEncoding DefaultInternalEncoding { get; set; }

		internal RubyBinder Binder
		{
			get
			{
				return _binder;
			}
		}

		public Exception CurrentException
		{
			get
			{
				return _currentException;
			}
			internal set
			{
				_currentException = RubyUtils.GetVisibleException(value);
			}
		}

		public int CurrentSafeLevel
		{
			get
			{
				return _currentSafeLevel;
			}
		}

		private object SymbolsLock
		{
			get
			{
				return _symbols;
			}
		}

		public object StandardInput { get; set; }

		public object StandardOutput { get; set; }

		public object StandardErrorOutput { get; set; }

		public RuntimeErrorSink RuntimeErrorSink
		{
			get
			{
				return _runtimeErrorSink;
			}
		}

		private object ShutdownHandlersLock
		{
			get
			{
				return _shutdownHandlers;
			}
		}

		public Action InterruptSignalHandler { get; set; }

		public CallSite<Func<CallSite, object, MutableString>> StringConversionSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _stringConversionSite, ConvertToSAction.Make(this));
			}
		}

		private object SendSitesLock
		{
			get
			{
				return _sendSites;
			}
		}

		public DynamicDelegateCreator DelegateCreator
		{
			get
			{
				if (_delegateCreator == null)
				{
					Interlocked.CompareExchange(ref _delegateCreator, new DynamicDelegateCreator(this), null);
				}
				return _delegateCreator;
			}
		}

		public void SeedRandomNumberGenerator(IntegerValue value)
		{
			lock (_randomNumberGeneratorLock)
			{
				_randomNumberGenerator = new Random(value.IsFixnum ? value.Fixnum : value.Bignum.GetHashCode());
				_randomNumberGeneratorSeed = value.ToObject();
			}
		}

		public IDisposable ClassHierarchyLocker()
		{
			return _classHierarchyLock.CreateLocker();
		}

		public IDisposable ClassHierarchyUnlocker()
		{
			return _classHierarchyLock.CreateUnlocker();
		}

		[Conditional("DEBUG")]
		internal void RequiresClassHierarchyLock()
		{
		}

		public RubyContext(ScriptDomainManager manager, IDictionary<string, object> options)
			: base(manager)
		{
			ContractUtils.RequiresNotNull(manager, "manager");
			_options = new RubyOptions(options);
			_runtimeId = Interlocked.Increment(ref _RuntimeIdGenerator);
			_upTime = new Stopwatch();
			_upTime.Start();
			_binder = new RubyBinder(this);
			_symbols = new Dictionary<MutableString, RubySymbol>();
			_metaBinderFactory = new RubyMetaBinderFactory(this);
			_runtimeErrorSink = new RuntimeErrorSink(this);
			_equalityComparer = new EqualityComparer(this);
			_globalVariables = new Dictionary<string, GlobalVariable>();
			_moduleCache = new Dictionary<Type, RubyModule>();
			_namespaceCache = new Dictionary<NamespaceTracker, RubyModule>();
			_referenceTypeInstanceData = new WeakTable<object, RubyInstanceData>();
			_valueTypeInstanceData = new Dictionary<object, RubyInstanceData>();
			_inputProvider = new RubyInputProvider(this, _options.Arguments, _options.LocaleEncoding);
			_defaultExternalEncoding = _options.DefaultEncoding ?? _options.LocaleEncoding;
			_globalScope = base.DomainManager.Globals;
			_loader = new Loader(this);
			_emptyScope = new RubyTopLevelScope(this);
			_currentException = null;
			_currentSafeLevel = 0;
			_childProcessExitStatus = null;
			_inputSeparator = MutableString.CreateAscii("\n");
			_outputSeparator = null;
			_stringSeparator = null;
			_itemSeparator = null;
			_mainThread = Thread.CurrentThread;
			if (_options.MainFile != null)
			{
				CommandLineProgramPath = EncodePath(_options.MainFile);
			}
			if (_options.Verbosity <= 0)
			{
				Verbose = null;
			}
			else if (_options.Verbosity == 1)
			{
				Verbose = ScriptingRuntimeHelpers.False;
			}
			else
			{
				Verbose = ScriptingRuntimeHelpers.True;
			}
			_namespaces = new TopNamespaceTracker(manager);
			manager.AssemblyLoaded += delegate(object _, AssemblyLoadedEventArgs e)
			{
				AssemblyLoaded(e.Assembly);
			};
			foreach (Assembly loadedAssembly in manager.GetLoadedAssemblyList())
			{
				AssemblyLoaded(loadedAssembly);
			}
			Interlocked.CompareExchange(ref _Default, this, null);
			_loader.LoadBuiltins();
			InitializeFileDescriptors(base.DomainManager.SharedIO);
			InitializeGlobalConstants();
			InitializeGlobalVariables();
		}

		internal static void ClearThreadStatics()
		{
			_currentException = null;
		}

		private void InitializeGlobalVariables()
		{
			IronRuby.Runtime.GlobalVariables.DefineVariablesNoLock(this);
			DefineGlobalVariableNoLock("PROGRAM_NAME", IronRuby.Runtime.GlobalVariables.CommandLineProgramPath);
			DefineGlobalVariableNoLock("stdin", IronRuby.Runtime.GlobalVariables.InputStream);
			DefineGlobalVariableNoLock("stdout", IronRuby.Runtime.GlobalVariables.OutputStream);
			DefineGlobalVariableNoLock("defout", IronRuby.Runtime.GlobalVariables.OutputStream);
			DefineGlobalVariableNoLock("stderr", IronRuby.Runtime.GlobalVariables.ErrorOutputStream);
			DefineGlobalVariableNoLock("LOADED_FEATURES", IronRuby.Runtime.GlobalVariables.LoadedFiles);
			DefineGlobalVariableNoLock("LOAD_PATH", IronRuby.Runtime.GlobalVariables.LoadPath);
			DefineGlobalVariableNoLock("-I", IronRuby.Runtime.GlobalVariables.LoadPath);
			DefineGlobalVariableNoLock("-O", IronRuby.Runtime.GlobalVariables.InputSeparator);
			DefineGlobalVariableNoLock("-F", IronRuby.Runtime.GlobalVariables.StringSeparator);
			DefineGlobalVariableNoLock("FILENAME", IronRuby.Runtime.GlobalVariables.InputFileName);
			GlobalVariableInfo variable = new GlobalVariableInfo(base.DomainManager.Configuration.DebugMode || RubyOptions.DebugVariable);
			DefineGlobalVariableNoLock("VERBOSE", IronRuby.Runtime.GlobalVariables.Verbose);
			DefineGlobalVariableNoLock("-v", IronRuby.Runtime.GlobalVariables.Verbose);
			DefineGlobalVariableNoLock("-w", IronRuby.Runtime.GlobalVariables.Verbose);
			DefineGlobalVariableNoLock("DEBUG", variable);
			DefineGlobalVariableNoLock("-d", variable);
			DefineGlobalVariableNoLock("KCODE", IronRuby.Runtime.GlobalVariables.KCode);
			DefineGlobalVariableNoLock("-K", IronRuby.Runtime.GlobalVariables.KCode);
			DefineGlobalVariableNoLock("SAFE", IronRuby.Runtime.GlobalVariables.SafeLevel);
			try
			{
				TrySetCurrentProcessVariables();
			}
			catch (SecurityException)
			{
			}
		}

		private void TrySetCurrentProcessVariables()
		{
			Process currentProcess = Process.GetCurrentProcess();
			DefineGlobalVariableNoLock(Symbols.CurrentProcessId, new ReadOnlyGlobalVariableInfo(currentProcess.Id));
		}

		private void InitializeGlobalConstants()
		{
			MutableString value = MutableString.CreateAscii(MriVersion);
			MutableString value2 = MakePlatformString();
			MutableString value3 = MutableString.CreateAscii(MriReleaseDate);
			MutableString value4 = MutableString.CreateAscii("ironruby");
			using (ClassHierarchyLocker())
			{
				RubyClass objectClass = _objectClass;
				objectClass.SetConstantNoMutateNoLock("RUBY_ENGINE", value4);
				objectClass.SetConstantNoMutateNoLock("RUBY_VERSION", value);
				objectClass.SetConstantNoMutateNoLock("RUBY_PATCHLEVEL", MriPatchLevel);
				objectClass.SetConstantNoMutateNoLock("RUBY_PLATFORM", value2);
				objectClass.SetConstantNoMutateNoLock("RUBY_RELEASE_DATE", value3);
				objectClass.SetConstantNoMutateNoLock("RUBY_DESCRIPTION", MutableString.CreateAscii(MakeDescriptionString()));
				objectClass.SetConstantNoMutateNoLock("VERSION", value);
				objectClass.SetConstantNoMutateNoLock("PLATFORM", value2);
				objectClass.SetConstantNoMutateNoLock("RELEASE_DATE", value3);
				objectClass.SetConstantNoMutateNoLock("IRONRUBY_VERSION", MutableString.CreateAscii("1.1.3.0"));
				objectClass.SetConstantNoMutateNoLock("STDIN", StandardInput);
				objectClass.SetConstantNoMutateNoLock("STDOUT", StandardOutput);
				objectClass.SetConstantNoMutateNoLock("STDERR", StandardErrorOutput);
				ConstantStorage storage;
				if (objectClass.TryGetConstantNoAutoloadCheck("ARGF", out storage))
				{
					_inputProvider.Singleton = storage.Value;
				}
				objectClass.SetConstantNoMutateNoLock("ARGV", _inputProvider.CommandLineArguments);
			}
		}

		public static string MakeDescriptionString()
		{
			return string.Format(CultureInfo.InvariantCulture, "IronRuby {0} on {1}", new object[2]
			{
				IronRubyVersion,
				MakeRuntimeDesriptionString()
			});
		}

		internal static string MakeRuntimeDesriptionString()
		{
			Type type = typeof(object).Assembly.GetType("Mono.Runtime");
			if (!(type != null))
			{
				return string.Format(CultureInfo.InvariantCulture, ".NET {0}", new object[1] { Environment.Version });
			}
			return (string)type.GetMethod("GetDisplayName", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
		}

		private static MutableString MakePlatformString()
		{
			switch (Environment.OSVersion.Platform)
			{
			case PlatformID.MacOSX:
				return MutableString.CreateAscii("i386-darwin");
			case PlatformID.Unix:
				return MutableString.CreateAscii("i386-linux");
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.Win32NT:
				return MutableString.CreateAscii("i386-mswin32");
			default:
				return MutableString.CreateAscii("unknown");
			}
		}

		private void InitializeFileDescriptors(SharedIO io)
		{
			Stream stream = new ConsoleStream(io, ConsoleStreamType.Input);
			StandardInput = new RubyIO(this, stream, AllocateFileDescriptor(stream), IOMode.ReadOnly);
			stream = new ConsoleStream(io, ConsoleStreamType.Output);
			StandardOutput = new RubyIO(this, stream, AllocateFileDescriptor(stream), IOMode.WriteOnly | IOMode.WriteAppends);
			stream = new ConsoleStream(io, ConsoleStreamType.ErrorOutput);
			StandardErrorOutput = new RubyIO(this, stream, AllocateFileDescriptor(stream), IOMode.WriteOnly | IOMode.WriteAppends);
		}

		public void RegisterPrimitives(Action<RubyModule> mainSingletonTrait, Action<RubyModule> basicObjectInstanceTrait, Action<RubyModule> basicObjectClassTrait, Action<RubyModule> basicObjectConstantsInitializer, Action<RubyModule> kernelInstanceTrait, Action<RubyModule> kernelClassTrait, Action<RubyModule> kernelConstantsInitializer, Action<RubyModule> objectInstanceTrait, Action<RubyModule> objectClassTrait, Action<RubyModule> objectConstantsInitializer, Action<RubyModule> moduleInstanceTrait, Action<RubyModule> moduleClassTrait, Action<RubyModule> moduleConstantsInitializer, Action<RubyModule> classInstanceTrait, Action<RubyModule> classClassTrait, Action<RubyModule> classConstantsInitializer)
		{
			_mainSingletonTrait = mainSingletonTrait;
			TypeTracker typeTracker = ReflectionCache.GetTypeTracker(typeof(object));
			Delegate[] factories = new Delegate[1]
			{
				new Func<RubyScope, BlockParam, RubyClass, object>(RubyModule.CreateAnonymousModule)
			};
			Delegate[] factories2 = new Delegate[1]
			{
				new Func<RubyScope, BlockParam, RubyClass, RubyClass, object>(RubyClass.CreateAnonymousClass)
			};
			using (ClassHierarchyLocker())
			{
				_basicObjectClass = new RubyClass(this, Symbols.BasicObject, null, null, basicObjectInstanceTrait, basicObjectConstantsInitializer, null, null, null, null, null, false, false, ModuleRestrictions.NoNameMapping | ModuleRestrictions.NotPublished);
				_kernelModule = new RubyModule(this, Symbols.Kernel, kernelInstanceTrait, kernelConstantsInitializer, null, null, null, ModuleRestrictions.Builtin);
				_objectClass = new RubyClass(this, Symbols.Object, typeTracker.Type, null, objectInstanceTrait, objectConstantsInitializer, null, _basicObjectClass, new RubyModule[1] { _kernelModule }, typeTracker, null, false, false, ModuleRestrictions.NoNameMapping | ModuleRestrictions.NotPublished);
				_moduleClass = new RubyClass(this, Symbols.Module, typeof(RubyModule), null, moduleInstanceTrait, moduleConstantsInitializer, factories, _objectClass, null, null, null, false, false, ModuleRestrictions.Builtin);
				_classClass = new RubyClass(this, Symbols.Class, typeof(RubyClass), null, classInstanceTrait, classConstantsInitializer, factories2, _moduleClass, null, null, null, false, false, ModuleRestrictions.Builtin);
				_basicObjectClass.InitializeImmediateClass(_basicObjectClass.CreateSingletonClass(_classClass, basicObjectClassTrait));
				_objectClass.InitializeImmediateClass(_objectClass.CreateSingletonClass(_basicObjectClass.ImmediateClass, objectClassTrait));
				_moduleClass.InitializeImmediateClass(_moduleClass.CreateSingletonClass(_objectClass.ImmediateClass, moduleClassTrait));
				_classClass.InitializeImmediateClass(_classClass.CreateSingletonClass(_moduleClass.ImmediateClass, classClassTrait));
				_moduleClass.InitializeDummySingleton();
				_classClass.InitializeDummySingleton();
				_basicObjectClass.ImmediateClass.InitializeImmediateClass(_classClass.GetDummySingletonClass());
				_objectClass.ImmediateClass.InitializeImmediateClass(_classClass.GetDummySingletonClass());
				_moduleClass.ImmediateClass.InitializeImmediateClass(_classClass.GetDummySingletonClass());
				_classClass.ImmediateClass.InitializeImmediateClass(_classClass.GetDummySingletonClass());
				_kernelModule.InitializeImmediateClass(_moduleClass, kernelClassTrait);
				_objectClass.SetConstantNoMutateNoLock(_basicObjectClass.Name, _basicObjectClass);
				_objectClass.SetConstantNoMutateNoLock(_moduleClass.Name, _moduleClass);
				_objectClass.SetConstantNoMutateNoLock(_classClass.Name, _classClass);
				_objectClass.SetConstantNoMutateNoLock(_objectClass.Name, _objectClass);
				_objectClass.SetConstantNoMutateNoLock(_kernelModule.Name, _kernelModule);
			}
			AddModuleToCacheNoLock(typeof(BasicObject), _basicObjectClass);
			AddModuleToCacheNoLock(typeof(Kernel), _kernelModule);
			AddModuleToCacheNoLock(typeTracker.Type, _objectClass);
			AddModuleToCacheNoLock(typeof(RubyObject), _objectClass);
			AddModuleToCacheNoLock(_moduleClass.GetUnderlyingSystemType(), _moduleClass);
			AddModuleToCacheNoLock(_classClass.GetUnderlyingSystemType(), _classClass);
		}

		private void AssemblyLoaded(Assembly assembly)
		{
			_namespaces.LoadAssembly(assembly);
			AddExtensionAssembly(assembly);
		}

		internal void AddModuleToCacheNoLock(Type type, RubyModule module)
		{
			_moduleCache.Add(type, module);
		}

		internal void AddNamespaceToCacheNoLock(NamespaceTracker namespaceTracker, RubyModule module)
		{
			_namespaceCache.Add(namespaceTracker, module);
		}

		internal RubyModule GetOrCreateModule(NamespaceTracker tracker)
		{
			lock (ModuleCacheLock)
			{
				return GetOrCreateModuleNoLock(tracker);
			}
		}

		internal bool TryGetModule(NamespaceTracker namespaceTracker, out RubyModule result)
		{
			lock (NamespaceCacheLock)
			{
				return _namespaceCache.TryGetValue(namespaceTracker, out result);
			}
		}

		internal RubyModule GetOrCreateModule(Type moduleType)
		{
			lock (ModuleCacheLock)
			{
				return GetOrCreateModuleNoLock(moduleType);
			}
		}

		public bool TryGetModule(Type type, out RubyModule result)
		{
			lock (ModuleCacheLock)
			{
				return _moduleCache.TryGetValue(type, out result);
			}
		}

		internal bool TryGetModuleNoLock(Type type, out RubyModule result)
		{
			return _moduleCache.TryGetValue(type, out result);
		}

		internal bool TryGetClassNoLock(Type type, out RubyClass result)
		{
			RubyModule value;
			if (_moduleCache.TryGetValue(type, out value))
			{
				result = value as RubyClass;
				if (result == null)
				{
					throw new InvalidOperationException("Specified type doesn't represent a class");
				}
				return true;
			}
			result = null;
			return false;
		}

		internal RubyClass GetOrCreateClass(Type type)
		{
			lock (ModuleCacheLock)
			{
				return GetOrCreateClassNoLock(type);
			}
		}

		private RubyModule GetOrCreateModuleNoLock(NamespaceTracker tracker)
		{
			RubyModule value;
			if (_namespaceCache.TryGetValue(tracker, out value))
			{
				return value;
			}
			value = CreateModule(GetQualifiedName(tracker), null, null, null, null, tracker, null, ModuleRestrictions.None);
			_namespaceCache[tracker] = value;
			return value;
		}

		private RubyModule GetOrCreateModuleNoLock(Type moduleType)
		{
			RubyModule value;
			if (_moduleCache.TryGetValue(moduleType, out value))
			{
				return value;
			}
			TypeTracker typeTracker = (TypeTracker)MemberTracker.FromMemberInfo(moduleType);
			value = CreateModule(expandedMixins: (!moduleType.IsGenericType || moduleType.IsGenericTypeDefinition) ? null : new RubyModule[1] { GetOrCreateModuleNoLock(moduleType.GetGenericTypeDefinition()) }, name: GetQualifiedNameNoLock(moduleType), instanceTrait: null, classTrait: null, constantsInitializer: null, namespaceTracker: null, typeTracker: typeTracker, restrictions: ModuleRestrictions.None);
			_moduleCache[moduleType] = value;
			return value;
		}

		private RubyClass GetOrCreateClassNoLock(Type type)
		{
			RubyClass result;
			if (TryGetClassNoLock(type, out result))
			{
				return result;
			}
			RubyClass superClass = ((!type.IsByRef) ? GetOrCreateClassNoLock(type.BaseType) : _objectClass);
			TypeTracker tracker = (TypeTracker)MemberTracker.FromMemberInfo(type);
			RubyModule[] clrMixinsNoLock = GetClrMixinsNoLock(type);
			RubyModule[] expandedMixins;
			if (clrMixinsNoLock != null)
			{
				using (ClassHierarchyLocker())
				{
					expandedMixins = RubyModule.ExpandMixinsNoLock(superClass, clrMixinsNoLock);
				}
			}
			else
			{
				expandedMixins = RubyModule.EmptyArray;
			}
			result = CreateClass(GetQualifiedNameNoLock(type), type, null, null, null, null, null, superClass, expandedMixins, tracker, null, false, false, ModuleRestrictions.None);
			if (TypeUtils.IsComObjectType(type))
			{
				_comObjectClass = result;
			}
			_moduleCache[type] = result;
			return result;
		}

		private RubyModule[] GetClrMixinsNoLock(Type type)
		{
			List<RubyModule> list = new List<RubyModule>();
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				list.Add(GetOrCreateModuleNoLock(type.GetGenericTypeDefinition()));
			}
			RubyModule result2;
			if (type.IsArray)
			{
				RubyModule result;
				if (type.GetArrayRank() > 1 && TryGetModuleNoLock(typeof(MultiDimensionalArray), out result))
				{
					list.Add(result);
				}
			}
			else if (type.IsEnum && type.IsDefined(typeof(FlagsAttribute), false) && TryGetModuleNoLock(typeof(FlagEnumeration), out result2))
			{
				list.Add(result2);
			}
			foreach (Type declaredInterface in ReflectionUtils.GetDeclaredInterfaces(type))
			{
				list.Add(GetOrCreateModuleNoLock(declaredInterface));
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list.ToArray();
		}

		private void AddExtensionAssembly(Assembly assembly)
		{
			if (_potentialExtensionAssemblies != null)
			{
				lock (ExtensionsLock)
				{
					if (_potentialExtensionAssemblies != null)
					{
						_potentialExtensionAssemblies.Add(assembly);
						return;
					}
				}
			}
			LoadExtensions(ReflectionUtils.GetVisibleExtensionMethodGroups(assembly, true));
		}

		private void LoadExtensions(IEnumerable<KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>> extensionMethodGroups)
		{
			List<IEnumerable<ExtensionMethodInfo>> list = null;
			lock (ExtensionsLock)
			{
				foreach (KeyValuePair<string, IEnumerable<ExtensionMethodInfo>> extensionMethodGroup in extensionMethodGroups)
				{
					if (_availableExtensions == null)
					{
						_availableExtensions = new Dictionary<string, List<IEnumerable<ExtensionMethodInfo>>>();
					}
					string key = extensionMethodGroup.Key;
					List<IEnumerable<ExtensionMethodInfo>> value;
					if (_availableExtensions.TryGetValue(key, out value))
					{
						if (value == null)
						{
							if (list == null)
							{
								list = new List<IEnumerable<ExtensionMethodInfo>>();
							}
							value = list;
						}
					}
					else
					{
						_availableExtensions.Add(key, value = new List<IEnumerable<ExtensionMethodInfo>>());
					}
					value.Add(extensionMethodGroup.Value);
				}
			}
			if (list != null)
			{
				ActivateExtensions(list);
			}
		}

		public void ActivateExtensions(string @namespace)
		{
			ContractUtils.RequiresNotNull(@namespace, "namespace");
			Assembly[] array = null;
			if (_potentialExtensionAssemblies != null)
			{
				lock (ExtensionsLock)
				{
					if (_potentialExtensionAssemblies != null)
					{
						array = _potentialExtensionAssemblies.ToArray();
						_potentialExtensionAssemblies = null;
					}
				}
			}
			if (array != null)
			{
				List<KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>> list = new List<KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>>();
				Assembly[] array2 = array;
				foreach (Assembly assembly in array2)
				{
					list.AddRange(ReflectionUtils.GetVisibleExtensionMethodGroups(assembly, true));
				}
				LoadExtensions(list);
			}
			List<IEnumerable<ExtensionMethodInfo>> value;
			lock (ExtensionsLock)
			{
				_availableExtensions.TryGetValue(@namespace, out value);
				_availableExtensions[@namespace] = null;
			}
			if (value != null)
			{
				ActivateExtensions(value);
			}
		}

		private void ActivateExtensions(List<IEnumerable<ExtensionMethodInfo>> extensionLists)
		{
			Dictionary<Type, List<ExtensionMethodInfo>> dictionary = new Dictionary<Type, List<ExtensionMethodInfo>>();
			foreach (IEnumerable<ExtensionMethodInfo> extensionList in extensionLists)
			{
				foreach (ExtensionMethodInfo item in extensionList)
				{
					Type extendedType = item.ExtendedType;
					Type key = ((!extendedType.ContainsGenericParameters) ? extendedType : ((!extendedType.IsGenericParameter) ? (extendedType.IsArray ? typeof(Array) : extendedType.GetGenericTypeDefinition()) : typeof(object)));
					List<ExtensionMethodInfo> value;
					if (!dictionary.TryGetValue(key, out value))
					{
						dictionary.Add(key, value = new List<ExtensionMethodInfo>());
					}
					value.Add(item);
				}
			}
			using (ClassHierarchyLocker())
			{
				lock (ModuleCacheLock)
				{
					foreach (KeyValuePair<Type, List<ExtensionMethodInfo>> item2 in dictionary)
					{
						Type key2 = item2.Key;
						List<ExtensionMethodInfo> value2 = item2.Value;
						RubyModule rubyModule = ((key2.IsGenericTypeDefinition || key2.IsInterface) ? GetOrCreateModuleNoLock(key2) : GetOrCreateClassNoLock(key2));
						rubyModule.AddExtensionMethodsNoLock(value2);
					}
				}
			}
		}

		internal RubyClass CreateClass(string name, Type type, object classSingletonOf, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, Delegate[] factories, RubyClass superClass, RubyModule[] expandedMixins, TypeTracker tracker, RubyStruct.Info structInfo, bool isRubyClass, bool isSingletonClass, ModuleRestrictions restrictions)
		{
			RubyClass rubyClass = new RubyClass(this, name, type, classSingletonOf, instanceTrait, constantsInitializer, factories, superClass, expandedMixins, tracker, structInfo, isRubyClass, isSingletonClass, restrictions);
			rubyClass.InitializeImmediateClass(superClass.ImmediateClass, classTrait);
			return rubyClass;
		}

		internal RubyModule CreateModule(string name, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] expandedMixins, NamespaceTracker namespaceTracker, TypeTracker typeTracker, ModuleRestrictions restrictions)
		{
			RubyModule rubyModule = new RubyModule(this, name, instanceTrait, constantsInitializer, expandedMixins, namespaceTracker, typeTracker, restrictions);
			rubyModule.InitializeImmediateClass(_moduleClass, classTrait);
			return rubyModule;
		}

		public RubyClass GetOrCreateSingletonClass(object obj)
		{
			RubyModule rubyModule = obj as RubyModule;
			if (rubyModule != null)
			{
				return rubyModule.GetOrCreateSingletonClass();
			}
			return GetOrCreateInstanceSingleton(obj, null, null, null, null);
		}

		internal RubyClass GetOrCreateMainSingleton(object obj, RubyModule[] expandedMixins)
		{
			return GetOrCreateInstanceSingleton(obj, _mainSingletonTrait, null, null, expandedMixins);
		}

		internal RubyClass GetOrCreateInstanceSingleton(object obj, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] expandedMixins)
		{
			if (obj == null)
			{
				return _nilClass;
			}
			if (obj is bool)
			{
				if (!(bool)obj)
				{
					return _falseClass;
				}
				return _trueClass;
			}
			RubyInstanceData data = null;
			RubyClass immediateClassOf = GetImmediateClassOf(obj, ref data);
			if (immediateClassOf.IsSingletonClass)
			{
				return immediateClassOf;
			}
			RubyClass rubyClass = CreateClass(null, null, obj, instanceTrait, classTrait, constantsInitializer, null, immediateClassOf, expandedMixins, null, null, true, true, ModuleRestrictions.None);
			using (ClassHierarchyLocker())
			{
				immediateClassOf = GetImmediateClassOf(obj, ref data);
				if (immediateClassOf.IsSingletonClass)
				{
					return immediateClassOf;
				}
				SetInstanceSingletonOfNoLock(obj, ref data, rubyClass);
				bool flag = obj is IRubyObject;
				return rubyClass;
			}
		}

		internal RubyModule DefineModule(RubyModule owner, string name)
		{
			RubyModule rubyModule = CreateModule(owner.MakeNestedModuleName(name), null, null, null, null, null, null, ModuleRestrictions.None);
			PublishModule(name, owner, rubyModule);
			return rubyModule;
		}

		internal RubyClass DefineClass(RubyModule owner, string name, RubyClass superClass, RubyStruct.Info structInfo)
		{
			if (superClass.TypeTracker != null && superClass.TypeTracker.Type.ContainsGenericParameters)
			{
				throw RubyExceptions.CreateTypeError(string.Format("{0}: cannot inherit from open generic instantiation {1}. Only closed instantiations are supported.", name, superClass.Name));
			}
			string name2 = owner.MakeNestedModuleName(name);
			RubyClass rubyClass = CreateClass(name2, null, null, null, null, null, null, superClass, null, null, structInfo, true, false, ModuleRestrictions.None);
			PublishModule(name, owner, rubyClass);
			superClass.ClassInheritedEvent(rubyClass);
			return rubyClass;
		}

		private static void PublishModule(string name, RubyModule owner, RubyModule module)
		{
			if (name != null)
			{
				owner.SetConstant(name, module);
				if (owner.IsObjectClass)
				{
					module.Publish(name);
				}
			}
		}

		private T PrepareLibraryModuleDefinition<T>(string name, RubyClass super, RubyModule[] mixins, ModuleRestrictions restrictions, bool builtin, out RubyModule[] expandedMixins) where T : RubyModule
		{
			using (ClassHierarchyLocker())
			{
				expandedMixins = RubyModule.ExpandMixinsNoLock(super, mixins);
				ConstantStorage storage;
				if (name != null && !builtin && _objectClass.TryGetConstantNoAutoloadNoInit(name, out storage))
				{
					T val = storage.Value as T;
					bool flag = typeof(T) == typeof(RubyClass);
					if (val == null || val.IsClass != flag)
					{
						throw RubyExceptions.CreateTypeError("`{0}' is not a {1}", name, flag ? "class" : "module");
					}
					if (flag && (restrictions & ModuleRestrictions.AllowReopening) == 0)
					{
						throw RubyExceptions.CreateTypeError("cannot redefine {1} `{0}'", name, flag ? "class" : "module");
					}
					return val;
				}
			}
			return null;
		}

		internal RubyModule DefineLibraryModule(string name, Type type, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] mixins, ModuleRestrictions restrictions, bool builtin)
		{
			RubyModule[] expandedMixins;
			RubyModule result = PrepareLibraryModuleDefinition<RubyModule>(name, null, mixins, restrictions, builtin, out expandedMixins);
			bool flag = result != null;
			if (!flag)
			{
				lock (ModuleCacheLock)
				{
					if (!(flag = TryGetModuleNoLock(type, out result)))
					{
						if (name == null)
						{
							name = GetQualifiedNameNoLock(type);
						}
						result = CreateModule(name, instanceTrait, classTrait, constantsInitializer ?? RubyModule.EmptyInitializer, expandedMixins, null, GetLibraryModuleTypeTracker(type, restrictions), restrictions);
						AddModuleToCacheNoLock(type, result);
					}
				}
			}
			if (flag)
			{
				result.IncludeLibraryModule(instanceTrait, classTrait, constantsInitializer, mixins, builtin);
			}
			return result;
		}

		internal RubyClass DefineLibraryClass(string name, Type type, Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyClass super, RubyModule[] mixins, Delegate[] factories, ModuleRestrictions restrictions, bool builtin)
		{
			RubyModule[] expandedMixins;
			RubyClass result = PrepareLibraryModuleDefinition<RubyClass>(name, super, mixins, restrictions, builtin, out expandedMixins);
			bool flag = result != null;
			if (!flag)
			{
				lock (ModuleCacheLock)
				{
					if (!(flag = TryGetClassNoLock(type, out result)))
					{
						if (name == null)
						{
							name = GetQualifiedNameNoLock(type);
						}
						if (super == null)
						{
							super = GetOrCreateClassNoLock(type.BaseType);
						}
						result = CreateClass(name, type, null, instanceTrait, classTrait, constantsInitializer ?? RubyModule.EmptyInitializer, factories, super, expandedMixins, GetLibraryModuleTypeTracker(type, restrictions), null, false, false, restrictions);
						AddModuleToCacheNoLock(type, result);
					}
				}
			}
			if (flag)
			{
				if (super != null && super != result.SuperClass)
				{
					throw RubyExceptions.CreateTypeError("superclass mismatch for class {0}", name);
				}
				if (factories != null && factories.Length != 0)
				{
					throw RubyExceptions.CreateTypeError("Cannot add factories to an existing class");
				}
				result.IncludeLibraryModule(instanceTrait, classTrait, constantsInitializer, mixins, builtin);
				return result;
			}
			if (!builtin)
			{
				super.ClassInheritedEvent(result);
			}
			return result;
		}

		private static TypeTracker GetLibraryModuleTypeTracker(Type type, ModuleRestrictions restrictions)
		{
			if ((restrictions & ModuleRestrictions.NoUnderlyingType) == 0)
			{
				return ReflectionCache.GetTypeTracker(type);
			}
			return null;
		}

		public RubyModule GetModule(Type type)
		{
			if (RubyModule.IsModuleType(type))
			{
				return GetOrCreateModule(type);
			}
			return GetOrCreateClass(type);
		}

		public RubyModule GetModule(NamespaceTracker namespaceTracker)
		{
			return GetOrCreateModule(namespaceTracker);
		}

		public RubyClass GetClass(Type type)
		{
			ContractUtils.Requires(!RubyModule.IsModuleType(type));
			return GetOrCreateClass(type);
		}

		public RubyClass GetClassOf(object obj)
		{
			return TryGetClassOfRubyObject(obj) ?? GetOrCreateClass(obj.GetType());
		}

		private RubyClass TryGetClassOfRubyObject(object obj)
		{
			if (obj == null)
			{
				return _nilClass;
			}
			if (obj is bool)
			{
				if (!(bool)obj)
				{
					return _falseClass;
				}
				return _trueClass;
			}
			IRubyObject rubyObject = obj as IRubyObject;
			if (rubyObject != null)
			{
				return rubyObject.ImmediateClass.GetNonSingletonClass();
			}
			return null;
		}

		public RubyClass GetImmediateClassOf(object obj)
		{
			RubyInstanceData data = null;
			return GetImmediateClassOf(obj, ref data);
		}

		private RubyClass GetImmediateClassOf(object obj, ref RubyInstanceData data)
		{
			RubyClass rubyClass = TryGetImmediateClassOf(obj, ref data);
			if (rubyClass != null)
			{
				return rubyClass;
			}
			rubyClass = GetClassOf(obj);
			if (data != null)
			{
				data.UpdateImmediateClass(rubyClass);
			}
			return rubyClass;
		}

		private RubyClass TryGetImmediateClassOf(object obj, ref RubyInstanceData data)
		{
			IRubyObject rubyObject = obj as IRubyObject;
			if (rubyObject != null)
			{
				return rubyObject.ImmediateClass;
			}
			if (data != null || (data = TryGetInstanceData(obj)) != null)
			{
				return data.ImmediateClass;
			}
			return null;
		}

		private void SetInstanceSingletonOfNoLock(object obj, ref RubyInstanceData data, RubyClass singleton)
		{
			IRubyObject rubyObject = obj as IRubyObject;
			if (rubyObject != null)
			{
				rubyObject.ImmediateClass = singleton;
			}
			else if (data != null)
			{
				data.ImmediateClass = singleton;
			}
			else
			{
				(data = GetInstanceData(obj)).ImmediateClass = singleton;
			}
		}

		internal RubyClass TryGetSingletonOf(object obj, ref RubyInstanceData data)
		{
			RubyClass rubyClass = TryGetImmediateClassOf(obj, ref data);
			if (rubyClass == null)
			{
				return null;
			}
			if (!rubyClass.IsSingletonClass)
			{
				return null;
			}
			return rubyClass;
		}

		public bool IsKindOf(object obj, RubyModule m)
		{
			return GetImmediateClassOf(obj).HasAncestor(m);
		}

		public bool IsInstanceOf(object value, object classObject)
		{
			RubyClass rubyClass = classObject as RubyClass;
			if (rubyClass != null)
			{
				return GetClassOf(value).IsSubclassOf(rubyClass);
			}
			return false;
		}

		public string GetClassName(object obj)
		{
			return GetClassName(obj, false);
		}

		public string GetClassDisplayName(object obj)
		{
			return GetClassName(obj, true);
		}

		private string GetClassName(object obj, bool display)
		{
			RubyClass rubyClass = TryGetClassOfRubyObject(obj);
			if (rubyClass != null)
			{
				return rubyClass.Name;
			}
			return GetTypeName(obj.GetType(), display);
		}

		public string GetTypeName(Type type, bool display)
		{
			lock (ModuleCacheLock)
			{
				RubyModule result;
				if (TryGetModuleNoLock(type, out result))
				{
					if (display)
					{
						return result.GetDisplayName(this, false).ToString();
					}
					return result.Name;
				}
				return GetQualifiedNameNoLock(type);
			}
		}

		private string GetQualifiedNameNoLock(Type type)
		{
			return GetQualifiedNameNoLock(type, this, false);
		}

		internal static string GetQualifiedNameNoLock(Type type, RubyContext context, bool noGenericArgs)
		{
			return AppendQualifiedNameNoLock(new StringBuilder(), type, context, noGenericArgs).ToString();
		}

		private static StringBuilder AppendQualifiedNameNoLock(StringBuilder result, Type type, RubyContext context, bool noGenericArgs)
		{
			if (type.IsGenericParameter)
			{
				return result.Append(type.Name);
			}
			Type elementType = type.GetElementType();
			if (elementType != null)
			{
				AppendQualifiedNameNoLock(result, elementType, context, noGenericArgs);
				if (type.IsByRef)
				{
					result.Append('&');
				}
				else if (type.IsArray)
				{
					result.Append('[');
					result.Append(',', type.GetArrayRank() - 1);
					result.Append(']');
				}
				else
				{
					result.Append('*');
				}
				return result;
			}
			if (type.DeclaringType != null)
			{
				AppendQualifiedNameNoLock(result, type.DeclaringType, context, noGenericArgs);
				result.Append("::");
			}
			else if (type.Namespace != null)
			{
				result.Append(type.Namespace.Replace(Type.Delimiter.ToString(), "::"));
				result.Append("::");
			}
			result.Append(ReflectionUtils.GetNormalizedTypeName(type));
			if (!noGenericArgs && type.IsGenericType)
			{
				result.Append("[");
				Type[] genericArguments = type.GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (i > 0)
					{
						result.Append(", ");
					}
					RubyModule result2;
					if (context != null && context.TryGetModuleNoLock(genericArguments[i], out result2))
					{
						result.Append(result2.Name);
					}
					else
					{
						AppendQualifiedNameNoLock(result, genericArguments[i], context, noGenericArgs);
					}
				}
				result.Append("]");
			}
			return result;
		}

		private static string GetQualifiedName(NamespaceTracker namespaceTracker)
		{
			ContractUtils.RequiresNotNull(namespaceTracker, "namespaceTracker");
			if (namespaceTracker.Name == null)
			{
				return string.Empty;
			}
			return namespaceTracker.Name.Replace(Type.Delimiter.ToString(), "::");
		}

		public MethodResolutionResult ResolveMethod(object target, string name, bool includePrivate)
		{
			RubyClass immediateClassOf = GetImmediateClassOf(target);
			return immediateClassOf.ResolveMethod(name, includePrivate ? VisibilityContext.AllVisible : new VisibilityContext(immediateClassOf));
		}

		public MethodResolutionResult ResolveMethod(object target, string name, VisibilityContext visibility)
		{
			return GetImmediateClassOf(target).ResolveMethod(name, visibility);
		}

		public bool TryGetModule(RubyGlobalScope autoloadScope, string moduleName, out RubyModule result)
		{
			using (ClassHierarchyLocker())
			{
				result = _objectClass;
				int num = 0;
				int num2;
				do
				{
					num2 = moduleName.IndexOf("::", num, StringComparison.Ordinal);
					string name;
					if (num2 < 0)
					{
						name = moduleName.Substring(num);
					}
					else
					{
						name = moduleName.Substring(num, num2 - num);
						num = num2 + 2;
					}
					ConstantStorage value;
					if (!result.TryResolveConstantNoLock(autoloadScope, name, out value))
					{
						result = null;
						return false;
					}
					result = value.Value as RubyModule;
					if (result == null)
					{
						return false;
					}
				}
				while (num2 >= 0);
				return true;
			}
		}

		public object ResolveMissingConstant(RubyModule owner, string name)
		{
			if (owner.IsObjectClass)
			{
				object value;
				if (RubyOps.TryGetGlobalScopeConstant(this, _globalScope, name, out value))
				{
					return value;
				}
				if ((value = _namespaces.TryGetPackageAny(name)) != null)
				{
					return TrackerToModule(value);
				}
			}
			throw RubyExceptions.CreateNameError(string.Format("uninitialized constant {0}::{1}", owner.Name, name));
		}

		internal object TrackerToModule(object value)
		{
			TypeGroup typeGroup = value as TypeGroup;
			if (typeGroup != null)
			{
				return value;
			}
			TypeTracker typeTracker = value as TypeTracker;
			if (typeTracker != null)
			{
				return GetModule(typeTracker.Type);
			}
			NamespaceTracker namespaceTracker = value as NamespaceTracker;
			if (namespaceTracker != null)
			{
				return GetModule(namespaceTracker);
			}
			return value;
		}

		internal RubyInstanceData TryGetInstanceData(object obj)
		{
			IRubyObject rubyObject = obj as IRubyObject;
			if (rubyObject != null)
			{
				return rubyObject.TryGetInstanceData();
			}
			if (obj == null)
			{
				return _nilInstanceData;
			}
			RubyInstanceData value;
			if (!RubyUtils.HasObjectState(obj))
			{
				lock (ValueTypeInstanceDataLock)
				{
					_valueTypeInstanceData.TryGetValue(obj, out value);
					return value;
				}
			}
			TryGetClrTypeInstanceData(obj, out value);
			return value;
		}

		internal bool TryGetClrTypeInstanceData(object obj, out RubyInstanceData result)
		{
			lock (ReferenceTypeInstanceDataLock)
			{
				return _referenceTypeInstanceData.TryGetValue(obj, out result);
			}
		}

		internal RubyInstanceData GetInstanceData(object obj)
		{
			IRubyObject rubyObject = obj as IRubyObject;
			if (rubyObject != null)
			{
				return rubyObject.GetInstanceData();
			}
			if (obj == null)
			{
				return _nilInstanceData;
			}
			RubyInstanceData value;
			if (RubyUtils.HasObjectState(obj))
			{
				lock (ReferenceTypeInstanceDataLock)
				{
					if (!_referenceTypeInstanceData.TryGetValue(obj, out value))
					{
						_referenceTypeInstanceData.Add(obj, value = new RubyInstanceData());
						return value;
					}
					return value;
				}
			}
			lock (ValueTypeInstanceDataLock)
			{
				if (!_valueTypeInstanceData.TryGetValue(obj, out value))
				{
					_valueTypeInstanceData.Add(obj, value = new RubyInstanceData());
					return value;
				}
				return value;
			}
		}

		public bool HasInstanceVariables(object obj)
		{
			RubyInstanceData rubyInstanceData = TryGetInstanceData(obj);
			if (rubyInstanceData != null)
			{
				return rubyInstanceData.HasInstanceVariables;
			}
			return false;
		}

		public string[] GetInstanceVariableNames(object obj)
		{
			RubyInstanceData rubyInstanceData = TryGetInstanceData(obj);
			if (rubyInstanceData == null)
			{
				return ArrayUtils.EmptyStrings;
			}
			return rubyInstanceData.GetInstanceVariableNames();
		}

		public bool TryGetInstanceVariable(object obj, string name, out object value)
		{
			RubyInstanceData rubyInstanceData = TryGetInstanceData(obj);
			if (rubyInstanceData == null || !rubyInstanceData.TryGetInstanceVariable(name, out value))
			{
				value = null;
				return false;
			}
			return true;
		}

		private RubyInstanceData MutateInstanceVariables(object obj)
		{
			RubyInstanceData data;
			if (IsObjectFrozen(obj, out data))
			{
				throw RubyExceptions.CreateObjectFrozenError();
			}
			return data;
		}

		public void SetInstanceVariable(object obj, string name, object value)
		{
			(MutateInstanceVariables(obj) ?? GetInstanceData(obj)).SetInstanceVariable(name, value);
		}

		public bool TryRemoveInstanceVariable(object obj, string name, out object value)
		{
			RubyInstanceData rubyInstanceData = MutateInstanceVariables(obj) ?? TryGetInstanceData(obj);
			if (rubyInstanceData == null || !rubyInstanceData.TryRemoveInstanceVariable(name, out value))
			{
				value = null;
				return false;
			}
			return true;
		}

		internal void CopyInstanceData(object source, object target, bool copySingletonMembers)
		{
			RubyInstanceData data = null;
			RubyInstanceData data2 = TryGetInstanceData(source);
			if (data2 != null && data2.HasInstanceVariables)
			{
				data2.CopyInstanceVariablesTo(data = GetInstanceData(target));
			}
			if (!copySingletonMembers)
			{
				return;
			}
			using (ClassHierarchyLocker())
			{
				RubyClass rubyClass = TryGetSingletonOf(source, ref data2);
				if (rubyClass != null)
				{
					RubyClass rubyClass2 = rubyClass.Duplicate(target);
					rubyClass2.InitializeMembersFrom(rubyClass);
					SetInstanceSingletonOfNoLock(target, ref data, rubyClass2);
				}
			}
		}

		public IRubyObjectState GetObjectState(object obj)
		{
			return (obj as IRubyObjectState) ?? GetInstanceData(obj);
		}

		public IRubyObjectState TryGetObjectState(object obj)
		{
			return (obj as IRubyObjectState) ?? TryGetInstanceData(obj);
		}

		public bool IsObjectFrozen(object obj)
		{
			RubyInstanceData data;
			return IsObjectFrozen(obj, out data);
		}

		private bool IsObjectFrozen(object obj, out RubyInstanceData data)
		{
			IRubyObjectState rubyObjectState = obj as IRubyObjectState;
			if (rubyObjectState != null)
			{
				data = null;
				return rubyObjectState.IsFrozen;
			}
			data = TryGetInstanceData(obj);
			if (data == null)
			{
				return false;
			}
			return data.IsFrozen;
		}

		public bool IsObjectTainted(object obj)
		{
			IRubyObjectState rubyObjectState = TryGetObjectState(obj);
			if (rubyObjectState == null)
			{
				return false;
			}
			return rubyObjectState.IsTainted;
		}

		public bool IsObjectUntrusted(object obj)
		{
			IRubyObjectState rubyObjectState = TryGetObjectState(obj);
			if (rubyObjectState == null)
			{
				return false;
			}
			return rubyObjectState.IsUntrusted;
		}

		public void GetObjectTrust(object obj, out bool tainted, out bool untrusted)
		{
			IRubyObjectState rubyObjectState = TryGetObjectState(obj);
			if (rubyObjectState != null)
			{
				tainted = rubyObjectState.IsTainted;
				untrusted = rubyObjectState.IsUntrusted;
			}
			else
			{
				tainted = false;
				untrusted = false;
			}
		}

		public void FreezeObject(object obj)
		{
			GetObjectState(obj).Freeze();
		}

		public void SetObjectTaint(object obj, bool taint)
		{
			GetObjectState(obj).IsTainted = taint;
		}

		public void SetObjectTrustiness(object obj, bool untrusted)
		{
			GetObjectState(obj).IsUntrusted = untrusted;
		}

		public object TaintObjectBy(object obj, object source)
		{
			IRubyObjectState rubyObjectState = TryGetObjectState(source);
			if (rubyObjectState != null)
			{
				bool isTainted = rubyObjectState.IsTainted;
				bool isUntrusted = rubyObjectState.IsUntrusted;
				if (isTainted || isUntrusted)
				{
					IRubyObjectState objectState = GetObjectState(obj);
					objectState.IsTainted |= isTainted;
					objectState.IsUntrusted |= isUntrusted;
				}
			}
			return obj;
		}

		public object FreezeObjectBy(object obj, object source)
		{
			IRubyObjectState rubyObjectState = TryGetObjectState(source);
			if (rubyObjectState != null && rubyObjectState.IsFrozen)
			{
				GetObjectState(obj).Freeze();
			}
			return obj;
		}

		public MutableString Inspect(object obj)
		{
			RubyClass classOf = GetClassOf(obj);
			CallSite<Func<CallSite, object, object>> inspectSite = classOf.InspectSite;
			CallSite<Func<CallSite, object, MutableString>> inspectResultConversionSite = classOf.InspectResultConversionSite;
			return inspectResultConversionSite.Target(inspectResultConversionSite, inspectSite.Target(inspectSite, obj));
		}

		public object GetGlobalVariable(string name)
		{
			object value;
			TryGetGlobalVariable(null, name, out value);
			return value;
		}

		public void DefineGlobalVariable(string name, object value)
		{
			lock (GlobalVariablesLock)
			{
				_globalVariables[name] = new GlobalVariableInfo(value);
			}
		}

		public void DefineReadOnlyGlobalVariable(string name, object value)
		{
			lock (GlobalVariablesLock)
			{
				_globalVariables[name] = new ReadOnlyGlobalVariableInfo(value);
			}
		}

		public void DefineGlobalVariable(string name, GlobalVariable variable)
		{
			ContractUtils.RequiresNotNull(variable, "variable");
			lock (GlobalVariablesLock)
			{
				_globalVariables[name] = variable;
			}
		}

		internal void DefineGlobalVariableNoLock(string name, GlobalVariable variable)
		{
			_globalVariables[name] = variable;
		}

		public bool DeleteGlobalVariable(string name)
		{
			lock (GlobalVariablesLock)
			{
				return _globalVariables.Remove(name);
			}
		}

		public void AliasGlobalVariable(string newName, string oldName)
		{
			lock (GlobalVariablesLock)
			{
				GlobalVariable value;
				if (!_globalVariables.TryGetValue(oldName, out value))
				{
					DefineGlobalVariableNoLock(oldName, value = new GlobalVariableInfo(null, false));
				}
				_globalVariables[newName] = value;
			}
		}

		public void SetGlobalVariable(RubyScope scope, string name, object value)
		{
			lock (GlobalVariablesLock)
			{
				GlobalVariable value2;
				if (_globalVariables.TryGetValue(name, out value2))
				{
					value2.SetValue(this, scope, name, value);
				}
				else
				{
					_globalVariables[name] = new GlobalVariableInfo(value);
				}
			}
		}

		public bool TryGetGlobalVariable(RubyScope scope, string name, out object value)
		{
			lock (GlobalVariablesLock)
			{
				GlobalVariable value2;
				if (_globalVariables.TryGetValue(name, out value2))
				{
					value = value2.GetValue(this, scope);
					return true;
				}
			}
			value = null;
			return false;
		}

		internal bool TryGetGlobalVariable(string name, out GlobalVariable variable)
		{
			lock (GlobalVariablesLock)
			{
				return _globalVariables.TryGetValue(name, out variable);
			}
		}

		internal Exception SetCurrentException(object value)
		{
			Exception ex = value as Exception;
			if (value != null && ex == null)
			{
				throw RubyExceptions.CreateTypeError("assigning non-exception to $!");
			}
			return _currentException = ex;
		}

		internal RubyArray GetCurrentExceptionBacktrace()
		{
			Exception currentException = _currentException;
			if (currentException == null)
			{
				return null;
			}
			return RubyExceptionData.GetInstance(currentException).Backtrace;
		}

		internal RubyArray SetCurrentExceptionBacktrace(object value)
		{
			Exception currentException = _currentException;
			if (currentException == null)
			{
				throw RubyExceptions.CreateArgumentError("$! not set");
			}
			RubyArray rubyArray = RubyUtils.AsArrayOfStrings(value);
			if (value != null && rubyArray == null)
			{
				throw RubyExceptions.CreateTypeError("backtrace must be Array of String");
			}
			RubyExceptionData.GetInstance(currentException).Backtrace = rubyArray;
			return rubyArray;
		}

		public void SetSafeLevel(int value)
		{
			if (_currentSafeLevel <= value)
			{
				_currentSafeLevel = value;
				return;
			}
			throw RubyExceptions.CreateSecurityError(string.Format("tried to downgrade safe level from {0} to {1}", _currentSafeLevel, value));
		}

		public RubySymbol CreateSymbol(MutableString str)
		{
			return CreateSymbol(str, true);
		}

		public RubySymbol CreateAsciiSymbol(string str)
		{
			return CreateSymbol(MutableString.CreateAscii(str), false);
		}

		public RubySymbol CreateSymbol(string str, RubyEncoding encoding)
		{
			return CreateSymbol(MutableString.CreateMutable(str, encoding), false);
		}

		public RubySymbol CreateSymbol(byte[] bytes, RubyEncoding encoding)
		{
			MutableString str = MutableString.CreateBinary(bytes, encoding);
			return CreateSymbol(str, false);
		}

		public RubySymbol CreateSymbol(MutableString str, bool clone)
		{
			lock (SymbolsLock)
			{
				RubySymbol value;
				if (!_symbols.TryGetValue(str, out value))
				{
					value = new RubySymbol((clone ? str.Clone() : str).Freeze(), _symbols.Count + RubySymbol.MinId, _runtimeId);
					_symbols.Add(str, value);
					return value;
				}
				return value;
			}
		}

		public RubySymbol FindSymbol(int id)
		{
			lock (SymbolsLock)
			{
				foreach (RubySymbol value in _symbols.Values)
				{
					if (value.Id == id)
					{
						return value;
					}
				}
			}
			return null;
		}

		public RubyArray GetAllSymbols()
		{
			lock (SymbolsLock)
			{
				return new RubyArray(_symbols.Values);
			}
		}

		public RubyEncoding GetIdentifierEncoding()
		{
			return RubyEncoding.UTF8;
		}

		public RubySymbol EncodeIdentifier(string identifier)
		{
			return CreateSymbol(identifier, GetIdentifierEncoding());
		}

		public object StringifyIdentifier(string identifier)
		{
			return CreateSymbol(identifier, RubyEncoding.UTF8);
		}

		public RubyArray StringifyIdentifiers(IList<string> identifiers)
		{
			RubyArray rubyArray = new RubyArray(identifiers.Count);
			foreach (string identifier in identifiers)
			{
				rubyArray.Add(StringifyIdentifier(identifier));
			}
			return rubyArray;
		}

		private FileDescriptor TryGetFileDescriptorNoLock(int descriptor)
		{
			if (descriptor >= 0 && descriptor < _fileDescriptors.Count)
			{
				return _fileDescriptors[descriptor];
			}
			return null;
		}

		private int AddFileDescriptorNoLock(FileDescriptor fd)
		{
			for (int i = 0; i < _fileDescriptors.Count; i++)
			{
				if (_fileDescriptors[i] == null)
				{
					_fileDescriptors[i] = fd;
					return i;
				}
			}
			_fileDescriptors.Add(fd);
			return _fileDescriptors.Count - 1;
		}

		public Stream GetStream(int descriptor)
		{
			lock (_fileDescriptors)
			{
				FileDescriptor fileDescriptor = TryGetFileDescriptorNoLock(descriptor);
				return (fileDescriptor != null) ? fileDescriptor.Stream : null;
			}
		}

		public void SetStream(int descriptor, Stream stream)
		{
			ContractUtils.RequiresNotNull(stream, "stream");
			lock (_fileDescriptors)
			{
				FileDescriptor fileDescriptor = TryGetFileDescriptorNoLock(descriptor);
				if (fileDescriptor == null)
				{
					throw RubyExceptions.CreateEBADF();
				}
				if (fileDescriptor.Stream != stream)
				{
					fileDescriptor.Close();
					_fileDescriptors[descriptor] = new FileDescriptor(stream);
				}
			}
		}

		public void RedirectFileDescriptor(int descriptor, int toDescriptor)
		{
			lock (_fileDescriptors)
			{
				FileDescriptor fileDescriptor = TryGetFileDescriptorNoLock(descriptor);
				if (fileDescriptor == null)
				{
					throw RubyExceptions.CreateEBADF();
				}
				FileDescriptor fileDescriptor2 = TryGetFileDescriptorNoLock(toDescriptor);
				if (fileDescriptor2 == null)
				{
					throw RubyExceptions.CreateEBADF();
				}
				if (fileDescriptor != fileDescriptor2)
				{
					fileDescriptor.Close();
					fileDescriptor2.DuplicateCount++;
					_fileDescriptors[descriptor] = fileDescriptor2;
				}
			}
		}

		public int AllocateFileDescriptor(Stream stream)
		{
			ContractUtils.RequiresNotNull(stream, "stream");
			lock (_fileDescriptors)
			{
				return AddFileDescriptorNoLock(new FileDescriptor(stream));
			}
		}

		public int DuplicateFileDescriptor(int descriptor)
		{
			lock (_fileDescriptors)
			{
				FileDescriptor fileDescriptor = TryGetFileDescriptorNoLock(descriptor);
				if (fileDescriptor == null)
				{
					throw RubyExceptions.CreateEBADF();
				}
				fileDescriptor.DuplicateCount++;
				return AddFileDescriptorNoLock(fileDescriptor);
			}
		}

		public void CloseStream(int descriptor)
		{
			lock (_fileDescriptors)
			{
				FileDescriptor fileDescriptor = TryGetFileDescriptorNoLock(descriptor);
				if (fileDescriptor == null)
				{
					throw RubyExceptions.CreateEBADF();
				}
				fileDescriptor.Close();
				_fileDescriptors[descriptor] = null;
			}
		}

		public void RemoveFileDescriptor(int descriptor)
		{
			lock (_fileDescriptors)
			{
				if (TryGetFileDescriptorNoLock(descriptor) == null)
				{
					throw RubyExceptions.CreateEBADF();
				}
				_fileDescriptors[descriptor] = null;
			}
		}

		public void ReportWarning(string message)
		{
			ReportWarning(message, false);
		}

		public void ReportWarning(string message, bool isVerbose)
		{
			_runtimeErrorSink.Add(null, message, SourceSpan.None, isVerbose ? 12288 : 8192, Severity.Warning);
		}

		public RubyEncoding GetPathEncoding()
		{
			return RubyEncoding.UTF8;
		}

		public MutableString EncodePath(string path)
		{
			return EncodePath(path, GetPathEncoding());
		}

		public MutableString TryEncodePath(string path)
		{
			MutableString mutableString = MutableString.Create(path, GetPathEncoding());
			if (!mutableString.ContainsInvalidCharacters())
			{
				return mutableString;
			}
			return null;
		}

		internal static MutableString EncodePath(string path, RubyEncoding encoding)
		{
			MutableString mutableString = MutableString.Create(path, encoding);
			mutableString.ContainsInvalidCharacters();
			try
			{
				return MutableString.Create(path, encoding).CheckEncoding();
			}
			catch (EncoderFallbackException ex)
			{
				throw RubyExceptions.CreateEINVAL(ex, "Path \"{0}\" contains characters that cannot be represented in encoding {1}: {2}", path.ToAsciiString(), encoding.Name, ex.Message);
			}
		}

		public string DecodePath(MutableString path)
		{
			try
			{
				if (path.Encoding == RubyEncoding.Binary)
				{
					return path.ToString(Encoding.UTF8);
				}
				return path.ConvertToString();
			}
			catch (DecoderFallbackException)
			{
				throw RubyExceptions.CreateEINVAL("Invalid multi-byte sequence in path `{0}'", path.ToAsciiString());
			}
		}

		private void EnsureLibraryData()
		{
			if (_libraryData == null)
			{
				Interlocked.CompareExchange(ref _libraryData, new Dictionary<object, object>(), null);
			}
		}

		public bool TryGetLibraryData(object key, out object value)
		{
			EnsureLibraryData();
			lock (_libraryData)
			{
				return _libraryData.TryGetValue(key, out value);
			}
		}

		public object GetOrCreateLibraryData(object key, Func<object> valueFactory)
		{
			object value;
			if (TryGetLibraryData(key, out value))
			{
				return value;
			}
			value = valueFactory();
			object actualValue;
			TryAddLibraryData(key, value, out actualValue);
			return actualValue;
		}

		public bool TryAddLibraryData(object key, object value, out object actualValue)
		{
			EnsureLibraryData();
			lock (_libraryData)
			{
				if (_libraryData.TryGetValue(key, out actualValue))
				{
					return false;
				}
				_libraryData.Add(key, actualValue = value);
				return true;
			}
		}

		public void TrySetLibraryData(object key, object value)
		{
			EnsureLibraryData();
			lock (_libraryData)
			{
				_libraryData[key] = value;
			}
		}

		public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
		{
			ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
			ContractUtils.RequiresNotNull(options, "options");
			ContractUtils.RequiresNotNull(errorSink, "errorSink");
			ContractUtils.Requires(sourceUnit.LanguageContext == this, "Language mismatch.");
			RubyCompilerOptions rubyCompilerOptions = (RubyCompilerOptions)options;
			Expression<Func<RubyScope, object, object>> expression = ParseSourceCode<Func<RubyScope, object, object>>(sourceUnit, rubyCompilerOptions, errorSink);
			if (expression == null)
			{
				return null;
			}
			return new RubyScriptCode(expression, sourceUnit, rubyCompilerOptions.FactoryKind);
		}

		internal Expression<T> ParseSourceCode<T>(SourceUnit sourceUnit, RubyCompilerOptions options, ErrorSink errorSink)
		{
			SourceUnitTree sourceUnitTree = new Parser().Parse(sourceUnit, options, errorSink);
			if (sourceUnitTree == null)
			{
				return null;
			}
			return TransformTree<T>(sourceUnitTree, sourceUnit, options);
		}

		internal Expression<T> TransformTree<T>(SourceUnitTree ast, SourceUnit sourceUnit, RubyCompilerOptions options)
		{
			return ast.Transform<T>(new AstGenerator(this, options, sourceUnit.Document, ast.Encoding, sourceUnit.Kind == SourceCodeKind.InteractiveCode));
		}

		public override CompilerOptions GetCompilerOptions()
		{
			RubyCompilerOptions rubyCompilerOptions = new RubyCompilerOptions(_options);
			rubyCompilerOptions.FactoryKind = TopScopeFactoryKind.Hosted;
			return rubyCompilerOptions;
		}

		public override CompilerOptions GetCompilerOptions(Scope scope)
		{
			RubyCompilerOptions rubyCompilerOptions = new RubyCompilerOptions(_options);
			rubyCompilerOptions.FactoryKind = TopScopeFactoryKind.Hosted;
			RubyCompilerOptions rubyCompilerOptions2 = rubyCompilerOptions;
			RubyGlobalScope rubyGlobalScope = (RubyGlobalScope)scope.GetExtension(base.ContextId);
			if (rubyGlobalScope != null && rubyGlobalScope.TopLocalScope != null)
			{
				rubyCompilerOptions2.LocalNames = rubyGlobalScope.TopLocalScope.GetVisibleLocalNames();
			}
			return rubyCompilerOptions2;
		}

		public override ErrorSink GetCompilerErrorSink()
		{
			return _runtimeErrorSink;
		}

		public override ScriptCode LoadCompiledCode(Delegate method, string path, string customData)
		{
			SourceUnit sourceUnit = new SourceUnit(this, NullTextContentProvider.Null, path, SourceCodeKind.File);
			return new RubyScriptCode((Func<RubyScope, object, object>)method, sourceUnit, TopScopeFactoryKind.Hosted);
		}

		internal RubyGlobalScope InitializeGlobalScope(Scope globalScope, bool createHosted, bool bindGlobals)
		{
			ScopeExtension extension = globalScope.GetExtension(base.ContextId);
			if (extension != null)
			{
				return (RubyGlobalScope)extension;
			}
			RubyObject rubyObject = new RubyObject(_objectClass);
			RubyClass orCreateMainSingleton = GetOrCreateMainSingleton(rubyObject, null);
			RubyGlobalScope rubyGlobalScope = new RubyGlobalScope(this, globalScope, rubyObject, createHosted);
			if (bindGlobals)
			{
				orCreateMainSingleton.SetMethodNoEvent(this, Symbols.MethodMissing, new RubyScopeMethodMissingInfo(RubyMemberFlags.Private, orCreateMainSingleton));
				orCreateMainSingleton.SetGlobalScope(rubyGlobalScope);
			}
			return (RubyGlobalScope)globalScope.SetExtension(base.ContextId, rubyGlobalScope);
		}

		public override int ExecuteProgram(SourceUnit program)
		{
			try
			{
				RubyCompilerOptions rubyCompilerOptions = new RubyCompilerOptions(_options);
				rubyCompilerOptions.FactoryKind = TopScopeFactoryKind.Main;
				RubyCompilerOptions options = rubyCompilerOptions;
				CompileSourceCode(program, options, _runtimeErrorSink).Run();
			}
			catch (SystemExit systemExit)
			{
				return systemExit.Status;
			}
			return 0;
		}

		public void RegisterShutdownHandler(Proc proc)
		{
			ContractUtils.RequiresNotNull(proc, "proc");
			lock (ShutdownHandlersLock)
			{
				_shutdownHandlers.Add(proc);
			}
		}

		private void ExecuteShutdownHandlers()
		{
			SystemExit systemExit = null;
			Exception ex = null;
			while (true)
			{
				Proc[] array;
				lock (ShutdownHandlersLock)
				{
					if (_shutdownHandlers.Count == 0)
					{
						break;
					}
					array = _shutdownHandlers.ToReverseArray();
					_shutdownHandlers.Clear();
					goto IL_004e;
				}
				IL_004e:
				Proc[] array2 = array;
				foreach (Proc proc in array2)
				{
					try
					{
						proc.Call(null);
					}
					catch (SystemExit systemExit2)
					{
						systemExit = systemExit2;
					}
					catch (Exception ex2)
					{
						Exception ex4 = (CurrentException = ex2);
						ex = ex4;
						_runtimeErrorSink.WriteMessage(MutableString.CreateMutable(FormatException(ex4), RubyEncoding.UTF8));
					}
				}
			}
			if (systemExit != null)
			{
				throw systemExit;
			}
			if (ex != null)
			{
				throw new SystemExit(1);
			}
		}

		public override void Shutdown()
		{
			_upTime.Stop();
			if (RubyOptions.Profile)
			{
				List<Profiler.MethodCounter> profile = Profiler.Instance.GetProfile();
				using (TextWriter textWriter = File.CreateText("profile.log"))
				{
					int num = 0;
					long num2 = 0L;
					string[] array = new string[profile.Count];
					long[] array2 = new long[profile.Count];
					int num3 = 0;
					foreach (Profiler.MethodCounter item in profile)
					{
						string id = item.Id;
						if (id.Length > num)
						{
							num = id.Length;
						}
						num2 += item.Ticks;
						array[num3] = id;
						array2[num3] = item.Ticks;
						num3++;
					}
					Array.Sort(array2, array);
					for (int num4 = array.Length - 1; num4 >= 0; num4--)
					{
						long num5 = array2[num4];
						textWriter.WriteLine("{0,-" + (num + 4) + "} {1,8:F0} ms {2,5:F1}%", array[num4], new TimeSpan(Utils.DateTimeTicksFromStopwatch(num5)).TotalMilliseconds, (double)num5 / (double)num2 * 100.0);
					}
					textWriter.WriteLine("{0,-" + (num + 4) + "} {1,8:F0} ms", "total", new TimeSpan(Utils.DateTimeTicksFromStopwatch(num2)).TotalMilliseconds);
				}
			}
			if (Options.PerfStats)
			{
				using (TextWriter textWriter2 = File.CreateText("perfstats.log"))
				{
					textWriter2.WriteLine(string.Format("\r\n  total:         {0}\r\n  binding:       {1} ({2} calls)\r\n", _upTime.Elapsed, "N/A", "N/A"));
					PerfTrack.DumpStats(textWriter2);
				}
			}
			_loader.SaveCompiledCode();
			ExecuteShutdownHandlers();
			_currentException = null;
		}

		public override string FormatException(Exception exception)
		{
			SyntaxError syntaxError = exception as SyntaxError;
			if (syntaxError != null && syntaxError.HasLineInfo)
			{
				return FormatErrorMessage(syntaxError.Message, null, syntaxError.File, syntaxError.Line, syntaxError.Column, syntaxError.LineSourceCode);
			}
			RubyClass classOf = GetClassOf(exception);
			RubyExceptionData instance = RubyExceptionData.GetInstance(exception);
			string clrMessage = RubyExceptionData.GetClrMessage(this, instance.Message);
			RubyArray backtrace = instance.Backtrace;
			StringBuilder stringBuilder = new StringBuilder();
			if (backtrace != null && backtrace.Count > 0)
			{
				stringBuilder.AppendFormat("{0}: {1} ({2})", Protocols.ToClrStringNoThrow(this, backtrace[0]), clrMessage, classOf.Name);
				stringBuilder.AppendLine();
				for (int i = 1; i < backtrace.Count; i++)
				{
					stringBuilder.Append("\tfrom ").Append(Protocols.ToClrStringNoThrow(this, backtrace[i])).AppendLine();
				}
			}
			else
			{
				stringBuilder.AppendFormat("unknown: {0} ({1})", clrMessage, classOf.Name).AppendLine();
			}
			if (Options.ShowClrExceptions)
			{
				stringBuilder.AppendLine().AppendLine();
				stringBuilder.AppendLine("CLR exception:");
				stringBuilder.Append(base.FormatException(exception));
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		internal static string FormatErrorMessage(string message, string prefix, string file, int line, int column, string lineSource)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(file ?? "unknown");
			stringBuilder.Append(':');
			stringBuilder.Append((file != null) ? line : 0);
			stringBuilder.Append(": ");
			if (prefix != null)
			{
				stringBuilder.Append(prefix);
				stringBuilder.Append(": ");
			}
			stringBuilder.Append(message);
			stringBuilder.AppendLine();
			if (lineSource != null)
			{
				stringBuilder.Append(lineSource);
				stringBuilder.AppendLine();
				if (column > 0)
				{
					stringBuilder.Append(' ', column - 1);
					stringBuilder.Append('^');
					stringBuilder.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		public override TService GetService<TService>(params object[] args)
		{
			if (typeof(TService) == typeof(RubyService))
			{
				return (TService)(object)(_rubyService ?? (_rubyService = new RubyService(this, (ScriptEngine)args[0])));
			}
			if (typeof(TService) == typeof(TokenizerService))
			{
				return (TService)(object)new Tokenizer();
			}
			return base.GetService<TService>(args);
		}

		public override void SetSearchPaths(ICollection<string> paths)
		{
			ContractUtils.RequiresNotNullItems(paths, "paths");
			_loader.SetLoadPaths(paths);
		}

		public override ICollection<string> GetSearchPaths()
		{
			return _loader.GetLoadPathStrings();
		}

		public override SourceCodeReader GetSourceReader(Stream stream, Encoding defaultEncoding, string path)
		{
			ContractUtils.RequiresNotNull(stream, "stream");
			ContractUtils.RequiresNotNull(defaultEncoding, "defaultEncoding");
			ContractUtils.Requires(stream.CanRead && stream.CanSeek, "stream", "The stream must support seeking and reading");
			return GetSourceReader(stream, defaultEncoding);
		}

		private SourceCodeReader GetSourceReader(Stream stream, Encoding defaultEncoding)
		{
			long num = stream.Position;
			StreamReader streamReader = new StreamReader(stream, BinaryEncoding.Instance, true);
			streamReader.Peek();
			Encoding encoding = ((streamReader.CurrentEncoding != BinaryEncoding.Instance) ? streamReader.CurrentEncoding : null);
			Encoding encoding2 = null;
			string encodingName;
			if (Tokenizer.TryParseEncodingHeader(streamReader, out encodingName))
			{
				encoding2 = GetEncodingByRubyName(encodingName);
				if (!RubyEncoding.AsciiIdentity(encoding2))
				{
					throw new IOException(string.Format("Encoding '{0}' is not allowed in preamble.", encoding2.WebName));
				}
			}
			if (encoding != null)
			{
				num += encoding.GetPreamble().Length;
			}
			stream.Seek(num, SeekOrigin.Begin);
			Encoding encoding3 = encoding2 ?? encoding ?? defaultEncoding;
			return new SourceCodeReader(new StreamReader(stream, encoding3, false), encoding3);
		}

		public Encoding GetEncodingByRubyName(string name)
		{
			ContractUtils.RequiresNotNull(name, "name");
			string text = name.ToUpperInvariant();
			switch (text)
			{
			case "BINARY":
			case "ASCII-8BIT":
				return BinaryEncoding.Instance;
			case "FILESYSTEM":
				return GetPathEncoding().StrictEncoding;
			case "LOCALE":
				return _options.LocaleEncoding.StrictEncoding;
			case "EXTERNAL":
				return _defaultExternalEncoding.StrictEncoding;
			case "SJIS":
				return Encoding.GetEncoding(932);
			case "WINDOWS-31J":
				return Encoding.GetEncoding(932);
			case "MACCYRILLIC":
				return Encoding.GetEncoding(10007);
			case "EUC-JP":
				return Encoding.GetEncoding(51932);
			case "ISO-2022-JP":
				return Encoding.GetEncoding(50220);
			case "CP1025":
				return Encoding.GetEncoding(21025);
			default:
			{
				string value;
				if (RubyEncoding.Aliases.TryGetValue(name, out value))
				{
					return GetEncodingByRubyName(value);
				}
				int result;
				if (text.StartsWith("CP", StringComparison.Ordinal) && int.TryParse(text.Substring(2), out result))
				{
					try
					{
						return Encoding.GetEncoding(result);
					}
					catch (NotSupportedException)
					{
					}
				}
				return Encoding.GetEncoding(name);
			}
			}
		}

		public RubyEncoding GetRubyEncoding(MutableString name)
		{
			if (!name.IsAscii())
			{
				throw new ArgumentException(string.Format("Unknown encoding: '{0}'", name.ToAsciiString()));
			}
			return RubyEncoding.GetRubyEncoding(GetEncodingByRubyName(name.ToString()));
		}

		public RubyEncoding GetRubyEncoding(string name)
		{
			return RubyEncoding.GetRubyEncoding(GetEncodingByRubyName(name));
		}

		public override string FormatObject(DynamicOperations operations, object obj)
		{
			CallSite<Func<CallSite, object, object>> orCreateSite = operations.GetOrCreateSite<object, object>(RubyCallAction.Make(this, "inspect", RubyCallSignature.WithImplicitSelf(1)));
			CallSite<Func<CallSite, object, MutableString>> orCreateSite2 = operations.GetOrCreateSite<object, MutableString>(ConvertToSAction.Make(this));
			return orCreateSite2.Target(orCreateSite2, orCreateSite.Target(orCreateSite, obj)).ToString();
		}

		public override GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase)
		{
			if (ignoreCase)
			{
				return base.CreateGetMemberBinder(name, ignoreCase);
			}
			return _metaBinderFactory.InteropGetMember(name);
		}

		public override SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase)
		{
			if (ignoreCase)
			{
				return base.CreateSetMemberBinder(name, ignoreCase);
			}
			return _metaBinderFactory.InteropSetMemberExact(name);
		}

		public override InvokeMemberBinder CreateCallBinder(string name, bool ignoreCase, CallInfo callInfo)
		{
			if (ignoreCase || callInfo.ArgumentNames.Count != 0)
			{
				return base.CreateCallBinder(name, ignoreCase, callInfo);
			}
			return _metaBinderFactory.InteropInvokeMember(name, callInfo);
		}

		public override CreateInstanceBinder CreateCreateBinder(CallInfo callInfo)
		{
			if (callInfo.ArgumentNames.Count != 0)
			{
				return base.CreateCreateBinder(callInfo);
			}
			return _metaBinderFactory.InteropCreateInstance(callInfo);
		}

		public override ConvertBinder CreateConvertBinder(Type toType, bool? explicitCast)
		{
			return _metaBinderFactory.InteropConvert(toType, explicitCast ?? true);
		}

		public IList<string> GetForeignDynamicMemberNames(object obj)
		{
			if (obj is IRubyDynamicMetaObjectProvider)
			{
				return ArrayUtils.EmptyStrings;
			}
			if (TypeUtils.IsComObject(obj))
			{
				return new List<string>(ComBinder.GetDynamicMemberNames(obj));
			}
			return GetMemberNames(obj);
		}

		public CallSite<TSiteFunc> GetOrCreateSendSite<TSiteFunc>(string methodName, RubyCallSignature callSignature) where TSiteFunc : class
		{
			lock (SendSitesLock)
			{
				CallSite value;
				if (_sendSites.TryGetValue(Key.Create(methodName, callSignature), out value))
				{
					return (CallSite<TSiteFunc>)value;
				}
				CallSite<TSiteFunc> callSite = CallSite<TSiteFunc>.Create(RubyCallAction.Make(this, methodName, callSignature));
				_sendSites.Add(Key.Create(methodName, callSignature), callSite);
				return callSite;
			}
		}

		internal object Send(ref CallSite<Func<CallSite, object, object, object>> site, string eventName, object target, string memberName)
		{
			if (site == null)
			{
				Interlocked.CompareExchange(ref site, CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(this, eventName, RubyCallSignature.WithImplicitSelf(1))), null);
			}
			return site.Target(site, target, EncodeIdentifier(memberName));
		}

		public bool RespondTo(object target, string methodName)
		{
			return RubyOps.IsTrue(Send(ref _respondTo, "respond_to?", target, methodName));
		}

		internal void ReportTraceEvent(string operation, RubyScope scope, RubyModule module, string name, string fileName, int lineNumber)
		{
			if (_traceListener != null && !_traceListenerSuspended)
			{
				try
				{
					_traceListenerSuspended = true;
					_traceListener.Call(null, MutableString.CreateAscii(operation), (fileName != null) ? scope.RubyContext.EncodePath(fileName) : null, ScriptingRuntimeHelpers.Int32ToObject(lineNumber), EncodeIdentifier(name), new Binding(scope), module.IsSingletonClass ? ((RubyClass)module).SingletonClassOf : module);
				}
				finally
				{
					_traceListenerSuspended = false;
				}
			}
		}
	}
}

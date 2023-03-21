using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[DebuggerTypeProxy(typeof(DebugView))]
	public class RubyModule : IDuplicable, IRubyObject, IRubyObjectState, IRubyDynamicMetaObjectProvider, IDynamicMetaObjectProvider
	{
		private enum State
		{
			Uninitialized,
			Initializing,
			Initialized
		}

		private enum MemberTableState
		{
			Uninitialized,
			Initializing,
			Initialized
		}

		private sealed class AutoloadedConstant
		{
			private readonly MutableString _path;

			private bool _loaded;

			public bool Loaded
			{
				get
				{
					return _loaded;
				}
			}

			public MutableString Path
			{
				get
				{
					return _path;
				}
			}

			public AutoloadedConstant(MutableString path)
			{
				_path = path;
			}

			public bool Load(RubyGlobalScope autoloadScope)
			{
				if (_loaded)
				{
					return false;
				}
				using (autoloadScope.Context.ClassHierarchyUnlocker())
				{
					_loaded = true;
					return autoloadScope.Context.Loader.LoadFile(autoloadScope.Scope, null, _path, LoadFlags.Require);
				}
			}
		}

		private enum ConstantLookupResult
		{
			NotFound,
			Found,
			FoundAutoload
		}

		internal sealed class DebugView
		{
			private readonly RubyModule _obj;

			[DebuggerDisplay("{GetModuleName(A),nq}", Name = "{GetClassKind(),nq}", Type = "")]
			public object A
			{
				get
				{
					return _obj.ImmediateClass;
				}
			}

			[DebuggerDisplay("{B}", Name = "tainted?", Type = "")]
			public bool B
			{
				get
				{
					return _obj.IsTainted;
				}
				set
				{
					_obj.IsTainted = value;
				}
			}

			[DebuggerDisplay("{C}", Name = "untrusted?", Type = "")]
			public bool C
			{
				get
				{
					return _obj.IsUntrusted;
				}
				set
				{
					_obj.IsUntrusted = value;
				}
			}

			[DebuggerDisplay("{D}", Name = "frozen?", Type = "")]
			public bool D
			{
				get
				{
					return _obj.IsFrozen;
				}
				set
				{
					if (value)
					{
						_obj.Freeze();
					}
				}
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public object E
			{
				get
				{
					RubyInstanceData rubyInstanceData = _obj.TryGetInstanceData();
					if (rubyInstanceData == null)
					{
						return new RubyInstanceData.VariableDebugView[0];
					}
					return rubyInstanceData.GetInstanceVariablesDebugView(_obj.ImmediateClass.Context);
				}
			}

			[DebuggerDisplay("{GetModuleName(F),nq}", Name = "super", Type = "")]
			public object F
			{
				get
				{
					return _obj.GetSuperClass();
				}
			}

			[DebuggerDisplay("", Name = "mixins", Type = "")]
			public object G
			{
				get
				{
					return _obj.GetMixins();
				}
			}

			[DebuggerDisplay("", Name = "instance methods", Type = "")]
			public object H
			{
				get
				{
					return GetMethods(RubyMethodAttributes.Instance);
				}
			}

			[DebuggerDisplay("", Name = "singleton methods", Type = "")]
			public object I
			{
				get
				{
					return GetMethods(RubyMethodAttributes.Singleton);
				}
			}

			public DebugView(RubyModule obj)
			{
				_obj = obj;
			}

			private string GetClassKind()
			{
				if (!_obj.ImmediateClass.IsSingletonClass)
				{
					return "class";
				}
				return "singleton class";
			}

			private static string GetModuleName(object module)
			{
				RubyModule rubyModule = (RubyModule)module;
				if (rubyModule == null)
				{
					return null;
				}
				return rubyModule.GetDisplayName(rubyModule.Context, false).ToString();
			}

			private Dictionary<string, RubyMemberInfo> GetMethods(RubyMethodAttributes attributes)
			{
				Dictionary<string, RubyMemberInfo> result = new Dictionary<string, RubyMemberInfo>();
				using (_obj.Context.ClassHierarchyLocker())
				{
					_obj.ForEachMember(false, attributes | RubyMethodAttributes.VisibilityMask, delegate(string name, RubyModule _, RubyMemberInfo info)
					{
						result[name] = info;
					});
				}
				return result;
			}
		}

		internal class Meta : RubyMetaObject<RubyModule>
		{
			public override RubyContext Context
			{
				get
				{
					return base.Value.Context;
				}
			}

			protected override MethodInfo ContextConverter
			{
				get
				{
					return Methods.GetContextFromModule;
				}
			}

			public Meta(Expression expression, BindingRestrictions restrictions, RubyModule value)
				: base(expression, restrictions, value)
			{
			}
		}

		public static readonly RubyModule[] EmptyArray = new RubyModule[0];

		internal static int _globalMethodVersion = 0;

		internal static int _globalModuleId = 0;

		private readonly RubyContext _context;

		private readonly NamespaceTracker _namespaceTracker;

		private readonly TypeTracker _typeTracker;

		private readonly ModuleRestrictions _restrictions;

		private readonly WeakReference _weakSelf;

		private string _name;

		private RubyInstanceData _instanceData;

		private RubyClass _immediateClass;

		public readonly VersionHandle Version;

		public readonly int Id;

		private WeakList<RubyClass> _dependentClasses;

		private MemberTableState _constantsState;

		private Action<RubyModule> _constantsInitializer;

		private Dictionary<string, ConstantStorage> _constants;

		private MemberTableState _methodsState;

		private Dictionary<string, RubyMemberInfo> _methods;

		private Action<RubyModule> _methodsInitializer;

		private Dictionary<string, object> _classVariables;

		private RubyModule[] _mixins;

		internal Dictionary<string, List<ExtensionMethodInfo>> _extensionMethods;

		private CallSite<Func<CallSite, object, object, object>> _constantMissingCallbackSite;

		private CallSite<Func<CallSite, object, object, object>> _methodAddedCallbackSite;

		private CallSite<Func<CallSite, object, object, object>> _methodRemovedCallbackSite;

		private CallSite<Func<CallSite, object, object, object>> _methodUndefinedCallbackSite;

		internal static readonly Action<RubyModule> EmptyInitializer = delegate
		{
		};

		public TypeTracker TypeTracker
		{
			get
			{
				return _typeTracker;
			}
		}

		public NamespaceTracker NamespaceTracker
		{
			get
			{
				return _namespaceTracker;
			}
		}

		public bool IsInterface
		{
			get
			{
				if (_typeTracker != null)
				{
					return _typeTracker.Type.IsInterface;
				}
				return false;
			}
		}

		public bool IsClrModule
		{
			get
			{
				if (_typeTracker != null)
				{
					return IsModuleType(_typeTracker.Type);
				}
				return false;
			}
		}

		public bool IsDummySingletonClass
		{
			get
			{
				return _immediateClass == this;
			}
		}

		public virtual bool IsSingletonClass
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsClass
		{
			get
			{
				return false;
			}
		}

		public bool IsObjectClass
		{
			get
			{
				return object.ReferenceEquals(this, Context.ObjectClass);
			}
		}

		public bool IsBasicObjectClass
		{
			get
			{
				return object.ReferenceEquals(this, Context.BasicObjectClass);
			}
		}

		public bool IsComClass
		{
			get
			{
				return object.ReferenceEquals(this, Context.ComObjectClass);
			}
		}

		public ModuleRestrictions Restrictions
		{
			get
			{
				return _restrictions;
			}
		}

		internal RubyModule[] Mixins
		{
			get
			{
				return _mixins;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			internal set
			{
				_name = value;
			}
		}

		public RubyContext Context
		{
			get
			{
				return _context;
			}
		}

		internal virtual RubyGlobalScope GlobalScope
		{
			get
			{
				return null;
			}
		}

		internal WeakReference WeakSelf
		{
			get
			{
				return _weakSelf;
			}
		}

		internal Dictionary<string, List<ExtensionMethodInfo>> ExtensionMethods
		{
			get
			{
				return _extensionMethods;
			}
		}

		internal bool ConstantInitializationNeeded
		{
			get
			{
				return _constantsState == MemberTableState.Uninitialized;
			}
		}

		internal bool MethodInitializationNeeded
		{
			get
			{
				return _methodsState == MemberTableState.Uninitialized;
			}
		}

		internal WeakList<RubyClass> DependentClasses
		{
			get
			{
				if (_dependentClasses == null)
				{
					_dependentClasses = new WeakList<RubyClass>();
				}
				return _dependentClasses;
			}
		}

		public RubyClass ImmediateClass
		{
			get
			{
				return _immediateClass;
			}
			set
			{
				throw new InvalidOperationException("Cannot change the immediate class of a module");
			}
		}

		public virtual bool IsFrozen
		{
			get
			{
				return IsModuleFrozen;
			}
		}

		internal bool IsModuleFrozen
		{
			get
			{
				if (_instanceData != null)
				{
					return _instanceData.IsFrozen;
				}
				return false;
			}
		}

		public bool IsTainted
		{
			get
			{
				return GetInstanceData().IsTainted;
			}
			set
			{
				GetInstanceData().IsTainted = value;
			}
		}

		public bool IsUntrusted
		{
			get
			{
				return GetInstanceData().IsUntrusted;
			}
			set
			{
				GetInstanceData().IsUntrusted = value;
			}
		}

		protected virtual bool CanIncludeClrInterface
		{
			get
			{
				return true;
			}
		}

		public virtual Type GetUnderlyingSystemType()
		{
			if (IsClrModule)
			{
				return _typeTracker.Type;
			}
			throw new InvalidOperationException();
		}

		internal virtual RubyClass GetSuperClass()
		{
			return null;
		}

		internal void InitializeImmediateClass(RubyClass cls)
		{
			_immediateClass = cls;
		}

		internal void InitializeImmediateClass(RubyClass singletonSuperClass, Action<RubyModule> trait)
		{
			RubyClass rubyClass;
			if (IsClass)
			{
				rubyClass = CreateSingletonClass(singletonSuperClass, trait);
				rubyClass.InitializeImmediateClass(_context.ClassClass.GetDummySingletonClass());
			}
			else if (trait != null)
			{
				rubyClass = CreateSingletonClass(singletonSuperClass, trait);
				rubyClass.InitializeImmediateClass(singletonSuperClass.GetDummySingletonClass());
			}
			else
			{
				rubyClass = singletonSuperClass;
			}
			InitializeImmediateClass(rubyClass);
		}

		internal object ConstantMissing(string name)
		{
			return Context.Send(ref _constantMissingCallbackSite, "const_missing", this, name);
		}

		public virtual void MethodAdded(string name)
		{
			Context.Send(ref _methodAddedCallbackSite, Symbols.MethodAdded, this, name);
		}

		internal virtual void MethodRemoved(string name)
		{
			Context.Send(ref _methodRemovedCallbackSite, Symbols.MethodRemoved, this, name);
		}

		internal virtual void MethodUndefined(string name)
		{
			Context.Send(ref _methodUndefinedCallbackSite, Symbols.MethodUndefined, this, name);
		}

		public RubyModule(RubyClass metaModuleClass)
			: this(metaModuleClass, null)
		{
		}

		protected RubyModule(RubyClass metaModuleClass, string name)
			: this(metaModuleClass.Context, name, null, null, null, null, null, ModuleRestrictions.None)
		{
			InitializeImmediateClass(metaModuleClass, null);
		}

		internal RubyModule(RubyContext context, string name, Action<RubyModule> methodsInitializer, Action<RubyModule> constantsInitializer, RubyModule[] expandedMixins, NamespaceTracker namespaceTracker, TypeTracker typeTracker, ModuleRestrictions restrictions)
		{
			_context = context;
			_name = name;
			_methodsInitializer = methodsInitializer;
			_constantsInitializer = constantsInitializer;
			_namespaceTracker = namespaceTracker;
			_typeTracker = typeTracker;
			_mixins = expandedMixins ?? EmptyArray;
			_restrictions = restrictions;
			_weakSelf = new WeakReference(this);
			Version = new VersionHandle(Interlocked.Increment(ref _globalMethodVersion));
			Id = Interlocked.Increment(ref _globalModuleId);
		}

		private void InitializeConstantTableNoLock()
		{
			if (!ConstantInitializationNeeded)
			{
				return;
			}
			_constants = new Dictionary<string, ConstantStorage>();
			_constantsState = MemberTableState.Initializing;
			try
			{
				if (_constantsInitializer != EmptyInitializer)
				{
					if (_constantsInitializer != null)
					{
						_constantsInitializer(this);
					}
					else if (_typeTracker != null && !_typeTracker.Type.IsInterface)
					{
						LoadNestedTypes();
					}
				}
			}
			finally
			{
				_constantsInitializer = null;
				_constantsState = MemberTableState.Initialized;
			}
		}

		private void InitializeMethodTableNoLock()
		{
			if (!MethodInitializationNeeded)
			{
				return;
			}
			InitializeDependencies();
			_methods = new Dictionary<string, RubyMemberInfo>();
			_methodsState = MemberTableState.Initializing;
			try
			{
				if (_methodsInitializer != null)
				{
					_methodsInitializer(this);
				}
			}
			finally
			{
				_methodsInitializer = null;
				_methodsState = MemberTableState.Initialized;
			}
		}

		internal void InitializeMethodsNoLock()
		{
			if (MethodInitializationNeeded)
			{
				InitializeMethodsNoLock(GetUninitializedAncestors(true));
			}
		}

		internal void InitializeMethodsNoLock(IList<RubyModule> modules)
		{
			for (int num = modules.Count - 1; num >= 0; num--)
			{
				modules[num].InitializeMethodTableNoLock();
			}
		}

		internal void InitializeConstantsNoLock()
		{
			if (ConstantInitializationNeeded)
			{
				InitializeConstantsNoLock(GetUninitializedAncestors(false));
			}
		}

		internal void InitializeConstantsNoLock(IList<RubyModule> modules)
		{
			for (int num = modules.Count - 1; num >= 0; num--)
			{
				modules[num].InitializeConstantTableNoLock();
			}
		}

		private List<RubyModule> GetUninitializedAncestors(bool methods)
		{
			List<RubyModule> list = new List<RubyModule>();
			list.Add(this);
			list.AddRange(_mixins);
			RubyClass superClass = GetSuperClass();
			while (superClass != null && (methods ? superClass.MethodInitializationNeeded : superClass.ConstantInitializationNeeded))
			{
				list.Add(superClass);
				list.AddRange(superClass._mixins);
				superClass = superClass.SuperClass;
			}
			return list;
		}

		private void InitializeClassVariableTable()
		{
			if (_classVariables == null)
			{
				Interlocked.CompareExchange(ref _classVariables, new Dictionary<string, object>(), null);
			}
		}

		private void LoadNestedTypes()
		{
			BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public;
			if (Context.DomainManager.Configuration.PrivateBinding)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			Type[] nestedTypes = _typeTracker.Type.GetNestedTypes(bindingFlags);
			List<TypeTracker> list = new List<TypeTracker>();
			List<string> list2 = new List<string>();
			Type[] array = nestedTypes;
			foreach (Type type in array)
			{
				TypeTracker typeTracker = (NestedTypeTracker)MemberTracker.FromMemberInfo(type);
				string text = (type.IsGenericType ? ReflectionUtils.GetNormalizedTypeName(type) : type.Name);
				int num = list2.IndexOf(text);
				if (num != -1)
				{
					list[num] = TypeGroup.UpdateTypeEntity(list[num], typeTracker);
					list2[num] = text;
				}
				else
				{
					list.Add(typeTracker);
					list2.Add(text);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				TypeTracker typeTracker2 = list[j];
				ConstantStorage value;
				if (typeTracker2 is TypeGroup)
				{
					value = new ConstantStorage(typeTracker2, new WeakReference(typeTracker2));
				}
				else
				{
					RubyModule module = Context.GetModule(typeTracker2.Type);
					value = new ConstantStorage(module, module.WeakSelf);
				}
				_constants[list2[j]] = value;
			}
		}

		internal void InitializeMembersFrom(RubyModule module)
		{
			Mutate();
			if (module._namespaceTracker != null && _constants == null)
			{
				module.InitializeConstantsNoLock();
				InitializeConstantsNoLock();
			}
			else
			{
				_constantsInitializer = Utils.CloneInvocationChain(module._constantsInitializer);
				_constantsState = module._constantsState;
			}
			_constants = ((module._constants != null) ? new Dictionary<string, ConstantStorage>(module._constants) : null);
			if (module._namespaceTracker != null)
			{
				foreach (KeyValuePair<string, object> item in module._namespaceTracker)
				{
					_constants.Add(item.Key, new ConstantStorage(item.Value));
				}
			}
			_methodsInitializer = Utils.CloneInvocationChain(module._methodsInitializer);
			_methodsState = module._methodsState;
			if (module._methods != null)
			{
				_methods = new Dictionary<string, RubyMemberInfo>(module._methods.Count);
				foreach (KeyValuePair<string, RubyMemberInfo> method in module._methods)
				{
					_methods[method.Key] = method.Value.Copy(method.Value.Flags, this);
				}
			}
			else
			{
				_methods = null;
			}
			_classVariables = ((module._classVariables != null) ? new Dictionary<string, object>(module._classVariables) : null);
			_mixins = ArrayUtils.Copy(module._mixins);
			MethodsUpdated("InitializeFrom");
		}

		public void InitializeModuleCopy(RubyModule module)
		{
			if (_context.IsObjectFrozen(this))
			{
				throw RubyExceptions.CreateTypeError("can't modify frozen Module");
			}
			using (Context.ClassHierarchyLocker())
			{
				InitializeMembersFrom(module);
			}
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			RubyClass immediateClass = _immediateClass;
			RubyModule rubyModule = new RubyModule(immediateClass.IsSingletonClass ? immediateClass.SuperClass : immediateClass, null);
			if (copySingletonMembers && immediateClass.IsSingletonClass)
			{
				RubyClass orCreateSingletonClass = rubyModule.GetOrCreateSingletonClass();
				using (Context.ClassHierarchyLocker())
				{
					orCreateSingletonClass.InitializeMembersFrom(immediateClass);
				}
			}
			_context.CopyInstanceData(this, rubyModule, false);
			return rubyModule;
		}

		private void Mutate()
		{
			if (IsFrozen)
			{
				throw RubyExceptions.CreateRuntimeError(string.Format("can't modify frozen {0}", IsClass ? "class" : "module"));
			}
		}

		[Conditional("DEBUG")]
		internal void OwnedMethodCachedInSite()
		{
		}

		internal virtual void InitializeDependencies()
		{
		}

		internal void AddDependentClass(RubyClass dependentClass)
		{
			foreach (RubyClass dependentClass2 in DependentClasses)
			{
				if (object.ReferenceEquals(dependentClass, dependentClass2))
				{
					return;
				}
			}
			DependentClasses.Add(dependentClass.WeakSelf);
		}

		private void IncrementMethodVersion()
		{
			if (IsClass)
			{
				Version.Method = Interlocked.Increment(ref _globalMethodVersion);
			}
		}

		internal void MethodsUpdated(string reason)
		{
			Func<RubyModule, bool> func = delegate(RubyModule module)
			{
				module.IncrementMethodVersion();
				return false;
			};
			func(this);
			ForEachRecursivelyDependentClass(func);
		}

		private bool ForEachRecursivelyDependentClass(Func<RubyModule, bool> action)
		{
			if (_dependentClasses != null)
			{
				foreach (RubyClass dependentClass in _dependentClasses)
				{
					if (action(dependentClass))
					{
						return true;
					}
					if (dependentClass.ForEachRecursivelyDependentClass(action))
					{
						return true;
					}
				}
			}
			return false;
		}

		public RubyInstanceData TryGetInstanceData()
		{
			return _instanceData;
		}

		public RubyInstanceData GetInstanceData()
		{
			return RubyOps.GetInstanceData(ref _instanceData);
		}

		public void Freeze()
		{
			GetInstanceData().Freeze();
		}

		int IRubyObject.BaseGetHashCode()
		{
			return base.GetHashCode();
		}

		bool IRubyObject.BaseEquals(object other)
		{
			return base.Equals(other);
		}

		string IRubyObject.BaseToString()
		{
			return base.ToString();
		}

		public override string ToString()
		{
			return _name ?? "<anonymous>";
		}

		public static object CreateAnonymousModule(RubyScope scope, BlockParam body, RubyClass self)
		{
			RubyModule rubyModule = new RubyModule(self, null);
			if (body == null)
			{
				return rubyModule;
			}
			return RubyUtils.EvaluateInModule(rubyModule, body, null, rubyModule);
		}

		public RubyClass GetOrCreateSingletonClass()
		{
			if (IsDummySingletonClass)
			{
				throw new InvalidOperationException("Dummy singleton class has no singleton class");
			}
			RubyClass immediateClass = _immediateClass;
			RubyClass superClass;
			RubyClass cls;
			if (!immediateClass.IsSingletonClass)
			{
				superClass = immediateClass;
				cls = immediateClass.GetDummySingletonClass();
			}
			else
			{
				if (!immediateClass.IsDummySingletonClass)
				{
					return immediateClass;
				}
				superClass = immediateClass.SuperClass;
				cls = immediateClass;
			}
			RubyClass rubyClass = CreateSingletonClass(superClass, null);
			rubyClass.InitializeImmediateClass(cls);
			Interlocked.CompareExchange(ref _immediateClass, rubyClass, immediateClass);
			return _immediateClass;
		}

		internal RubyClass CreateSingletonClass(RubyClass superClass, Action<RubyModule> trait)
		{
			TypeTracker tracker = (IsSingletonClass ? null : _typeTracker);
			return new RubyClass(Context, null, null, this, trait, null, null, superClass, null, tracker, null, false, true, Restrictions);
		}

		public bool ForEachAncestor(bool inherited, Func<RubyModule, bool> action)
		{
			if (inherited)
			{
				return ForEachAncestor(action);
			}
			return ForEachDeclaredAncestor(action);
		}

		internal virtual bool ForEachAncestor(Func<RubyModule, bool> action)
		{
			return ForEachDeclaredAncestor(action);
		}

		internal bool ForEachDeclaredAncestor(Func<RubyModule, bool> action)
		{
			if (action(this))
			{
				return true;
			}
			RubyModule[] mixins = _mixins;
			foreach (RubyModule arg in mixins)
			{
				if (action(arg))
				{
					return true;
				}
			}
			return false;
		}

		public string MakeNestedModuleName(string nestedModuleSimpleName)
		{
			if (!IsObjectClass && nestedModuleSimpleName != null)
			{
				return _name + "::" + nestedModuleSimpleName;
			}
			return nestedModuleSimpleName;
		}

		public void ForEachConstant(bool inherited, Func<RubyModule, string, object, bool> action)
		{
			ForEachAncestor(inherited, (RubyModule module) => action(module, null, Missing.Value) || module.EnumerateConstants(action));
		}

		public void SetConstant(string name, object value)
		{
			using (Context.ClassHierarchyLocker())
			{
				SetConstantNoLock(name, value);
			}
		}

		internal void Publish(string name)
		{
			RubyOps.ScopeSetMember(_context.TopGlobalScope, name, this);
		}

		private void SetConstantNoLock(string name, object value)
		{
			Mutate();
			SetConstantNoMutateNoLock(name, value);
		}

		internal void SetConstantNoMutateNoLock(string name, object value)
		{
			InitializeConstantsNoLock();
			_context.ConstantAccessVersion++;
			_constants[name] = new ConstantStorage(value);
		}

		public bool SetConstantChecked(string name, object value)
		{
			using (Context.ClassHierarchyLocker())
			{
				ConstantStorage value2;
				ConstantLookupResult constantLookupResult = TryLookupConstantNoLock(false, false, null, name, out value2);
				SetConstantNoLock(name, value);
				return constantLookupResult == ConstantLookupResult.Found;
			}
		}

		public void SetAutoloadedConstant(string name, MutableString path)
		{
			ConstantStorage value;
			if (!TryGetConstant((RubyGlobalScope)null, name, out value))
			{
				SetConstant(name, new AutoloadedConstant(MutableString.Create(path).Freeze()));
			}
		}

		public MutableString GetAutoloadedConstantPath(string name)
		{
			using (Context.ClassHierarchyLocker())
			{
				ConstantStorage storage;
				AutoloadedConstant autoloadedConstant;
				return (TryGetConstantNoAutoloadCheck(name, out storage) && (autoloadedConstant = storage.Value as AutoloadedConstant) != null && !autoloadedConstant.Loaded) ? autoloadedConstant.Path : null;
			}
		}

		internal bool TryResolveConstant(RubyContext callerContext, RubyGlobalScope autoloadScope, string name, out ConstantStorage value)
		{
			if (callerContext == Context)
			{
				return TryResolveConstantNoLock(autoloadScope, name, out value);
			}
			return TryResolveConstant(autoloadScope, name, out value);
		}

		public bool TryGetConstant(RubyGlobalScope autoloadScope, string name, out object value)
		{
			ConstantStorage value2;
			bool result = TryGetConstant(autoloadScope, name, out value2);
			value = value2.Value;
			return result;
		}

		internal bool TryGetConstant(RubyGlobalScope autoloadScope, string name, out ConstantStorage value)
		{
			using (Context.ClassHierarchyLocker())
			{
				return TryGetConstantNoLock(autoloadScope, name, out value);
			}
		}

		internal bool TryGetConstantNoLock(RubyGlobalScope autoloadScope, string name, out ConstantStorage value)
		{
			return TryLookupConstantNoLock(false, false, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
		}

		internal bool TryResolveConstant(RubyGlobalScope autoloadScope, string name, out ConstantStorage value)
		{
			using (Context.ClassHierarchyLocker())
			{
				return TryResolveConstantNoLock(autoloadScope, name, out value);
			}
		}

		internal bool TryResolveConstantNoLock(RubyGlobalScope autoloadScope, string name, out ConstantStorage value)
		{
			return TryLookupConstantNoLock(true, true, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
		}

		private ConstantLookupResult TryLookupConstantNoLock(bool included, bool inherited, RubyGlobalScope autoloadScope, string name, out ConstantStorage value)
		{
			value = default(ConstantStorage);
			AutoloadedConstant autoloadedConstant;
			do
			{
				ConstantStorage storage;
				RubyModule rubyModule = (included ? TryResolveConstantNoAutoloadCheck(inherited, name, out storage) : (TryGetConstantNoAutoloadCheck(name, out storage) ? this : null));
				if (rubyModule == null)
				{
					return ConstantLookupResult.NotFound;
				}
				autoloadedConstant = storage.Value as AutoloadedConstant;
				if (autoloadedConstant == null)
				{
					value = storage;
					return ConstantLookupResult.Found;
				}
				if (autoloadScope == null)
				{
					return ConstantLookupResult.FoundAutoload;
				}
				if (autoloadScope.Context != Context)
				{
					throw RubyExceptions.CreateTypeError(string.Format("Cannot autoload constants to a foreign runtime #{0}", autoloadScope.Context.RuntimeId));
				}
				object value2;
				rubyModule.TryRemoveConstantNoLock(name, out value2);
			}
			while (autoloadedConstant.Load(autoloadScope));
			return ConstantLookupResult.NotFound;
		}

		private RubyModule TryResolveConstantNoAutoloadCheck(bool inherited, string name, out ConstantStorage value)
		{
			ConstantStorage storage = default(ConstantStorage);
			RubyModule owner = null;
			if (ForEachAncestor(inherited, (RubyModule module) => (owner = module).TryGetConstantNoAutoloadCheck(name, out storage)))
			{
				value = storage;
				return owner;
			}
			value = storage;
			return null;
		}

		internal bool TryGetConstantNoAutoloadCheck(string name, out ConstantStorage storage)
		{
			if (name.Length == 0)
			{
				storage = default(ConstantStorage);
				return false;
			}
			InitializeConstantsNoLock();
			if (_constants.TryGetValue(name, out storage))
			{
				if (storage.IsRemoved)
				{
					storage = default(ConstantStorage);
					return false;
				}
				return true;
			}
			object value;
			if (_namespaceTracker != null && _namespaceTracker.TryGetValue(name, out value))
			{
				storage = new ConstantStorage(_context.TrackerToModule(value));
				return true;
			}
			storage = default(ConstantStorage);
			return false;
		}

		internal bool TryGetConstantNoAutoloadNoInit(string name, out ConstantStorage storage)
		{
			storage = default(ConstantStorage);
			if (_constants != null && _constants.TryGetValue(name, out storage))
			{
				return !storage.IsRemoved;
			}
			return false;
		}

		public bool TryRemoveConstant(string name, out object value)
		{
			using (Context.ClassHierarchyLocker())
			{
				return TryRemoveConstantNoLock(name, out value);
			}
		}

		private bool TryRemoveConstantNoLock(string name, out object value)
		{
			InitializeConstantsNoLock();
			ConstantStorage value2;
			bool flag;
			if (_constants.TryGetValue(name, out value2))
			{
				if (value2.IsRemoved)
				{
					value = null;
					return false;
				}
				value = value2.Value;
				flag = true;
			}
			else
			{
				value = null;
				flag = false;
			}
			object value3;
			if (_namespaceTracker != null && _namespaceTracker.TryGetValue(name, out value3))
			{
				_constants[name] = ConstantStorage.Removed;
				_context.ConstantAccessVersion++;
				value = value3;
				flag = true;
			}
			else if (flag)
			{
				_constants.Remove(name);
				_context.ConstantAccessVersion++;
			}
			return flag;
		}

		public bool EnumerateConstants(Func<RubyModule, string, object, bool> action)
		{
			InitializeConstantsNoLock();
			foreach (KeyValuePair<string, ConstantStorage> constant in _constants)
			{
				string key = constant.Key;
				ConstantStorage value = constant.Value;
				if (!value.IsRemoved && action(this, key, value.Value))
				{
					return true;
				}
			}
			if (_namespaceTracker != null)
			{
				foreach (KeyValuePair<string, object> item in _namespaceTracker)
				{
					string key2 = item.Key;
					if (!_constants.ContainsKey(key2) && action(this, key2, item.Value))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void ForEachInstanceMethod(bool inherited, Func<RubyModule, string, RubyMemberInfo, bool> action)
		{
			ForEachAncestor(inherited, delegate(RubyModule module)
			{
				if (module.IsClrModule && !IsClrModule)
				{
					return false;
				}
				return action(module, null, null) || module.EnumerateMethods(action);
			});
		}

		public void AddMethodAlias(string newName, string oldName)
		{
			using (Context.ClassHierarchyLocker())
			{
				RubyMemberInfo info = ResolveMethodNoLock(oldName, VisibilityContext.AllVisible, MethodLookup.ReturnForwarder | MethodLookup.FallbackToObject).Info;
				if (info == null)
				{
					throw RubyExceptions.CreateUndefinedMethodError(this, oldName);
				}
				if (!info.IsRubyMember)
				{
					SetMethodNoEventNoLock(Context, newName, info.Copy(info.Flags, info.DeclaringModule));
				}
				else
				{
					SetMethodNoEventNoLock(Context, newName, info);
				}
			}
			MethodAdded(newName);
		}

		public void SetDefinedMethodNoEventNoLock(RubyContext callerContext, string name, RubyMemberInfo method, RubyMethodVisibility visibility)
		{
			SetMethodNoEventNoLock(callerContext, name, method.Copy((RubyMemberFlags)visibility, this));
		}

		public void SetVisibilityNoEventNoLock(RubyContext callerContext, string name, RubyMemberInfo method, RubyMethodVisibility visibility)
		{
			bool skipHidden = false;
			RubyMemberInfo method2;
			if (TryGetMethod(name, ref skipHidden, out method2))
			{
				SetMethodNoEventNoLock(callerContext, name, method.Copy((RubyMemberFlags)visibility, this));
			}
			else
			{
				SetMethodNoEventNoLock(callerContext, name, new SuperForwarderInfo((RubyMemberFlags)visibility, method.DeclaringModule, name));
			}
		}

		public void SetModuleFunctionNoEventNoLock(RubyContext callerContext, string name, RubyMemberInfo method)
		{
			RubyClass orCreateSingletonClass = GetOrCreateSingletonClass();
			orCreateSingletonClass.SetMethodNoEventNoLock(callerContext, name, method.Copy(RubyMemberFlags.Public, orCreateSingletonClass));
		}

		public void AddMethod(RubyContext callerContext, string name, RubyMemberInfo method)
		{
			Mutate();
			SetMethodNoEvent(callerContext, name, method);
			MethodAdded(name);
		}

		public void SetMethodNoEvent(RubyContext callerContext, string name, RubyMemberInfo method)
		{
			using (Context.ClassHierarchyLocker())
			{
				SetMethodNoEventNoLock(callerContext, name, method);
			}
		}

		public void SetMethodNoEventNoLock(RubyContext callerContext, string name, RubyMemberInfo method)
		{
			Mutate();
			SetMethodNoMutateNoEventNoLock(callerContext, name, method);
		}

		internal void SetMethodNoMutateNoEventNoLock(RubyContext callerContext, string name, RubyMemberInfo method)
		{
			if (callerContext != _context)
			{
				throw RubyExceptions.CreateTypeError(string.Format("Cannot define a method on a {0} `{1}' defined in a foreign runtime #{2}", IsClass ? "class" : "module", _name, _context.RuntimeId));
			}
			if (method.IsUndefined && name == Symbols.Initialize)
			{
				throw RubyExceptions.CreateTypeError("Cannot undefine `initialize' method");
			}
			PrepareMethodUpdate(name, method);
			InitializeMethodsNoLock();
			_methods[name] = method;
		}

		internal void AddExtensionMethodsNoLock(List<ExtensionMethodInfo> extensions)
		{
			PrepareExtensionMethodsUpdate(extensions);
			if (_extensionMethods == null)
			{
				_extensionMethods = new Dictionary<string, List<ExtensionMethodInfo>>();
			}
			foreach (ExtensionMethodInfo extension in extensions)
			{
				List<ExtensionMethodInfo> value;
				if (!_extensionMethods.TryGetValue(extension.Method.Name, out value))
				{
					_extensionMethods.Add(extension.Method.Name, value = new List<ExtensionMethodInfo>());
				}
				value.Add(extension);
			}
		}

		private static bool IsPartiallyInstantiated(Type type)
		{
			if (type.IsGenericParameter)
			{
				return false;
			}
			if (type.IsArray)
			{
				return !type.GetElementType().IsGenericParameter;
			}
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				if (!type2.IsGenericParameter)
				{
					return true;
				}
			}
			return false;
		}

		internal virtual void PrepareExtensionMethodsUpdate(List<ExtensionMethodInfo> extensions)
		{
			if (_dependentClasses == null)
			{
				return;
			}
			foreach (RubyClass dependentClass in _dependentClasses)
			{
				dependentClass.PrepareExtensionMethodsUpdate(extensions);
			}
		}

		internal virtual void PrepareMethodUpdate(string methodName, RubyMemberInfo method)
		{
			InitializeMethodsNoLock();
			if (_dependentClasses == null)
			{
				return;
			}
			foreach (RubyClass dependentClass in _dependentClasses)
			{
				dependentClass.PrepareMethodUpdate(methodName, method, 0);
			}
		}

		internal int InvalidateGroupsInDependentClasses(string methodName, int maxLevel)
		{
			int num = -1;
			if (_dependentClasses != null)
			{
				foreach (RubyClass dependentClass in _dependentClasses)
				{
					num = Math.Max(num, dependentClass.InvalidateGroupsInSubClasses(methodName, maxLevel));
				}
				return num;
			}
			return num;
		}

		internal bool TryGetDefinedMethod(string name, out RubyMemberInfo method)
		{
			if (_methods == null)
			{
				method = null;
				return false;
			}
			return _methods.TryGetValue(name, out method);
		}

		internal bool TryGetDefinedMethod(string name, ref bool skipHidden, out RubyMemberInfo method)
		{
			if (TryGetDefinedMethod(name, out method))
			{
				if (method.IsHidden || (skipHidden && !method.IsRemovable))
				{
					skipHidden = true;
					method = null;
					return false;
				}
				return true;
			}
			return false;
		}

		internal IEnumerable<KeyValuePair<string, RubyMemberInfo>> GetMethods()
		{
			return _methods;
		}

		internal void AddMethodNoCacheInvalidation(string name, RubyMemberInfo method)
		{
			_methods.Add(name, method);
		}

		internal bool RemoveMethodNoCacheInvalidation(string name)
		{
			return _methods.Remove(name);
		}

		public bool RemoveMethod(string name)
		{
			if (RemoveMethodNoEvent(name))
			{
				MethodRemoved(name);
				return true;
			}
			return false;
		}

		private bool RemoveMethodNoEvent(string name)
		{
			Mutate();
			using (Context.ClassHierarchyLocker())
			{
				InitializeMethodsNoLock();
				RubyMemberInfo value;
				if (_methods.TryGetValue(name, out value))
				{
					if (value.IsHidden || value.IsUndefined)
					{
						return false;
					}
					if (IsBasicObjectClass && name == Symbols.Initialize)
					{
						return false;
					}
					if (value.IsRemovable)
					{
						if (value.InvalidateSitesOnOverride || value.InvalidateGroupsOnRemoval)
						{
							MethodsUpdated("RemoveMethod: " + name);
						}
						if (value.InvalidateGroupsOnRemoval)
						{
							InvalidateGroupsInDependentClasses(name, int.MaxValue);
						}
						_methods.Remove(name);
					}
					else
					{
						SetMethodNoEventNoLock(Context, name, RubyMemberInfo.HiddenMethod);
					}
					return true;
				}
				if (TryGetClrMember(name, false, out value))
				{
					SetMethodNoEventNoLock(Context, name, RubyMemberInfo.HiddenMethod);
					return true;
				}
				return false;
			}
		}

		public void UndefineMethod(string name)
		{
			UndefineMethodNoEvent(name);
			MethodUndefined(name);
		}

		public void UndefineMethodNoEvent(string name)
		{
			SetMethodNoEvent(Context, name, RubyMemberInfo.UndefinedMethod);
		}

		public void HideMethod(string name)
		{
			SetMethodNoEvent(Context, name, RubyMemberInfo.HiddenMethod);
		}

		public MethodResolutionResult ResolveMethodForSite(string name, VisibilityContext visibility)
		{
			using (Context.ClassHierarchyLocker())
			{
				return ResolveMethodForSiteNoLock(name, visibility);
			}
		}

		public MethodResolutionResult ResolveMethod(string name, VisibilityContext visibility)
		{
			using (Context.ClassHierarchyLocker())
			{
				return ResolveMethodNoLock(name, visibility);
			}
		}

		public MethodResolutionResult ResolveMethodForSiteNoLock(string name, VisibilityContext visibility)
		{
			return ResolveMethodForSiteNoLock(name, visibility, MethodLookup.Default);
		}

		internal MethodResolutionResult ResolveMethodForSiteNoLock(string name, VisibilityContext visibility, MethodLookup options)
		{
			return ResolveMethodNoLock(name, visibility, options).InvalidateSitesOnOverride();
		}

		public MethodResolutionResult ResolveMethodNoLock(string name, VisibilityContext visibility)
		{
			return ResolveMethodNoLock(name, visibility, MethodLookup.Default);
		}

		public MethodResolutionResult ResolveMethodNoLock(string name, VisibilityContext visibility, MethodLookup options)
		{
			InitializeMethodsNoLock();
			RubyMemberInfo info = null;
			RubyModule owner = null;
			bool skipHidden = false;
			bool foundCallerSelf = false;
			MethodResolutionResult result = ((!ForEachAncestor(delegate(RubyModule module)
			{
				owner = module;
				foundCallerSelf |= module == visibility.Class;
				return module.TryGetMethod(name, ref skipHidden, (options & MethodLookup.Virtual) != 0, out info);
			})) ? MethodResolutionResult.NotFound : ((info != null && !info.IsUndefined) ? ((!IsMethodVisible(info, owner, visibility, foundCallerSelf)) ? new MethodResolutionResult(info, owner, false) : ((!info.IsSuperForwarder) ? new MethodResolutionResult(info, owner, true) : (((options & MethodLookup.ReturnForwarder) == 0) ? owner.ResolveSuperMethodNoLock(((SuperForwarderInfo)info).SuperName, owner) : new MethodResolutionResult(info, owner, true)))) : MethodResolutionResult.NotFound));
			if (!result.Found && (options & MethodLookup.FallbackToObject) != 0 && !IsClass)
			{
				return _context.ObjectClass.ResolveMethodNoLock(name, visibility, options & ~MethodLookup.FallbackToObject);
			}
			return result;
		}

		private bool IsMethodVisible(RubyMemberInfo method, RubyModule owner, VisibilityContext visibility, bool foundCallerSelf)
		{
			if (visibility.Class == null)
			{
				return visibility.IsVisible(method.Visibility);
			}
			if (method.Visibility == RubyMethodVisibility.Protected)
			{
				if (foundCallerSelf)
				{
					return true;
				}
				return visibility.Class.ForEachAncestor((RubyModule module) => module == owner || module == this);
			}
			return method.Visibility == RubyMethodVisibility.Public;
		}

		public MethodResolutionResult ResolveSuperMethodNoLock(string name, RubyModule callerModule)
		{
			InitializeMethodsNoLock();
			RubyMemberInfo info = null;
			RubyModule owner = null;
			bool foundModule = false;
			bool skipHidden = false;
			if (ForEachAncestor(delegate(RubyModule module)
			{
				if (module == callerModule)
				{
					foundModule = true;
					return false;
				}
				owner = module;
				return foundModule && module.TryGetMethod(name, ref skipHidden, out info) && !info.IsSuperForwarder;
			}) && !info.IsUndefined)
			{
				return new MethodResolutionResult(info, owner, true);
			}
			return MethodResolutionResult.NotFound;
		}

		public RubyMemberInfo GetMethod(string name)
		{
			ContractUtils.RequiresNotNull(name, "name");
			using (Context.ClassHierarchyLocker())
			{
				InitializeMethodsNoLock();
				bool skipHidden = false;
				RubyMemberInfo method;
				TryGetMethod(name, ref skipHidden, out method);
				return method;
			}
		}

		internal bool TryGetMethod(string name, ref bool skipHidden, out RubyMemberInfo method)
		{
			return TryGetMethod(name, ref skipHidden, false, out method);
		}

		internal bool TryGetMethod(string name, ref bool skipHidden, bool virtualLookup, out RubyMemberInfo method)
		{
			if (TryGetDefinedMethod(name, ref skipHidden, out method))
			{
				return true;
			}
			if (virtualLookup)
			{
				string name2;
				if ((name2 = RubyUtils.TryMangleMethodName(name)) != null && TryGetDefinedMethod(name2, ref skipHidden, out method) && method.IsRubyMember)
				{
					return true;
				}
				if (this != Context.KernelModule)
				{
					if (name == "GetHashCode" && TryGetDefinedMethod("hash", out method) && method.IsRubyMember)
					{
						return true;
					}
					if (name == "Equals" && TryGetDefinedMethod("eql?", out method) && method.IsRubyMember)
					{
						return true;
					}
					if (name == "ToString" && TryGetDefinedMethod("to_s", out method) && method.IsRubyMember)
					{
						return true;
					}
				}
			}
			if (!skipHidden)
			{
				return TryGetClrMember(name, virtualLookup, out method);
			}
			return false;
		}

		private bool TryGetClrMember(string name, bool virtualLookup, out RubyMemberInfo method)
		{
			if (_typeTracker != null && !IsModuleType(_typeTracker.Type))
			{
				bool flag = (Restrictions & ModuleRestrictions.NoNameMapping) == 0;
				bool unmangleNames = !virtualLookup && flag;
				if (TryGetClrMember(_typeTracker.Type, name, flag, unmangleNames, out method))
				{
					_methods.Add(name, method);
					return true;
				}
			}
			method = null;
			return false;
		}

		protected virtual bool TryGetClrMember(Type type, string name, bool mapNames, bool unmangleNames, out RubyMemberInfo method)
		{
			method = null;
			return false;
		}

		public bool EnumerateMethods(Func<RubyModule, string, RubyMemberInfo, bool> action)
		{
			InitializeMethodsNoLock();
			foreach (KeyValuePair<string, RubyMemberInfo> method in _methods)
			{
				if (method.Value.IsRubyMember && action(this, method.Key, method.Value))
				{
					return true;
				}
			}
			if (_typeTracker != null && !IsModuleType(_typeTracker.Type))
			{
				foreach (string item in EnumerateClrMembers(_typeTracker.Type))
				{
					if (action(this, item, RubyMemberInfo.InteropMember))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected virtual IEnumerable<string> EnumerateClrMembers(Type type)
		{
			return ArrayUtils.EmptyStrings;
		}

		public void ForEachMember(bool inherited, RubyMethodAttributes attributes, IEnumerable<string> foreignMembers, Action<string, RubyModule, RubyMemberInfo> action)
		{
			Dictionary<string, RubyMemberInfo> visited = new Dictionary<string, RubyMemberInfo>();
			bool instanceMethods = (attributes & RubyMethodAttributes.Instance) != 0;
			bool singletonMethods = (attributes & RubyMethodAttributes.Singleton) != 0;
			if (foreignMembers != null)
			{
				foreach (string foreignMember in foreignMembers)
				{
					action(foreignMember, this, RubyMemberInfo.InteropMember);
					visited.Add(foreignMember, RubyMemberInfo.InteropMember);
				}
			}
			bool stop = false;
			ForEachInstanceMethod(true, delegate(RubyModule module, string name, RubyMemberInfo member)
			{
				if (member == null)
				{
					if (stop)
					{
						return true;
					}
					if (instanceMethods)
					{
						stop = !inherited && (!IsClass || (module.IsClass && !module.IsSingletonClass));
					}
					else if (singletonMethods)
					{
						if ((!inherited && module != this) || (module.IsClass && !module.IsSingletonClass))
						{
							return true;
						}
					}
					else
					{
						stop = !inherited;
					}
				}
				else if (!visited.ContainsKey(name))
				{
					if (!member.IsUndefined && !member.IsHidden && ((uint)member.Visibility & (uint)attributes) != 0)
					{
						action(name, module, member);
					}
					visited.Add(name, member);
				}
				return false;
			});
		}

		public void ForEachMember(bool inherited, RubyMethodAttributes attributes, Action<string, RubyModule, RubyMemberInfo> action)
		{
			ForEachMember(inherited, attributes, null, action);
		}

		public void ForEachClassVariable(bool inherited, Func<RubyModule, string, object, bool> action)
		{
			ForEachAncestor(inherited, (RubyModule module) => action(module, null, Missing.Value) || module.EnumerateClassVariables(action));
		}

		public void SetClassVariable(string name, object value)
		{
			InitializeClassVariableTable();
			Mutate();
			_classVariables[name] = value;
		}

		public bool TryGetClassVariable(string name, out object value)
		{
			value = null;
			if (_classVariables != null)
			{
				return _classVariables.TryGetValue(name, out value);
			}
			return false;
		}

		public bool RemoveClassVariable(string name)
		{
			if (_classVariables != null)
			{
				return _classVariables.Remove(name);
			}
			return false;
		}

		public RubyModule TryResolveClassVariable(string name, out object value)
		{
			RubyModule result = null;
			object constValue = null;
			using (Context.ClassHierarchyLocker())
			{
				if (ForEachAncestor(delegate(RubyModule module)
				{
					if (module._classVariables != null && module._classVariables.TryGetValue(name, out constValue))
					{
						result = module;
						return true;
					}
					return false;
				}))
				{
					value = constValue;
					return result;
				}
			}
			value = null;
			return null;
		}

		public bool EnumerateClassVariables(Func<RubyModule, string, object, bool> action)
		{
			if (_classVariables != null)
			{
				foreach (KeyValuePair<string, object> classVariable in _classVariables)
				{
					if (action(this, classVariable.Key, classVariable.Value))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsModuleType(Type type)
		{
			if (!type.IsInterface)
			{
				return type.IsGenericTypeDefinition;
			}
			return true;
		}

		public bool HasAncestor(RubyModule module)
		{
			using (Context.ClassHierarchyLocker())
			{
				return HasAncestorNoLock(module);
			}
		}

		public bool HasAncestorNoLock(RubyModule module)
		{
			return ForEachAncestor(true, (RubyModule m) => m == module);
		}

		public RubyModule[] GetMixins()
		{
			using (Context.ClassHierarchyLocker())
			{
				return ArrayUtils.Copy(_mixins);
			}
		}

		public void IncludeModules(params RubyModule[] modules)
		{
			using (Context.ClassHierarchyLocker())
			{
				IncludeModulesNoLock(modules);
			}
		}

		internal void IncludeModulesNoLock(RubyModule[] modules)
		{
			Mutate();
			RubyUtils.RequireMixins(this, modules);
			RubyModule[] array = ExpandMixinsNoLock(GetSuperClass(), _mixins, modules);
			RubyModule[] array2 = array;
			foreach (RubyModule rubyModule in array2)
			{
				if (rubyModule.IsInterface && !CanIncludeClrInterface && Array.IndexOf(_mixins, rubyModule) == -1)
				{
					throw new InvalidOperationException(string.Format("Interface `{0}' cannot be included in class `{1}' because its underlying type has already been created", rubyModule.Name, Name));
				}
			}
			MixinsUpdated(_mixins, _mixins = array);
			_context.ConstantAccessVersion++;
		}

		internal void InitializeNewMixin(RubyModule mixin)
		{
			if (_methodsState != 0)
			{
				mixin.InitializeMethodTableNoLock();
			}
			if (_constantsState != 0)
			{
				mixin.InitializeConstantTableNoLock();
			}
		}

		internal virtual void MixinsUpdated(RubyModule[] oldMixins, RubyModule[] newMixins)
		{
		}

		public static RubyModule[] ExpandMixinsNoLock(RubyClass superClass, RubyModule[] modules)
		{
			return ExpandMixinsNoLock(superClass, EmptyArray, modules);
		}

		private static RubyModule[] ExpandMixinsNoLock(RubyClass superClass, RubyModule[] existing, IList<RubyModule> added)
		{
			List<RubyModule> list = new List<RubyModule>(existing);
			ExpandMixinsNoLock(superClass, list, 0, added, true);
			return list.ToArray();
		}

		private static int ExpandMixinsNoLock(RubyClass superClass, List<RubyModule> existing, int index, IList<RubyModule> added, bool recursive)
		{
			foreach (RubyModule item in added)
			{
				int num = existing.IndexOf(item);
				if (num >= 0)
				{
					index = num + 1;
					if (recursive)
					{
						index = ExpandMixinsNoLock(superClass, existing, index, item._mixins, false);
					}
					continue;
				}
				num = ExpandMixinsNoLock(superClass, existing, index, item._mixins, false);
				if (superClass == null || !superClass.HasAncestorNoLock(item))
				{
					existing.Insert(index, item);
					index = num + 1;
				}
				else
				{
					index = num;
				}
			}
			return index;
		}

		internal virtual bool IsSuperClassAncestor(RubyModule module)
		{
			return false;
		}

		internal List<Type> GetImplementedInterfaces()
		{
			List<Type> list = new List<Type>();
			using (Context.ClassHierarchyLocker())
			{
				RubyModule[] mixins = _mixins;
				foreach (RubyModule rubyModule in mixins)
				{
					if (rubyModule.IsInterface && !rubyModule.TypeTracker.Type.IsGenericTypeDefinition && !list.Contains(rubyModule.TypeTracker.Type))
					{
						list.Add(rubyModule.TypeTracker.Type);
					}
				}
				return list;
			}
		}

		private void IncludeTraitNoLock(ref Action<RubyModule> initializer, MemberTableState tableState, Action<RubyModule> trait)
		{
			if (tableState == MemberTableState.Uninitialized)
			{
				if (initializer != null && initializer != EmptyInitializer)
				{
					initializer = (Action<RubyModule>)Delegate.Combine(initializer, trait);
				}
				else
				{
					initializer = trait;
				}
				return;
			}
			using (Context.ClassHierarchyUnlocker())
			{
				trait(this);
			}
		}

		internal void IncludeLibraryModule(Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, RubyModule[] mixins, bool builtin)
		{
			if (!builtin)
			{
				Mutate();
			}
			using (Context.ClassHierarchyLocker())
			{
				if (instanceTrait != null)
				{
					IncludeTraitNoLock(ref _methodsInitializer, _methodsState, instanceTrait);
				}
				if (constantsInitializer != null)
				{
					IncludeTraitNoLock(ref _constantsInitializer, _constantsState, constantsInitializer);
				}
				if (classTrait != null)
				{
					RubyClass orCreateSingletonClass = GetOrCreateSingletonClass();
					orCreateSingletonClass.IncludeTraitNoLock(ref orCreateSingletonClass._methodsInitializer, orCreateSingletonClass._methodsState, classTrait);
				}
				IncludeModulesNoLock(mixins);
			}
		}

		public string GetName(RubyContext context)
		{
			if (context != _context)
			{
				return _name + "@" + _context.RuntimeId;
			}
			return _name;
		}

		public MutableString GetDisplayName(RubyContext context, bool showEmptyName)
		{
			if (IsSingletonClass)
			{
				RubyClass rubyClass = (RubyClass)this;
				MutableString mutableString = MutableString.CreateMutable(context.GetIdentifierEncoding());
				int num = 0;
				while (true)
				{
					num++;
					mutableString.Append("#<Class:");
					object singletonClassOf = rubyClass.SingletonClassOf;
					RubyModule rubyModule = singletonClassOf as RubyModule;
					if (rubyModule == null || (!rubyModule.IsSingletonClass && rubyModule.Name == null))
					{
						num++;
						mutableString.Append("#<");
						mutableString.Append(rubyClass.SuperClass.GetName(context));
						mutableString.Append(':');
						RubyUtils.AppendFormatHexObjectId(mutableString, RubyUtils.GetObjectId(_context, singletonClassOf));
						break;
					}
					if (!rubyModule.IsSingletonClass)
					{
						mutableString.Append(rubyModule.GetName(context));
						break;
					}
					rubyClass = (RubyClass)rubyModule;
				}
				return mutableString.Append('>', num);
			}
			if (_name == null)
			{
				if (showEmptyName)
				{
					return null;
				}
				MutableString mutableString2 = MutableString.CreateMutable(context.GetIdentifierEncoding());
				mutableString2.Append("#<");
				mutableString2.Append(_context.GetClassOf(this).GetName(context));
				mutableString2.Append(':');
				RubyUtils.AppendFormatHexObjectId(mutableString2, RubyUtils.GetObjectId(_context, this));
				mutableString2.Append('>');
				return mutableString2;
			}
			return MutableString.CreateMutable(GetName(context), context.GetIdentifierEncoding());
		}

		internal string GetDebuggerDisplayValue(object obj)
		{
			return Context.Inspect(obj).ConvertToString();
		}

		internal string GetDebuggerDisplayType()
		{
			return Name;
		}

		public virtual DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new Meta(parameter, BindingRestrictions.Empty, this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public sealed class RubyClass : RubyModule, IDuplicable
	{
		private sealed class ClrOverloadInfo
		{
			public OverloadInfo Overload { get; set; }

			public RubyOverloadGroupInfo Owner { get; set; }
		}

		internal new sealed class Meta : RubyModule.Meta
		{
			public Meta(Expression expression, BindingRestrictions restrictions, RubyClass value)
				: base(expression, restrictions, value)
			{
				ContractUtils.RequiresNotNull(value, "value");
			}

			public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
			{
				return InteropBinder.InvokeMember.Bind(CreateMetaContext(), binder, this, args, binder.FallbackCreateInstance);
			}

			public override IEnumerable<string> GetDynamicMemberNames()
			{
				List<string> names = new List<string>();
				using (Context.ClassHierarchyLocker())
				{
					base.Value.ImmediateClass.ForEachMember(true, RubyMethodAttributes.Public, delegate (string name, RubyModule module, RubyMemberInfo member)
					{
						names.Add(name);
					});
				}
				return names;
			}
		}

		public const string MainSingletonName = "__MainSingleton";

		private readonly int _level;

		private readonly RubyClass _superClass;

		private RubyClass _dummySingletonClass;

		private readonly bool _isSingletonClass;

		private readonly bool _isRubyClass;

		private readonly object _singletonClassOf;

		private Type _underlyingSystemType;

		private RubyGlobalScope _globalScope;

		private readonly RubyStruct.Info _structInfo;

		private bool _isUninitializedCopy;

		private readonly Delegate[] _factories;

		private bool _dependenciesInitialized;

		internal int _extensionVersion;

		private CallSite<Func<CallSite, object, object>> _inspectSite;

		private CallSite<Func<CallSite, object, MutableString>> _inspectResultConversionSite;

		private CallSite<Func<CallSite, object, object, object>> _eqlSite;

		private CallSite<Func<CallSite, object, object>> _hashSite;

		private CallSite<Func<CallSite, object, object>> _toStringSite;

		private CallSite<Func<CallSite, object, object>> _toArraySplatSite;

		private CallSite<Func<CallSite, object, object, object>> _newSite;

		private CallSite<Func<CallSite, object, object, object>> _classInheritedCallbackSite;

		private CallSite<Func<CallSite, object, object, object>> _singletonMethodAddedCallbackSite;

		private CallSite<Func<CallSite, object, object, object>> _singletonMethodRemovedCallbackSite;

		private CallSite<Func<CallSite, object, object, object>> _singletonMethodUndefinedCallbackSite;

		private static Dictionary<Key<Type, string, bool>, int> _clrFailedMemberLookupCache = new Dictionary<Key<Type, string, bool>, int>();

		private MethodBase[] _clrVectorFactories;

		internal Dictionary<string, bool> ClrSingletonMethods { get; set; }

		public CallSite<Func<CallSite, object, object>> InspectSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _inspectSite, base.Context, "inspect", 0);
			}
		}

		public CallSite<Func<CallSite, object, object, object>> NewSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _newSite, base.Context, "new", 1);
			}
		}

		internal CallSite<Func<CallSite, object, object>> ToImplicitTrySplatSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _toArraySplatSite, ProtocolConversionAction<ImplicitTrySplatAction>.Make(base.Context));
			}
		}

		public CallSite<Func<CallSite, object, MutableString>> InspectResultConversionSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _inspectResultConversionSite, ConvertToSAction.Make(base.Context));
			}
		}

		public CallSite<Func<CallSite, object, object, object>> EqualsSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _eqlSite, base.Context, "Equals", new RubyCallSignature(1, (RubyCallFlags)80));
			}
		}

		internal CallSite<Func<CallSite, object, object>> GetHashCodeSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _hashSite, base.Context, "GetHashCode", new RubyCallSignature(0, (RubyCallFlags)80));
			}
		}

		public CallSite<Func<CallSite, object, object>> ToStringSite
		{
			get
			{
				return RubyUtils.GetCallSite(ref _toStringSite, base.Context, "ToString", new RubyCallSignature(0, (RubyCallFlags)80));
			}
		}

		public RubyClass SuperClass
		{
			get
			{
				return _superClass;
			}
		}

		public int Level
		{
			get
			{
				return _level;
			}
		}

		public override bool IsClass
		{
			get
			{
				return true;
			}
		}

		public override bool IsSingletonClass
		{
			get
			{
				return _isSingletonClass;
			}
		}

		public object SingletonClassOf
		{
			get
			{
				return _singletonClassOf;
			}
		}

		public bool IsRubyClass
		{
			get
			{
				return _isRubyClass;
			}
		}

		internal RubyStruct.Info StructInfo
		{
			get
			{
				return _structInfo;
			}
		}

		internal override RubyGlobalScope GlobalScope
		{
			get
			{
				return _globalScope;
			}
		}

		protected override bool CanIncludeClrInterface
		{
			get
			{
				if (!IsSingletonClass)
				{
					return _underlyingSystemType == null;
				}
				return false;
			}
		}

		public RubyClass NominalClass
		{
			get
			{
				if (!IsSingletonClass)
				{
					return this;
				}
				return SuperClass;
			}
		}

		public override bool IsFrozen
		{
			get
			{
				if (!_isSingletonClass)
				{
					return base.IsModuleFrozen;
				}
				RubyClass rubyClass = this;
				while (true)
				{
					RubyModule rubyModule = rubyClass._singletonClassOf as RubyModule;
					if (rubyModule == null)
					{
						break;
					}
					if (rubyModule.IsSingletonClass)
					{
						rubyClass = (RubyClass)rubyModule;
						continue;
					}
					return rubyModule.IsModuleFrozen;
				}
				return base.Context.IsObjectFrozen(rubyClass._singletonClassOf);
			}
		}

		internal void ClassInheritedEvent(RubyClass subClass)
		{
			if (_classInheritedCallbackSite == null)
			{
				Interlocked.CompareExchange(ref _classInheritedCallbackSite, CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(base.Context, Symbols.Inherited, RubyCallSignature.WithImplicitSelf(1))), null);
			}
			_classInheritedCallbackSite.Target(_classInheritedCallbackSite, this, subClass);
		}

		public override void MethodAdded(string name)
		{
			if (IsSingletonClass)
			{
				base.Context.Send(ref _singletonMethodAddedCallbackSite, Symbols.SingletonMethodAdded, _singletonClassOf, name);
			}
			else
			{
				base.MethodAdded(name);
			}
		}

		internal override void MethodRemoved(string name)
		{
			if (IsSingletonClass)
			{
				base.Context.Send(ref _singletonMethodRemovedCallbackSite, Symbols.SingletonMethodRemoved, _singletonClassOf, name);
			}
			else
			{
				base.MethodRemoved(name);
			}
		}

		internal override void MethodUndefined(string name)
		{
			if (IsSingletonClass)
			{
				base.Context.Send(ref _singletonMethodUndefinedCallbackSite, Symbols.SingletonMethodUndefined, _singletonClassOf, name);
			}
			else
			{
				base.MethodUndefined(name);
			}
		}

		internal override RubyClass GetSuperClass()
		{
			return _superClass;
		}

		internal void InitializeDummySingleton()
		{
			_dummySingletonClass = CreateDummySingleton();
		}

		internal RubyClass GetDummySingletonClass()
		{
			if (_dummySingletonClass == null)
			{
				Interlocked.CompareExchange(ref _dummySingletonClass, CreateDummySingleton(), null);
			}
			return _dummySingletonClass;
		}

		private RubyClass CreateDummySingleton()
		{
			RubyClass rubyClass = new RubyClass(base.Context, null, null, this, null, null, null, base.ImmediateClass, null, null, null, false, true, ModuleRestrictions.None);
			rubyClass.InitializeImmediateClass(rubyClass);
			return rubyClass;
		}

		internal void SetGlobalScope(RubyGlobalScope value)
		{
			_globalScope = value;
		}

		public override Type GetUnderlyingSystemType()
		{
			if (_isSingletonClass)
			{
				throw RubyExceptions.CreateTypeError("A singleton class doesn't have underlying system type.");
			}
			if (_underlyingSystemType == null)
			{
				Interlocked.Exchange(ref _underlyingSystemType, RubyTypeDispenser.GetOrCreateType((_superClass != null) ? _superClass.GetUnderlyingSystemType() : typeof(BasicObject), GetImplementedInterfaces(), _superClass != null && (_superClass.Restrictions & ModuleRestrictions.NoOverrides) != 0));
			}
			return _underlyingSystemType;
		}

		public RubyClass(RubyClass rubyClass)
			: this(rubyClass.Context, null, null, null, null, null, null, rubyClass.Context.ObjectClass, null, null, null, true, false, ModuleRestrictions.None)
		{
			InitializeImmediateClass(rubyClass, null);
		}

		internal RubyClass(RubyContext context, string name, Type type, object singletonClassOf, Action<RubyModule> methodsInitializer, Action<RubyModule> constantsInitializer, Delegate[] factories, RubyClass superClass, RubyModule[] expandedMixins, TypeTracker tracker, RubyStruct.Info structInfo, bool isRubyClass, bool isSingletonClass, ModuleRestrictions restrictions)
			: base(context, name, methodsInitializer, constantsInitializer, expandedMixins, (type != typeof(object)) ? null : context.Namespaces, tracker, restrictions)
		{
			_underlyingSystemType = type;
			_superClass = superClass;
			_isSingletonClass = isSingletonClass;
			_isRubyClass = isRubyClass;
			_singletonClassOf = singletonClassOf;
			_factories = factories ?? IronRuby.Runtime.Utils.EmptyDelegates;
			if (superClass != null)
			{
				_level = superClass.Level + 1;
				_structInfo = structInfo ?? superClass._structInfo;
			}
			else
			{
				_level = 0;
			}
		}

		internal override void InitializeDependencies()
		{
			if (!_dependenciesInitialized)
			{
				_dependenciesInitialized = true;
				if (_superClass != null)
				{
					_superClass.AddDependentClass(this);
				}
				AddAsDependencyOf(base.Mixins);
			}
		}

		internal void AddAsDependencyOf(IList<RubyModule> modules)
		{
			for (int i = 0; i < modules.Count; i++)
			{
				modules[i].AddDependentClass(this);
			}
		}

		internal override void MixinsUpdated(RubyModule[] oldMixins, RubyModule[] newMixins)
		{
			int num = oldMixins.Length - 1;
			for (int num2 = newMixins.Length - 1; num2 >= 0; num2--)
			{
				RubyModule rubyModule = newMixins[num2];
				if (num < 0 || rubyModule != oldMixins[num])
				{
					rubyModule.AddDependentClass(this);
					InitializeNewMixin(rubyModule);
					if (!rubyModule.MethodInitializationNeeded)
					{
						foreach (KeyValuePair<string, RubyMemberInfo> method in rubyModule.GetMethods())
						{
							PrepareMethodUpdate(method.Key, method.Value, num2 + 1);
						}
					}
				}
				else
				{
					num--;
				}
			}
		}

		internal override void PrepareExtensionMethodsUpdate(List<ExtensionMethodInfo> extensions)
		{
			_extensionVersion++;
			if (base.MethodInitializationNeeded)
			{
				return;
			}
			MethodsUpdated("ExtensionMethods");
			foreach (ExtensionMethodInfo extension in extensions)
			{
				InvalidateGroupsInSubClasses(extension.Method.Name, int.MaxValue);
			}
		}

		internal override void PrepareMethodUpdate(string methodName, RubyMemberInfo method)
		{
			PrepareMethodUpdate(methodName, method, 0);
		}

		internal void PrepareMethodUpdate(string methodName, RubyMemberInfo method, int mixinsToSkip)
		{
			bool flag = false;
			if (_isSingletonClass && !_superClass.IsSingletonClass && !_superClass.IsRubyClass && !(_singletonClassOf is IRubyObject))
			{
				Dictionary<string, bool> clrSingletonMethods = _superClass.ClrSingletonMethods;
				if (clrSingletonMethods != null)
				{
					if (!clrSingletonMethods.ContainsKey(methodName))
					{
						clrSingletonMethods[methodName] = true;
						_superClass.MethodsUpdated("SetSingletonMethod: " + methodName);
						flag = true;
					}
				}
				else
				{
					clrSingletonMethods = (_superClass.ClrSingletonMethods = new Dictionary<string, bool>());
					clrSingletonMethods[methodName] = true;
					_superClass.MethodsUpdated("SetSingletonMethod: " + methodName);
					flag = true;
				}
			}
			if (base.MethodInitializationNeeded)
			{
				return;
			}
			int modulesToSkip;
			int num;
			if (mixinsToSkip > 0)
			{
				modulesToSkip = mixinsToSkip + 1;
				num = _level - 1;
			}
			else
			{
				modulesToSkip = 0;
				num = _level;
			}
			RubyMemberInfo rubyMemberInfo = ResolveOverriddenMethod(methodName, modulesToSkip);
			if (rubyMemberInfo == null)
			{
				if (!flag)
				{
					HashSet<string> missingMethodsCachedInSites = base.Context.MissingMethodsCachedInSites;
					if (missingMethodsCachedInSites != null && missingMethodsCachedInSites.Contains(methodName))
					{
						MethodsUpdated("SetMethod: " + methodName);
					}
				}
				return;
			}
			if (rubyMemberInfo.InvalidateSitesOnOverride && !flag)
			{
				MethodsUpdated("SetMethod: " + methodName);
			}
			RubyOverloadGroupInfo rubyOverloadGroupInfo = rubyMemberInfo as RubyOverloadGroupInfo;
			if (rubyOverloadGroupInfo != null)
			{
				if (rubyOverloadGroupInfo.MaxCachedOverloadLevel > num)
				{
					if (mixinsToSkip > 0)
					{
						InvalidateGroupsInSubClasses(methodName, rubyOverloadGroupInfo.MaxCachedOverloadLevel);
					}
					else
					{
						InvalidateGroupsInDependentClasses(methodName, rubyOverloadGroupInfo.MaxCachedOverloadLevel);
					}
					if (method.IsRemovable)
					{
						method.InvalidateGroupsOnRemoval = true;
					}
				}
			}
			else if (method.IsRemovable && rubyMemberInfo.InvalidateGroupsOnRemoval)
			{
				method.InvalidateGroupsOnRemoval = true;
			}
		}

		private RubyMemberInfo ResolveOverriddenMethod(string name, int modulesToSkip)
		{
			RubyMemberInfo result = null;
			bool skipHidden = false;
			if (ForEachAncestor(delegate (RubyModule module)
			{
				if (modulesToSkip > 0)
				{
					modulesToSkip--;
					return false;
				}
				return module.TryGetDefinedMethod(name, ref skipHidden, out result) && !result.IsSuperForwarder;
			}))
			{
				if (result == null || result.IsUndefined)
				{
					return null;
				}
				return result;
			}
			return null;
		}

		internal int InvalidateGroupsInSubClasses(string methodName, int maxLevel)
		{
			if (_level > maxLevel)
			{
				return -1;
			}
			RubyMemberInfo method;
			if (TryGetDefinedMethod(methodName, out method))
			{
				RubyOverloadGroupInfo rubyOverloadGroupInfo = method as RubyOverloadGroupInfo;
				if (rubyOverloadGroupInfo != null)
				{
					RemoveMethodNoCacheInvalidation(methodName);
					return Math.Max(_level, InvalidateGroupsInDependentClasses(methodName, maxLevel));
				}
				return -1;
			}
			return InvalidateGroupsInDependentClasses(methodName, maxLevel);
		}

		object IDuplicable.Duplicate(RubyContext context, bool copySingletonMembers)
		{
			if (IsSingletonClass)
			{
				throw RubyExceptions.CreateTypeError("can't copy singleton class");
			}
			if (base.IsBasicObjectClass)
			{
				throw RubyExceptions.CreateTypeError("can't copy the root class");
			}
			using (base.Context.ClassHierarchyLocker())
			{
				RubyClass rubyClass = Duplicate(null);
				rubyClass._isUninitializedCopy = true;
				return rubyClass;
			}
		}

		public void InitializeClassCopy(RubyClass rubyClass)
		{
			if (!_isUninitializedCopy)
			{
				throw RubyExceptions.CreateTypeError("already initialized class");
			}
			_isUninitializedCopy = false;
			InitializeModuleCopy(rubyClass);
			base.Name = null;
		}

		internal RubyClass Duplicate(object singletonClassOf)
		{
			RubyClass rubyClass = base.Context.CreateClass(base.Name, _underlyingSystemType, singletonClassOf, null, null, null, _factories, _superClass, null, null, _structInfo, IsRubyClass, IsSingletonClass, ModuleRestrictions.None);
			if (!IsSingletonClass)
			{
				rubyClass.ImmediateClass.InitializeMembersFrom(base.ImmediateClass);
				base.Context.CopyInstanceData(this, rubyClass, false);
			}
			return rubyClass;
		}

		public static object CreateAnonymousClass(RubyScope scope, BlockParam body, RubyClass self, [Optional] RubyClass superClass)
		{
			RubyContext rubyContext = scope.RubyContext;
			RubyModule innerMostModuleForConstantLookup = scope.GetInnerMostModuleForConstantLookup();
			RubyClass rubyClass = rubyContext.DefineClass(innerMostModuleForConstantLookup, null, superClass ?? rubyContext.ObjectClass, null);
			if (body == null)
			{
				return rubyClass;
			}
			return RubyUtils.EvaluateInModule(rubyClass, body, new RubyClass[1] { rubyClass }, rubyClass);
		}

		internal override bool ForEachAncestor(Func<RubyModule, bool> action)
		{
			for (RubyClass rubyClass = this; rubyClass != null; rubyClass = rubyClass._superClass)
			{
				if (rubyClass.ForEachDeclaredAncestor(action))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsSubclassOf(RubyClass super)
		{
			RubyClass rubyClass = this;
			do
			{
				if (rubyClass == super)
				{
					return true;
				}
				rubyClass = rubyClass.SuperClass;
			}
			while (rubyClass != null);
			return false;
		}

		public bool IsException()
		{
			return IsSubclassOf(base.Context.ExceptionClass);
		}

		public RubyClass GetNonSingletonClass()
		{
			RubyClass rubyClass = this;
			while (rubyClass != null && rubyClass.IsSingletonClass)
			{
				rubyClass = rubyClass._superClass;
			}
			return rubyClass;
		}

		internal RubyMemberInfo ResolveMethodMissingForSite(string name, RubyMethodVisibility incompatibleVisibility)
		{
			MethodResolutionResult methodResolutionResult = ResolveMethodForSiteNoLock(Symbols.MethodMissing, VisibilityContext.AllVisible);
			if (incompatibleVisibility == RubyMethodVisibility.None)
			{
				methodResolutionResult.InvalidateSitesOnMissingMethodAddition(name, base.Context);
			}
			return methodResolutionResult.Info;
		}

		private static bool IsFailureCached(Type type, string methodName, bool isStatic, int extensionVersion)
		{
			bool result = false;
			Key<Type, string, bool> key = Key.Create(type, methodName, isStatic);
			Dictionary<Key<Type, string, bool>, int> dictionary = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
			if (dictionary != null)
			{
				int value;
				if (dictionary.TryGetValue(key, out value))
				{
					if (value != extensionVersion)
					{
						dictionary.Remove(key);
						result = false;
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = false;
				}
				Interlocked.Exchange(ref _clrFailedMemberLookupCache, dictionary);
			}
			return result;
		}

		private static void CacheFailure(Type type, string methodName, bool isStatic, int extensionVersion)
		{
			Dictionary<Key<Type, string, bool>, int> dictionary = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
			if (dictionary != null)
			{
				dictionary[Key.Create(type, methodName, isStatic)] = extensionVersion;
				Interlocked.Exchange(ref _clrFailedMemberLookupCache, dictionary);
			}
		}

		public bool TryGetClrMember(string name, Type asType, out RubyMemberInfo method)
		{
			RubyClass rubyClass = this;
			while (rubyClass.TypeTracker == null)
			{
				rubyClass = rubyClass.SuperClass;
			}
			Type type = rubyClass.TypeTracker.Type;
			if (asType != null && !asType.IsAssignableFrom(type))
			{
				throw RubyExceptions.CreateNameError(string.Format("`{0}' does not inherit from `{1}'", rubyClass.Name, base.Context.GetTypeName(asType, true)));
			}
			method = null;
			using (base.Context.ClassHierarchyLocker())
			{
				return rubyClass.TryGetClrMember(asType ?? type, name, true, true, BindingFlags.Default, out method);
			}
		}

		public bool TryGetClrConstructor(out RubyMemberInfo method)
		{
			OverloadInfo[] constructors;
			if (base.TypeTracker != null && (constructors = GetConstructors(base.TypeTracker.Type)).Length > 0)
			{
				method = new RubyMethodGroupInfo(constructors, this, true);
				return true;
			}
			method = null;
			return false;
		}

		protected override bool TryGetClrMember(Type type, string name, bool mapNames, bool unmangleNames, out RubyMemberInfo method)
		{
			if (IsFailureCached(type, name, _isSingletonClass, _extensionVersion))
			{
				method = null;
				return false;
			}
			if (TryGetClrMember(type, name, mapNames, unmangleNames, BindingFlags.DeclaredOnly, out method))
			{
				return true;
			}
			CacheFailure(type, name, _isSingletonClass, _extensionVersion);
			method = null;
			return false;
		}

		private bool TryGetClrMember(Type type, string name, bool mapNames, bool unmangleNames, BindingFlags basicBindingFlags, out RubyMemberInfo method)
		{
			basicBindingFlags |= BindingFlags.Public | BindingFlags.NonPublic;
			BindingFlags bindingFlags = basicBindingFlags | (_isSingletonClass ? BindingFlags.Static : BindingFlags.Instance);
			if (type == typeof(object))
			{
				bindingFlags |= BindingFlags.Instance;
			}
			string clrName;
			if (mapNames && !_isSingletonClass && (clrName = RubyUtils.ToClrOperatorName(name)) != null)
			{
				if (TryGetClrMethod(type, basicBindingFlags | BindingFlags.Static, true, name, null, clrName, null, out method))
				{
					return true;
				}
			}
			else if (mapNames && (name == "[]" || name == "[]="))
			{
				if (type.IsArray && !_isSingletonClass)
				{
					bool flag = name.Length == 3;
					TryGetClrMethod(type, bindingFlags, false, name, null, flag ? "Set" : "Get", null, out method);
					return true;
				}
				object[] customAttributes = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
				if (customAttributes.Length == 1)
				{
					bool isWrite = name.Length == 3;
					if (TryGetClrProperty(type, bindingFlags, isWrite, name, ((DefaultMemberAttribute)customAttributes[0]).MemberName, null, out method))
					{
						return true;
					}
				}
			}
			else if (name.LastCharacter() == 61)
			{
				string text = name.Substring(0, name.Length - 1);
				string text2 = (unmangleNames ? RubyUtils.TryUnmangleMethodName(text) : null);
				if (TryGetClrProperty(type, bindingFlags, true, name, text, text2, out method))
				{
					return true;
				}
				if (TryGetClrField(type, bindingFlags, true, text, text2, out method))
				{
					return true;
				}
			}
			else
			{
				string text3 = (unmangleNames ? RubyUtils.TryUnmangleMethodName(name) : null);
				if (TryGetClrMethod(type, bindingFlags, false, name, null, name, text3, out method))
				{
					return true;
				}
				if (TryGetClrProperty(type, bindingFlags, false, name, name, text3, out method))
				{
					return true;
				}
				if (TryGetClrEvent(type, bindingFlags, name, text3, out method))
				{
					return true;
				}
				if (TryGetClrField(type, bindingFlags, false, name, text3, out method))
				{
					return true;
				}
			}
			method = null;
			return false;
		}

		protected override IEnumerable<string> EnumerateClrMembers(Type type)
		{
			BindingFlags basicBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
			string defaultIndexerName = null;
			if (!_isSingletonClass)
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
				if (customAttributes.Length == 1)
				{
					defaultIndexerName = ((DefaultMemberAttribute)customAttributes[0]).MemberName;
				}
			}
			try
			{
				MethodInfo[] methods = type.GetMethods(basicBindingFlags | BindingFlags.Static | BindingFlags.Instance);
				foreach (MethodInfo method in methods)
				{
					if (!IsVisible(method.Attributes, method.DeclaringType, false))
					{
						continue;
					}
					if (method.IsSpecialName)
					{
						string name = RubyUtils.MapOperator(method);
						if (name != null)
						{
							yield return name;
						}
						if (method.IsStatic != _isSingletonClass)
						{
							continue;
						}
						if (method.Name.StartsWith("get_"))
						{
							string propertyName2 = method.Name.Substring(4);
							yield return propertyName2;
							if (propertyName2 == defaultIndexerName)
							{
								yield return "[]";
							}
						}
						if (method.Name.StartsWith("set_"))
						{
							string propertyName = method.Name.Substring(4);
							yield return propertyName + "=";
							if (propertyName == defaultIndexerName)
							{
								yield return "[]=";
							}
						}
					}
					else if (method.IsStatic == _isSingletonClass)
					{
						yield return method.Name;
					}
				}
			}
			finally
			{
			}
			BindingFlags bindingFlags = basicBindingFlags | (_isSingletonClass ? BindingFlags.Static : BindingFlags.Instance);
			try
			{
				FieldInfo[] fields = type.GetFields(bindingFlags);
				foreach (FieldInfo field in fields)
				{
					if (IsVisible(field))
					{
						yield return field.Name;
						if (IsWriteable(field))
						{
							yield return field.Name + "=";
						}
					}
				}
			}
			finally
			{
			}
			try
			{
				EventInfo[] events = type.GetEvents(bindingFlags);
				foreach (EventInfo evnt in events)
				{
					yield return evnt.Name;
				}
			}
			finally
			{
			}
		}

		private bool TryGetClrMethod(Type type, BindingFlags bindingFlags, bool specialNameOnly, string name, string clrNamePrefix, string clrName, string altClrName, out RubyMemberInfo method)
		{
			List<OverloadInfo> list = new List<OverloadInfo>(GetDeclaredClrMethods(type, bindingFlags, clrNamePrefix, clrName, altClrName, specialNameOnly));
			if (list.Count == 0)
			{
				method = null;
				return false;
			}
			if ((bindingFlags & BindingFlags.DeclaredOnly) == 0)
			{
				method = MakeGroup(list, list.Count, specialNameOnly, true);
				return true;
			}
			List<RubyClass> ancestors = new List<RubyClass>();
			RubyMemberInfo inheritedRubyMember = null;
			bool skipHidden = false;
			ForEachAncestor(delegate (RubyModule module)
			{
				if (module != this)
				{
					if (module.TryGetDefinedMethod(name, ref skipHidden, out inheritedRubyMember) && !inheritedRubyMember.IsSuperForwarder)
					{
						return true;
					}
					if (!skipHidden && module.TypeTracker != null && module.IsClass)
					{
						ancestors.Add((RubyClass)module);
					}
				}
				return false;
			});
			Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo> methods = null;
			if (inheritedRubyMember != null)
			{
				RubyOverloadGroupInfo rubyOverloadGroupInfo = inheritedRubyMember as RubyOverloadGroupInfo;
				if (rubyOverloadGroupInfo != null)
				{
					AddMethodsOverwriteExisting(ref methods, rubyOverloadGroupInfo.MethodBases, rubyOverloadGroupInfo.OverloadOwners, specialNameOnly);
				}
				else if (inheritedRubyMember.IsRemovable)
				{
					inheritedRubyMember.InvalidateGroupsOnRemoval = true;
				}
			}
			for (int num = ancestors.Count - 1; num >= 0; num--)
			{
				IEnumerable<OverloadInfo> declaredClrMethods = ancestors[num].GetDeclaredClrMethods(ancestors[num].TypeTracker.Type, bindingFlags, clrNamePrefix, clrName, altClrName, specialNameOnly);
				if (AddMethodsOverwriteExisting(ref methods, declaredClrMethods, null, specialNameOnly))
				{
					ancestors[num].AddMethodNoCacheInvalidation(name, ancestors[num].MakeGroup(methods.Values));
				}
			}
			if (methods != null)
			{
				AddMethodsOverwriteExisting(ref methods, list, null, specialNameOnly);
				method = MakeGroup(methods.Values);
			}
			else
			{
				method = MakeGroup(list, list.Count, specialNameOnly, false);
			}
			return true;
		}

		private IEnumerable<OverloadInfo> GetDeclaredClrMethods(Type type, BindingFlags bindingFlags, string prefix, string name, string altName, bool specialNameOnly)
		{
			string memberName = prefix + name;
			string altMemberName = prefix + altName;
			MemberInfo[] methods = GetDeclaredClrMethods(type, bindingFlags, memberName);
			MemberInfo[] altMethods = ((altName != null) ? GetDeclaredClrMethods(type, bindingFlags, altMemberName) : IronRuby.Runtime.Utils.EmptyMemberInfos);
			foreach (MethodBase method in methods.Concat(altMethods))
			{
				if (IsVisible(method.Attributes, method.DeclaringType, specialNameOnly))
				{
					yield return new ReflectionOverloadInfo(method);
				}
			}
			if ((bindingFlags & BindingFlags.Instance) == 0)
			{
				yield break;
			}
			IEnumerable<ExtensionMethodInfo> extensions = GetClrExtensionMethods(type, memberName);
			IEnumerable<ExtensionMethodInfo> altExtensions = GetClrExtensionMethods(type, altMemberName);
			foreach (ExtensionMethodInfo extension in extensions.Concat(altExtensions))
			{
				ExtensionMethodInfo extensionMethodInfo = extension;
				yield return new ReflectionOverloadInfo(extensionMethodInfo.Method);
			}
		}

		private static MemberInfo[] GetDeclaredClrMethods(Type type, BindingFlags bindingFlags, string name)
		{
			if (name.LastCharacter() == 42)
			{
				name += "*";
			}
			return type.GetMember(name, MemberTypes.Method, bindingFlags | BindingFlags.InvokeMethod);
		}

		private IEnumerable<ExtensionMethodInfo> GetClrExtensionMethods(Type type, string name)
		{
			List<ExtensionMethodInfo> extensions;
			if (_extensionMethods != null && _extensionMethods.TryGetValue(name, out extensions))
			{
				foreach (ExtensionMethodInfo item in extensions)
				{
					yield return item;
				}
			}
			try
			{
				RubyModule[] mixins = base.Mixins;
				foreach (RubyModule mixin in mixins)
				{
					if (mixin._extensionMethods == null || !mixin._extensionMethods.TryGetValue(name, out extensions))
					{
						continue;
					}
					foreach (ExtensionMethodInfo extension in extensions)
					{
						ExtensionMethodInfo extensionMethodInfo = extension;
						if (extensionMethodInfo.IsExtensionOf(type))
						{
							yield return extension;
						}
					}
				}
			}
			finally
			{
			}
		}

		private bool AddMethodsOverwriteExisting(ref Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo> methods, IEnumerable<OverloadInfo> newOverloads, RubyOverloadGroupInfo[] overloadOwners, bool specialNameOnly)
		{
			bool result = false;
			int num = 0;
			foreach (OverloadInfo newOverload in newOverloads)
			{
				if (IsVisible(newOverload.Attributes, newOverload.DeclaringType, specialNameOnly))
				{
					Key<string, ValueArray<Type>> key = Key.Create(newOverload.Name, new ValueArray<Type>(ReflectionUtils.GetParameterTypes(newOverload.Parameters)));
					if (methods == null)
					{
						methods = new Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo>();
					}
					methods[key] = new ClrOverloadInfo
					{
						Overload = newOverload,
						Owner = ((overloadOwners != null) ? overloadOwners[num] : null)
					};
					result = true;
				}
				num++;
			}
			return result;
		}

		private bool IsVisible(MethodAttributes attributes, Type declaringType, bool specialNameOnly)
		{
			if (specialNameOnly && (attributes & MethodAttributes.SpecialName) == 0)
			{
				return false;
			}
			if (base.Context.DomainManager.Configuration.PrivateBinding)
			{
				return true;
			}
			switch (attributes & MethodAttributes.MemberAccessMask)
			{
				case MethodAttributes.Private:
				case MethodAttributes.FamANDAssem:
				case MethodAttributes.Assembly:
					return false;
				case MethodAttributes.Family:
				case MethodAttributes.FamORAssem:
					if (declaringType != null && declaringType.IsVisible)
					{
						return !declaringType.IsSealed;
					}
					return false;
				default:
					return true;
			}
		}

		private bool IsVisible(FieldInfo field)
		{
			if (base.Context.DomainManager.Configuration.PrivateBinding)
			{
				return true;
			}
			if (field.IsPrivate || field.IsAssembly || field.IsFamilyAndAssembly)
			{
				return false;
			}
			if (field.IsProtected())
			{
				if (field.DeclaringType != null && field.DeclaringType.IsVisible)
				{
					return !field.DeclaringType.IsSealed;
				}
				return false;
			}
			return true;
		}

		private bool IsWriteable(FieldInfo field)
		{
			if (!field.IsInitOnly)
			{
				return !field.IsLiteral;
			}
			return false;
		}

		private int GetVisibleMethodCount(IEnumerable<OverloadInfo> members, bool specialNameOnly)
		{
			int num = 0;
			foreach (OverloadInfo member in members)
			{
				if (IsVisible(member.Attributes, member.DeclaringType, specialNameOnly))
				{
					num++;
				}
			}
			return num;
		}

		private RubyOverloadGroupInfo MakeGroup(ICollection<ClrOverloadInfo> allMethods)
		{
			OverloadInfo[] array = new OverloadInfo[allMethods.Count];
			RubyOverloadGroupInfo[] array2 = new RubyOverloadGroupInfo[array.Length];
			int num = 0;
			foreach (ClrOverloadInfo allMethod in allMethods)
			{
				array[num] = allMethod.Overload;
				array2[num] = allMethod.Owner;
				num++;
			}
			RubyOverloadGroupInfo rubyOverloadGroupInfo = new RubyOverloadGroupInfo(array, this, array2, _isSingletonClass);
			foreach (ClrOverloadInfo allMethod2 in allMethods)
			{
				if (allMethod2.Owner != null)
				{
					allMethod2.Owner.CachedInGroup(rubyOverloadGroupInfo);
				}
				else
				{
					allMethod2.Owner = rubyOverloadGroupInfo;
				}
			}
			return rubyOverloadGroupInfo;
		}

		private RubyMethodGroupInfo MakeGroup(IEnumerable<OverloadInfo> members, int visibleMemberCount, bool specialNameOnly, bool isDetached)
		{
			OverloadInfo[] array = new OverloadInfo[visibleMemberCount];
			int num = 0;
			foreach (OverloadInfo member in members)
			{
				if (IsVisible(member.Attributes, member.DeclaringType, specialNameOnly))
				{
					array[num++] = member;
				}
			}
			if (!isDetached)
			{
				return new RubyOverloadGroupInfo(array, this, null, _isSingletonClass);
			}
			return new RubyMethodGroupInfo(array, this, _isSingletonClass);
		}

		private bool TryGetClrProperty(Type type, BindingFlags bindingFlags, bool isWrite, string name, string clrName, string altClrName, out RubyMemberInfo method)
		{
			return TryGetClrMethod(type, bindingFlags, true, name, isWrite ? "set_" : "get_", clrName, altClrName, out method);
		}

		private bool TryGetClrField(Type type, BindingFlags bindingFlags, bool isWrite, string name, string altName, out RubyMemberInfo method)
		{
			if (!TryGetClrField(type, bindingFlags, isWrite, name, out method))
			{
				if (altName != null)
				{
					return TryGetClrField(type, bindingFlags, isWrite, altName, out method);
				}
				return false;
			}
			return true;
		}

		private bool TryGetClrField(Type type, BindingFlags bindingFlags, bool isWrite, string name, out RubyMemberInfo method)
		{
			FieldInfo field = type.GetField(name, bindingFlags);
			if (field != null && IsVisible(field) && (!isWrite || IsWriteable(field)))
			{
				bool isDetached = (bindingFlags & BindingFlags.DeclaredOnly) != 0;
				method = new RubyFieldInfo(field, RubyMemberFlags.Public, this, isWrite, isDetached);
				return true;
			}
			method = null;
			return false;
		}

		private bool TryGetClrEvent(Type type, BindingFlags bindingFlags, string name, string altName, out RubyMemberInfo method)
		{
			if (!TryGetClrEvent(type, bindingFlags, name, out method))
			{
				if (altName != null)
				{
					return TryGetClrEvent(type, bindingFlags, altName, out method);
				}
				return false;
			}
			return true;
		}

		private bool TryGetClrEvent(Type type, BindingFlags bindingFlags, string name, out RubyMemberInfo method)
		{
			EventInfo @event = type.GetEvent(name, bindingFlags);
			if (@event != null)
			{
				bool isDetached = (bindingFlags & BindingFlags.DeclaredOnly) != 0;
				method = new RubyEventInfo((EventTracker)MemberTracker.FromMemberInfo(@event), RubyMemberFlags.Public, this, isDetached);
				return true;
			}
			method = null;
			return false;
		}

		public void BuildObjectAllocation(MetaObjectBuilder metaBuilder, CallArguments args, string methodName)
		{
			ArgsBuilder argsBuilder = new ArgsBuilder(0, 0, 0, 0, false);
			argsBuilder.AddCallArguments(metaBuilder, args);
			if (!metaBuilder.Error && !BuildAllocatorCall(metaBuilder, args, () => Microsoft.Scripting.Ast.Utils.Constant(base.Name)))
			{
				metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyClass))));
			}
		}

		public void BuildObjectConstruction(MetaObjectBuilder metaBuilder, CallArguments args, string methodName)
		{
			BuildObjectConstructionNoFlow(metaBuilder, args, methodName);
			metaBuilder.BuildControlFlow(args);
		}

		public void BuildClrObjectConstruction(MetaObjectBuilder metaBuilder, CallArguments args, string methodName)
		{
			OverloadInfo[] constructors;
			if (base.TypeTracker == null)
			{
				metaBuilder.SetError(Methods.MakeNotClrTypeError.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyClass))));
			}
			else if ((constructors = GetConstructors(base.TypeTracker.Type)).Length == 0)
			{
				metaBuilder.SetError(Methods.MakeConstructorUndefinedError.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyClass))));
			}
			else
			{
				RubyMethodGroupBase.BuildCallNoFlow(metaBuilder, args, methodName, constructors, SelfCallConvention.NoSelf, true);
			}
		}

		public void BuildObjectConstructionNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string methodName)
		{
			if (IsSingletonClass)
			{
				metaBuilder.SetError(Methods.MakeVirtualClassInstantiatedError.OpCall());
				return;
			}
			Type underlyingSystemType = GetUnderlyingSystemType();
			RubyMemberInfo info;
			using (base.Context.ClassHierarchyLocker())
			{
				metaBuilder.AddVersionTest(this);
				info = ResolveMethodForSiteNoLock(Symbols.Initialize, VisibilityContext.AllVisible).Info;
			}
			bool flag = info is RubyLibraryMethodInfo;
			bool flag2 = info.IsRubyMember && !flag;
			bool flag3 = flag && !info.DeclaringModule.IsObjectClass && !info.DeclaringModule.IsBasicObjectClass;
			if (flag2 || (flag3 && _isRubyClass))
			{
				bool flag4 = BuildAllocatorCall(metaBuilder, args, () => Microsoft.Scripting.Ast.Utils.Constant(base.Name));
				if (!metaBuilder.Error)
				{
					if (!flag4)
					{
						metaBuilder.SetError(Methods.MakeMissingDefaultConstructorError.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyClass)), Expression.Constant(info.DeclaringModule.Name)));
					}
					else if (!info.IsEmpty)
					{
						BuildOverriddenInitializerCall(metaBuilder, args, info);
					}
				}
				return;
			}
			SelfCallConvention callConvention = SelfCallConvention.SelfIsParameter;
			bool implicitProtocolConversions = false;
			if (typeof(Delegate).IsAssignableFrom(underlyingSystemType))
			{
				BuildDelegateConstructorCall(metaBuilder, args, underlyingSystemType);
				return;
			}
			OverloadInfo[] array;
			if (underlyingSystemType.IsArray && underlyingSystemType.GetArrayRank() == 1)
			{
				array = GetClrVectorFactories();
			}
			else if (_structInfo != null)
			{
				array = new OverloadInfo[1]
				{
					new ReflectionOverloadInfo(Methods.CreateStructInstance)
				};
			}
			else if (_factories.Length != 0)
			{
				array = ArrayUtils.ConvertAll(_factories, (Delegate d) => new ReflectionOverloadInfo(d.Method));
			}
			else
			{
				array = GetConstructors((underlyingSystemType == typeof(object)) ? typeof(RubyObject) : underlyingSystemType);
				if (underlyingSystemType.IsValueType)
				{
					if (array.Length == 0 || GetConstructor(underlyingSystemType) == null)
					{
						array = ArrayUtils.Append(array, new ReflectionOverloadInfo(Methods.CreateDefaultInstance));
					}
				}
				else if (array.Length == 0)
				{
					metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Expression.Convert(args.TargetExpression, typeof(RubyClass))));
					return;
				}
				callConvention = SelfCallConvention.NoSelf;
				implicitProtocolConversions = true;
			}
			RubyMethodGroupBase.BuildCallNoFlow(metaBuilder, args, methodName, array, callConvention, implicitProtocolConversions);
			if (!metaBuilder.Error)
			{
				metaBuilder.Result = MarkNewException(metaBuilder.Result);
				if (args.Signature.HasBlock)
				{
					metaBuilder.ControlFlowBuilder = RubyMethodGroupBase.RuleControlFlowBuilder;
				}
			}
		}

		private OverloadInfo[] GetConstructors(Type type)
		{
			return ReflectionOverloadInfo.CreateArray(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | (base.Context.DomainManager.Configuration.PrivateBinding ? BindingFlags.NonPublic : BindingFlags.Default)));
		}

		private ConstructorInfo GetConstructor(Type type, params Type[] parameterTypes)
		{
			return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | (base.Context.DomainManager.Configuration.PrivateBinding ? BindingFlags.NonPublic : BindingFlags.Default), null, parameterTypes, null);
		}

		private Expression MarkNewException(Expression expression)
		{
			if (!IsException())
			{
				return expression;
			}
			return Methods.MarkException.OpCall(Microsoft.Scripting.Ast.Utils.Convert(expression, typeof(Exception)));
		}

		private static void BuildOverriddenInitializerCall(MetaObjectBuilder metaBuilder, CallArguments args, RubyMemberInfo initializer)
		{
			Expression result = metaBuilder.Result;
			metaBuilder.Result = null;
			ParameterExpression temporary = metaBuilder.GetTemporary(result.Type, "#instance");
			args.SetTarget(temporary, null);
			if (initializer is RubyMethodInfo || initializer is RubyLambdaMethodInfo)
			{
				initializer.BuildCallNoFlow(metaBuilder, args, Symbols.Initialize);
			}
			else
			{
				metaBuilder.Result = Microsoft.Scripting.Ast.Utils.LightDynamic(RubyCallAction.Make(args.RubyContext, "initialize", new RubyCallSignature(args.Signature.ArgumentCount, (args.Signature.Flags & (RubyCallFlags)(-33)) | RubyCallFlags.HasImplicitSelf)), args.GetCallSiteArguments(temporary));
			}
			if (!metaBuilder.Error)
			{
				metaBuilder.Result = Methods.PropagateRetrySingleton.OpCall(Expression.Assign(temporary, result), metaBuilder.Result);
				if (args.Signature.HasBlock)
				{
					metaBuilder.ControlFlowBuilder = RubyMethodInfo.RuleControlFlowBuilder;
				}
			}
		}

		public bool BuildAllocatorCall(MetaObjectBuilder metaBuilder, CallArguments args, Func<Expression> defaultExceptionMessage)
		{
			Expression allocatorNewExpression = GetAllocatorNewExpression(args, defaultExceptionMessage);
			if (allocatorNewExpression != null)
			{
				metaBuilder.Result = MarkNewException(allocatorNewExpression);
				return true;
			}
			return false;
		}

		private Expression GetAllocatorNewExpression(CallArguments args, Func<Expression> defaultExceptionMessage)
		{
			Type type = GetUnderlyingSystemType();
			if (type == typeof(object))
			{
				type = typeof(RubyObject);
			}
			if (_structInfo != null)
			{
				return Methods.AllocateStructInstance.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(RubyClass)));
			}
			ConstructorInfo constructor;
			if (IsException())
			{
				if ((constructor = GetConstructor(type, typeof(string))) != null)
				{
					return Expression.New(constructor, defaultExceptionMessage());
				}
				if ((constructor = GetConstructor(type, typeof(string), typeof(Exception))) != null)
				{
					return Expression.New(constructor, defaultExceptionMessage(), Microsoft.Scripting.Ast.Utils.Constant(null));
				}
			}
			if ((constructor = GetConstructor(type, typeof(RubyClass))) != null)
			{
				return Expression.New(constructor, Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(RubyClass)));
			}
			if ((constructor = GetConstructor(type, typeof(RubyContext))) != null)
			{
				return Expression.New(constructor, Microsoft.Scripting.Ast.Utils.Convert(args.MetaContext.Expression, typeof(RubyContext)));
			}
			if ((constructor = GetConstructor(type)) != null)
			{
				return Expression.New(constructor);
			}
			if (type.IsValueType && type != typeof(int) && type != typeof(double))
			{
				return Expression.New(type);
			}
			return null;
		}

		private void BuildDelegateConstructorCall(MetaObjectBuilder metaBuilder, CallArguments args, Type type)
		{
			if (args.Signature.HasBlock)
			{
				RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
				if (!metaBuilder.Error)
				{
					metaBuilder.Result = Methods.CreateDelegateFromProc.OpCall(Microsoft.Scripting.Ast.Utils.Constant(type), Microsoft.Scripting.Ast.Utils.Convert(args.GetBlockExpression(), typeof(Proc)));
				}
				return;
			}
			IList<DynamicMetaObject> list = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
			if (!metaBuilder.Error)
			{
				ConvertBinder convertBinder = args.RubyContext.CreateConvertBinder(type, true);
				DynamicMetaObject metaResult = convertBinder.Bind(list[0], DynamicMetaObject.EmptyMetaObjects);
				metaBuilder.SetMetaResult(metaResult, args);
			}
		}

		private OverloadInfo[] GetClrVectorFactories()
		{
			if (_clrVectorFactories == null)
			{
				Type elementType = GetUnderlyingSystemType().GetElementType();
				_clrVectorFactories = new MethodInfo[2]
				{
					Methods.CreateVector.MakeGenericMethod(elementType),
					Methods.CreateVectorWithValues.MakeGenericMethod(elementType)
				};
			}
			return ReflectionOverloadInfo.CreateArray(_clrVectorFactories);
		}

		public override DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new Meta(parameter, BindingRestrictions.Empty, this);
		}
	}
}
